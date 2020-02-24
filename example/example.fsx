#load "../src/EffFs/EffFs.fs"
open EffFs

type RandomInt = RandomInt of int
type Println = Println of obj

let inline hoge() = eff {
  let! a = RandomInt 10000
  do! Println "Hello!"
  return (float a / 10000.0)
}

let inline foo(): Eff<'a, ^h> =
  eff {
    let! a = RandomInt 100
    let! x = hoge()
    do! Println x
    do! Println a
    let b = a + a
    return (a, b)
  }

module Handlers =
  let rand = System.Random()

  type Handler1 = Handler1 with
    static member inline Handle(x) = x

    // allow nested effect
    static member inline Handle(e, k) = Eff.nest e k

    static member inline Handle(RandomInt a, k) =
      rand.Next(a) |> k

    static member inline Handle(Println a, k) =
      printfn "%A" a; k()

  type Handler2 = {name : string } with

    static member inline Handle(x) = x

    static member inline Handle(RandomInt a, k) =
      // capture the handler
      Eff.capture(fun h ->
        printfn "[%s]: RandomInt(%d)" h.name a
        rand.Next(a) |> k
      )

    static member inline Handle(Println a, k) =
      // capture the handler
      Eff.capture(fun h ->
        printfn "[%s]: Println(%A)" h.name a
        printfn "%A" a; k()
      )

    static member inline Handle(e, k) =
      // Hack in nested effect
      Eff.capture(fun h ->
        printfn "[%s]: Nest(%A)" h.name e
        e |> Eff.handle { name = h.name + "-Nested"} |> k
      )


foo()
|> Eff.handle Handlers.Handler1
|> printfn "%A"

printfn "---"

let bar : Eff<_, Handlers.Handler2> = foo()

bar
|> Eff.handle { Handlers.Handler2.name = "Handler2" }
|> printfn "%A"

// example output
(*
"Hello!"
0.7097
88
(88, 176)
---
[Handler2]: RandomInt(100)
[Handler2]: Nest(Eff <fun:bar@69-2>)
[Handler2-Nested]: RandomInt(10000)
[Handler2-Nested]: Println("Hello!")
"Hello!"
[Handler2]: Println(0.0669)
0.0669
[Handler2]: Println(95)
95
(95, 190)
*)

// printfn "---"

// module Debug =
//   type E1 = E1
//   type E2 = E2
//   type E3 = E3

//   let inline hoge() =
//     eff {
//       let! x = E1
//       let y = if true then 0 else 1
//       for i in 0..3 do
//         printfn "aaa"
//         do! E2
//         // do! E2
//         // if i = 3 then return -1
//         // return -1
//       // use! a = E3
//       // if true then return -1
//       let b = 1
//       return x
//     }

//   type H = H with
//     static member inline Handle(x) = x
//     static member inline Handle(E1, k) =
//       k 100
//     static member inline Handle(E2, k) =
//       printfn "hello"; k()
//     static member inline Handle(E3, k) =
//       { new System.IDisposable with
//         member __.Dispose() = printfn "disposed"
//       } |> k

//   let inline main() = hoge() |> Eff.handle H |> printfn "%A"

// Debug.main()