namespace ChatWSServer.Configuration
{
    public class RabbitMQConfig
    {
        public string ConnectionString { get; init; }
        public string BotsResponseQueue { get; init; }
        public int ExecutionDelay { get; init; }
    }
}