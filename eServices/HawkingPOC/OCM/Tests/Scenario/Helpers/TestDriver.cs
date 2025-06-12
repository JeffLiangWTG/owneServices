using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OcmPoc.Tests.Scenario.Helpers
{
	class TestDriver
    {
        readonly IEnumerable<IGenerator> generators;
        readonly IEnumerable<IReceiver> receivers;

        const string messageLine = "0123456789abcdef";

        public TestDriver(int messageSize, int quantity, string[] flows)
        {
            var flowSpecs = flows.Select(f => new FlowSpec(f)).ToList();
            var lineCount = messageSize / messageLine.Length;

            generators = flowSpecs.Select(flow => GeneratorFactory.Create(flow.Sender, flow.Recipient, quantity)
                                                                  .PrepareMessage(messageLine, lineCount))
								  .ToList();

            receivers =
                flowSpecs.GroupBy(fs => fs.Recipient)
                         .Select(g => ReceiverFactory.Create(g.Key, g.Count()).Initialise())
                         .ToList();

			MessageCount = flows.Length * quantity;
			TotalData = 1L * MessageCount * messageSize;
        }

		public long TotalData { get; }
		public int MessageCount { get; }

		public async Task RunTestAsync(TimeSpan timeout)
        {
			var receiveTasks = receivers.Select(r => r.WaitForMessages(timeout));

			await Task.WhenAll(generators.Select(g => g.GenerateMessages()));
			await Task.WhenAll(receiveTasks);
        }
    }
}
