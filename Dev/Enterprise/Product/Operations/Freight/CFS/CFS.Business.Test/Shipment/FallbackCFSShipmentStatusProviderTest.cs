using System.Threading;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Moq;

namespace Enterprise.Freight.CFS.Business.Testing
{
	public class FallbackCFSShipmentStatusProviderTest : TestCaseWithFactory
	{
		public void TestDetailsFromMessagesCore()
		{
			var shipment = Factory.New<GatePassShipment>();
			var provider = new FallbackCFSShipmentStatusProvider(shipment);
			Assert(provider.DetailsFromMessages.IsEmpty);
		}

		public void TestCanSaveOrPrint()
		{
			AssertEquals("precondition", Constants.CountryCodes.Australia, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

			var shipment = Factory.New<GatePassShipment>();
			var mainProvider = new DummyCFSShipmentStatusProvider(shipment);
			CFSShipmentStatusProvider.SetDummyForTest(mainProvider);
			var provider = new FallbackCFSShipmentStatusProvider(shipment);

			shipment.Logs.AddNew(Events.SeaCargoDepotEvent, OldCFSShipmentStatusProvider.CMRGatePassStatuses.Held);
			var mock = new Mock<ISaveAndPrintUI>();
			mock.Setup(m => m.Ask(It.IsAny<string>())).Returns(false);
			AssertEquals(false, provider.CanSaveAndPrint(mock.Object));

			mainProvider.ShortStatusExposed = CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased;
			mainProvider.CanSaveAndPrintExposed = true;
			AssertEquals(true, provider.CanSaveAndPrint(mock.Object));

			mainProvider.ShortStatusExposed = ZString.Empty;
			AssertEquals(false, provider.CanSaveAndPrint(mock.Object));

			Thread.Sleep(200);

			shipment.Logs.AddNew(Events.SeaCargoDepotEvent, OldCFSShipmentStatusProvider.CMRGatePassStatuses.Clear);
			AssertEquals(true, provider.CanSaveAndPrint(mock.Object));

			Thread.Sleep(200);

			shipment.Logs.AddNew(Events.SeaCargoDepotEvent, OldCFSShipmentStatusProvider.CMRGatePassStatuses.Held);
			AssertEquals(false, provider.CanSaveAndPrint(mock.Object));
		}

		public void TestCanSaveOrPrintNotAU()
		{
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = "NZ";
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "NZAKL";
			GlbBranch.CurrentBranch.GB_Code = "AKL";
			GlbDepartment.CurrentDepartment.GE_Code = "AKL";
			AssertEquals("precondition", Constants.CountryCodes.NewZealand, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

			var shipment = Factory.New<GatePassShipment>();
			var provider = new FallbackCFSShipmentStatusProvider(shipment);
			var mock = new Mock<ISaveAndPrintUI>();
			mock.Setup(m => m.Ask(It.IsAny<string>())).Returns(false);
			AssertEquals(true, provider.CanSaveAndPrint(mock.Object));
		}
	}
}
