using System.Collections.Generic;
using System.Text.RegularExpressions;
using Res = Enterprise.Warehouse.Transit.DataTransfer.Universal.Res;

namespace Enterprise.Warehouse.Transit.DataTransfer
{
	public class WhsTransitLogInstructionMapper
	{
		public WhsTransitLogInstructionMapper()
		{
			MessageFormats.Add(Messages.DGClassLimit(".*"));
			MessageFormats.Add(Messages.DGSubstanceLimit(".*"));
			MessageFormats.Add(Messages.DGCountryReferenceLimit(".*"));
		}

		readonly HashSet<string> MessageFormats = new HashSet<string>();

#if DEBUG

		public void AddMessageFormatsForTesting(string msg)
		{
			MessageFormats.Add(msg);
		}

#endif

		public static class Messages
		{
			public static string DGClassLimit(params string[] parameters) => Res.GetString("3f4d8a83-cd70-4666-8b4c-a0eaf4655f4f", "The packages on this Receive/Dispatch Instruction could not be created as the UNDG Class threshold on the Transit Warehouse is set to zero, and the warehouse is not currently permitted to handle UNDG Class {0} goods.", parameters);
			public static string DGSubstanceLimit(params string[] parameters) => Res.GetString("c84bfd0f-424a-4f2f-939c-613f1873beba", "The packages on this Receive/Dispatch Instruction could not be created as the UNDG Substance threshold on the Transit Warehouse is set to zero, and the warehouse is not currently permitted to handle UNDG Substance {0} goods.", parameters);
			public static string DGCountryReferenceLimit(params string[] parameters) => Res.GetString("b76f4770-7d54-4855-8633-a4f27ec8883b", "The packages on this Receive/Dispatch Instruction could not be created as the UNDG Country Reference threshold on the Transit Warehouse is set to zero, and the warehouse is not currently permitted to handle UNDG Country Reference {0} goods.", parameters);
		}

		public static class Codes
		{
			public const string DGClassLimit = "DGCLS";
			public const string DGSubstanceLimit = "DGSUB";
			public const string DGCountryReferenceLimit = "DGDCR";
		}

		public static string GetMessageByCode(string code, params string[] parameters)
		{
			var message = string.Empty;
			switch (code)
			{
				case Codes.DGClassLimit:
					message = Messages.DGClassLimit(parameters);
					break;
				case Codes.DGSubstanceLimit:
					message = Messages.DGSubstanceLimit(parameters);
					break;
				case Codes.DGCountryReferenceLimit:
					message = Messages.DGCountryReferenceLimit(parameters);
					break;
			}

			return message;
		}

		public string GetMessageFromLog(string log)
		{
			var message = string.Empty;
			foreach (var pattern in MessageFormats)
			{
				var regex = new Regex(pattern, RegexOptions.Multiline);
				if (regex.IsMatch(log))
				{
					message = regex.Match(log).Value;
					break;
				}
			}

			return message;
		}
	}
}
