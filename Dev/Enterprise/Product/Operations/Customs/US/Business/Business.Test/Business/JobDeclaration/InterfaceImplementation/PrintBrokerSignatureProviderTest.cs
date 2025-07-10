using System;
using System.Drawing;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	class PrintBrokerSignatureProviderTest : TestCaseWithFactory
	{
		public void TestShouldPrintBrokerSignature()
		{
			var declaration = Factory.New<JobDeclaration>();
			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			AssertEquals("Shouldn't print broker signature.", false, cusEntryHeader.ShouldPrintBrokerSignature);

			USCustomsDataRegistry.Instance.PrintBrokerSignatureOnEntryDocs.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AssertEquals("Should print broker signature.", true, cusEntryHeader.ShouldPrintBrokerSignature);
		}

		public void TestBrokerSignature()
		{
			USCustomsDataRegistry.Instance.EntryDeclarant.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			var declaration = Factory.New<JobDeclaration>();
			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();

			USCustomsDataRegistry.Instance.PrintBrokerSignatureOnEntryDocs.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			AssertNull(cusEntryHeader.BrokerSignature3461);

			USCustomsDataRegistry.Instance.PrintBrokerSignatureOnEntryDocs.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AssertNull(cusEntryHeader.BrokerSignature3461);

			var broker = Factory.New<GlbStaff>();
			broker.GS_Code = "INC";
			declaration.JE_GS_NKCusAgent = broker.GS_Code;

			USCustomsDataRegistry.Instance.PrintBrokerSignatureOnEntryDocs.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			AssertNull(cusEntryHeader.BrokerSignature3461);

			USCustomsDataRegistry.Instance.PrintBrokerSignatureOnEntryDocs.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AssertNull(cusEntryHeader.BrokerSignature3461);

			broker.SignatureImage = new Bitmap(2, 1);
			AssertNotNull(cusEntryHeader.BrokerSignature3461);
		}

		public void TestSignatory()
		{
			USCustomsDataRegistry.Instance.EntryDeclarant.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			var broker = Factory.New<GlbStaff>();
			broker.GS_Code = "INC";
			var broker2 = Factory.New<GlbStaff>();
			broker2.GS_Code = "ICT";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_GS_NKCusAgent = broker.GS_Code;

			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			USCustomsDataRegistry.Instance.PrintBrokerSignatureOnEntryDocs.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			AssertEquals(broker.PK, cusEntryHeader.Signatory.PK);

			USCustomsDataRegistry.Instance.PrintBrokerSignatureOnEntryDocs.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			USCustomsDataRegistry.Instance.PrintSignatureOnEntryDocsBroker.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, broker2.PK.ToGuid());
			AssertEquals(broker2.PK, cusEntryHeader.Signatory.PK);
		}
	}
}
