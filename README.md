# Prisoner's Dilemma Dojo

## The game idea

[Prisoner's Dilemma](https://en.wikipedia.org/wiki/Prisoner%27s_dilemma) is a simple game
with two players. In each round, the players can choose either to _collaborate_ or to _betray_.
Depending on the choices, they get some points:

 - If both _betray_, they both get 1 point
 - If both _collaborate_, they both get 2 points
 - If player A _betrays_ but B _collaborates_, then A gets 3 points, but B gets 0 points

In an interated version of the game, this is repeated a number of times and the total score
is calculated based on, say, 100 iterations. In this case, players can follow various strategies
that can also adapt based on what the other player does. Simple ones that ignore the other
player are:

 - **Always collaborate** - we always collaborate, regardless of what the other player does
 - **Choose random** - choose randomly between collaboration and betrayal.

A somewhat more sophisticated strategy that responds to the other player:

 - **Copy the opponent** - do whatever the opponent did last time.

## Implementing strategies

Strategies are implemented in the `console/strategies` folder. Each strategy has one `fs` file.
For example, the "copy the opponent" strategy looks like this:

```
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
```

Each strategy needs to define its own `State` type, which is used to keep state between
iterations, `initial` value of the state and two functions:

 - `play` returns an `Action`, which is either `Action.Collaborate` or `Action.Betray`,
   depending on the vale of the state (this is the initial value in the first round)
 - `learn` updates the state based on a game - it gets the previous state together with
   your action and the action of the opponent as arguments.

Finally, each strategy also must register itself by calling `register` with a name,
initial state and the two functions.

When adding a new strategy, you need to add the strategy file and make the following two changes:

 - In `console/console.fsproj`, you need to add new `Compile` node to include your file
 - In `console/register.fs`, you need to add a new line to call your `register` function

## Running the game

Assuming you have .NET Core installed, you can run the game by compiling and running the
application in the `console` directory (use `\` slashes on Windows):

```
cd console
dotnet build
dotnet bin/Debug/netcoreapp2.2/console.dll
```

This will generate a table with results in the `output.html` file.
