using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.NZ.Business.Declaration.CustomsChargeCalculators.Testing
{
	public class DutyCalculatorTestObjects
	{
		public DutyCalculatorTestObjects(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}

		readonly BusinessObjectFactory factory;

		void SetupAllObjects()
		{
			fDeclaration = JobDeclaration.New(factory);
			fDeclaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			fDeclaration.JE_DateOfArrival = new ZDateTime(2004, 12, 12);
			fDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;

			fInvoiceHeader = fDeclaration.Invoices.AddNew();
			fInvoiceHeader.JZ_InvoiceNumber = "1001001";
			fInvoiceHeader.JZ_IncoTerm = "CIF";
			fInvoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.NewZealand;
			fInvoiceHeader.JZ_InvoiceAmount = 3010.00m;

			fOSFrtCharge = fInvoiceHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 1000m, "NZD");
			fOSInsCharge = fInvoiceHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 10m, "NZD");

			fEntryHeader = (FormalEntry.CusEntryHeader)fDeclaration.CusEntryHeader;

			fInvoiceLine = fInvoiceHeader.JobComInvoiceLines.AddNew();
			fInvoiceLine.JI_LinePrice = 2000m;
			fInvoiceLine.JI_Tariff = "7007.21.02.01K";
			fInvoiceLine.JI_CountryOfOrigin = "US";
			fInvoiceLine.JI_QualifiesForPreferentialDuty = "N";
			fInvoiceLine.JI_CustomsQuantity = 10000m;
			fInvoiceLine.JI_CustomsUnitQty = "NMB";

			fEntryLine = fEntryHeader.MergedLines.AddNew();
			fEntryLine.CL_CustomsValue = 2000m;
			fInvoiceLine.JI_CL = fEntryLine.PK;
			fEntryLine.CL_AdValoremTariff = fInvoiceLine.JI_Tariff.Left(15);
			fDeclaration.ResumeApportionment();
		}

		public JobDeclaration Declaration
		{
			get
			{
				if (fDeclaration == null)
				{
					SetupAllObjects();
				}
				return fDeclaration;
			}
		}
		JobDeclaration fDeclaration;

		public JobComInvoiceHeader InvoiceHeader
		{
			get
			{
				if (fInvoiceHeader == null)
				{
					SetupAllObjects();
				}
				return fInvoiceHeader;
			}
		}
		JobComInvoiceHeader fInvoiceHeader;

		public InvoiceCharge OSFrtCharge
		{
			get
			{
				if (fOSFrtCharge == null)
				{
					SetupAllObjects();
				}
				return fOSFrtCharge;
			}
		}
		InvoiceCharge fOSFrtCharge;

		public InvoiceCharge OSInsCharge
		{
			get
			{
				if (fOSInsCharge == null)
				{
					SetupAllObjects();
				}
				return fOSInsCharge;
			}
		}
		InvoiceCharge fOSInsCharge;

		public FormalEntry.CusEntryHeader EntryHeader
		{
			get
			{
				if (fEntryHeader == null)
				{
					SetupAllObjects();
				}
				return fEntryHeader;
			}
		}
		FormalEntry.CusEntryHeader fEntryHeader;

		public CusEntryLine EntryLine
		{
			get
			{
				if (fEntryLine == null)
				{
					SetupAllObjects();
				}
				return fEntryLine;
			}
		}
		CusEntryLine fEntryLine;

		public JobComInvoiceLine InvoiceLine
		{
			get
			{
				if (fInvoiceLine == null)
				{
					SetupAllObjects();
				}
				return fInvoiceLine;
			}
		}
		JobComInvoiceLine fInvoiceLine;
	}

	public class DutyCalculatorTestObjectsTest : TestCaseWithFactory
	{
		public void TestDutyCalcTestObject()
		{
			var testObjects = new DutyCalculatorTestObjects(Factory);
			AssertNotNull(testObjects.InvoiceLine);
			AssertNotNull(testObjects.Declaration);
			AssertNotNull(testObjects.OSFrtCharge);
		}
	}
}
