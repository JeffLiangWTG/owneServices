using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OcmPoc.Core.Services;

namespace OcmPoc.Transceivers
{
	class Receiver : IRunnable
	{
		readonly IReceivingConnection connection;
		readonly IMessageBuilder messageBuilder;
		readonly TimeSpan pollInterval;
		readonly IMessageFlowService messageFlowService;

		public Receiver(IReceivingConnection connection, IMessageBuilder messageBuilder, IMessageFlowService messageFlowService)
		{
			this.connection = connection;
			this.messageBuilder = messageBuilder;
			this.pollInterval = connection.PollInterval;
			this.messageFlowService = messageFlowService;
		}

		public async Task RunAsync(CancellationToken token)
		{
			while (!token.IsCancellationRequested)
			{
				await Task.WhenAll(
					ReceiveMessagesAsync(token),
					Task.Delay(pollInterval, token)
				);
			}
		}

		async Task ReceiveMessagesAsync(CancellationToken token)
		{
			Console.WriteLine("Polling for new messages");

			await connection.ConnectAsync();

			var waitingMessages = await connection.ListAsync();

			Console.WriteLine("Found waiting messages:");
			Console.WriteLine(String.Join(Environment.NewLine, waitingMessages));

			foreach (var messageName in waitingMessages.OrderBy(x => x))
			{
				if (token.IsCancellationRequested) { break; }
				await ReceiveMessageAsync(messageName);
			}

			await connection.DisconnectAsync();
		}

		async Task ReceiveMessageAsync(string name)
		{
			try
			{
				Console.WriteLine($"Receiving message {name}");
				var message = messageBuilder.Build(name, await GetMessageBodyAsync(name));

				var messageFlow = await messageFlowService.CreateNewFlowAsync(message);

				await connection.AcknowledgeAsync(name);

				Console.WriteLine($"Message {name} acknowledged");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error receiving message {name}: {ex}");
			}
		}

		async Task<byte[]> GetMessageBodyAsync(string name)
		{
			using (var messageStream = await connection.ReceiveAsync(name))
			using (var memory = new MemoryStream())
			{
				await messageStream.CopyToAsync(memory);
				return memory.ToArray();
			}
		}
	}
}
