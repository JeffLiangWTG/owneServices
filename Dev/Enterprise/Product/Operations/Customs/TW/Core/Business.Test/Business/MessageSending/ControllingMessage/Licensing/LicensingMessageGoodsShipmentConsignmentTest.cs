using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.TW;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(LicensingMessageGoodsShipmentConsignment))]
	sealed class LicensingMessageGoodsShipmentConsignmentTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestData()
		{
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Code = "TESTVESSEL";
			vessel.RV_LloydsNumber = "9143245";
			vessel.RV_RadioCallSign = "KBBL";

			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_ContainerMode = ContainerModeList.Codes.BreakBulk;
			declaration.JE_RL_NKFinalDestination = "ESMAD";
			declaration.JE_VesselName = "TESTVESSEL";
			declaration.JE_RL_NKOrigin = "TWKEL";
			declaration.JE_SLD = "ZQ2";

			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(consignment.LoadingLocation.ID, NUnit.Framework.Is.EqualTo("TWKEL").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(consignment.ManifestSerialNumber, NUnit.Framework.Is.EqualTo("ZQ2").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(consignment.ArrivalTransportMeansTypeCode, NUnit.Framework.Is.EqualTo(TransportCodeList.Codes.SeaPackedSundryGoods).Using(CustomComparers.TypeComparison), "ArrivalTransportMeans.TypeCode");
				NUnit.Framework.Assert.That(consignment.DepartureTransportMeans, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Messaging.ITransportMeans)), "DepartureTransportMeans - should be [null]");
				NUnit.Framework.Assert.That(consignment.UnloadingLocation, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Messaging.ILocation)), "UnloadingLocation - should be [null]");
			});
		}

		[ExpectNoExceptions]
		public void TestAdditionalInformations()
		{
			var reservedFields1 = declaration.ReservedFields.AddNew();
			reservedFields1.CY_Code = "A";
			reservedFields1.CY_Data = "A1";

			var reservedFields2 = declaration.ReservedFields.AddNew();
			reservedFields2.CY_Code = "B";
			reservedFields2.CY_Data = "B1";
			var additionalInformations = consignment.AdditionalInformations;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(additionalInformations.Count(), NUnit.Framework.Is.EqualTo(2));
				NUnit.Framework.Assert.That(additionalInformations.Any(x => x.StatementCode == "A" && x.StatementDescription == "A1"), NUnit.Framework.Is.True);
				NUnit.Framework.Assert.That(additionalInformations.Any(x => x.StatementCode == "B" && x.StatementDescription == "B1"), NUnit.Framework.Is.True);
			});
		}

		[ExpectNoExceptions]
		public void TestTranshipmentLocation()
		{
			var leg1 = declaration.Transports.AddNew();
			leg1.JW_ETD = new ZDateTime(2023, 3, 2);
			leg1.JW_RL_NKLoadPort = "TWTPE";
			leg1.JW_ETA = new ZDateTime(2023, 3, 4);
			leg1.JW_RL_NKDiscPort = "TWKEL";

			var leg2 = declaration.Transports.AddNew();
			leg2.JW_LegOrder = 2;
			leg2.JW_ETD = new ZDateTime(2023, 3, 4);
			leg2.JW_RL_NKLoadPort = "TWKEL";
			leg2.JW_ETA = new ZDateTime(2023, 3, 5);
			leg2.JW_RL_NKDiscPort = "CNSHA";

			var leg3 = declaration.Transports.AddNew();
			leg3.JW_LegOrder = 3;
			leg3.JW_ETD = new ZDateTime(2023, 3, 5);
			leg3.JW_RL_NKLoadPort = "CNSHA";
			leg3.JW_ETA = new ZDateTime(2023, 3, 6);
			leg3.JW_RL_NKDiscPort = "JPTYO";

			NUnit.Framework.Assert.That(consignment.TranshipmentLocation.ID, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestTransitDeparture()
		{
			var leg1 = declaration.Transports.AddNew();
			leg1.JW_LegOrder = 3;
			leg1.JW_ETD = new ZDateTime(2023, 3, 5);
			leg1.JW_RL_NKLoadPort = "JPTYO";
			leg1.JW_ETA = new ZDateTime(2023, 3, 6);
			leg1.JW_RL_NKDiscPort = "CNSHA";

			var leg2 = declaration.Transports.AddNew();
			leg2.JW_LegOrder = 2;
			leg2.JW_ETD = new ZDateTime(2023, 3, 4);
			leg2.JW_RL_NKLoadPort = "CNSHA";
			leg2.JW_ETA = new ZDateTime(2023, 3, 5);
			leg2.JW_RL_NKDiscPort = "TWKEL";

			var leg3 = declaration.Transports.AddNew();
			leg3.JW_ETD = new ZDateTime(2023, 3, 2);
			leg3.JW_RL_NKLoadPort = "TWKEL";
			leg3.JW_ETA = new ZDateTime(2023, 3, 4);
			leg3.JW_RL_NKDiscPort = "TWTPE";

			NUnit.Framework.Assert.That(consignment.TransitDeparture.ID, NUnit.Framework.Is.EqualTo("CNSHA").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestTransportContractDocuments()
		{
			declaration.JE_TransportMode = "AIR";
			declaration.JE_MasterBill = "M01";
			declaration.JE_HouseBill = "H01";
			CombineAssertions("AIR", () =>
			{
				NUnit.Framework.Assert.That(consignment.TransportContractDocuments.Count(), NUnit.Framework.Is.EqualTo(2), "Count");
				NUnit.Framework.Assert.That(consignment.TransportContractDocuments.Any(x => x.TypeCode == "741" && x.ID == "M01"), NUnit.Framework.Is.True, "Type: 741");
				NUnit.Framework.Assert.That(consignment.TransportContractDocuments.Any(x => x.TypeCode == "703" && x.ID == "H01"), NUnit.Framework.Is.True, "Type: 703");
			});

			declaration.JE_TransportMode = "SEA";
			CombineAssertions("SEA", () =>
			{
				NUnit.Framework.Assert.That(consignment.TransportContractDocuments.Count(), NUnit.Framework.Is.EqualTo(2), "Count");
				NUnit.Framework.Assert.That(consignment.TransportContractDocuments.Any(x => x.TypeCode == "704" && x.ID == "M01"), NUnit.Framework.Is.True, "Type: 704");
				NUnit.Framework.Assert.That(consignment.TransportContractDocuments.Any(x => x.TypeCode == "714" && x.ID == "H01"), NUnit.Framework.Is.True, "Type: 714");
			});
		}

		public void TestTransportEquipments()
		{
			var cusContainer = declaration.CusContainers.AddNew();
			cusContainer.CO_ContainerNumber = "UUUU1234567";
			cusContainer.CO_IsPart = true;
			cusContainer.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;
			cusContainer.CO_Seal = "Seal1";
			cusContainer.CO_SecondSeal = "Seal2";
			var refContainer = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP");
			cusContainer.CO_RC = refContainer.PK;

			var cusContainer2 = declaration.CusContainers.AddNew();
			cusContainer2.CO_ContainerNumber = "UUUU1234568";

			CombineAssertions(() =>
			{
				var transportEquipments = consignment.TransportEquipments;
				var transportEquipment = transportEquipments.First();
				NUnit.Framework.Assert.That(transportEquipments.Count(), NUnit.Framework.Is.EqualTo(2), "Count");
				NUnit.Framework.Assert.That(transportEquipment.CharacteristicCode, NUnit.Framework.Is.EqualTo("40GP").Using(CustomComparers.TypeComparison), "CharacteristicCode");
				NUnit.Framework.Assert.That(transportEquipment.UsedCapacityCode, NUnit.Framework.Is.EqualTo("5").Using(CustomComparers.TypeComparison), "UsedCapacityCode");
				AssertContainsExactElementsInExactOrder(new[] { "SEAL1", "SEAL2" }, transportEquipment.Seals);

				AssertContainsExactElementsInExactOrder("IDs", new[] { "UUUU1234567", "UUUU1234568" }, transportEquipments.Select(x => x.ID));
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.NewWithValidTestData<JobDeclaration>();
			header = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			consignment = new LicensingMessageGoodsShipmentConsignment(header);
		}

		JobDeclaration declaration;
		CusTWControllingMessageHeader header;
		LicensingMessageGoodsShipmentConsignment consignment;
	}
}
