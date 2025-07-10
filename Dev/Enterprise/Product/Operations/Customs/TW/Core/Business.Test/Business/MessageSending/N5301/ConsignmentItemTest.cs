using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Business.N5301;
using Enterprise.Customs.TW.Messaging;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class ConsignmentItemTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestSplit()
		{
			NUnit.Framework.Assert.That(consignmentItem.Split.ToString(), NUnit.Framework.Is.Null.Or.Empty);
		}

		[ExpectNoExceptions]
		public void TestCommodity()
		{
			NUnit.Framework.Assert.That(consignmentItem.Commodity, NUnit.Framework.Is.Not.EqualTo(default(ICommodity)));
			NUnit.Framework.Assert.That(consignmentItem.Commodity, NUnit.Framework.Is.TypeOf(typeof(Commodity)));
		}

		[ExpectNoExceptions]
		public void TestGoodsMeasure()
		{
			NUnit.Framework.Assert.That(consignmentItem.GoodsMeasure, NUnit.Framework.Is.Not.EqualTo(default(IGoodsMeasure)));
			NUnit.Framework.Assert.That(consignmentItem.GoodsMeasure, NUnit.Framework.Is.TypeOf(typeof(GoodsMeasure)));
		}

		[ExpectNoExceptions]
		public void TestPackaging()
		{
			NUnit.Framework.Assert.That(consignmentItem.Packaging, NUnit.Framework.Is.Not.EqualTo(default(IPackaging)));
			NUnit.Framework.Assert.That(consignmentItem.Packaging, NUnit.Framework.Is.TypeOf(typeof(Packaging)));

			header.ArrivalBill.B0_ManifestUQ = "PCE";
			consignmentItem = new ConsignmentItem(header);
			NUnit.Framework.Assert.That(consignmentItem.Packaging.TypeCode, NUnit.Framework.Is.EqualTo("PCE").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestTransportContractDocuments()
		{
			var bill = header.MovementBill;
			bill.B0_MasterBillNumber = "69517920011";
			bill.B0_HouseBillNumber = "H000001";
			var expected = header.GetTransportContractDocuments(header.MovementBill, (id, typeCode) => new TransportContractDocumentWrapper(id, typeCode));
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(consignmentItem.TransportContractDocuments.Select(x => x.ID), NUnit.Framework.Is.EqualTo(expected.Select(x => x.ID)), "TransportContractDocument.ID should be same.");
				NUnit.Framework.Assert.That(consignmentItem.TransportContractDocuments.Select(x => x.TypeCode), NUnit.Framework.Is.EqualTo(expected.Select(x => x.TypeCode)), "TransportContractDocument.TypeCode should be same.");
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.NewWithValidTestData<CusInBondHeader>();
			var arrivalBill = header.ArrivalBill;
			var movementBill = header.MovementBill;
			var moveHeader = header.MovementHeader;
			var moveDetail = moveHeader.InBondMoveDetail;
			var moveLine = moveDetail.InBondMoveLineItem;
			moveLine.BI_Quantity = 3m;
			moveLine.BI_QuantityUQ = "KG";
			consignmentItem = new ConsignmentItem(header);
		}

		CusInBondHeader header;
		IConsignmentItem consignmentItem;
	}
}
