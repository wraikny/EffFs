[![](https://github.com/wraikny/EffFs/workflows/CI/badge.svg)](https://github.com/wraikny/EffFs/actions?workflow=CI)

# EffFs
Something like Algebraic Effects, but I don't know.

## Example
```fsharp
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

let rand = System.Random()

type Handler = Handler with
  static member inline Handle(x) = x

  static member inline Handle(RandomInt a, k) =
    rand.Next(a) |> k
  
  static member inline Handle(Println a, k) =
    printfn "%A" a; k()

foo()
|> Eff.handle Handler
|> printfn "%A"
```

output example:
```
66
(66, 132)
```

## .NET Core
```sh
$ dotnet --version
3.1.101
```

## Build
```sh
$ dotnet build src/EffFs # Debug
$ dotnet build src/EffFs -c Release
```

## Example
```
$ dotnet fsi --exec example/example.fs
```
