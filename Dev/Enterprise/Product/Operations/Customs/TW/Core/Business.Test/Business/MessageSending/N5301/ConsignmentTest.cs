using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Business.N5301;
using Enterprise.Customs.TW.Messaging;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class ConsignmentTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestManifestSerialNumber()
		{
			NUnit.Framework.Assert.That(consignment.ManifestSerialNumber, NUnit.Framework.Is.EqualTo("X123").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestAdditionalInformations()
		{
			NUnit.Framework.Assert.That(consignment.AdditionalInformations, NUnit.Framework.Is.EqualTo(default(System.Collections.Generic.IEnumerable<IAdditionalInformation>)));
		}

		[ExpectNoExceptions]
		public void TestArrivalTransportMeansTypeCode()
		{
			NUnit.Framework.Assert.That(consignment.ArrivalTransportMeansTypeCode.ToString(), NUnit.Framework.Is.Null.Or.Empty);
		}

		[ExpectNoExceptions]
		public void TestBorderTransportMeans()
		{
			NUnit.Framework.Assert.That(consignment.BorderTransportMeans, NUnit.Framework.Is.EqualTo(default(ITransportMeans)));
		}

		[ExpectNoExceptions]
		public void TestCarrier()
		{
			NUnit.Framework.Assert.That(consignment.Carrier, NUnit.Framework.Is.EqualTo(default(IPartyDetails)));
		}

		[ExpectNoExceptions]
		public void TestConsignmentItem()
		{
			NUnit.Framework.Assert.That(consignment.ConsignmentItem, NUnit.Framework.Is.Not.EqualTo(default(IConsignmentItem)));
			NUnit.Framework.Assert.That(consignment.ConsignmentItem, NUnit.Framework.Is.TypeOf(typeof(ConsignmentItem)));
		}

		[ExpectNoExceptions]
		public void TestGoodsLocation()
		{
			NUnit.Framework.Assert.That(consignment.GoodsLocation.ToString(), NUnit.Framework.Is.Null.Or.Empty);
		}

		[ExpectNoExceptions]
		public void TestLoadingLocation()
		{
			NUnit.Framework.Assert.That(consignment.LoadingLocation.ID, NUnit.Framework.Is.EqualTo("TWTPE").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestTransportContractDocuments()
		{
			var bill = header.ArrivalBill;
			bill.B0_MasterBillNumber = "X123456789";
			bill.B0_HouseBillNumber = "X987654321";
			var expected = header.GetTransportContractDocuments(header.ArrivalBill, (id, typeCode) => new TransportContractDocumentWrapper(id, typeCode));
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(consignment.TransportContractDocuments.Select(x => x.ID), NUnit.Framework.Is.EqualTo(expected.Select(x => x.ID)), "TransportContractDocument.ID should be same.");
				NUnit.Framework.Assert.That(consignment.TransportContractDocuments.Select(x => x.TypeCode), NUnit.Framework.Is.EqualTo(expected.Select(x => x.TypeCode)), "TransportContractDocument.TypeCode should be same.");
			});
		}

		[ExpectNoExceptions]
		public void TestTransportEquipments()
		{
			var moveDetail = header.MovementHeader.InBondMoveDetail;
			var container1 = moveDetail.Containers.AddNew();
			container1.BC_ContainerNum = "1111";
			var container2 = moveDetail.Containers.AddNew();
			container2.BC_ContainerNum = "2222";
			NUnit.Framework.Assert.That(consignment.TransportEquipments.Count(), NUnit.Framework.Is.EqualTo(2));
		}

		[ExpectNoExceptions]
		public void TestBondedGoods()
		{
			NUnit.Framework.Assert.That(consignment.BondedGoods, NUnit.Framework.Is.EqualTo(default(IBondedGoods)));
		}

		[ExpectNoExceptions]
		public void TestShippingOrderNumber()
		{
			header.MovementBill.B0_ReferenceID = "MB12";
			NUnit.Framework.Assert.That(consignment.ShippingOrderNumber, NUnit.Framework.Is.EqualTo("MB12").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestDepartureTransportMeans()
		{
			NUnit.Framework.Assert.That(consignment.DepartureTransportMeans, NUnit.Framework.Is.Not.EqualTo(default(ITransportMeans)));
			NUnit.Framework.Assert.That(consignment.DepartureTransportMeans, NUnit.Framework.Is.TypeOf(typeof(DepartureTransportMeans)));
		}

		[ExpectNoExceptions]
		public void TestTransitTransportMeansTypeCode()
		{
			header.MovementHeader.BM_ExportTransportMode = TranshipmentTransportCodeList.Codes.SeaContainer;
			NUnit.Framework.Assert.That(consignment.TransitTransportMeansTypeCode, NUnit.Framework.Is.EqualTo(TranshipmentTransportCodeList.Codes.SeaContainer).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestGoodsLocations()
		{
			NUnit.Framework.Assert.That(consignment.GoodsLocations, NUnit.Framework.Is.EqualTo(default(System.Collections.Generic.IEnumerable<CargoWise.Types.ZString>)));
		}

		[ExpectNoExceptions]
		public void TestUnloadingLocation()
		{
			NUnit.Framework.Assert.That(consignment.UnloadingLocation.ID.ToString(), NUnit.Framework.Is.Null.Or.Empty);
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
			arrivalBill.B0_ReferenceID = "X123";
			header.BH_RL_NKImportLoadPort = "TWTPE";
			consignment = new Consignment(header);
		}

		CusInBondHeader header;
		IConsignment consignment;
	}
}
