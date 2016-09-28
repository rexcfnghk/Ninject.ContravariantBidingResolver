namespace Ninject.ContravariantBindingResolver.FSharp

open System.Reflection
open Ninject.Components
open Ninject.Planning.Bindings.Resolvers

type ContravariantBindingResolver () =
    inherit NinjectComponent ()
    interface IBindingResolver with
        member __.Resolve (bindings, service) =
            if not service.IsGenericType
            then Seq.empty
            else
                let genericType = service.GetGenericTypeDefinition ()
                let genericArguments = genericType.GetGenericArguments ()
                if Array.length genericArguments = 1 || 
                    GenericParameterAttributes.Contravariant
                    |> genericArguments.[0].GenericParameterAttributes.HasFlag
                    |> not
                then Seq.empty
                else
                    let argument = service.GetGenericArguments () |> Array.exactlyOne
                    bindings
                    |> Seq.map (fun (KeyValue (k, v)) -> 
                        k, k.GetGenericArguments () |> Array.exactlyOne, v)
                    |> Seq.filter (fun (key, genericArgument, _) -> 
                        key.IsGenericType && 
                        key.GetGenericTypeDefinition () = genericType &&
                        genericArgument <> argument && 
                        genericArgument.IsAssignableFrom argument)
                    |> Seq.collect (fun (_, _, v) -> v)