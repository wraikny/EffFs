System.Environment.CurrentDirectory <- __SOURCE_DIRECTORY__
#load "../src/EffFs/EffFs.fs"
#load "../src/EffFs/Library/Random.fs"

open EffFs
open EffFs.Library

let inline f() = eff {
  let! a = Random.int 1 10
  let a : int = a
  return! Random.array 10 (Random.int 0 a)
}

type Handler() =
  let rand = System.Random()

  member private __.Rand with get() = rand

  static member inline Handle(x) = x
  static member inline Handle(Random.RandomEffect f, k) =
    Eff.capture(fun (h: Handler) -> f h.Rand |> k)

let handler = Handler()

let result: int[] = f() |> Eff.handle handler

result |> printfn "%A"
