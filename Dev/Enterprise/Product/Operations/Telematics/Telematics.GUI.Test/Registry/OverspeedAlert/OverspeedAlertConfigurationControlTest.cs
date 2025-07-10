using CargoWise.EntityFramework;
using Enterprise.Registry.GUI;
using Enterprise.Telematics.Business.Registry;
using Enterprise.Telematics.GUI.Registry;
using NUnit.Framework;

namespace Enterprise.Telematics.GUI.Test
{
	[TestedType(typeof(OverspeedAlertConfigurationControl))]
	class OverspeedAlertConfigurationControlTest : Enterprise.Registry.GUI.Testing.RegistryZUserControlTestCase
	{
		public void TestVisiblityOfControls()
		{
			using (var dummyForm = new System.Windows.Forms.Form())
			using (var control = new OverspeedAlertConfigurationControl())
			{
				dummyForm.Controls.Add(control);
				dummyForm.Show();
				var entity = GetNewBusinessEntity();
				control.SetDataBinding(entity, null);
				AssertEquals(((OverspeedAlertConfiguration)entity).IsOverspeedAlertTypeShownForX, true);
				ZArchitecture.GUI.ZDropEdit overspeedAlertType = (ZArchitecture.GUI.ZDropEdit)control.Controls["overspeedAlertType"];
				overspeedAlertType.Select();
				overspeedAlertType.SelectItem("ALY");
				overspeedAlertType.CommitBoundValue();
				AssertEquals(((OverspeedAlertConfiguration)entity).IsOverspeedAlertTypeShownForX, false);
			}
		}

		protected override IBusiness GetNewBusinessEntity()
		{
			return new OverspeedAlertConfiguration(null, Factory);
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((OverspeedAlertConfigurationControl)control).ReadOnly;
		}
	}
}
