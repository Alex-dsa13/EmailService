namespace ClientApp.RabbitMq
{
    public interface IRabbitMqService
    {
        string GetResponse();
        void RegisterRpcClient();
        void Close();

        Task<string> CallAsync(string message,
                        CancellationToken cancellationToken = default);

    }
}
