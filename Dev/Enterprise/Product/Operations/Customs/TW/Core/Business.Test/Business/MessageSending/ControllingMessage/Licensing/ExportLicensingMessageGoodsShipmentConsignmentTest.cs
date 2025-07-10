using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.TW;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(ExportLicensingMessageGoodsShipmentConsignment))]
	sealed class ExportLicensingMessageGoodsShipmentConsignmentTest : TestCaseWithFactory
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

			consignment = new ExportLicensingMessageGoodsShipmentConsignment(header);
			CombineAssertions("Export", () =>
			{
				NUnit.Framework.Assert.That(consignment.ArrivalTransportMeansTypeCode.ToString(), NUnit.Framework.Is.Null.Or.Empty, "ArrivalTransportMeans - should be [null] or [empty]");
				NUnit.Framework.Assert.That(consignment.DepartureTransportMeans.ID, NUnit.Framework.Is.EqualTo("KBBL").Using(CustomComparers.TypeComparison), "DepartureTransportMeans.ID");
				NUnit.Framework.Assert.That(consignment.DepartureTransportMeans.TypeCode, NUnit.Framework.Is.EqualTo(TransportCodeList.Codes.SeaPackedSundryGoods).Using(CustomComparers.TypeComparison), "DepartureTransportMeans.TypeCode");
				NUnit.Framework.Assert.That(consignment.UnloadingLocation.ID, NUnit.Framework.Is.EqualTo("ESMAD").Using(CustomComparers.TypeComparison), "UnloadingLocation.ID");
				NUnit.Framework.Assert.That(consignment.ArrivalTransportMeansTypeCode.ToString(), NUnit.Framework.Is.Null.Or.Empty, "ArrivalTransportMeans.TypeCode - should be [null] or [empty]");
				NUnit.Framework.Assert.That(consignment.TransitDeparture.ID.ToString(), NUnit.Framework.Is.Null.Or.Empty, "TransitDeparture - should be [null] or [empty]");
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			header = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			consignment = new ExportLicensingMessageGoodsShipmentConsignment(header);
		}

		JobDeclaration declaration;
		CusTWControllingMessageHeader header;
		ExportLicensingMessageGoodsShipmentConsignment consignment;
	}
}
