using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.US.Business.BIRD.ACS;
using Enterprise.Customs.US.Business.MessageBuilders;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.Input
{
	public partial class OGAFD01 : Messaging.Business.MessageBuildingBlocks.Input.Abstract.OGAFD01, IBIRDOGALineRecord, IBIRDOGALineIDRecord
	{
		#region IBIRDOGALineRecord Members

		void IBIRDOGALineRecord.Update(IOGALine ogaLine, INotifications notifications)
		{
			FDA fdaLine = (FDA)ogaLine;

			fdaLine.US_FDALineNo = FDALineNumber;
			fdaLine.US_FDAProductCode = FDAProductCode;
			fdaLine.US_FDACargoStorageCode = CargoStorageStatus;
			fdaLine.US_UC_NKFDAProduction = FDACountryOfProduction;

			if (!AffirmationOfComplianceCode.IsEmpty)
			{
				switch (AffirmationOfComplianceCode)
				{
					case AffirmationCodeConstants.Codes.PND:
						fdaLine.US_PND = true;
						break;
					case AffirmationCodeConstants.Codes.PNC:
						fdaLine.US_PNC = AffirmationOfComplianceQualifier;
						break;
					default:
						fdaLine.AffirmationCodes.AddNew(AffirmationOfComplianceCode, AffirmationOfComplianceQualifier);
						notifications.AddWarning("The FD01 record affirmation code should only contain either PNC or PND and system received a code '" + AffirmationOfComplianceCode + "'. It is imported into the Affirmation Codes grid available on the FDA form.");
						break;
				}
			}

			if (!FDAActualManufacturerNumber.IsEmpty)
			{
				var fdaManufacturerAddress = BIRDOrganisationMatching.GetOrganisationAddress(fdaLine.Factory, OrgMatchedCustomsRegNoType.MID, FDAActualManufacturerNumber);

				if (fdaManufacturerAddress == null)
				{
					fdaManufacturerAddress = OrganisationCreator.CreateManufacturerAndSendNameAddressQueryMessage(fdaLine.Factory, FDAActualManufacturerNumber);
					if (fdaManufacturerAddress != null)
					{
						notifications.AddWarning("FDA Manufacturer:" + ZString.Format(OrganisationCreator.ManufacturerCreated, FDAActualManufacturerNumber));
					}
					else if (!FDAActualManufacturerNumber.IsLettersAndNumbersOnlyOrEmpty)
					{
						notifications.AddWarning("FDA Manufacturer:" + ZString.Format(OrganisationCreator.NoManufacturerCreatedAsMIDInvalid, FDAActualManufacturerNumber));
					}
				}

				fdaLine.US_FDAManufacturerAddress = fdaManufacturerAddress?.PK ?? ZGuid.Empty;
			}

			if (!FDAActualShipperSupplierNumber.IsEmpty)
			{
				var supplierAddress = BIRDOrganisationMatching.GetOrganisationAddress(fdaLine.Factory, OrgMatchedCustomsRegNoType.MID, FDAActualShipperSupplierNumber);

				if (supplierAddress == null)
				{
					supplierAddress = OrganisationCreator.CreateManufacturerAndSendNameAddressQueryMessage(fdaLine.Factory, FDAActualShipperSupplierNumber);
					if (supplierAddress != null)
					{
						notifications.AddWarning("FDA Shipper/Supplier:" + ZString.Format(OrganisationCreator.ManufacturerCreated, FDAActualShipperSupplierNumber));
					}
					else if (!FDAActualShipperSupplierNumber.IsLettersAndNumbersOnlyOrEmpty)
					{
						notifications.AddWarning("FDA Shipper/Supplier:" + ZString.Format(OrganisationCreator.NoManufacturerCreatedAsMIDInvalid, FDAActualShipperSupplierNumber));
					}
				}

				fdaLine.US_FDAShipperAddress = supplierAddress?.PK ?? ZGuid.Empty;
			}
		}

		#endregion

		#region IBIRDOGALineIDRecord Members

		OGAType IBIRDOGALineIDRecord.OGAType
		{
			get { return OGAType.FDA; }
		}

		void IBIRDOGALineIDRecord.SetOGAIndicator(JobComInvoiceLine invoiceLine)
		{
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
		}

		#endregion
	}
}
