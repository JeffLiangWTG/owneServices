using System;
using CargoWise.Application;
using Enterprise.LandedCosting.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;
using NUnit.Framework;

namespace Enterprise.LandedCosting.Module.Testing
{
	[TestedType(typeof(LandedCostingController))]
	sealed class LandedCostingControllerTest : LandedCostingControllerAbstractTest
	{
		public void TestGetPlugInType()
		{
			var declaration = Factory.New(ObjectFactory.GetType<Integration.Customs.IBaseJobDeclaration>());
			var controller = new LandedCostingController();
			using (var tabControl = new ZTabControl())
			{
				var plugIns = new PlugIns(declaration, tabControl);
				plugIns.Add(controller.ID);
				using (var plugIn = plugIns.GetPlugIn(ControllerIDs.LandedCosting))
				{
					AssertType<LandedCostingPlugIn>(plugIn);
				}
			}
		}

		public override Type ControllerToBashType => typeof(LandedCostingController);

		protected override string CountryCode => "AU";

		protected override ControllerID GetControllerID() => ControllerIDs.LandedCosting;

		protected override LandedCostingControllerBase GetControllerToTest() => new LandedCostingController();
	}
}
