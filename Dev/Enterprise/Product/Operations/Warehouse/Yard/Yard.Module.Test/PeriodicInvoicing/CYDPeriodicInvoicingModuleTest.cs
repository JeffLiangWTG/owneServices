using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.Warehouse.Invoicing.Module.Test;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.Module.Test
{
	[TestedType(typeof(CYDPeriodicInvoicingModule))]
	public class CYDPeriodicInvoicingModuleTest : PeriodicInvoicingModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.CYDPeriodicInvoicing;
		}

		protected override LicenceCheckpoint GetLicenceCheckpoint()
		{
			return Env.Licence.ContainerYard;
		}

		protected override SecurityCheckpoint GetSecurityCheckpoint()
		{
			return Env.Security.CYDPeriodicInvoicing;
		}

		#region TestSecurityCheckPoint_Message

		protected override void ShowNewFormForTest()
		{
			Env.Security.CYDPeriodicInvoicingNew.IsAllowed = false;
			using (var module = new TestInvoicingModule())
			{
				module.ShowNewForm();
			}
		}

		class TestInvoicingModule : CYDPeriodicInvoicingModule
		{
			public new IZForm ShowNewForm()
			{
				return base.ShowNewForm();
			}
		}

		#endregion
	}
}
