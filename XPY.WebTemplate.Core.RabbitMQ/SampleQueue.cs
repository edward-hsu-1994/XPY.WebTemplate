using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace XPY.WebTemplate.Core.RabbitMQ {
    public class SampleQueue : QueueConsumerBase {
        public SampleQueue(EventingBasicConsumer consumer) : base(consumer) {

        }
    }
}
