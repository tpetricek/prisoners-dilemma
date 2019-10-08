module Prisoner.Domain

type Action =
  | Collaborate
  | Betray

type Strategy = 
  { Name : string 
    Initial : obj 
    Play : obj -> Action
    Learn : obj -> Action -> Action -> obj }

let strategies = ResizeArray<Strategy>()

let register (name:string) (initial:'TState) (play:'TState -> Action) 
    (learn:'TState -> Action -> Action -> 'TState) = 
  { Name = name; Initial = box initial;
    Play = fun o -> play (unbox o) 
    Learn = fun o a1 a2 -> box (learn (unbox o) a1 a2) }
  |> strategies.Add
