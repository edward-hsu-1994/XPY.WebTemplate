using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace XPY.WebTemplate.Core.RabbitMQ {
    public abstract class QueueConsumerBase {

        public EventingBasicConsumer Consumer { get; }
        public QueueConsumerBase(
            EventingBasicConsumer consumer) {
            Consumer = consumer;
        }
    }
}