using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Core;

namespace WebSocket.Uniswap.Infrastructure
{
    public static class CandleEvent
    {
        public static event Action<IEnumerable<global::Program.Candle>> candles = _ => { };

        public static Task GetCandles(string uniswapId, int resolutionSeconds)
        {
            var fsharpFunc = FuncConvert.ToFSharpFunc<IEnumerable<global::Program.Candle>>(t=>candles(t));
            var backgroundJob = Task.Run(() => global::Program.Logic.getCandles(uniswapId, fsharpFunc));
            return backgroundJob;
        }
    }
}