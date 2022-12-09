using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;

namespace ClientApp.RabbitMq
{
    public class RabbitMqService : IRabbitMqService
    {
        private string _response;

        private const string QueueName = "rpc_email_queue";

        private IConnection _connection;

        private IModel _channel;

        private string _replyQueueName;

        private EventingBasicConsumer _consumer;

        private ConcurrentDictionary<string, TaskCompletionSource<string>>
            _callbackMapper = new ConcurrentDictionary<string, TaskCompletionSource<string>>();


        private void OnReceived(object model, BasicDeliverEventArgs ea)
        {
            var suchTaskExists = _callbackMapper.TryRemove(ea.BasicProperties.CorrelationId,
                                                                    out var tcs);

            if (!suchTaskExists) return;

            var body = ea.Body;
            var response = Encoding.UTF8.GetString(body.ToArray());

            _response = response;
            tcs.TrySetResult(response);
        }


        public void RegisterRpcClient()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _replyQueueName = _channel.QueueDeclare().QueueName;

            _consumer = new EventingBasicConsumer(_channel);
            _consumer.Received += OnReceived;
        }
        public Task<string> CallAsync(string message,
                        CancellationToken cancellationToken = default)
        {

            var correlationId = Guid.NewGuid().ToString();
            var tcs = new TaskCompletionSource<string>();
            _callbackMapper.TryAdd(correlationId, tcs);

            var props = _channel.CreateBasicProperties();
            props.CorrelationId = correlationId;
            props.ReplyTo = _replyQueueName;

            var messageBytes = Encoding.UTF8.GetBytes(message);

            _channel.BasicPublish(exchange: "",
                                  routingKey: QueueName,
                                  basicProperties: props,
                                  body: messageBytes);

            _channel.BasicConsume(consumer: _consumer, 
                                  queue: _replyQueueName, 
                                  autoAck: true);

            cancellationToken.Register(() =>
                _callbackMapper.TryRemove(correlationId, out _));
            return tcs.Task;
        }
        
        public void Close() => _connection.Close();
        public string GetResponse()
        {
            return _response; 
        }
        
    }
}