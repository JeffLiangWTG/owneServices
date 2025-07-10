using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(PesticideLineCollection))]
	public class PesticideLineCollectionTest : BusinessObjectCollectionTestCase
	{
		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new PesticideLineCollection(Pesticide);
		}

		Pesticide Pesticide
		{
			get
			{
				if (pesticide == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
					declaration.US_EnableCRL = true;
					var invoiceHeader = declaration.Invoices.AddNew();
					invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
					var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
					pesticide = invoiceLine.PSTLines.AddNew();
				}
				return pesticide;
			}
		}
		Pesticide pesticide;

		#endregion
	}
}
