using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.SG.V4.Business;
using Enterprise.Customs.SG.V4.GUI;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Module.Testing
{
	[TestedType(typeof(JobDeclarationController))]
	sealed class JobDeclarationControllerTest : Customs.Module.Testing.JobDeclarationControllerTestCase
	{
		public void TestGetPlugin()
		{
			var controller = new JobDeclarationControllerTestClass();
			var shipment = Factory.New<ForwardingShipment>();
			var otherCountryDeclaration = Factory.New<JobDeclaration>();
			otherCountryDeclaration.JE_JS = shipment.PK;
			Factory.Save();
			using (var plugin = controller.GetPlugIn(shipment))
			{
				Assert("If only declarations on a shipment are non SG declarations, plugin should recognise a New TradeNet4 declaration is required", plugin is BrokeragePlugIn);
			}

			shipment = Factory.New<ForwardingShipment>();
			var sgDeclaration = Factory.New<JobDeclaration>();
			sgDeclaration.JE_JS = shipment.PK;
			sgDeclaration.JE_GB = Factory.LoadFromNaturalKey<GlbBranch>(GlbBranchSchema.GB_Code, "SIN").PK;
			Factory.Save();
			using (var plugin = controller.GetPlugIn(shipment))
			{
				Assert("When an SG TN4 declaration exists, plugin should recognise the existing TN4 declaration", plugin is BrokeragePlugIn);
			}

			sgDeclaration.JE_ApplicationCode = "";
			Factory.Save();
			using (var plugin = controller.GetPlugIn(shipment))
			{
				Assert("For non TN4 SG declarations plugin should be for old V3 decs", plugin is V3.GUI.V3BrokeragePlugin);
			}

			sgDeclaration.JE_ApplicationCode = "ITF";
			Factory.Save();
			using (var plugin = controller.GetPlugIn(shipment))
			{
				Assert("For ITF SG declarations plugin should be for TN4 declaration", plugin is BrokeragePlugIn);
			}
		}

		public override Type ControllerToBashType => typeof(JobDeclarationController);

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.JobDeclaration;

		protected override bool CountryHasExWarehouse => false;

		sealed class JobDeclarationControllerTestClass : JobDeclarationController
		{
			public new ZPlugIn GetPlugIn(IBusiness businessEntity) => base.GetPlugIn(businessEntity);
		}
	}
}
