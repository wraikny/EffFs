#load "../src/EffFs/EffFs.fs"

type RandomInt = RandomInt of int
type PrintInt = PrintInt of int

[<AutoOpen>]
module Handler =
  let rand = System.Random()

  type Handler = Handler with
    static member inline Handle(RandomInt a, k) =
      rand.Next(a) |> k
    
    static member inline Handle(PrintInt a, k) =
      printfn "%d" a; k a

open EffFs

let inline hoge< ^h > =
  eff {
    let! a = RandomInt 100
    let! _ = PrintInt 100
    let b = a + a
    return (a, b)
  }

hoge |> perform Handler |> printfn "%A"

// example output
(*
100
(93, 186)
*)

