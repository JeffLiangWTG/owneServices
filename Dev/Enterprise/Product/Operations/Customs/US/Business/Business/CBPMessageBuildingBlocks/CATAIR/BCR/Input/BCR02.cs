using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.US.Business.BIRD.ACS;
using Enterprise.Customs.US.Business.BIRD.Common;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.Input
{
	public partial class BCR02 : Messaging.Business.MessageBuildingBlocks.Input.Abstract.BCR02, IBIRDLineRecord, IBIRDLineIDRecord
	{
		#region IBIRDLineRecord Members

		void IBIRDLineRecord.Update(JobComInvoiceLine invoiceLine, INotifications notifications)
		{
			IBIRDLineRecord lineRecord = this;

			lineRecord.SetTariffs(invoiceLine);

			if (LineItemValue > 0m)
			{
				lineRecord.UpdateValue(invoiceLine, LineItemValue);
			}

			invoiceLine.JI_OA_ConsigneeAddress = BIRDOrganisationMatching.GetCustomsRecordOrMainAddressOfOrganisation(invoiceLine.Factory, OrgMatchedCustomsRegNoType.EIN, UltimateConsignee, "Ultimate Consignee", notifications);

			invoiceLine.JI_OA_ManufacturerAddress = BIRDOrganisationMatching.GetOrganisationAddress(invoiceLine.Factory, OrgMatchedCustomsRegNoType.MID, ManufacturerIDCode, "Manufacturer", notifications)?.PK ?? ZGuid.Empty;

			invoiceLine.US_UC_NKCountryOfOrigin = CountryOfOrigin;
		}

		ZString IBIRDTariffRecord.Tariff
		{
			get { return TariffNumber; }
		}

		#endregion

		#region IBIRDLineIDRecord Members

		ZInt IBIRDLineIDRecord.DelimiterInvSequence
		{
			get { return 1; }
		}

		ZInt IBIRDLineIDRecord.LineNumber
		{
			get { return 0; }
		}

		#endregion
	}
}
