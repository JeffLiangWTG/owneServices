using System;
using CargoWise.Types;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing
{
	[TestedType(typeof(JobComInvoiceGroupHeader))]
	sealed class JobComInvoiceGroupHeaderTest : Customs.Business.Testing.BaseJobComInvoiceGroupHeaderTest
	{
		protected override Type ExpectedTypeOfCharges => typeof(Common.JobComInvChargeCollection<GroupInvoiceCharge>);

		public void TestDeclaration()
		{
			AssertType<JobDeclaration>(header.JobDeclaration);
		}

		public void TestJobComInvoiceHeaders()
		{
			AssertType<InvoiceHeaderActiveCollection>(header.JobComInvoiceHeaders);
		}

		public override void TestChargeTypeList()
		{
			CombineAssertions(() =>
			{
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				var invoice = declaration.Invoices.AddNew();
				var charges = invoice.Charges.AddNew();
				var chargeTypeList1 = charges.Lookups.ChargeTypeList;
				var chargeTypeList2 = charges.Lookups.ChargeTypeList;
				AssertEquals("import: list1 = list2", expected: true, object.ReferenceEquals(chargeTypeList1, chargeTypeList2));
				var customsChargeTypeListImp = new NOInvoiceChargeTypesImport();
				AssertEquals("import: compare codes", customsChargeTypeListImp.CodesAsString, chargeTypeList1.CodesAsString);

				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				charges = invoice.Charges.AddNew();
				chargeTypeList1 = charges.Lookups.ChargeTypeList;
				chargeTypeList2 = charges.Lookups.ChargeTypeList;
				AssertEquals("export: list1 = list2", expected: true, object.ReferenceEquals(chargeTypeList1, chargeTypeList2));
				var customsChargeTypeListExp = new NOInvoiceChargeTypesExport();
				AssertEquals("export: compare codes", customsChargeTypeListExp.CodesAsString, chargeTypeList1.CodesAsString);
			});
		}

		protected override ZString OFTChargeDescription => NOInvoiceChargeTypesExport.Descriptions.OverseasFreight.ToString().ToUpper();

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			header = declaration.JobComInvoiceGroupHeaders[0];
		}
		JobComInvoiceGroupHeader header;
		JobDeclaration declaration;
	}
}
