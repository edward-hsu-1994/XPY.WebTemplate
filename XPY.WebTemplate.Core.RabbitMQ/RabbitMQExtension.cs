using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using XPY.WebTemplate.Core.RabbitMQ;

namespace Microsoft.Extensions.DependencyInjection {
    /// <summary>
    /// RabbitMQ擴充
    /// </summary>
    public static class RabbitMQExtension {
        public static void AddRabbitMQ(
            this IServiceCollection services,
            ConnectionFactory connectionFactory
            ) {
            services.AddSingleton(sp => {
                return connectionFactory.CreateConnection();
            });
            services.AddSingleton(sp => {
                return sp.GetService<IConnection>().CreateModel();
            });
        }


        public static void AddRabbitQueue<TQueueConsumer>(
            this IServiceCollection services)
            where TQueueConsumer : QueueConsumerBase {
            services.AddSingleton(sp => {
                var options = sp.GetService<IOptions<QueueConsumerOptions<TQueueConsumer>>>();
                var model = sp.GetService<IModel>();
                var queue = model.QueueDeclare(options.Value.Name, options.Value.Durable, options.Value.Exclusive, options.Value.AutoDelete, options.Value.Arguments);

                var consumer = new EventingBasicConsumer(model);

                var constructor = typeof(TQueueConsumer).GetConstructors().Single();

                List<object> pvalue = new List<object>();
                foreach (var ptype in constructor.GetParameters().Select(x => x.ParameterType)) {
                    if (ptype == typeof(EventingBasicConsumer)) {
                        pvalue.Add(consumer);
                    } else {
                        pvalue.Add(sp.GetService(ptype));
                    }
                }

                var result = (TQueueConsumer)Activator.CreateInstance(typeof(TQueueConsumer), pvalue.ToArray());
                model.BasicConsume(options.Value.Name, options.Value.AutoAck, consumer);

                return result;
            });
        }
    }
}
