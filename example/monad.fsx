#load "../src/EffFs/EffFs.fs"
open EffFs

module Handlers =
  type Option = Option with
    static member inline Handle(x) = Some x

    static member inline Handle(x, k) =
      Eff.bindWith Option.bind x k

  type Seq = Seq with
    static member inline Handle(x) = seq [ x ]

    static member inline Handle(x, k: 'a -> Eff<'b, ^h>) =
      Eff.bindWith Seq.collect x k


  type MonadSeq = MonadSeq with
    static member inline Handle(x) = seq [ x ]

    static member inline Handle(x, k) =
      x |> function
      | Some a -> k a
      | None -> Eff.pure' Seq.empty

    static member inline Handle(x, k) =
      x |> function
      | Ok a -> k a
      | Error e ->
        printfn "Error(%A)" e
        Eff.pure' Seq.empty

    static member inline Handle(x, k: 'a -> Eff<'b, ^h>) =
      Eff.bindWith Seq.collect x k

let printOption = function Some x -> printfn "Some(%A)" x | _ -> printfn "None"

let inline sample1 handler effect =
  eff {
    let! a = effect
    let b = 10
    let c = a + b + 10
    let! d = effect
    return (a, c)
  } |> Eff.handle handler

Some 0
|> sample1 Handlers.Option
|> printOption

printfn "-----"

None
|> sample1 Handlers.Option
|> printOption

printfn "-----"

[1..3]
|> sample1 Handlers.Seq
|> Seq.toList
|> printfn "%A"

printfn "-----"


eff {
  let! a = Some 1
  let! b = [1; 2; 3]
  let! c = Ok 100
  return (a, a * b)
}
|> Eff.handle Handlers.MonadSeq
|> Seq.toList
|> printfn "%A"

printfn "-----"

eff {
  let! a = None
  let! b = [1; 2; 3]
  let! c = Ok 100
  return (a, a * b)
}
|> Eff.handle Handlers.MonadSeq
|> Seq.toList
|> printfn "%A"

printfn "-----"

eff {
  let! a = Some -1
  let! b = [1; 2; 3]
  let! c = (if b = 2 then Error "b = 2" else Ok 10)
  return (a, a * b)
}
|> Eff.handle Handlers.MonadSeq
|> Seq.toList
|> printfn "%A"

(*

Some((0, 20))
-----
None
-----
[(1, 21); (1, 21); (1, 21); (2, 22); (2, 22); (2, 22); (3, 23); (3, 23); (3, 23)]
-----
[(1, 1); (1, 2); (1, 3)]
-----
[]
-----
Error("b = 2")
[(-1, -1); (-1, -3)]

*)