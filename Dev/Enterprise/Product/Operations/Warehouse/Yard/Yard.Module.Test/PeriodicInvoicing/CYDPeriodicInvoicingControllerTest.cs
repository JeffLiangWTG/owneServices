using Enterprise.Warehouse.Invoicing.Module;
using Enterprise.Warehouse.Invoicing.Module.Test;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.Module.Test
{
	[TestedType(typeof(CYDPeriodicInvoicingController))]
	public class CYDPeriodicInvoicingControllerTest : PeriodicInvoicingControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.CYDPeriodicInvoicing;
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.CYDPeriodicInvoicing;
		}

		protected override PeriodicInvoicingController GetPeriodicInvoicingController()
		{
			return new CYDPeriodicInvoicingController();
		}
	}
}
