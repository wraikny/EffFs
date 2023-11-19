#load "../src/EffFs/EffFs.fs"
#load "../src/EffFs/DynamicHandler.fs"

open EffFs
open EffFs.DynamicHandler

[<Struct; NoEquality; NoComparison>]
type Answer =
  struct

    static member Effect(_) = Eff.marker<int>
  end

[<Struct; NoEquality; NoComparison>]
type Print =
  | Print of string

  static member Effect(_) = Eff.marker<unit>

let inline compute () =
  eff {
    let! x = Answer()
    let! y = Answer()
    do! Print <| string (x + y)
    return x * y
  }

compute ()
|> Eff.handle (
  Eff.dynamic {
    value (fun x -> x * 1000)
    handle (fun (_: Answer) -> 42)
    handle (fun (Print s) -> printfn "[Print]%s" s)
  }
)
|> printfn "Result: %A"

let valueHandler () = Eff.dynamic { value (fun x -> x) }
let answerHandler = Eff.dynamic { handle (fun (_: Answer) -> 42) }
let printHandler = Eff.dynamic { handle (fun (Print s) -> printfn "[Dynamic]%s" s) }

compute ()
|> Eff.handle (
  Eff.dynamic {
    compose answerHandler
    compose (valueHandler ())
    compose printHandler
  }
)
|> printfn "Composed: %A"
