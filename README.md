[![](https://github.com/wraikny/EffFs/workflows/CI/badge.svg)](https://github.com/wraikny/EffFs/actions?workflow=CI)

# EffFs
F# Effect System based on SRTP.  
Caution: Multiple handlers cannot be composed.  

## Example
And [more examples](example/example.fsx)

```fsharp
#load "../src/EffFs/EffFs.fs"
open EffFs

type RandomInt = RandomInt of int with
  static member Effect = Eff.marker<int>

type Logging = Logging of string with
  static member Effect = Eff.marker<unit>

let inline foo() =
  eff {
    let! a = RandomInt 100
    do! Logging (sprintf "%d" a)
    let b = a + a
    return (a, b)
  }

let rand = System.Random()

type Handler = Handler with
  static member inline Handle(x) = x

  static member inline Handle(RandomInt a, k) =
    rand.Next(a) |> k

  static member inline Handle(Logging s, k) =
    printfn "[Log] %s" s; k()

foo()
|> Eff.handle Handler
|> printfn "%A"


// example output
(*
[Log] 66
(66, 132)
*)
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
