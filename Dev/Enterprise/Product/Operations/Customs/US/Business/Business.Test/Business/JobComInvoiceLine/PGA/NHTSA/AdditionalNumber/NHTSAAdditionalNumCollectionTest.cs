using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(NHTSAAdditionalNumCollection))]
	public class NHTSAAdditionalNumCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestAddAndDeleteNumberCollection()
		{
			Number.US_NHTAdditionalIdentityNumQualifier = ItemIdentityNumberQualifierList.Codes.VehicleIdentificationNumberVIN;
			Number.US_NHTAdditionalIdentityNumber = "SALLDHMV2AA100000";
			AssertEquals(ItemIdentityNumberQualifierList.Codes.VehicleIdentificationNumberVIN, DetailsLine.US_NHTIdentityNumQualifier);
			AssertEquals("SALLDHMV2AA100000", DetailsLine.US_NHTIdentityNumber);

			var newNumber = DetailsLine.AdditionalNumbers.AddNew();
			newNumber.US_NHTAdditionalIdentityNumQualifier = ItemIdentityNumberQualifierList.Codes.EngineNumber;
			newNumber.US_NHTAdditionalIdentityNumber = "112233";
			AssertEquals(ItemIdentityNumberQualifierList.Codes.VehicleIdentificationNumberVIN, DetailsLine.US_NHTIdentityNumQualifier);
			AssertEquals("SALLDHMV2AA100000", DetailsLine.US_NHTIdentityNumber);

			DetailsLine.AdditionalNumbers.RemoveAndDeleteAll();
			AssertEquals(ZString.Empty, DetailsLine.US_NHTIdentityNumQualifier);
			AssertEquals(ZString.Empty, DetailsLine.US_NHTIdentityNumber);
		}

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return DetailsLine.AdditionalNumbers;
		}

		NHTSADetails DetailsLine
		{
			get
			{
				if (detailsLine == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
					declaration.US_EnableCRL = true;
					var invoiceHeader = declaration.Invoices.AddNew();
					invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
					var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
					var header = invoiceLine.NHTSALines.AddNew();
					detailsLine = header.NHTSADetails.AddNew();
					Factory.Save();
				}
				return detailsLine;
			}
		}
		NHTSADetails detailsLine;

		NHTSAAdditionalNum Number
		{
			get { return number ?? (number = DetailsLine.AdditionalNumbers.OfType<NHTSAAdditionalNum>().FirstOrDefault() ?? DetailsLine.AdditionalNumbers.AddNew()); }
		}
		NHTSAAdditionalNum number;

		#endregion
	}
}
