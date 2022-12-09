using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace MailSendApp
{
    class Program
    {
        static private IModel _channel;


        public static void Main()
        {
            var factory = new ConnectionFactory()
            {
                HostName = "localhost",
                DispatchConsumersAsync = true
            };

            using var connection = factory.CreateConnection();
            _channel = connection.CreateModel();

            _channel.QueueDeclare(queue: "rpc_email_queue",
                                  durable: false,
                                  exclusive: false,
                                  autoDelete: false,
                                  arguments: null);


            _channel.BasicQos(prefetchSize: 0,
                              prefetchCount: 1,
                              global: false);

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.Received += OnReceived;

            _channel.BasicConsume(queue: "rpc_email_queue",
                                     autoAck: false,
                                     consumer: consumer);

            Console.ReadLine();
        }


        private static async Task OnReceived(object model, BasicDeliverEventArgs ea)
        {
            string response = null;

            var body = ea.Body;
            var props = ea.BasicProperties;
            var replyProps = _channel.CreateBasicProperties();
            replyProps.CorrelationId = props.CorrelationId;


            var message = Encoding.UTF8.GetString(body.ToArray());

            string[] data = message.Split('/');
            var email = data[0];
            var emailMessage = data[1];


            var sendEmail = new EmailService();
            var t = Task.Run(() => sendEmail.SendEmailAsync(email, emailMessage));
            await t;


            response = sendEmail.ServerResponse;


            var responseBytes = Encoding.UTF8.GetBytes(response);

            _channel.BasicPublish(exchange: "",
                                  routingKey: props.ReplyTo,
                                  basicProperties: replyProps,
                                  body: responseBytes);

            _channel.BasicAck(deliveryTag: ea.DeliveryTag,
                              multiple: false);
        }

    }
}


