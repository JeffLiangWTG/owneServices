using System.Globalization;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.US.Business.BIRD.ACS;
using Enterprise.Customs.US.Business.BIRD.Common;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.Input
{
	public partial class ENS60 : Messaging.Business.MessageBuildingBlocks.Input.Abstract.ENS60, IBIRDLineRecord
	{
		#region IBIRDLineRecord Members

		void IBIRDLineRecord.Update(JobComInvoiceLine invoiceLine, INotifications notifications)
		{
			invoiceLine.US_CVDCaseNo = CountervailingCaseNumber;
			invoiceLine.US_ADDCaseNo = AntidumpingCaseNumber;

			if (!ManufacturerSupplierCode.IsEmpty)
			{
				var manufacturerAddress = BIRDOrganisationMatching.GetOrganisationAddress(invoiceLine.Factory, OrgMatchedCustomsRegNoType.MID, ManufacturerSupplierCode, "Manufacturer/Supplier", notifications);

				if (manufacturerAddress == null)
				{
					manufacturerAddress = OrganisationCreator.CreateManufacturerAndSendNameAddressQueryMessage(invoiceLine.Factory, ManufacturerSupplierCode);
					if (manufacturerAddress != null)
					{
						notifications.AddWarning("Manufacturer/Supplier: " + string.Format(CultureInfo.InvariantCulture, OrganisationCreator.ManufacturerCreated, ManufacturerSupplierCode));
					}
					else if (!ManufacturerSupplierCode.IsLettersAndNumbersOnlyOrEmpty)
					{
						notifications.AddWarning("Manufacturer/Supplier: " + string.Format(CultureInfo.InvariantCulture, OrganisationCreator.NoManufacturerCreatedAsMIDInvalid, ManufacturerSupplierCode));
					}
				}
				ZString countryOfOrigin = invoiceLine.US_UC_NKCountryOfOrigin;
				invoiceLine.JI_OA_ManufacturerAddress = manufacturerAddress?.PK ?? ZGuid.Empty;
				invoiceLine.US_UC_NKCountryOfOrigin = countryOfOrigin;
			}

			invoiceLine.US_IsBondedADD = BondedADDIndicator == "1";
			invoiceLine.US_IsBondedCVD = BondedCVDIndicator == "1";

			invoiceLine.CusEntryLine.Fees.UpdateOrAddCharge(Core.Constants.USCustoms.FeeCodes.AntidumpingDuty, AntidumpingDuty);

			invoiceLine.CusEntryLine.Fees.UpdateOrAddCharge(Core.Constants.USCustoms.FeeCodes.CountervailingDuty, CountervailingDuty);

			if (invoiceLine.ImportTariff != null)
			{
				foreach (ZString feeCode in invoiceLine.ImportTariff.GetRelatedFeeCodes())
				{
					if (CusFeeCodeConstants.IsExciseTax(feeCode))
					{
						invoiceLine.CusEntryLine.Fees.UpdateOrAddCharge(feeCode, InternalRevenueServiceIRSTax);
						break;
					}
				}
			}
		}

		ZString IBIRDTariffRecord.Tariff
		{
			get { return ZString.Empty; }
		}

		#endregion
	}
}
