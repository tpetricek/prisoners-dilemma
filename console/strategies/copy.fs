module CopyStrategy
open Prisoner.Domain

type State = 
  { Previous : Action }

let initial = 
  { Previous = Collaborate }

let play state = 
  state.Previous

let learn state your opponent =
  { Previous = opponent }

let register() =
  register "Copy strategy" initial play learn
