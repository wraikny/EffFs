module EffFs.DynamicHandler

open System
open System.Collections.Generic

[<Sealed>]
type DynamicHandler<'i, 'o> internal (valueHandler, handlers: IReadOnlyDictionary<Type, obj>) =
  member val ValueHandler = valueHandler
  member val Handlers = handlers

  static member Value(h: DynamicHandler<'i, 'o>, x: 'i): 'o = h.ValueHandler(x)

  static member inline Handle(e: obj, k: 'a -> Eff<_, DynamicHandler<_, _>>): Eff<_, DynamicHandler<_, _>> =
    Eff.capture (fun (h: DynamicHandler<_, _>) ->
      let ty = e.GetType()

      if not <| h.Handlers.ContainsKey(ty) then
        failwithf "DynamicHandler<_, _> doesn't contains handler for type '%s'" ty.Name

      let handle = h.Handlers.[e.GetType()] |> unbox<obj -> 'a>
      handle e |> k
    )

  static member internal Compose(valueHandler, d1: DynamicHandler<_, _>, d2: DynamicHandler<_, _>) =
    let d = Dictionary<_, _>()
    for x in d1.Handlers do d.[x.Key] <- x.Value
    for x in d2.Handlers do d.[x.Key] <- x.Value
    DynamicHandler<_, _>(valueHandler, d)


[<Struct>]
type ValueDraft<'i, 'o> = ValueDraft of ('i -> 'o)


[<Struct>]
type HandlersDraft = HandlersDraft of (Type * obj) list


[<Struct>]
type DynamicHandlerBuilder =
  member __.Run((valueHandler, ())) = ValueDraft valueHandler

  member __.Run(((), handlers: (Type * obj) list)) = HandlersDraft handlers

  member __.Run((valueHandler, handlers: (Type * obj) list)) =
    let d = new Dictionary<Type, obj>()
    for (k, v) in handlers do d.[k] <- v
    DynamicHandler(valueHandler, d)

  member inline __.Yield(_) = ((), ())

  [<CustomOperation("value")>]
  member __.ValueHandle(((), d), f: 'i -> 'o) = (f, d)

  [<CustomOperationAttribute("handle")>]
  member inline __.AddHandle((vh, d: (Type * obj) list), f: ^e -> 'a): _
    when 'e: (static member Effect: 'e -> EffectTypeMarker<'a>) =
    let h : obj -> 'a = unbox< ^e > >> f
    (vh, (typeof<'e>, box h) :: d)

  [<CustomOperationAttribute("handle")>]
  member inline this.AddHandle((vh, ()), f) =
    this.AddHandle((vh, List.empty), f)

  [<CustomOperationAttribute("compose")>]
  member this.Compose(((), hs), ValueDraft f) = this.ValueHandle(((), hs), f)

  [<CustomOperationAttribute("compose")>]
  member __.Compose((vh, hs: (Type * obj) list), HandlersDraft handlers) =
    if handlers.IsEmpty then (vh, hs)
    else (vh, hs |> List.append handlers)

  [<CustomOperationAttribute("compose")>]
  member __.Compose((vh, ()), HandlersDraft handlers) =
    (vh, handlers)

  [<CustomOperationAttribute("compose")>]
  member __.Compose(((), hs: (Type * obj) list), handler: DynamicHandler<'i, 'o>) =
    handler.Handlers.Count |> function
    | 0 -> (handler.ValueHandler, hs)
    | _ -> (handler.ValueHandler, [ for x in handler.Handlers -> (x.Key, x.Value)])

  [<CustomOperationAttribute("compose")>]
  member __.Compose(((), ()), handler: DynamicHandler<'i, 'o>) =
    (handler.ValueHandler, handler.Handlers)


[<RequireQualifiedAccess>]
module Eff =
  let dynamic = new DynamicHandlerBuilder()

  let compose valueHandler d1 d2 = DynamicHandler.Compose(valueHandler, d1, d2)
