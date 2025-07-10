using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Types;
using Enterprise.Customs.SG.MHUB.MHX;
using Enterprise.Customs.SG.V4.MHUB;

namespace Enterprise.Customs.SG.MHUB
{
	// This class looks in the output file for a log of command and response lines. 

	public class MhxScriptSuccessChecker
	{
		public MhxScriptSuccessChecker(Mhx4ProfileCreator creator, MHUBConstants.CommandType command)
		{
			this.command = command;
			this.creator = creator;
		}

		public MHUBConstants.ResponseStatusCodes ResponseSuccessFlag
		{
			get
			{
				ResponseToCommandPairs = new List<ServerResponse>();
				AllOutput = ReadOutputFile();
				var lines = Regex.Split(AllOutput, "\r\n");
				var indexOfCommand = FindRelevantCommand(lines);
				var allResponses = ZString.Empty;
				for (int j = indexOfCommand; j < lines.Length; j++)
				{
					var nextLine = lines[j + 1];
					if (nextLine.StartsWith("RES:", System.StringComparison.OrdinalIgnoreCase))
					{
						allResponses += nextLine.Replace("RES:", "\r\n");
					}
					else
					{
						break;
					}
				}
				if (allResponses.IsEmpty)
				{
					// Likely a RES line was written without a CMD line. Try looking for any RES lines
					foreach (var line in lines)
					{
						if (line.StartsWith("RES:", System.StringComparison.OrdinalIgnoreCase))
						{
							allResponses += line.Replace("RES:", "\r\n");
						}
					}
				}

				ResponseToCommandPairs = ServerResponses.ExtractServerResponsePairsWithoutHtml(allResponses);
				return ObtainSuccessOrFailureFromResponses(ResponseToCommandPairs, command);
			}
		}

		internal static MHUBConstants.ResponseStatusCodes ObtainSuccessOrFailureFromResponses(IEnumerable<ServerResponse> responseToCommandPairs, MHUBConstants.CommandType command)
		{
			var successes = new List<ServerResponse>();
			var fails = new List<ServerResponse>();
			var empties = new List<ServerResponse>();
			foreach (var resLine in responseToCommandPairs)
			{
				if (resLine != null && resLine.ContainsKey(MHUBConstants.Parameters.requestStatus))
				{
					var lastStatusCode = resLine.GetValue(MHUBConstants.Parameters.requestStatus);
					switch (lastStatusCode)
					{
						case "0":
							successes.Add(resLine);
							break;
						case "1":
							empties.Add(resLine);
							break;
						default:
							fails.Add(resLine);
							break;
					}
				}
			}
			if (command == MHUBConstants.CommandType.Retrieve)
			{
				if (empties.Count == 1 && fails.Count == 0)
				{
					return MHUBConstants.ResponseStatusCodes.MailboxEmpty;
				}
			}

			if (successes.Count == responseToCommandPairs.Count() && fails.Count == 0)
			{
				return MHUBConstants.ResponseStatusCodes.CompleteSuccess;
			}
			else if (successes.Count < responseToCommandPairs.Count() && successes.Count > 0 && fails.Count > 0)
			{
				return MHUBConstants.ResponseStatusCodes.PartialSuccess;
			}
			else if (successes.Count == 0 && fails.Count > 0)
			{
				return MHUBConstants.ResponseStatusCodes.Failed;
			}
			return MHUBConstants.ResponseStatusCodes.Unknown;
		}

		int FindRelevantCommand(string[] lines)
		{
			// The log will have command and response lines for multiple actions. e.g. login, submit, logout. We only want to look at the relevant pair. 
			for (int i = 0; i < lines.Length; i++)
			{
				if (lines[i].StartsWith("CMD:Command=" + command, System.StringComparison.OrdinalIgnoreCase))
				{
					return i;
				}
			}
			return 0; //Output file for response to Retrieve has crap (nulls) on the first line; this actually works fine
		}

		public IEnumerable<ServerResponse> ResponseToCommandPairs
		{
			get; private set;
		}

		public string AllOutput
		{
			get; private set;
		}

		string ReadOutputFile()
		{
			return File.ReadAllText(creator.OutputFile.FullName);
		}

		readonly MHUBConstants.CommandType command;
		readonly Mhx4ProfileCreator creator;
	}
}
