[![](https://github.com/wraikny/EffFs/workflows/CI/badge.svg)](https://github.com/wraikny/EffFs/actions?workflow=CI)

# EffFs
F# Effect System based on SRTP.  
Caution: Multiple handlers cannot be composed.  

[SharpLab compilation result](https://sharplab.io/#v2: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=) 
of [example.fsx](example/example.fsx)

## Example
And [more examples](example/example.fsx)

```fsharp
#load "../src/EffFs/EffFs.fs"
open EffFs

type RandomInt = RandomInt of int with
  static member Effect = Eff.marker<int>

type Logging = Logging of string with
  static member Effect = Eff.marker<unit>

let inline foo() = eff {
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
