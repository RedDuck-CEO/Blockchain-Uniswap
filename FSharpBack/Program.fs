// Learn more about F# at http://docs.microsoft.com/dotnet/fsharp

open System
open FSharp.Data.GraphQL
open FSharp.Data.GraphQL.Ast
open Newtonsoft.Json
open Newtonsoft.Json.Linq

module Requests = 
    
    let top100Query =
        """query q {
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
    
    let swapsQuery id = 
        $"""query q {{
               swaps(orderBy: timestamp, orderDirection: desc, where:
                {{ pair: "{id}" }}
               ) {{
                    amount0In
                    amount0Out
                    amount1In
                    amount1Out
                    timestamp
                }}
               }}"""
  
    let pairInfoQuery id =
           $"""query q {{
               pair(id: "{id}"){{
                   reserve0
                   reserve1
                   token0Price
                   token1Price
               }}
              }}"""
    
    let requestMaker query =
        use connection = new GraphQLClientConnection()
        let request : GraphQLRequest =
            { Query = query
              Variables = [||]
              ServerUrl = "https://api.thegraph.com/subgraphs/name/uniswap/uniswap-v2"
              HttpHeaders = [| |]
              OperationName = Some "q" }
        GraphQLClient.sendRequest connection request
    
    type Swaps = { amount0In: float; amount0Out: float; amount1In:float; amount1Out: float; timestamp: int64 } 
    type PairInfo = { reserve0: float; reserve1: float; price0: float; price1: float }
    
    let mapTop100 (token: JToken Option) =
        let mapper (token : JProperty) =
            let strConcat x y = x + "/" + y
            token.Value.["pairs"] |> Seq.map (fun x -> ((x.["id"].ToString()), strConcat (x.["token0"].["symbol"].ToString()) (x.["token1"].["symbol"].ToString())))
        match token with
        |Some token -> token.Children<JProperty>() |> Seq.last |> mapper |> List.ofSeq |> Some
        |None -> None
      
    let mapSwaps (token: JToken Option) =
        let mapper (token : JProperty) =
            token.Value.["swaps"] |> Seq.map (fun x -> { amount0In=(float x.["amount0In"]); amount0Out=(float x.["amount0Out"]); amount1In=(float x.["amount1In"]); amount1Out=(float x.["amount1Out"]); timestamp=(int64 x.["timestamp"]);})
        match token with
        |Some token -> token.Children<JProperty>() |> Seq.last |> mapper |> List.ofSeq |> Some
        |None -> None
        
    let mapPairInfo (token: JToken Option) =
        let mapper (token : JProperty) =
            let info = token.Value.["pair"]
            { reserve0 = (float info.["reserve0"]); reserve1 = (float info.["reserve1"]); price0 = (float info.["token0Price"]); price1 = (float info.["token1Price"]) } 
        match token with
        |Some token -> token.Children<JProperty>() |> Seq.last |> mapper |> Some
        |None -> None
        
    let deserialize (data : string) =
        if String.IsNullOrWhiteSpace(data)
        then None
        else data |> JToken.Parse |> Some
    
    let allPr x = printfn "%A" x
    
    let takeTop100 = top100Query |> requestMaker |> deserialize |> mapTop100
    let takeSwaps idPair = idPair |> swapsQuery |> requestMaker |> deserialize |> mapSwaps
    let takeInfo idPair = idPair |> pairInfoQuery |> requestMaker |> deserialize |> mapPairInfo
    
    //"0xa478c2975ab1ea89e8196811f51a7b7ade33eb11" |> takeSwaps |> allPr
    takeTop100 |> allPr 