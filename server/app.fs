module Prisoner.App
open System
open System.IO
open System.Diagnostics
open System.Xml.Linq
open Giraffe

// ----------------------------------------------------------------------------

let (</>) a b = Path.Combine(a, b)
let consoleRoot = Directory.GetCurrentDirectory() </> ".." </> "console"

let updateConsoleProject () =
  let strategies = 
    [ for f in Directory.GetFiles(consoleRoot </> "strategies") 
               |> Array.filter(fun f -> f.EndsWith ".fs") ->
        let decl = File.ReadLines(f) |> Seq.head
        Path.GetFileName(f), decl.Split(' ').[1] ]

  let compiles = 
    [ yield "domain.fs" 
      for f, _ in strategies -> "strategies/" + f 
      yield "register.fs"
      yield "main.fs" ]

  let xd = XDocument.Load(consoleRoot </> "console.fsproj")
  let ig = xd.Element(XName.Get "Project").Element(XName.Get "ItemGroup")
  ig.RemoveAll()
  for f in compiles do 
    XElement.Parse(sprintf "<Compile Include=\"" + f + "\" />") |> ig.Add
  xd.Save(consoleRoot </> "console.fsproj")

  let register = 
    [ yield "module Prisoner.Register"
      yield ""
      yield "let registerAll() ="
      for _, m in strategies ->
        "  " + m + ".register()" ]
  File.WriteAllLines(consoleRoot </> "register.fs", register)

// ----------------------------------------------------------------------------

let rebuildAndRun () =
  let psi = 
    ProcessStartInfo(FileName="dotnet", WorkingDirectory=consoleRoot, 
      Arguments="build", UseShellExecute=false)  
  let ps = System.Diagnostics.Process.Start(psi)
  ps.WaitForExit()
  let psi = 
    ProcessStartInfo(FileName="dotnet", WorkingDirectory=consoleRoot, 
      Arguments="bin/Debug/netcoreapp2.2/console.dll", UseShellExecute=false)  
  let ps = System.Diagnostics.Process.Start(psi)
  ps.WaitForExit()

// ----------------------------------------------------------------------------

// https://github.com/mathias-brandewinder/fsibot/blob/master/FsiBot/FsiBot/Filters.fs
let badBoys = [   
    "System.IO"
    "System.Net"
    "System.Threading"
    "System.Reflection"
    "System.Diagnostics"
    "Console."
    "System.Environment"
    "System.AppDomain"
    "System.Runtime"
    "Microsoft." ]   

let app : HttpHandler = 
  choose [
    route "/" >=> htmlFile "web/index.html"
    route "/results" >=> htmlFile (consoleRoot </> "output.html")
    route "/upload" >=> fun r c ->
      let name = c.Request.Form.["name"] |> Seq.head
      let code = c.Request.Form.["code"] |> Seq.head
      let extension =
        if badBoys |> Seq.exists (fun bad -> code.Replace(" ","").Contains(bad))
        then ".fs_approve_manually"
        else ".fs" 
      File.WriteAllText(consoleRoot </> "strategies" </> (name.Replace("..", "").Replace("/", "").Replace("\\", "") + extension), code)
      updateConsoleProject ()
      rebuildAndRun ()
      htmlFile (consoleRoot </> "output.html") r c
  ]
