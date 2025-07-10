using System.Collections.Generic;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Customs.SG.Access.Business
{
	internal class AsycudaEventMessagePinInterpretationGenerator : AsycudaEventMessageSuccessInterpretationGenerator
	{
		public AsycudaEventMessagePinInterpretationGenerator(Event universalEvent)
			: base(universalEvent)
		{
		}

		protected override string ActionPurposeCodeDesc => "AIR" + universalEvent.DataContext.ActionPurposeCode + " SG ACCESS";

		protected override List<KeyValuePair<string, string>> GetContentFields()
		{
			return new List<KeyValuePair<string, string>>
			{
				new KeyValuePair<string, string>("Manifest Country", "ManifestCountry"),
				new KeyValuePair<string, string>("UEN Number", "UENNumber"),
				new KeyValuePair<string, string>("Original Create Date", "OriginalCreateDate" ),
				new KeyValuePair<string, string>("Original Serial Number", "OriginalSerialNumber"),
				new KeyValuePair<string, string>("Cycle Date", "CycleDate"),
				new KeyValuePair<string, string>("Cycle Number", "CycleNumber"),
				new KeyValuePair<string, string>("Flight Number", "FlightNumber"),
				new KeyValuePair<string, string>("Arrival Date", "ArrivalDate")
			};
		}
	}
}
