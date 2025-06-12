using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using OcmPoc.Infrastructure.MessageInterfaces.Documents;
using OcmPoc.Mapping.Interface;

namespace OcmPoc.Mapping.Provider
{
	public class ProviderAMapping : IMapping
	{
		static readonly Regex filePattern = new Regex(@"^To_(?<to>[^_]+)_(?<tracking>[^_]+)_?(?<responseTo>[^_\.]*).txt$", RegexOptions.Compiled);

		public void Initialise()
		{
		}

		public Task<MapForSendResult> MapAsync(MapForSendCommand command)
		{
			var message = command.Message;

			var responseTo = String.IsNullOrWhiteSpace(message.ResponseTo) ? "" : $"_{message.ResponseTo}";
			var fileName = $"From_{message.Sender}_{message.TrackingId}{responseTo}.txt";
			var content = String.Join("\n", message.Content);

			var mapResult = new MapForSendResult(fileName, Encoding.UTF8.GetBytes(content));

			return Task.FromResult(mapResult);
		}

		public Task<MapReceivedResult> MapAsync(MapReceivedCommand command)
		{
			var match = filePattern.Match(command.MessageName);

			if (!match.Success)
			{
				throw new MappingFailedException($"Message name '{command.MessageName}' is not in the expected format.");
			}

			var commonMessage = new CommonMessage
			{
				Sender = "Provider-A",
				Recipient = match.Groups["to"].Value,
				ResponseTo = match.Groups["responseTo"].Value,
				TrackingId = match.Groups["tracking"].Value
			};

			commonMessage.Content.AddRange(
				Encoding.UTF8.GetString(command.Content)
					.Split(new[] { '\n' }, StringSplitOptions.None)
					.Select(l => l.Trim()));

			return Task.FromResult(new MapReceivedResult(commonMessage));
		}
	}
}
