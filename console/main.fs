module Prisoner.Main
open System
open System.IO
open Prisoner.Domain
open System.Text

// ----------------------------------------------------------------------------
// Generating HTML output
// ----------------------------------------------------------------------------

type Node =
  | Node of string * (string * string) list * Node list
  | Text of string

type H() =
  static member (?) (h:H, name:string) = fun attrs children -> Node(name, attrs, children)

let h = H()
let text s = Text(s)

let rec format (sb:StringBuilder) nd =
  match nd with
  | Text(s) -> sb.Append(s) |> ignore
  | Node(name, attrs, children) ->
      sb.Append("<" + name) |> ignore
      for k, v in attrs do sb.Append(" " + k + "=\"" + v + "\"") |> ignore
      sb.Append(">") |> ignore
      for c in children do format sb c
      sb.Append("</" + name + ">") |> ignore

// ----------------------------------------------------------------------------
// Playing the game
// ----------------------------------------------------------------------------

let play s1 s2 =
  let mutable o1 = s1.Initial
  let mutable o2 = s2.Initial
  let mutable score1 = 0
  let mutable score2 = 0
  for i in 1 .. 100 do
    let a1, a2 = s1.Play o1, s2.Play o2
    o1 <- s1.Learn o1 a1 a2
    o2 <- s2.Learn o2 a2 a1
    let gain1, gain2 =
      match a1, a2 with
      | Collaborate, Collaborate -> 2, 2
      | Collaborate, Betray -> 0, 3
      | Betray, Collaborate -> 3, 0
      | Betray, Betray -> 1, 1
    score1 <- score1 + gain1
    score2 <- score2 + gain2
  score1, score2

let playAll() =
  strategies |> Seq.map (fun s1 ->
    strategies |> Seq.map (fun s2 ->
      play s1 s2 ) |> Array.ofSeq) |> Array.ofSeq

// ----------------------------------------------------------------------------
// Run and generate HTML table
// ----------------------------------------------------------------------------

let stats (games:seq<int * int>) =
  Seq.sumBy fst games,
  games |> Seq.fold (fun (w,d,l) (a,b) ->
    if (a = b) then (w, d+1, l)
    elif (a > b) then (w+1, d, l)
    else (w, d, l+1)) (0, 0, 0)

let generate results =
  let sorted =
    Seq.zip strategies results
    |> Seq.map (fun (s, res) -> s, res, stats res)
    |> Seq.sortByDescending (fun (_, _, (score, _)) -> score)

  h?table ["class", "table table-striped"] [
    h?thead [] [ h?tr [] [
      yield h?td [] []
      for s in strategies -> h?td [] [ text s.Name ]
      yield h?td [] [text "Wins"]
      yield h?td [] [text "Draws"]
      yield h?td [] [text "Losses"]
      yield h?td [] [text "Total score"]
    ] ]
    h?tbody [] [
      for s, rs, (score, (w, d, l)) in sorted -> h?tr [] [
        yield h?td [] [ text s.Name ]
        for r1, r2 in rs -> h?td [] [ text (sprintf "%d:%d" r1 r2) ]
        yield h?td [] [ text (string w) ]
        yield h?td [] [ text (string d) ]
        yield h?td [] [ text (string l) ]
        yield h?td [] [ text (string score) ]
      ]
    ]
  ]

[<EntryPoint>]
let main _ =
  Register.registerAll()
  let results = playAll()
  let node = generate results
  let sb = StringBuilder()
  format sb node
  let template = Path.Combine(Directory.GetCurrentDirectory(), "template.html")
  let output = Path.Combine(Directory.GetCurrentDirectory(), "output.html")
  let html = File.ReadAllText(template).Replace("[TABLE]", sb.ToString())
  File.WriteAllText(output, html)
  0
