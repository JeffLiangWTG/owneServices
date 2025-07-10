using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.US.Business.BIRD.ACS;
using Enterprise.Customs.US.Business.BIRD.Common;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.Input
{
	public partial class CRLH5 : Messaging.Business.MessageBuildingBlocks.Input.Abstract.CRLH5, IBIRDLineRecord, IBIRDLineIDRecord
	{
		#region IBIRDLineRecord Members

		void IBIRDLineRecord.Update(JobComInvoiceLine invoiceLine, INotifications notifications)
		{
			IBIRDLineRecord lineRecord = this;

			lineRecord.SetTariffs(invoiceLine);

			if (!ManufacturerShipper.IsEmpty)
			{
				var manufacturerAddress = BIRDOrganisationMatching.GetOrganisationAddress
					(invoiceLine.Factory, OrgMatchedCustomsRegNoType.MID, ManufacturerShipper, "Manufacturer", notifications);

				if (manufacturerAddress == null)
				{
					manufacturerAddress = OrganisationCreator.CreateManufacturerAndSendNameAddressQueryMessage(invoiceLine.Factory, ManufacturerShipper);
					if (manufacturerAddress != null)
					{
						notifications.AddWarning("Manufacturer:" + ZString.Format(OrganisationCreator.ManufacturerCreated, ManufacturerShipper));
					}
					else if (!ManufacturerShipper.IsLettersAndNumbersOnlyOrEmpty)
					{
						notifications.AddWarning("Manufacturer:" + ZString.Format(OrganisationCreator.NoManufacturerCreatedAsMIDInvalid, ManufacturerShipper));
					}
				}
				invoiceLine.JI_OA_ManufacturerAddress = manufacturerAddress?.PK ?? ZGuid.Empty;
			}

			invoiceLine.JI_OA_ConsigneeAddress = BIRDOrganisationMatching.GetCustomsRecordOrMainAddressOfOrganisation(invoiceLine.Factory, OrgMatchedCustomsRegNoType.EIN, LineItemUltimateConsignee, "Line Level Ultimate Consignee", notifications);

			invoiceLine.US_UC_NKCountryOfOrigin = CountryOfOrigin;

			lineRecord.UpdateValue(invoiceLine, LineItemValue);
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
			get { return RecordControlNumber; }
		}

		#endregion
	}
}
