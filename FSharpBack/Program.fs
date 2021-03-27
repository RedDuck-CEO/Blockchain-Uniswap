// Learn more about F# at http://docs.microsoft.com/dotnet/fsharp

open System
open FSharp.Data.GraphQL
open FSharp.Data.GraphQL.Ast
open Newtonsoft.Json
open Newtonsoft.Json.Linq

module Requests = 
    let deserializeTop100 (token: JToken Option) =
        let mapper (token : JProperty) =
            let strConcat x y = x + "/" + y
            token.Value.["pairs"] |> Seq.map (fun x -> ((x.["id"].ToString()), strConcat (x.["token0"].["symbol"].ToString()) (x.["token1"].["symbol"].ToString())))
        match token with
        |Some token -> token.Children<JProperty>() |> Seq.last |> mapper |> List.ofSeq |> Some
        |None -> None
    
    let top100 =
        use connection = new GraphQLClientConnection()
        let request : GraphQLRequest =
            { Query = """query q {
               pairs(first: 100, orderBy: reserveUSD, orderDirection: desc) {
                 id
                 token0{
                   symbol
                 }
                 token1{
                   symbol
                 }
               }
              }"""
              Variables = [||]
              ServerUrl = "https://api.thegraph.com/subgraphs/name/uniswap/uniswap-v2"
              HttpHeaders = [| |]
              OperationName = Some "q" }
        GraphQLClient.sendRequest connection request
    
    let swaps = 
        use connection = new GraphQLClientConnection()
        let request : GraphQLRequest =
            { Query = """query q {
                  swaps(orderBy: timestamp, orderDirection: desc, where:
                   { pair: "0xa478c2975ab1ea89e8196811f51a7b7ade33eb11" }
                  ) {
                       amount0In
                       amount0Out
                       amount1In
                       amount1Out
                       timestamp
                   }
                  }"""
              Variables = [||]
              ServerUrl = "https://api.thegraph.com/subgraphs/name/uniswap/uniswap-v2"
              HttpHeaders = [| |]
              OperationName = Some "q" }
        let response = GraphQLClient.sendRequest connection request
        response
  
    type Swaps = { amount0In: float; amount0Out: float; amount1In:float; amount1Out: float; timestamp: int64 } 
      
    let deserializeSwaps (token: JToken Option) =
        let mapper (token : JProperty) =
            let strConcat x y = x + "/" + y
            token.Value.["swaps"] |> Seq.map (fun x -> { amount0In=(float x.["amount0In"]); amount0Out=(float x.["amount0Out"]); amount1In=(float x.["amount1In"]); amount1Out=(float x.["amount1Out"]); timestamp=(int64 x.["timestamp"]);})
        match token with
        |Some token -> token.Children<JProperty>() |> Seq.last |> mapper |> List.ofSeq |> Some
        |None -> None
        
    let deserialize (data : string) =
        if String.IsNullOrWhiteSpace(data)
        then None
        else data |> JToken.Parse |> Some
    
    let allPr x = printfn "%A" x
    
    let takeTop100 = top100 |> deserialize |> deserializeTop100
    let takeSwaps = swaps |> deserialize |> deserializeSwaps
    
    takeSwaps |> allPr