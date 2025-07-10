using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.TransportConsignment.Business.Testing
{
	[TestedType(typeof(DtbLinehaulManifest))]
	sealed class DtbLinehaulManifestTest : EnterpriseBusinessObjectTestCase
	{
		public void TestAddRemovePackage_UsesCorrectEvent()
		{
			var consignment = Helper.CreateBookingConsignment();
			var package1 = Helper.CreatePackage(consignment, 10m, 10m);
			package1.KP_PackageID = "ALEE";
			Factory.Save();

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.MainAddress.OA_City = "Mumbai";

			var manifest = Factory.New<DtbLinehaulManifest>();
			manifest.LHM_ManifestID = "MANIFEST1";
			manifest.LHM_OA_OriginDepot = orgHeader.MainAddress.PK;

			manifest.PackageToAdd = package1.KP_PackageID;
			var result = manifest.AddPackage();
			AssertEquals(ZString.Empty, result);
			AssertEquals(1, manifest.Packages.Count);
			var recentLog = manifest.Packages[0].Logs.MostRecentLog;
			AssertEquals(Events.FreightLoadedCode, recentLog.SL_SE_NKEvent);

			manifest.RemovePackage(package1);
			AssertEquals(true, recentLog.SL_IsCancelled);
		}

		public void TestIJobCostingPlugIn()
		{
			var manifest = Factory.New<DtbLinehaulManifest>();
			manifest.LHM_ManifestID = "96712065";

			IJobCostingPlugIn costingPlugin = manifest;
			AssertEquals(null, costingPlugin.ConsolCurrency);
			AssertEquals(0m, costingPlugin.ConsolExchangeRate);
			AssertEquals(null, costingPlugin.DischargePort);
			AssertEquals(0m, costingPlugin.ExchangeRateForCurrency(null, ZGuid.Empty));
			AssertEquals(false, costingPlugin.IsMasterCollect);
			AssertEquals("96712065", costingPlugin.JK_UniqueConsignRef);
			AssertEquals(null, costingPlugin.LoadPort);
			AssertEquals(null, costingPlugin.ProfitLossContainer);
			AssertEquals(null, costingPlugin.ReceivingAgent);
			AssertEquals(null, costingPlugin.ReceivingAgentAPInvoicingParty);
			AssertEquals(null, costingPlugin.ReceivingAgentARInvoicingParty);
			AssertEquals(null, costingPlugin.SendingAgent);
			AssertEquals(null, costingPlugin.SendingAgentAPInvoicingParty);
			AssertEquals(null, costingPlugin.SendingAgentAPInvoicingParty);
			AssertEquals(ZString.Empty, costingPlugin.TransportMode);
			AssertEquals(ZString.Empty, costingPlugin.ContainerMode);
			AssertEquals(ZString.Empty, costingPlugin.ConsolType);
			AssertEquals(ApportionmentMethodModules.TransportBooking, costingPlugin.Module);
			AssertEquals(ZString.Empty, costingPlugin.Direction);
			AssertEquals(ZString.Empty, costingPlugin.GetPrepaidCollect(null));
			AssertEquals(new CodeDescriptionPairList(), costingPlugin.PrepaidCollectList);
			AssertEquals(typeof(DtbLinehaulManifestCostSupporter), costingPlugin.CostSupporter.GetType());
		}

		#region CarrierServiceLevel

		public void TestCarrierServiceLevelNoExceptionFromNonUniqueCode()
		{
			var manifest = Factory.New<DtbLinehaulManifest>();
			manifest.LHM_PL_NKCarrierServiceLevel = "ABC";

			var serviceLevel = Factory.New<OrgCarrierServiceLevel>();
			serviceLevel.PL_Code = "ABC";

			var serviceLevel2 = Factory.New<OrgCarrierServiceLevel>();
			serviceLevel2.PL_Code = "ABC";

			AssertNoExceptionThrown("Accessing CarrierServiceLevel should not throw exception", () => { _ = manifest.CarrierServiceLevel; });
		}

		#endregion

		#region Implementation

		TransportBookingConsignmentTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingConsignmentTestHelper(Factory)); }
		}

		TransportBookingConsignmentTestHelper helper;

		#endregion

	}
}
