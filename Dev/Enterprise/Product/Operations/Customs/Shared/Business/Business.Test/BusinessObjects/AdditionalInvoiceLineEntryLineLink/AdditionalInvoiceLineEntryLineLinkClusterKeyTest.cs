using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.ClusterKey;
using Enterprise.ZArchitecture.Business.ClusterKey.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(AdditionalInvoiceLineEntryLineLink))]
	sealed class AdditionalInvoiceLineEntryLineLinkClusterKeyTest : ClusterKeyWorkerMandatoryTest
	{
		protected override IEnumerable<IClusterKeyWorker> PrepareDataAndGetExpectedClusterKeyChildren() => null;

		protected override IClusterKeyEntity NewClusterKeyEntity()
		{
			var entryLine = (CusEntryLine)NewParentObject();
			var invoice = entryLine.Declaration.Invoices.AddNew();
			var invLine = invoice.InvoiceLines.AddNew();

			var link = Factory.New<AdditionalInvoiceLineEntryLineLink>();
			link.BU_CL = entryLine.PK;
			link.BU_JI = invLine.PK;
			return link;
		}

		protected override EnterpriseBusinessObject NewParentObject()
		{
			var mockDec = Factory.NewMoq<BaseJobDeclaration>();
			mockDec.Setup(m => m.AllowEntryLinesToBeLinkedToAnotherJob).Returns(true);
			var testDec = mockDec.Object;
			var entryHeader = testDec.CustomsEntryHeaders.AddNew();
			return entryHeader.MergedLines.AddNew();
		}
	}
}
