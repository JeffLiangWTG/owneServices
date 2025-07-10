using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Definitions;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(GlbDepartmentForm))]
	sealed class GlbDepartmentFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			GlbDepartment dept = Factory.New<GlbDepartment>();
			return new GlbDepartmentForm(dept);
		}

		public void TestCostCentreCheckBoxVisibility()
		{
			var allClientTypes = Enum.GetValues(typeof(Clients)).Cast<Clients>();

			foreach (var clientType in allClientTypes)
			{
				EnvProxy.Instance.Registry.ExpectedClientDLL = null;

				using (ClientHookLoader.Instance.OverrideClientAssemblyForTest(clientType))
				{
					using (var form = (GlbDepartmentForm)GetFormToBashCore())
					{
						form.Show();

						var isVisible = (clientType == Clients.EDI);
						AssertEquals($"IsCostCentreCheckBox's Visible value should be {isVisible} for client type {clientType}", (clientType == Clients.EDI), form.IsCostCentreCheckBoxVisible);
					}
				}
			}
		}
	}
}
