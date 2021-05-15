// #load "../src/EffFs/EffFs.fs"
// #load "../src/EffFs/DynamicHandler.fs"
#r "../src/EffFs/bin/Release/netstandard2.1/EffFs.dll"

open EffFs
open EffFs.DynamicHandler

type Answer = Answer with
  static member Effect(_) = Eff.marker<int>

type Print = Print of string with
  static member Effect(_) = Eff.marker<unit>

let inline compute () = eff {
  let! x = Answer
  let! y = Answer
  do! Print <| string(x + y)
  return x * y
}

compute()
|> Eff.handle (
  Eff.dynamic {
    value (fun x -> x * 1000)
    handle (fun Answer -> 42)
    handle (fun (Print s) -> printfn "[Print]%s" s)
  }
)
|> printfn "Result: %A"

let valueHandler() = Eff.dynamic { value (fun x -> x) }
let answerHandler = Eff.dynamic { handle (fun Answer -> 42) }
let printHandler = Eff.dynamic { handle (fun (Print s) -> printfn "[Dynamic]%s" s) }

compute()
|> Eff.handle (
  Eff.dynamic {
    compose answerHandler
    compose (valueHandler())
    compose printHandler
  }
)
|> printfn "Composed: %A"

