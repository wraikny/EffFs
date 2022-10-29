#load "../src/EffFs/EffFs.fs"
open EffFs

[<Struct; NoEquality; NoComparison>]
type RandomInt =
  | RandomInt of int

  static member Effect(_) = Eff.marker<int>

[<Struct; NoEquality; NoComparison>]
type Logging =
  | Logging of string

  static member Effect(_) = Eff.marker<unit>

let logfn fmt =
  Printf.kprintf (sprintf "%s" >> Logging) fmt

let inline hoge () : Eff<_, _> =
  eff {
    let! a = RandomInt 10000
    do! Logging "Hello!"
    return (float a / 10000.0)
  }

let inline foo () : Eff<_, ^h> =
  eff {
    let! a = RandomInt 100
    let! x = hoge ()
    do! logfn "%f" x
    do! logfn "%d" a
    let b = a + a

    if true then
      do! Logging "Hello"

    do! Logging "Continuation"
    return (a, b)
  }

let rand = System.Random()

[<Struct; NoEquality; NoComparison>]
type Handler1 =
  struct
    static member inline Value(_, x) = x

    static member inline Handle(RandomInt a, k) = rand.Next(a) |> k

    static member inline Handle(Logging a, k) =
      printfn "%A" a
      k ()
  end

[<NoEquality; NoComparison>]
type Handler2 =
  { name: string }

  static member inline Value(_, x) =
    printfn "ValueHandler invoked"
    x

  static member inline Handle(RandomInt a, k) =
    // capture the handler
    Eff.capture
    <| fun h ->
         printfn "[%s]: RandomInt(%d)" h.name a
         rand.Next(a) |> k

  static member inline Handle(Logging a, k) =
    // capture the handler
    Eff.capture
    <| fun h ->
         printfn "[%s]: Logging(%A)" h.name a
         printfn "%A" a
         k ()


let main () =
  foo () |> Eff.handle (Handler1()) |> printfn "%A"

  printfn "---"

  let bar: Eff<_, Handler2> = foo ()

  bar |> Eff.handle { Handler2.name = "Handler2" } |> printfn "%A"

  printfn "---"

  RandomInt 100 |> Eff.handle (Handler1()) |> printfn "Random: %d"

main ()
