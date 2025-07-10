using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Business.Declaration.Testing
{
	[TestedType(typeof(JobComInvoiceGroupHeader))]
	public class JobComInvoiceGroupHeaderTest : EnterpriseBusinessObjectTestCase
	{
		public void TestChargeTypeList()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
			dec.JE_MessageType = JobMessageTypeList.Codes.Export;
			Customs.Business.ICommonInvoice commonInvoice = dec.TopGroupInvoice;
			var chargeTypeList1 = commonInvoice.ChargeTypeList;
			var chargeTypeList2 = commonInvoice.ChargeTypeList;
			AssertEquals(true, object.ReferenceEquals(chargeTypeList1, chargeTypeList2));
			var customsChargeTypeList = new CustomsChargeTypeList();
			customsChargeTypeList.Sort();
			AssertEquals(customsChargeTypeList.CodesAsString, chargeTypeList1.CodesAsString);

			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			chargeTypeList1 = commonInvoice.ChargeTypeList;
			chargeTypeList2 = commonInvoice.ChargeTypeList;
			AssertEquals(true, object.ReferenceEquals(chargeTypeList1, chargeTypeList2));
			AssertEquals(customsChargeTypeList.CodesAsString, chargeTypeList1.CodesAsString);

			dec.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			customsChargeTypeList = new CustomsChargeTypeList(true);
			customsChargeTypeList.Sort();
			chargeTypeList1 = commonInvoice.ChargeTypeList;
			chargeTypeList2 = commonInvoice.ChargeTypeList;
			AssertEquals(true, object.ReferenceEquals(chargeTypeList1, chargeTypeList2));
			AssertEquals(customsChargeTypeList.CodesAsString, chargeTypeList1.CodesAsString);
		}

		public void TestApportionmentProblemDueToCurrencyConverter()
		{
			using (CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ChargeDistributeByList.Codes.Value))
			{
				TestCaseHelper.ClearTable("RefExchangeRate");

				RefCurrency uSDCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "USD");
				RefExchangeRate rate1 = uSDCurrency.ExchangeRates.AddNew();
				rate1.RE_ExRateType = "CUS";
				rate1.RE_GC = GlbCompany.CurrentCompany.PK;
				rate1.RE_SellRate = 1.8m;
				rate1.RE_StartDate = new ZDateTime(2005, 1, 31);
				rate1.RE_ExpiryDate = new ZDateTime(2005, 1, 31);

				RefCurrency currency = Factory.New<RefCurrency>();
				currency.RX_Code = "~~~";

				RefExchangeRate rate2 = currency.ExchangeRates.AddNew();
				rate2.RE_ExRateType = "CUS";
				rate2.RE_GC = GlbCompany.CurrentCompany.PK;
				rate2.RE_SellRate = 1.8m;
				rate2.RE_StartDate = new ZDateTime(2005, 1, 31);
				rate2.RE_ExpiryDate = new ZDateTime(2005, 1, 31);

				JobDeclaration declaration = Factory.New<JobDeclaration>();
				declaration.JE_ExportDate = new ZDateTime(2005, 1, 1);
				declaration.JE_EDITransmitDate = new ZDateTime(2005, 1, 31);

				JobComInvoiceGroupHeader groupHeader = declaration.JobComInvoiceGroupHeaders[0];

				JobComInvoiceHeader invoice = groupHeader.JobComInvoiceHeaders.AddNew();
				invoice.JZ_RX_NKInvoice_Currency = currency.RX_Code;
				invoice.JZ_InvoiceAmount = 1000m;

				JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.JI_LinePrice = 1000m;

				groupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 150m, "USD");
				declaration.ResumeApportionment();

				AssertEquals("Invoice should have an apportioned row", 150m, invoice.GroupCharges[0].J7_Amount);
				AssertEquals("Invoice line should have an apportioned row", 150m, invoiceLine.ApportionedCharges[0].J7_Amount);
			}
		}

		public void TestGroupInvoiceCharge()
		{
			AssertEquals("GroupHeader.Charges.GetType()", typeof(JobComInvChargeCollection<GroupInvoiceCharge>), GroupHeader.Charges.GetType());
		}

		#region TestAddInfo
		public void TestAddInfo()
		{
			GroupHeader.AddInfo.ZN_OriginalEntryNumber = "123434";
			AssertEquals("GroupHeader.AddInfo.ZN_OriginalEntryNumber", "123434", GroupHeader.AddInfo.ZN_OriginalEntryNumber);
		}
		#endregion

		#region Implementation
		protected JobComInvoiceGroupHeader GroupHeader
		{
			get
			{
				if (fGroupHeader == null)
				{
					fGroupHeader = (JobComInvoiceGroupHeader)GetNewBusinessObject();
				}
				return fGroupHeader;
			}
		}
		JobComInvoiceGroupHeader fGroupHeader;

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewBusinessObjectForDeleteTest(Factory);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			JobDeclaration testDec = factory.New<JobDeclaration>();
			return testDec.JobComInvoiceGroupHeaders[0];
		}

		#endregion
	}
}
