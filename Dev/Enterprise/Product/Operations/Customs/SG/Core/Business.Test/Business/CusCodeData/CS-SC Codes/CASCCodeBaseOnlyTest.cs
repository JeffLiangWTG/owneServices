using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	class CASCCodeBaseOnlyTest : TestCaseWithFactory
	{
		public void TestSetDefaultValues()
		{
			var cASCCodeTestClass = Factory.New<CASCCodeTestClass>();
			AssertEquals("XXX", cASCCodeTestClass.CY_Type);
		}

		public void TestValidation()
		{
			var cASCCodeTestClass = Factory.New<CASCCodeTestClass>();
			Assert(cASCCodeTestClass.Validation is CASCCodeValidation);
		}

		public void TestSequenceNoIsDefaultedToCY_Order()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			var caSCCode = invoiceLine.CASCCode1s.AddNew();
			AssertEquals("CA/SC line sequence no. now defaults", (ZShort)1, caSCCode.CY_Order);
		}

		public void TestSequenceNoIncrements()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			var caSCCode = invoiceLine.CASCCode1s.AddNew();
			AssertEquals("First CA/SC line sequence no.", (ZShort)1, caSCCode.CY_Order);

			for (int i = 1; i < 50; i++)
			{
				caSCCode = invoiceLine.CASCCode1s.AddNew();
				caSCCode.CY_Data = "sequence no. " + i.ToString();
			}

			AssertEquals("Fiftieth CA/SC line sequence no.", (ZShort)50, caSCCode.CY_Order);
		}

		public void TestDeletionOfCACSCodeReOrdersSequenceNo()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			var caSCCode1 = invoiceLine.CASCCode1s.AddNew();
			AssertEquals("First CA/SC line sequence no. 1", (ZShort)1, caSCCode1.CY_Order);

			var caSCCode2 = invoiceLine.CASCCode1s.AddNew();
			AssertEquals("Second CA/SC line sequence no. 2", (ZShort)2, caSCCode2.CY_Order);

			var caSCCode3 = invoiceLine.CASCCode1s.AddNew();
			AssertEquals("Third CA/SC line sequence no. 3", (ZShort)3, caSCCode3.CY_Order);

			var caSCCode4 = invoiceLine.CASCCode1s.AddNew();
			AssertEquals("Fourth CA/SC line sequence no. 4", (ZShort)4, caSCCode4.CY_Order);

			caSCCode2.Delete();
			AssertEquals("First CA/SC line sequence no. remains 1", (ZShort)1, caSCCode1.CY_Order);
			AssertEquals("Third CA/SC line sequence no. now becomes second 2", (ZShort)2, caSCCode3.CY_Order);
			AssertEquals("Fourth CA/SC line sequence no. now becomes third 3", (ZShort)3, caSCCode4.CY_Order);
		}

		public void TestDeletionOfExistingCACSCodeReOrdersSequenceNo()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			var caSCCode1_1 = invoiceLine.CASCCode1s.AddNew();
			caSCCode1_1.CY_Data = "CASC Code 1 - line 1";
			AssertEquals("First CA/SC line sequence no. 1", (ZShort)1, caSCCode1_1.CY_Order);

			var caSCCode1_2 = invoiceLine.CASCCode1s.AddNew();
			caSCCode1_2.CY_Data = "CASC Code 1 - line 2";
			AssertEquals("Second CA/SC line sequence no. 2", (ZShort)2, caSCCode1_2.CY_Order);

			var caSCCode1_3 = invoiceLine.CASCCode1s.AddNew();
			caSCCode1_3.CY_Data = "CASC Code 1 - line 3";
			AssertEquals("Third CA/SC line sequence no. 3", (ZShort)3, caSCCode1_3.CY_Order);

			var caSCCode1_4 = invoiceLine.CASCCode1s.AddNew();
			caSCCode1_4.CY_Data = "CASC Code 1 - line 4";
			AssertEquals("Fourth CA/SC line sequence no. 4", (ZShort)4, caSCCode1_4.CY_Order);

			Factory.Save();

			var reloadedDeclaration = Factory.Load<JobDeclaration>(declaration.PK);
			var reloadedDeclaration_caSCCode1_2 = reloadedDeclaration.InvoiceLines[0].CASCCode1s[1];
			AssertEquals("Pre-condition", "CASC Code 1 - line 2", reloadedDeclaration_caSCCode1_2.CY_Data);

			reloadedDeclaration_caSCCode1_2.Delete();
			Factory.Save();

			var testDeclaration = Factory.Load<JobDeclaration>(declaration.PK);
			AssertEquals("CASCCode1s.Count after deleted code", 3, testDeclaration.InvoiceLines[0].CASCCode1s.Count);

			var testDeclaration_caSCCode1 = testDeclaration.InvoiceLines[0].CASCCode1s[0];
			var testDeclaration_caSCCode2 = testDeclaration.InvoiceLines[0].CASCCode1s[1];
			var testDeclaration_caSCCode3 = testDeclaration.InvoiceLines[0].CASCCode1s[2];
			AssertEquals("First CA/SC line sequence no. remains 1", (ZShort)1, testDeclaration_caSCCode1.CY_Order);
			AssertEquals("First CA/SC line data", "CASC Code 1 - line 1", testDeclaration_caSCCode1.CY_Data);
			AssertEquals("Third CA/SC line sequence no. now becomes second CASC code", (ZShort)2, testDeclaration_caSCCode2.CY_Order);
			AssertEquals("line data", "CASC Code 1 - line 3", testDeclaration_caSCCode2.CY_Data);
			AssertEquals("Fourth CA/SC line sequence no. now becomes third CASC code", (ZShort)3, testDeclaration_caSCCode3.CY_Order);
			AssertEquals("line data", "CASC Code 1 - line 4", testDeclaration_caSCCode3.CY_Data);
		}

		public void TestSequenceNoIsUnique()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			var caSCCode1 = invoiceLine.CASCCode1s.AddNew();
			AssertEquals("First CA/SC line sequence no. 1", (ZShort)1, caSCCode1.CY_Order);

			var caSCCode2 = invoiceLine.CASCCode1s.AddNew();
			AssertEquals("Second CA/SC line sequence no. 2", (ZShort)2, caSCCode2.CY_Order);

			var caSCCode3 = invoiceLine.CASCCode1s.AddNew();
			AssertEquals("Third CA/SC line sequence no. 3", (ZShort)3, caSCCode3.CY_Order);

			var caSCCode4 = invoiceLine.CASCCode1s.AddNew();
			AssertEquals("Fourth CA/SC line sequence no. 4", (ZShort)4, caSCCode4.CY_Order);

			caSCCode2.CY_Order = 1;
			AssertEquals("First CA/SC line now becomes sequence no. 2", (ZShort)2, caSCCode1.CY_Order);
			AssertEquals("Second CA/SC line sequence no. had been re-numbered to 1", (ZShort)1, caSCCode2.CY_Order);
			AssertEquals("Third CA/SC line sequence no. remains 3", (ZShort)3, caSCCode3.CY_Order);
			AssertEquals("Fourth CA/SC line sequence no. remains 4", (ZShort)4, caSCCode4.CY_Order);
		}

		public void TestManuallyEditSequenceNo()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			var caSCCode1 = invoiceLine.CASCCode1s.AddNew();
			AssertEquals("First CA/SC line sequence no. 1", (ZShort)1, caSCCode1.CY_Order);

			var caSCCode2 = invoiceLine.CASCCode1s.AddNew();
			AssertEquals("Second CA/SC line sequence no. 2", (ZShort)2, caSCCode2.CY_Order);

			var caSCCode3 = invoiceLine.CASCCode1s.AddNew();
			AssertEquals("Third CA/SC line sequence no. 3", (ZShort)3, caSCCode3.CY_Order);

			var caSCCode4 = invoiceLine.CASCCode1s.AddNew();
			AssertEquals("Fourth CA/SC line sequence no. 4", (ZShort)4, caSCCode4.CY_Order);

			caSCCode2.CY_Order = 4;
			AssertEquals("First CA/SC line sequence no. remains 1", (ZShort)1, caSCCode1.CY_Order);
			AssertEquals("Second CA/SC line sequence no. now edited to be fourth 4", (ZShort)4, caSCCode2.CY_Order);
			AssertEquals("Third CA/SC line sequence no. now re-ordered to become second 2", (ZShort)2, caSCCode3.CY_Order);
			AssertEquals("Fourth CA/SC line sequence no. now re-orderd to become third 3", (ZShort)3, caSCCode4.CY_Order);

			var caSCCode5 = invoiceLine.CASCCode1s.AddNew();
			caSCCode5.CY_Order = 8;
			AssertEquals("Fifth CA/SC line entered becomes sequence no. 5", (ZShort)5, caSCCode5.CY_Order);
		}

		public void TestSequenceNoIncrementsCASCCode2()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			var caSCCode2 = invoiceLine.CASCCode2s.AddNew();
			AssertEquals("First CA/SC 2 code line sequence no.", (ZShort)1, caSCCode2.CY_Order);

			for (int i = 1; i < 10; i++)
			{
				caSCCode2 = invoiceLine.CASCCode2s.AddNew();
				caSCCode2.CY_Data = "sequence no. " + i.ToString();
			}

			AssertEquals("Tenth CA/SC 2 line sequence no.", (ZShort)10, caSCCode2.CY_Order);
		}

		public void TestSequenceNoIncrementsCASCCode3()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			var caSCCode3 = invoiceLine.CASCCode3s.AddNew();
			AssertEquals("First CA/SC 3 code line sequence no.", (ZShort)1, caSCCode3.CY_Order);

			for (int i = 1; i < 15; i++)
			{
				caSCCode3 = invoiceLine.CASCCode3s.AddNew();
				caSCCode3.CY_Data = "sequence no. " + i.ToString();
			}

			AssertEquals("Fiftenth CA/SC 3 code line sequence no.", (ZShort)15, caSCCode3.CY_Order);
		}

		#region Test Class
		internal class CASCCodeTestClass : CASCCode
		{
			public CASCCodeTestClass(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public override string CusCodeDataType
			{
				get
				{
					return "XXX";
				}
			}
		}
		#endregion
	}
}
