module AlwaysCollaborateStrategy 
open Prisoner.Domain

type State = unit

let initial = ()

let play state = 
  Collaborate

let learn state your opponent =
  ()

let register() =
  register "Always collaborate strategy" initial play learn