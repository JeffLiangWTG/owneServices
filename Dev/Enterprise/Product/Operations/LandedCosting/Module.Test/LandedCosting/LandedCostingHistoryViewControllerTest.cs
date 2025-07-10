using System;
using Enterprise.LandedCosting.Business.Testing;
using Enterprise.LandedCosting.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;
using NUnit.Framework;

namespace Enterprise.LandedCosting.Module.Testing
{
	[TestedType(typeof(LandedCostingHistoryViewController))]
	sealed class LandedCostingHistoryViewControllerTest : LandedCostingControllerAbstractTest
	{
		public void TestGetPlugInType()
		{
			var dummyMaster = Factory.New<DummyLandedCostHistoryMaster>();
			var controller = new LandedCostingHistoryViewController();
			using (var tabControl = new ZTabControl())
			{
				var plugIns = new PlugIns(dummyMaster, tabControl);
				plugIns.Add(controller.ID);
				using (var plugIn = plugIns.GetPlugIn(ControllerIDs.LandedCostingHistoryView))
				{
					AssertType<LandedCostingHistoryViewPlugIn>(plugIn);
				}
			}
		}

		public override Type ControllerToBashType => typeof(LandedCostingHistoryViewController);

		protected override string CountryCode => "AU";

		protected override ControllerID GetControllerID() => ControllerIDs.LandedCostingHistoryView;

		protected override LandedCostingControllerBase GetControllerToTest() => new LandedCostingHistoryViewController();
	}
}
