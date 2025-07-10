using System.Collections.Generic;
using CargoWise.Common;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Customs.SG.Access.Business
{
	internal class AsycudaEventMessageSuccessInterpretationGenerator : ASYCUDA.Business.AsycudaEventMessageInterpretationGenerator
	{
		public AsycudaEventMessageSuccessInterpretationGenerator(Event universalEvent)
			: base(universalEvent)
		{
		}

		protected override string ActionPurposeCodeDesc => "AIR" + universalEvent.DataContext.ActionPurposeCode + " SG ACCESS";

		protected override List<KeyValuePair<string, string>> GetContentFields()
		{
			return new List<KeyValuePair<string, string>>
			{
				new KeyValuePair<string, string>("Manifest Country", "ManifestCountry"),
				new KeyValuePair<string, string>("Response Number", "ResponseNumber"),
				new KeyValuePair<string, string>("Response Time", "ResponseTime"),
				new KeyValuePair<string, string>("UEN Number", "UENNumber"),
				new KeyValuePair<string, string>("Original Create Date", "OriginalCreateDate" ),
				new KeyValuePair<string, string>("Original Serial Number", "OriginalSerialNumber"),
				new KeyValuePair<string, string>("Batch Date", "BatchDate")
			};
		}

		protected override void WriteConsignmentReference(Context consignmentReference, HtmlTableCreator tableCreator)
		{
			var consignmentNumber = FindContextValue("ConsignmentNumber", consignmentReference.SubContextCollection);
			var messageStatusCode = FindContextValue("MessageStatusCode", consignmentReference.SubContextCollection);
			var consignmentStatus = FindContextValue("ConsignmentStatus", consignmentReference.SubContextCollection);
			var manifestPermitNumbers = FindContextValue("ManifestPermitNumber", consignmentReference.SubContextCollection);
			tableCreator.WriteRowWithFormatting(
				new CellWithFormatting("Consignment", ColspanBold(2)),
				new CellWithFormatting(consignmentNumber),
				new CellWithFormatting(messageStatusCode),
				new CellWithFormatting(consignmentStatus),
				new CellWithFormatting(manifestPermitNumbers, Colspan(2))
			);
		}

		protected override void WriteHouseBills(Context masterBill, HtmlTableCreator tableCreator)
		{
			FindContext("HouseBill", masterBill.SubContextCollection)
				.ForEach(houseBill =>
				{
					var houseBillValue = houseBill.Value;
					var messageStatusCode = FindContextValue("MessageStatusCode", houseBill.SubContextCollection);
					var consignmentStatus = FindContextValue("ConsignmentStatus", houseBill.SubContextCollection);
					var manifestPermitNumber = FindContextValue("ManifestPermitNumber", houseBill.SubContextCollection);

					tableCreator.WriteRowWithFormatting(
						new CellWithFormatting("House Bill", ColspanBold(2)),
						new CellWithFormatting(houseBillValue),
						new CellWithFormatting(messageStatusCode),
						new CellWithFormatting(consignmentStatus),
						new CellWithFormatting(manifestPermitNumber, Colspan(2))
					);

					FindContext("ConsignmentReference", houseBill.SubContextCollection)
						.ForEach(consignmentReference => WriteConsignmentReference(consignmentReference, tableCreator));
				});
		}
	}
}
