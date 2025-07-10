using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.US.Business.BIRD.ACS;
using Enterprise.Customs.US.Business.MessageBuilders;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.Input
{
	public partial class OGAFD05 : Messaging.Business.MessageBuildingBlocks.Input.Abstract.OGAFD05, IBIRDOGALineRecord
	{
		#region IBIRDOGALineRecord Members

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "Baseline issue")]
		void IBIRDOGALineRecord.Update(IOGALine ogaLine, INotifications notifications)
		{
			FDA fda = (FDA)ogaLine;
			JobComInvoiceLine invoiceLine = fda.InvoiceLine;
			JobDeclaration declaration = invoiceLine.Declaration;

			ZString value = AffirmationOfComplianceQualifier;

			switch (AffirmationOfComplianceCode)
			{
				case AffirmationCodeConstants.Codes.OFT:
					fda.US_OFT = value;
					break;

				case AffirmationCodeConstants.Codes.CSH:
					fda.US_CSH = value;
					break;

				case AffirmationCodeConstants.Codes.PFR:
					fda.US_PFR = value.Left(fda.US_PFRInfo.MaxLength);
					break;

				case AffirmationCodeConstants.Codes.FME:
					fda.US_FME = value;
					break;

				case AffirmationCodeConstants.Codes.PFT:
					fda.US_PFT = value;
					break;

				case AffirmationCodeConstants.Codes.SFR:
					fda.US_SFR = value;
					break;

				case AffirmationCodeConstants.Codes.RNO:
				case AffirmationCodeConstants.Codes.CNO:

					CusContainer container = declaration.CusContainers.Find(AffirmationOfComplianceQualifier);
					if (container == null)
					{
						container = declaration.CusContainers.AddNew();
						container.CO_ContainerNumber = value;
					}

					fda.InvoiceLine.ContainersForInvoiceLinesForBindingOnly.FindByContainer(container).IsForInvoiceLine = true;
					var relatedContainer = fda.ContainersForInvoiceLine.FindByContainerNumber(container.CO_ContainerNumber);
					if (relatedContainer != null)
					{
						relatedContainer.IsForFDALine = true;
					}

					break;

				case AffirmationCodeConstants.Codes.BOL:
				case AffirmationCodeConstants.Codes.NHB:
				case AffirmationCodeConstants.Codes.AWB:
				case AffirmationCodeConstants.Codes.AWH:
					ZString billNumber =
						AffirmationOfComplianceCode == AffirmationCodeConstants.Codes.BOL ||
						AffirmationOfComplianceCode == AffirmationCodeConstants.Codes.NHB ?
						value.SubstringSafe(4) : value;

					FDARelatedBill relatedBill = fda.BillsAvailable.FindByBillNumber(billNumber);
					if (relatedBill != null)
					{
						relatedBill.IsForFDALine = true;
					}
					break;

				case AffirmationCodeConstants.Codes.APA:
					if (declaration.US_SchDArrival.IsEmpty)
					{
						declaration.US_SchDArrival = value;
					}
					else if (declaration.US_SchDArrival != value)
					{
						notifications.AddWarning("Port of Arrival in FDA is different to District Port of Unlading");
					}
					break;

				case AffirmationCodeConstants.Codes.ADA:
					int year = ZInt.ParseSafe(value.Right(4), 0);
					int month = ZInt.ParseSafe(value.Left(2), 0);
					int day = ZInt.ParseSafe(value.SubstringSafe(2, 2), 0);

					if (year > 0 && month > 0 && day > 0)
					{
						if (declaration.US_FDAADTA.IsEmpty || !declaration.US_FDAADTA.IsValid)
						{
							declaration.US_FDAADTA = ZDateTime.MinSmallDateTimeValue;
						}
						if (declaration.US_FDAADTA.Date == ZDateTime.MinSmallDateTimeValue.Date)
						{
							declaration.US_FDAADTA = new ZDateTime(year, month, day, declaration.US_FDAADTA.Hour, declaration.US_FDAADTA.Minute, 0);
						}
						else if (declaration.US_FDAADTA.Year != year ||
								 declaration.US_FDAADTA.Month != month ||
								 declaration.US_FDAADTA.Day != day)
						{
							notifications.AddWarning("There are at least two different arrival dates reported in FDA. CargoWise One has only one place to store it.");
						}
					}
					break;

				case AffirmationCodeConstants.Codes.APC:
					declaration.US_FDAAPC = value;
					break;

				case AffirmationCodeConstants.Codes.ATA:
					int hour = ZInt.ParseSafe(value.Left(2), 0);
					int minutes = ZInt.ParseSafe(value.Right(2), 0);

					if (hour > 0 || minutes > 0)//0000 is not a valid format of arrival time and Customs rejects it
					{
						if (declaration.US_FDAADTA.IsEmpty || !declaration.US_FDAADTA.IsValid)
						{
							declaration.US_FDAADTA = ZDateTime.MinSmallDateTimeValue;
						}
						if (declaration.US_FDAADTA.Hour == 0 && declaration.US_FDAADTA.Minute == 0)
						{
							declaration.US_FDAADTA = declaration.US_FDAADTA.AddHours(hour).AddMinutes(minutes);
						}
						else if (declaration.US_FDAADTA.Hour != hour || declaration.US_FDAADTA.Minute != minutes)
						{
							notifications.AddWarning("There are at least two different arrival times reported in FDA. CargoWise One has only one place to store it.");
						}
					}
					break;

				case AffirmationCodeConstants.Codes.VFT:
					//System should not do this for this code as it contains a partial detail for Air. It is mandatory for 3461 or 7501.
					break;

				case AffirmationCodeConstants.Codes.CAN:
				case AffirmationCodeConstants.Codes.PVL:
				case AffirmationCodeConstants.Codes.PVP:
					if (declaration.US_FDACAN.IsEmpty)
					{
						declaration.US_FDACAN = value;
					}
					else if (declaration.US_FDACAN != value)
					{
						notifications.AddWarning("There are at least two different carrier names reported in FDA. CargoWise One has only one place to store it.");
					}
					break;

				case AffirmationCodeConstants.Codes.CCN:
				case AffirmationCodeConstants.Codes.PVS:
				case AffirmationCodeConstants.Codes.PVC:
					if (declaration.US_FDACCN.IsEmpty)
					{
						declaration.US_FDACCN = value;
					}
					else if (declaration.US_FDACCN != value)
					{
						notifications.AddWarning("There are at least two different carrier countries reported in FDA. CargoWise One has only one place to store it.");
					}
					break;

				default:
					fda.AffirmationCodes.AddNew(AffirmationOfComplianceCode, AffirmationOfComplianceQualifier);
					break;
			}
		}

		#endregion
	}
}
