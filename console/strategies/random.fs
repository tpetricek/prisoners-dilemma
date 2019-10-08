module RandomStrategy
open Prisoner.Domain

type State = 
  { Generator : System.Random }

let initial = 
  { Generator = new System.Random() }

let play state = 
  if state.Generator.Next(2) = 0 then Collaborate
  else Betray

let learn state your opponent =
  state

let register () = 
  register "Random strategy" initial play learn