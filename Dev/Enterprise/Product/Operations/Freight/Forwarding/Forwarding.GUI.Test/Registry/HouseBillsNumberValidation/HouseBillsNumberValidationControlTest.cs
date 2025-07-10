using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Registry;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	[TestedType(typeof(HouseBillsNumberValidationControl))]
	class HouseBillsNumberValidationControlTest : Enterprise.Registry.GUI.Testing.RegistryZUserControlTestCase
	{
		public void TestUserDefinedConditionColumn()
		{
			var collection = new HouseBillsNumberValidationCollection();

			using (Form dummyForm = new Form())
			using (HouseBillsNumberValidationControl control = new HouseBillsNumberValidationControl())
			{
				control.SetDataBinding(collection, "");

				dummyForm.Controls.Add(control);
				dummyForm.Show();

				ZMacrosFindBoxColumnStyleInfo macroColumn = (ZMacrosFindBoxColumnStyleInfo)control.HouseBillsNumberValidationGrid.ColumnStyles[control.HouseBillsNumberValidationGrid.ColumnStyles.Count - 1];
				AssertEquals(typeof(ZMacrosFindBoxColumnStyleInfo), macroColumn.GetType());
				AssertEquals("UserDefinedCondition", macroColumn.ColumnName);
			}
		}

		#region Implementation

		protected override IBusiness GetNewBusinessEntity()
		{
			return new HouseBillsNumberValidationCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((HouseBillsNumberValidationControl)control).ReadOnly;
		}

		#endregion
	}
}
