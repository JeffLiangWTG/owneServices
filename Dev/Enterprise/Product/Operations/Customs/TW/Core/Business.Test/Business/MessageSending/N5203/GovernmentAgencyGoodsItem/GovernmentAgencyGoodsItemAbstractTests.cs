using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TW.Business.N5203;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	abstract class GovernmentAgencyGoodsItemAbstractTests<TGovernmentAgencyGoodsItem> : TestCaseWithFactory
		where TGovernmentAgencyGoodsItem : GovernmentAgencyGoodsItem
	{
		[ExpectNoExceptions]
		public virtual void TestSequenceNumeric()
		{
			EntryLine.CL_LineNumber = 1;
			NUnit.Framework.Assert.That(GovernmentAgencyGoodsItem.SequenceNumeric, NUnit.Framework.Is.EqualTo(1).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public virtual void TestCommodity()
		{
			NUnit.Framework.Assert.That(GovernmentAgencyGoodsItem.Commodity, NUnit.Framework.Is.TypeOf<Commodity>());
		}

		[ExpectNoExceptions]
		public virtual void TestAdditionalDocuments()
		{
			var assignedJobComInvLineRef = InvoiceLine.AssignedJobComInvLineRefsCollection.AddNew();
			assignedJobComInvLineRef.JG_ReferenceNumber = "A2111";
			NUnit.Framework.Assert.That(GovernmentAgencyGoodsItem.AdditionalDocuments.Count(), NUnit.Framework.Is.EqualTo(1));
		}

		[ExpectNoExceptions]
		public void TestAdditionalInformations()
		{
			TestTWCreator.AddInvoiceLineReservedFields(InvoiceLine);
			NUnit.Framework.Assert.That(GovernmentAgencyGoodsItem.AdditionalInformations, NUnit.Framework.Is.Not.EqualTo(default(System.Collections.Generic.IEnumerable<Enterprise.Customs.TW.Messaging.IAdditionalInformation>)));
			SharedHelperTest.AssertAdditionalInformations(GovernmentAgencyGoodsItem.AdditionalInformations);
		}

		[ExpectNoExceptions]
		public virtual void TestGoodsMeasure()
		{
			NUnit.Framework.Assert.That(GovernmentAgencyGoodsItem.GoodsMeasure.GetType(), NUnit.Framework.Is.EqualTo(typeof(N5203.GoodsMeasure)));
		}

		[ExpectNoExceptions]
		public void TestManufacturer()
		{
			NUnit.Framework.Assert.That(GovernmentAgencyGoodsItem.Manufacturer.GetType(), NUnit.Framework.Is.EqualTo(typeof(GovernmentAgencyGoodsItemManufacturer)));
		}

		[ExpectNoExceptions]
		public void TestOrigin()
		{
			NUnit.Framework.Assert.That(GovernmentAgencyGoodsItem.Origin.GetType(), NUnit.Framework.Is.EqualTo(typeof(Origin)));
		}

		[ExpectNoExceptions]
		public void TestPackaging()
		{
			CombineAssertions(() =>
			{
				InvoiceLine.JI_PackagingQTY = 123;
				InvoiceLine.JI_PackagingUQ = "PKG";
				var packaging = GovernmentAgencyGoodsItem.Packaging;
				NUnit.Framework.Assert.That(packaging.QuantityQuantity, NUnit.Framework.Is.EqualTo(123M).Using(CustomComparers.TypeComparison), "QuantityQuantity");
				NUnit.Framework.Assert.That(packaging.TypeCode, NUnit.Framework.Is.EqualTo("PKG").Using(CustomComparers.TypeComparison), "TypeCode");

				InvoiceLine.JI_PackagingQTY = ZDecimal.Zero;
				NUnit.Framework.Assert.That(GovernmentAgencyGoodsItem.Packaging, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Messaging.IPackaging)), "QTY == 0, not output Packaging - should be [null]");
			});
		}

		[ExpectNoExceptions]
		public void TestPreviousDocument()
		{
			InvoiceLine.JI_PreviousEntryNumber = ZString.Empty;
			InvoiceLine.JI_PreviousEntryLineNumber = 3;
			NUnit.Framework.Assert.That(GovernmentAgencyGoodsItem.PreviousDocument, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Messaging.IPreviousDocument)));
			InvoiceLine.JI_PreviousEntryNumber = "DXD2";
			InvoiceLine.JI_PreviousEntryLineNumber = ZShort.Zero;
			NUnit.Framework.Assert.That(GovernmentAgencyGoodsItem.PreviousDocument, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Messaging.IPreviousDocument)));
			InvoiceLine.JI_PreviousEntryNumber = "DXD2";
			InvoiceLine.JI_PreviousEntryLineNumber = 3;
			var previousDocument = GovernmentAgencyGoodsItem.PreviousDocument;
			NUnit.Framework.Assert.That(previousDocument, NUnit.Framework.Is.Not.EqualTo(default(Enterprise.Customs.TW.Messaging.IPreviousDocument)));
			NUnit.Framework.Assert.That(previousDocument.GetType(), NUnit.Framework.Is.EqualTo(typeof(PreviousDocumentWrapper)));
			NUnit.Framework.Assert.That(previousDocument.ID, NUnit.Framework.Is.EqualTo("DXD2").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(previousDocument.LineNumeric, NUnit.Framework.Is.EqualTo(3).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestApprovalDocument()
		{
			NUnit.Framework.Assert.That(GovernmentAgencyGoodsItem.ApprovalDocument, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Messaging.ILPCODetail)));
		}

		[ExpectNoExceptions]
		public void TestCommoditySpecification()
		{
			NUnit.Framework.Assert.That(GovernmentAgencyGoodsItem.CommoditySpecification, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Messaging.ICommoditySpecification)));
		}

		[ExpectNoExceptions]
		public void TestGoodsLicensingStatisticalMeasure()
		{
			NUnit.Framework.Assert.That(GovernmentAgencyGoodsItem.GoodsLicensingStatisticalMeasure, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Messaging.IGoodsLicensingStatisticalMeasure)));
		}

		[ExpectNoExceptions]
		public virtual void TestGoodsStatisticalMeasure()
		{
			NUnit.Framework.Assert.That(GovernmentAgencyGoodsItem.GoodsStatisticalMeasure.GetType(), NUnit.Framework.Is.EqualTo(typeof(GoodsStatisticalMeasure)));
		}

		[ExpectNoExceptions]
		public void TestMedicalInstrument()
		{
			NUnit.Framework.Assert.That(GovernmentAgencyGoodsItem.MedicalInstrument, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Messaging.ILPCODetail)));
		}

		[ExpectNoExceptions]
		public void TestPreBondedDocument()
		{
			InvoiceLine.PreviousBondedEntryNumber = ZString.Empty;
			InvoiceLine.PreviousBondedEntryLineNumber = 3;
			NUnit.Framework.Assert.That(GovernmentAgencyGoodsItem.PreBondedDocument, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Messaging.IPreviousDocument)));
			InvoiceLine.PreviousBondedEntryNumber = "DXD2";
			InvoiceLine.PreviousBondedEntryLineNumber = ZShort.Zero;
			NUnit.Framework.Assert.That(GovernmentAgencyGoodsItem.PreBondedDocument, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Messaging.IPreviousDocument)));
			InvoiceLine.PreviousBondedEntryNumber = "DXD2";
			InvoiceLine.PreviousBondedEntryLineNumber = 3;
			var preBondedDocument = GovernmentAgencyGoodsItem.PreBondedDocument;
			NUnit.Framework.Assert.That(preBondedDocument, NUnit.Framework.Is.Not.EqualTo(default(Enterprise.Customs.TW.Messaging.IPreviousDocument)));
			NUnit.Framework.Assert.That(preBondedDocument.GetType(), NUnit.Framework.Is.EqualTo(typeof(PreviousDocumentWrapper)));
			NUnit.Framework.Assert.That(preBondedDocument.ID, NUnit.Framework.Is.EqualTo("DXD2").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(preBondedDocument.LineNumeric, NUnit.Framework.Is.EqualTo(3).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestShippingIdentifications()
		{
			NUnit.Framework.Assert.That(GovernmentAgencyGoodsItem.ShippingIdentifications, NUnit.Framework.Is.EqualTo(default(System.Collections.Generic.IEnumerable<Enterprise.Customs.TW.Messaging.IShippingIdentification>)));
		}

		[ExpectNoExceptions]
		public void TestGovernmentProcedure()
		{
			NUnit.Framework.Assert.That(GovernmentAgencyGoodsItem.GovernmentProcedure.GetType(), NUnit.Framework.Is.EqualTo(typeof(GovernmentProcedure)));
		}

		[ExpectNoExceptions]
		public void TestControlInspectionStartDateTime()
		{
			NUnit.Framework.Assert.That(GovernmentAgencyGoodsItem.ControlInspectionStartDateTime, NUnit.Framework.Is.EqualTo(ZDateTime.Empty));
		}

		[ExpectNoExceptions]
		public void TestExaminationPlace()
		{
			NUnit.Framework.Assert.That(GovernmentAgencyGoodsItem.ExaminationPlace, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestTransportEquipments()
		{
			NUnit.Framework.Assert.That(GovernmentAgencyGoodsItem.TransportEquipments, NUnit.Framework.Is.EqualTo(default(System.Collections.Generic.IEnumerable<Enterprise.Customs.TW.Messaging.ITransportEquipment>)));
		}

		[ExpectNoExceptions]
		public void TestAdditionalDeclaration()
		{
			NUnit.Framework.Assert.That(GovernmentAgencyGoodsItem.AdditionalDeclaration, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Messaging.IAdditionalDeclaration)));
		}

		[ExpectNoExceptions]
		public void TestFirstInvoiceLine()
		{
			CreateTestInvoiceLineIfRequirement();
			var expectedInvoiceLine = EntryLine.InvoiceLines.Cast<JobComInvoiceLine>().First();
			NUnit.Framework.Assert.That(expectedInvoiceLine.PK, NUnit.Framework.Is.EqualTo(InvoiceLine.PK));
		}

		[ExpectNoExceptions]
		public void TestCusEntryLine()
		{
			var item = GovernmentAgencyGoodsItem as GovernmentAgencyGoodsItem;
			NUnit.Framework.Assert.That(EntryLine.PK, NUnit.Framework.Is.EqualTo(item.EntryLine.PK));
		}

		[ExpectNoExceptions]
		public void TestEntryLineGroup()
		{
			var invoiceLine2 = new TestTWCreator(Factory).CreateInvoiceLineForN5203(EntryHeader);
			invoiceLine2.JI_Group = "Vehicle Parts";
			NUnit.Framework.Assert.That(GovernmentAgencyGoodsItem.EntryLineGroupForDocument, NUnit.Framework.Is.EqualTo("Vehicle Parts").Using(CustomComparers.TypeComparison));
		}

		protected void CreateTestInvoiceLineIfRequirement()
		{
			invoiceLine ??= TestHelper.CreateInvoiceLineForN5203(EntryHeader);
		}

		TestTWCreator testHelper;
		TestTWCreator TestHelper => testHelper ??= new TestTWCreator(Factory);

		protected abstract TGovernmentAgencyGoodsItem GovernmentAgencyGoodsItem { get; }

		protected CusEntryHeader entryHeader;
		protected CusEntryHeader EntryHeader => entryHeader ??= TestHelper.CreateEntryHeaderForN5203();

		protected CusEntryLine EntryLine => EntryHeader.MergedLines.Cast<CusEntryLine>().First();

		protected JobComInvoiceLine invoiceLine;
		protected JobComInvoiceLine InvoiceLine
		{
			get
			{
				CreateTestInvoiceLineIfRequirement();
				return invoiceLine;
			}
		}
	}
}
