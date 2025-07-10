using System.Collections.Generic;
using System.Collections.Immutable;

namespace Enterprise.Freight.OnlineSailingSchedules
{
	public static class ErrorMessageCatalog
	{
		#region SuppressResourceStringsCheckRegion

		const string EmptyFilter = "Filter is empty. Must contain Load Port, Discharge Port and one of the ETA or ETD. Alternatively Voyage Number and either Vessel Id or IMO Number.";
		const string InvalidFilter = "Filter doesn't meet the minimum parameter requirements. Must contain Load Port, Discharge Port and one of the ETA or ETD. Alternatively either Vessel Name or IMO Number.";
		const string InvalidLegsCount = "Number of Legs is Invalid. Must be a number greater than zero.";
		const string InvalidTransitTime = "Transit Time is Invalid. Must be a number greater than or equal to zero.";
		const string MaxRecordsReached = "More than 500 records returned.";
		const string InvalidPortOfLoadingUnloco = "Port of Loading UNLOCO is invalid. It must be 5 characters long.";
		const string PortOfLoadingNotRecognized = "Port of Loading UNLOCO could not be recognized.";
		const string InvalidPortOfDischargeUnloco = "Port of Discharge UNLOCO is invalid. It must be 5 characters long.";
		const string PortOfDischargeNotRecognized = "Port of Discharge UNLOCO could not be recognized.";
		const string InvalidImoNumber = "Vessel IMO Number is Invalid.";
		const string InvalidEtdFrom = "ETD From is invalid. Must be in the format YYYY-MM-DD.";
		const string InvalidEtdTo = "ETD To is invalid. Must be in the format YYYY-MM-DD.";
		const string InvalidEtdRange = "ETD To must be greater than or equal to ETD From.";
		const string InvalidEtaFrom = "ETA From is invalid. Must be in the format YYYY-MM-DD.";
		const string InvalidEtaTo = "ETA To is invalid. Must be in the format YYYY-MM-DD.";
		const string InvalidEtaRange = "ETA To must be greater than or equal to ETA From.";
		const string InvalidRelatedPorts = "Related ports option is invalid. Must be a True or a False.";
		const string InvalidSameCarrierRoutes = "Same carrier routes option is invalid. Must be a True or a False.";
		const string InvalidScac = "Carrier SCAC is invalid. Must be 4 characters long.";
		const string ScacNotSupported = "Carrier SCAC is not supported.";

		#endregion

		public static ImmutableDictionary<string, string> ErrorMessageCollection { get; } =
			new Dictionary<string, string>
			{
				[EmptyFilter] = ResString.GetMultilingualString("7D687A70-784B-49C3-A7B8-6CA75955182B", "Filter is empty. Must contain Load Port, Discharge Port and one of the Arrival Date or ETD. Alternatively Voyage Number and either Vessel Id or IMO Number."),
				[InvalidFilter] = ResString.GetMultilingualString("292FFE1B-E465-421F-ACD1-B5D85EEEB6AE", "Filter doesn't meet the minimum parameter requirements. Must contain Load Port, Discharge Port and one of the Arrival Date or ETD. Alternatively either Vessel Name or IMO Number."),
				[InvalidLegsCount] = ResString.GetMultilingualString("3D3C6ACD-FBA2-47FB-8934-23E922A4AE41", "Number of Legs is Invalid. Must be a number greater than zero."),
				[InvalidTransitTime] = ResString.GetMultilingualString("F80E6F95-E7DA-4A65-8AE1-AEDC855EA57C", "Transit Time is Invalid. Must be a number greater than or equal to zero."),
				[MaxRecordsReached] = ResString.GetMultilingualString("491C3C6D-A8B6-4B69-B100-F6A851A80F98", "Too many records were returned. Please modify the filters to narrow down your search."),
				[InvalidPortOfLoadingUnloco] = ResString.GetMultilingualString("1066F6F9-1179-40BB-ADE7-0BC885A8C07C", "Origin UNLOCO is invalid. It must be 5 characters long."),
				[PortOfLoadingNotRecognized] = ResString.GetMultilingualString("65037115-4E07-4159-880A-133B3C7FDC23", "Origin UNLOCO could not be recognized."),
				[InvalidPortOfDischargeUnloco] = ResString.GetMultilingualString("5AE70900-30FD-49C5-B2DE-A805A8566702", "Destination UNLOCO is invalid. It must be 5 characters long."),
				[PortOfDischargeNotRecognized] = ResString.GetMultilingualString("78BF7EF7-E956-4487-864B-846F8F62A97F", "Destination UNLOCO could not be recognized."),
				[InvalidImoNumber] = ResString.GetMultilingualString("35CDB00F-DCBE-4D0F-AD22-866F024F46A6", "Vessel IMO Number is Invalid."),
				[InvalidEtdFrom] = ResString.GetMultilingualString("E0C42EE5-4F7C-44E5-8140-C23B9FDD4D09", "Departure Date From is invalid. Must be in the format YYYY-MM-DD."),
				[InvalidEtdTo] = ResString.GetMultilingualString("428A28AD-DFB2-415D-A656-1EC0B2D919E1", "Departure Date To is invalid. Must be in the format YYYY-MM-DD."),
				[InvalidEtdRange] = ResString.GetMultilingualString("A9ED7206-6F12-42CE-ABB7-7B00F64882A5", "Departure Date To must be greater than or equal to Departure Date From."),
				[InvalidEtaFrom] = ResString.GetMultilingualString("DB21D393-D37C-4FBA-98F7-1481E7D2EDF2", "Arrival Date From is invalid. Must be in the format YYYY-MM-DD."),
				[InvalidEtaTo] = ResString.GetMultilingualString("57AE7A9D-7F89-45C9-A507-D39340FD10C9", "Arrival Date To is invalid. Must be in the format YYYY-MM-DD."),
				[InvalidEtaRange] = ResString.GetMultilingualString("04D9A9F1-2265-4690-AEBF-6562AB585FBE", "Arrival Date To must be greater than or equal to Arrival Date From."),
				[InvalidRelatedPorts] = ResString.GetMultilingualString("AACAB77C-AEC8-447E-BAFA-A9AA959E8214", "Related ports option is invalid. Must be a True or a False."),
				[InvalidSameCarrierRoutes] = ResString.GetMultilingualString("D4FEF26D-1ADD-451A-BDE8-312C9322F39C", "Same carrier routes option is invalid. Must be a True or a False."),
				[InvalidScac] = ResString.GetMultilingualString("429EC80E-A69F-4BD6-A478-EE9D82DDA09B", "Carrier SCAC is invalid. Must be 4 characters long."),
				[ScacNotSupported] = ResString.GetMultilingualString("BB5DEC9B-AF0E-4124-8D9F-4056B622DFC8", "Carrier SCAC is not supported.")
			}.ToImmutableDictionary();

		public static string GetErrorMessage(string errorMessage)
		{
			if (ErrorMessageCollection.TryGetValue(errorMessage, out var errorMessageToDisplay))
			{
				return errorMessageToDisplay;
			}

			return ResString.GetMultilingualString("68306598-1E68-4ED3-ADB0-E231B1D836A1",
				"Unknown Error: {0}", errorMessage);
		}
	}
}
