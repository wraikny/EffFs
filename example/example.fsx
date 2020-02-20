#load "../src/EffFs/EffFs.fs"

type RandomInt = RandomInt of int
type PrintInt = PrintInt of int

let inline hoge() =
  EffFs.eff {
    let! a = RandomInt 100
    let! _ = PrintInt 100
    let b = a + a
    return (a, b)
  }

module Handler =
  let rand = System.Random()

  type Handler1 = Handler1 with
    static member inline Handle(RandomInt a, k) =
      rand.Next(a) |> k
    
    static member inline Handle(PrintInt a, k) =
      printfn "%d" a; k a

  type Handler2 = Handler2 with
    static member inline Handle(RandomInt a, k) =
      printfn "random: %d" a
      rand.Next(a) |> k
    
    static member inline Handle(PrintInt a, k) =
      printfn "print: %d" a
      printfn "%d" a; k a

hoge()
|> EffFs.perform Handler.Handler1
|> printfn "%A"

printfn "---"

hoge()
|> EffFs.perform Handler.Handler2
|> printfn "%A"

// example output
(*
100
(66, 132)
---
random: 100
print: 100
100
(77, 154)
*)

