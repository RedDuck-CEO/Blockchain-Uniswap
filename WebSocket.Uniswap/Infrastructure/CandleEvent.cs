using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Core;

namespace WebSocket.Uniswap.Infrastructure
{
    public class CandleEvent
    {
        public event Action<IEnumerable<global::Program.Candle>> candles = _ => { };

        public SortedSet<(string, int)> EventsInvoked = new SortedSet<(string, int)>();

        public Task GetCandles(string uniswapId, int resolutionSeconds)
        {
            if(EventsInvoked.TryGetValue((uniswapId, resolutionSeconds), out _))
            {
                return Task.CompletedTask;
            }
            else
            {
                EventsInvoked.Add((uniswapId, resolutionSeconds));
            }
            var fsharpFunc = FuncConvert.ToFSharpFunc<IEnumerable<global::Program.Candle>>(t=>candles(t));
            var backgroundJob = Task.Run(() => global::Program.Logic.getCandles(uniswapId, fsharpFunc));
            return backgroundJob;
        }
    }
}