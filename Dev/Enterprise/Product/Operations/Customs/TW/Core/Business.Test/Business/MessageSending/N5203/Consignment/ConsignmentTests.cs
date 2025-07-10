using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TW.Business.N5203;
using Enterprise.Customs.TW.Messaging;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class ConsignmentTests : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestManifestSerialNumber()
		{
			NUnit.Framework.Assert.That(Consignment.ManifestSerialNumber, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestAdditionalInformations()
		{
			TestTWCreator.AddDeclarationReservedFields(Declaration);
			NUnit.Framework.Assert.That(Consignment.AdditionalInformations, NUnit.Framework.Is.Not.EqualTo(default(System.Collections.Generic.IEnumerable<Enterprise.Customs.TW.Messaging.IAdditionalInformation>)));
			SharedHelperTest.AssertAdditionalInformations(Consignment.AdditionalInformations);
		}

		[ExpectNoExceptions]
		public void TestArrivalTransportMeansTypeCode()
		{
			NUnit.Framework.Assert.That(Consignment.ArrivalTransportMeansTypeCode, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestBorderTransportMeans()
		{
			NUnit.Framework.Assert.That(Consignment.BorderTransportMeans.GetType(), NUnit.Framework.Is.EqualTo(typeof(BorderTransportMeans)));
		}

		[ExpectNoExceptions]
		public void TestCarrier()
		{
			NUnit.Framework.Assert.That(Consignment.Carrier, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Messaging.IPartyDetails)));
		}

		[ExpectNoExceptions]
		public void TestConsignmentItem()
		{
			NUnit.Framework.Assert.That(Consignment.ConsignmentItem, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Messaging.IConsignmentItem)));
		}

		[ExpectNoExceptions]
		public void TestGoodsLocation()
		{
			NUnit.Framework.Assert.That(Consignment.GoodsLocation, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestLoadingLocation()
		{
			Declaration.JE_RL_NKOrigin = "TWKEL";
			NUnit.Framework.Assert.That(Consignment.LoadingLocation.ID, NUnit.Framework.Is.EqualTo("TWKEL").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestTransportContractDocuments()
		{
			Declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			Declaration.JE_MasterBill = "69517920011";
			EntryInstruction.CEI_Style = Constants.DeclarationTypes.Export.B2;
			NUnit.Framework.Assert.That(Consignment.TransportContractDocuments.Any(x => x.TypeCode == "741" && x.ID == "NIL"), NUnit.Framework.Is.True, "ShouldSendNILAsMasterBill is True and JE_TransportMode is Air");
			Declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			EntryInstruction.CEI_Style = Constants.DeclarationTypes.Export.F5;
			NUnit.Framework.Assert.That(Consignment.TransportContractDocuments.Any(x => x.TypeCode == "704" && x.ID == Declaration.JE_MasterBill), NUnit.Framework.Is.True, "ShouldSendNILAsMasterBill is False and JE_TransportMode is Sea");
			Declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			NUnit.Framework.Assert.That(Consignment.TransportContractDocuments.Any(x => x.TypeCode == "741" && x.ID == "695-17920011"), NUnit.Framework.Is.True, "ShouldSendNILAsMasterBill is False and JE_TransportMode is Air");
			Declaration.JE_HouseBill = "HH111111";
			NUnit.Framework.Assert.That(Consignment.TransportContractDocuments.Any(x => x.TypeCode == "703" && x.ID == Declaration.JE_HouseBill), NUnit.Framework.Is.True);
			Declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			NUnit.Framework.Assert.That(Consignment.TransportContractDocuments.Any(x => x.TypeCode == "714" && x.ID == Declaration.JE_HouseBill), NUnit.Framework.Is.True);
			var cnBill1 = Declaration.Bills.AddNew();
			cnBill1.CU_BillType = BillTypeList.Codes.ContainerNote;
			cnBill1.CU_BillNum = "CN00001";
			NUnit.Framework.Assert.That(Consignment.TransportContractDocuments.Any(x => x.TypeCode == "976" && x.ID == cnBill1.CU_BillNum), NUnit.Framework.Is.True);
			var cnBill2 = Declaration.Bills.AddNew();
			cnBill2.CU_BillType = BillTypeList.Codes.ContainerNote;
			cnBill2.CU_BillNum = "CN00002";
			NUnit.Framework.Assert.That(Consignment.TransportContractDocuments.Any(x => x.TypeCode == "976" && x.ID == cnBill2.CU_BillNum), NUnit.Framework.Is.True);
		}

		[ExpectNoExceptions]
		public void TestTransportEquipments()
		{
			var container = Declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "UUUU1234567";
			NUnit.Framework.Assert.That(Consignment.TransportEquipments.Count(), NUnit.Framework.Is.EqualTo(1));
			container = Declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "UUUU1234568";
			NUnit.Framework.Assert.That(Consignment.TransportEquipments.Count(), NUnit.Framework.Is.EqualTo(2));
		}

		[ExpectNoExceptions]
		public void TestBondedGoods()
		{
			NUnit.Framework.Assert.That(Consignment.BondedGoods.GetType(), NUnit.Framework.Is.EqualTo(typeof(BondedGoods)));
		}

		[ExpectNoExceptions]
		public void TestShippingOrderNumber()
		{
			Declaration.JE_SLD = "XX";
			NUnit.Framework.Assert.That(Consignment.ShippingOrderNumber, NUnit.Framework.Is.EqualTo(Declaration.JE_SLD));
		}

		[ExpectNoExceptions]
		public void TestDepartureTransportMeans()
		{
			NUnit.Framework.Assert.That(Consignment.DepartureTransportMeans.GetType(), NUnit.Framework.Is.EqualTo(typeof(DepartureTransportMeans)));
		}

		[ExpectNoExceptions]
		public void TestTransitTransportMeansTypeCode()
		{
			NUnit.Framework.Assert.That(Consignment.TransitTransportMeansTypeCode, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestGoodsLocations()
		{
			EntryInstruction.CEI_GoodsLocation = "ANP0060D";
			Declaration.JE_LocationOfGoods = "ANP0060D";
			var goodsLocations = Consignment.GoodsLocations;
			NUnit.Framework.Assert.That(goodsLocations.Count(), NUnit.Framework.Is.EqualTo(1));
			NUnit.Framework.Assert.That(goodsLocations.First(), NUnit.Framework.Is.EqualTo("ANP0060D").Using(CustomComparers.TypeComparison));
			EntryInstruction.CEI_GoodsLocation = null;
			Declaration.JE_LocationOfGoods = "";
			goodsLocations = Consignment.GoodsLocations;
			NUnit.Framework.Assert.That(goodsLocations.Count(), NUnit.Framework.Is.EqualTo(1));
			NUnit.Framework.Assert.That(goodsLocations.First(), NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
			EntryInstruction.CEI_GoodsLocation = "ANP0060D";
			Declaration.JE_LocationOfGoods = "XX1";
			goodsLocations = Consignment.GoodsLocations;
			NUnit.Framework.Assert.That(goodsLocations.Count(), NUnit.Framework.Is.EqualTo(2));
			NUnit.Framework.Assert.That(goodsLocations.Any(x => x.Equals("ANP0060D")), NUnit.Framework.Is.True);
			NUnit.Framework.Assert.That(goodsLocations.Any(x => x.Equals("XX1")), NUnit.Framework.Is.True);
			Declaration.JE_LocationOfGoods = "ANP0060D";
			goodsLocations = Consignment.GoodsLocations;
			NUnit.Framework.Assert.That(goodsLocations.Count(), NUnit.Framework.Is.EqualTo(1));
			NUnit.Framework.Assert.That(goodsLocations.Any(x => x.Equals("ANP0060D")), NUnit.Framework.Is.True);
			Declaration.JE_LocationOfGoods = "XX1";
			EntryInstruction.CEI_GoodsLocation = ZString.Empty;
			goodsLocations = Consignment.GoodsLocations;
			NUnit.Framework.Assert.That(goodsLocations.Count(), NUnit.Framework.Is.EqualTo(2));
			NUnit.Framework.Assert.That(goodsLocations.First(), NUnit.Framework.Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(goodsLocations.ElementAt(1), NUnit.Framework.Is.EqualTo("XX1").Using(CustomComparers.TypeComparison));
			Declaration.JE_LocationOfGoods = ZString.Empty;
			EntryInstruction.CEI_GoodsLocation = "XX9";
			goodsLocations = Consignment.GoodsLocations;
			NUnit.Framework.Assert.That(goodsLocations.Count(), NUnit.Framework.Is.EqualTo(1));
			NUnit.Framework.Assert.That(goodsLocations.First(), NUnit.Framework.Is.EqualTo("XX9").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestUnloadingLocation()
		{
			Declaration.JE_RL_NKFinalDestination = "TWKEL";
			NUnit.Framework.Assert.That(Consignment.UnloadingLocation.ID, NUnit.Framework.Is.EqualTo(Declaration.JE_RL_NKFinalDestination));
		}

		protected override void SetUp()
		{
			base.SetUp();
			var testHelper = new TestTWCreator(Factory);
			entryHeader = testHelper.CreateEntryHeaderForN5203();
		}

		CusEntryHeader entryHeader;
		IConsignment Consignment => new Consignment(entryHeader);
		JobDeclaration Declaration => entryHeader.Declaration;
		CusEntryInstruction EntryInstruction => entryHeader.EntryInstruction;
	}
}
