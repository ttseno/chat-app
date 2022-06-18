using System;
using System.Net.Http;
using System.Threading.Tasks;
using StocksBot.Configuration;

namespace StocksBot
{
    public interface IStooqClient
    {
        Task<StooqModel> GetStockQuotation(string stockCode);
    }
    
    public class StooqClient : IStooqClient
    {
        private readonly HttpClient _httpClient;
        private readonly AppConfiguration _configuration;
        
        public StooqClient(HttpClient httpClient, AppConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<StooqModel> GetStockQuotation(string stockCode)
        {
            var uri = $"{_configuration.StooqApiBaseUrl}/q/l/?s={stockCode.ToLower()}&f=sd2t2ohlcv&h&e=csv";
            var response = await _httpClient.GetStringAsync(uri);
            var lines = response.Split(Environment.NewLine);
            return new StooqModel(lines[1]);
        }
        
    }
    
    public record StooqModel {
        public string Symbol { get; init; }
        public string Date { get; init; }
        public string Time { get; init; }
        public double Open { get; init; }
        public double High { get; init; }
        public double Low { get; init; }
        public double Close { get; init; }
        public double Volume { get; init; }

        public StooqModel()
        {
        }

        public StooqModel(string line)
        {
            var values = line.Split(",");
            Symbol = values[0];
            Date = values[1];
            Time = values[2];
            Open = double.Parse(values[3]);
            High = double.Parse(values[4]);
            Low = double.Parse(values[5]);
            Close = double.Parse(values[6]); 
            Volume = double.Parse(values[7]);
        }

        public override string ToString()
        {
            return $"{Symbol} quote is ${Close.ToString()} per share";
        }
    }
}