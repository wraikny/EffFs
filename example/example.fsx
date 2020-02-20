#load "../src/EffFs/EffFs.fs"
open EffFs

type RandomInt = RandomInt of int
type Println = Println of obj

let inline foo() =
  eff {
    let! a = RandomInt 100
    do! Println a
    let b = a + a
    return (a, b)
  }

module Handlers =
  let rand = System.Random()

  type Handler1 = Handler1 with
    static member inline Handle(x) = x

    static member inline Handle(RandomInt a, k) =
      rand.Next(a) |> k
    
    static member inline Handle(Println a, k) =
      printfn "%A" a; k()

  type Handler2 = Handler2 with
    static member inline Handle(x) =
      let a, b = x
      (a * 1000) + b

    static member inline Handle(_:Eff<unit,_>, k) = k()

    static member inline Handle(RandomInt a, k) =
      printfn "random: %d" a
      rand.Next(a) |> k
    
    static member inline Handle(Println a, k) =
      printfn "print: %A" a
      printfn "%A" a; k()

foo()
|> Eff.handle Handlers.Handler1
|> printfn "%A"

printfn "---"

let bar : Eff<_, Handlers.Handler2> = foo()

bar
|> Eff.handle Handlers.Handler2
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
