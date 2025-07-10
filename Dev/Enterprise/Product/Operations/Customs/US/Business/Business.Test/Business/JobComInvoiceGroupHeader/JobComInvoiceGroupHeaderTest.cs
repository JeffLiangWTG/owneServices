using System;
using CargoWise.Types;
using Enterprise.Customs.Common;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(JobComInvoiceGroupHeader))]
	sealed class JobComInvoiceGroupHeaderTest : Customs.Business.Testing.BaseJobComInvoiceGroupHeaderTest
	{
		public override void TestChargeTypeList()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			Customs.Business.ICommonInvoice commonInvoice = dec.TopGroupInvoice;
			var chargeTypeList1 = commonInvoice.ChargeTypeList;
			var chargeTypeList2 = commonInvoice.ChargeTypeList;
			AssertEquals(true, object.ReferenceEquals(chargeTypeList1, chargeTypeList2));
			ZArchitecture.Core.CodeDescriptionPairList customsChargeTypeList = new USCustomsChargeTypeList();
			customsChargeTypeList.Sort();
			AssertEquals(customsChargeTypeList.CodesAsString, chargeTypeList1.CodesAsString);
			dec.JE_MessageType = JobMessageTypeList.Codes.Export;
			chargeTypeList1 = commonInvoice.ChargeTypeList;
			chargeTypeList2 = commonInvoice.ChargeTypeList;
			AssertEquals(true, object.ReferenceEquals(chargeTypeList1, chargeTypeList2));
			customsChargeTypeList = new Customs.Business.CustomsChargeTypeList();
			customsChargeTypeList.Sort();
			AssertEquals(customsChargeTypeList.CodesAsString, chargeTypeList1.CodesAsString);
		}

		public void TestSetDefaultsForInvoiceHeader()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			JobComInvoiceGroupHeader groupHeader = declaration.JobComInvoiceGroupHeaders[0];
			JobComInvoiceHeader invoiceHeader1 = declaration.Invoices.AddNew();
			AssertEquals("JZ_OH_Supplier", ZGuid.Empty, invoiceHeader1.JZ_OH_Supplier);
			AssertEquals("USPPIDocAddress.E2_OA_Address", ZGuid.Empty, invoiceHeader1.USPPIDocAddress.E2_OA_Address);
			AssertEquals("USPPIDocAddress.E2_Contact", "", invoiceHeader1.USPPIDocAddress.E2_Contact);
			AssertEquals("USPPIDocAddress.E2_Phone_Formatted", "", invoiceHeader1.USPPIDocAddress.E2_Phone_Formatted);
			invoiceHeader1.JZ_OH_Supplier = ZGuid.NewZGuid();
			invoiceHeader1.USPPIDocAddress.E2_OA_Address = ZGuid.NewZGuid();
			invoiceHeader1.USPPIDocAddress.E2_Contact = "Contact";
			invoiceHeader1.USPPIDocAddress.E2_Phone_Formatted = "12312";
			JobComInvoiceHeader invoiceHeader2 = declaration.Invoices.AddNew();
			AssertEquals("JZ_OH_Supplier", invoiceHeader1.JZ_OH_Supplier, invoiceHeader2.JZ_OH_Supplier);
			AssertEquals("USPPIDocAddress.E2_OA_Address", invoiceHeader1.USPPIDocAddress.E2_OA_Address, invoiceHeader2.USPPIDocAddress.E2_OA_Address);
			AssertEquals("USPPIDocAddress.E2_Contact", invoiceHeader1.USPPIDocAddress.E2_Contact, invoiceHeader2.USPPIDocAddress.E2_Contact);
			AssertEquals("USPPIDocAddress.E2_Phone_Formatted", invoiceHeader1.USPPIDocAddress.E2_Phone_Formatted, invoiceHeader2.USPPIDocAddress.E2_Phone_Formatted);
		}

		protected override Type ExpectedTypeOfCharges => typeof(JobComInvChargeCollection<GroupInvoiceCharge>);
	}
}
