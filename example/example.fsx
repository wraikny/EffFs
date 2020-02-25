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

(*
error FS0071: Type constraint mismatch when applying the default type 'obj' for a type inference variable.
internal error: Exception of type 'FSharp.Compiler.ConstraintSolver+LocallyAbortOperationThatFailsToResolveOverload' was thrown.
Consider adding further type constraintsF# Compiler(71)

error FS0071: 既定の型 'obj' を型推論の変数に適用するときに、型の制約が一致しませんでした。
内部エラー: Exception of type 'FSharp.Compiler.ConstraintSolver+LocallyAbortOperationThatFailsToResolveOverload' was thrown.
型の制約を増やしてください
*)
foo()
|> Eff.handle Handlers.Handler1
|> printfn "%A"

printfn "---"

// let bar : Eff<_, Handlers.Handler2> = foo()

// bar
// |> Eff.handle { Handlers.Handler2.name = "Handler2" }
// |> printfn "%A"

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
