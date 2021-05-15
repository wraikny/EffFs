System.Environment.CurrentDirectory <- __SOURCE_DIRECTORY__
#load "../src/EffFs/EffFs.fs"
#load "../src/EffFs/Library/Random.fs"

open EffFs
open EffFs.Library.Random

let inline f() = eff {
  let! a = RandomEffect.int 1 10
  let a : int = a
  return! RandomEffect.initArray 10 (RandomEffect.int 0 a)
}

[<Sealed>]
type Handler(seed) =
  member val private Rand = System.Random(seed) with get

  static member inline Value(_, x) = x
  static member inline Handle(RandomEffect f, k) =
    Eff.capture(fun (h: Handler) -> f h.Rand |> k)


let handler = Handler(0)

let result: int[] = f() |> Eff.handle handler

result |> printfn "%A"
