System.Environment.CurrentDirectory <- __SOURCE_DIRECTORY__
#load "../src/EffFs/EffFs.fs"
#load "../src/EffFs/Library/StateMachine.fs"
#load "../src/EffFs/Library/Log.fs"

open EffFs
open EffFs.Library

let inline effectful (): Eff<_, _> = eff {
  do! Log.log "Hello"
  do! Log.logf "%s" "world!"
}

[<Sealed>]
type Handler() =
  static member inline Value(_, x) = x
  static member inline Handle(Log.LogEffect s, k) =
    let date = System.DateTime.Now
    printfn "[Log][%O] %s" date s; k()

let handler = Handler()

(effectful ())
|> Eff.handle handler
