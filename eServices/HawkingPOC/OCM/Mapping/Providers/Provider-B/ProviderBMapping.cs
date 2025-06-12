using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using OcmPoc.Infrastructure.MessageInterfaces.Documents;
using OcmPoc.Mapping.Interface;

namespace OcmPoc.Mapping.Provider
{
	public class ProviderBMapping : IMapping
	{
		static readonly Regex fromPattern = new Regex("^From: (?<value>.*)$");
		static readonly Regex toPattern = new Regex("^To: (?<value>.*)$");
		static readonly Regex trackingPattern = new Regex("^Tracking Id: (?<value>.*)$");
		static readonly Regex responsePattern = new Regex("^Response To: (?<value>.*)$");
		static readonly Regex msgPattern = new Regex("^Msg: (?<value>.*)$");

		public void Initialise()
		{
		}

		public Task<MapForSendResult> MapAsync(MapForSendCommand command)
		{
			var message = command.Message;

			var fileName = "Message_<serialno>.msg";

			var contentBuilder = new StringBuilder();
			contentBuilder.AppendLine($"From: {message.Sender}");
			contentBuilder.AppendLine($"To: {message.Recipient}");
			contentBuilder.AppendLine($"Tracking Id: {message.TrackingId}");

			message.Content.ForEach(l => contentBuilder.AppendLine($"Msg: {l}"));
			
			return Task.FromResult(new MapForSendResult(fileName, Encoding.UTF8.GetBytes(contentBuilder.ToString())));
		}

		public Task<MapReceivedResult> MapAsync(MapReceivedCommand command)
		{
			var lines = Encoding.UTF8.GetString(command.Content)
				.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

			if (lines.Length < 4) { throw new MappingFailedException("Message is not expected format"); }

			var message = new CommonMessage
			{
				Sender = Match(fromPattern, lines[0]),
				Recipient = Match(toPattern, lines[1]),
				TrackingId = Match(trackingPattern, lines[2]),
				ResponseTo = Match(responsePattern, lines[3], false)
			};

			var headerLines = message.ResponseTo == null ? 3 : 4;

			message.Content.AddRange(
				lines.Skip(headerLines)
					 .Select(l => Match(msgPattern, l)));

			return Task.FromResult(new MapReceivedResult(message));
		}

		private string Match(Regex pattern, string line, bool fail = true)
		{
			var match = pattern.Match(line);

			if (match.Success) { return match.Groups["value"].Value; }

			if (fail) { throw new MappingFailedException($"Line not expected: {line}"); }

			return null;
		}
	}
}
	