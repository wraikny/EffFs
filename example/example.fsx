#load "../src/EffFs/EffFs.fs"
open EffFs

type RandomInt = RandomInt of int with
  static member Effect = Eff.output<int>

type Println = Println of obj with
  static member Effect = Eff.output<unit>

let inline hoge(): Eff<'a, ^h> = eff {
  let! a = RandomInt 10000
  do! Println "Hello!"
  return (float a / 10000.0)
}

let inline foo() =
  eff {
    let! a = RandomInt 100
    let! x = hoge()
    do! Println x
    do! Println a
    let b = a + a
    return (a, b)
  }

let rand = System.Random()

type Handler1 = Handler1 with
  static member inline Handle(x) = x

  static member inline Handle(RandomInt a, k) =
    rand.Next(a) |> k

  static member inline Handle(Println a, k) =
    printfn "%A" a; k()

type Handler2 = { name : string } with
  static member inline Handle(x) = x

  static member inline Handle(RandomInt a, k) =
    // capture the handler
    Eff.capture <| fun h ->
      printfn "[%s]: RandomInt(%d)" h.name a
      rand.Next(a) |> k

  static member inline Handle(Println a, k) =
    // capture the handler
    Eff.capture <| fun h ->
      printfn "[%s]: Println(%A)" h.name a
      printfn "%A" a; k()


let main () =
  foo()
  |> Eff.handle Handler1
  |> printfn "%A"

  printfn "---"

  let bar : Eff<_, Handler2> = foo()

  bar
  |> Eff.handle { Handler2.name = "Handler2" }
  |> printfn "%A"

  printfn "---"

  RandomInt 100
  |> Eff.handle Handler1
  |> printfn "Random: %d"

main ()