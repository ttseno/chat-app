namespace StocksBot.Configuration
{
    public class RabbitMQConfig
    {
        public string ConnectionString { get; init; }
        public string Queue { get; init; }
        public int ExecutionDelay { get; init; }
        public string ResponseQueue { get; init; }
        
    }
}