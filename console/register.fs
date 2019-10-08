module Prisoner.Register

let registerAll() =
  AlwaysCollaborateStrategy.register()
  CopyStrategy.register()
  RandomStrategy.register()
  TomasRandomStrategy.register()
