using System;
using System.Collections.Generic;
using System.Text;

namespace XPY.WebTemplate.Core.RabbitMQ {
    public class QueueConsumerOptions<TConsumer>
        where TConsumer : QueueConsumerBase {
        public string Name { get; set; }
        public bool Durable { get; set; }
        public bool Exclusive { get; set; }
        public bool AutoDelete { get; set; }
        public bool AutoAck { get; set; }
        public IDictionary<string, object> Arguments { get; set; }
    }
}
