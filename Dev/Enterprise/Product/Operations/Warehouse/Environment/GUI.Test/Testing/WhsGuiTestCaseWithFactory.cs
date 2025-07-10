using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.ZArchitecture;

namespace Enterprise.Warehouse.Environment.GUI.Testing
{
	public abstract class WhsGuiTestCaseWithFactory : WhsTestCaseWithFactoryEnv
	{
		#region Constructors

		public WhsGuiTestCaseWithFactory()
		{
		}

		#endregion

		#region Assert Binding

		public static void AssertBinding(Control control, string propertyName, string bindingMemberName)
		{
			KBinding binding = null;
			foreach (KBinding b in control.DataBindings)
			{
				if (b.PropertyName == propertyName)
				{
					binding = b;
					break;
				}
			}
			string name = control.Text + "." + propertyName + "." + bindingMemberName;
			AssertNotNull("Data Binding not found for " + name, binding);
			AssertEquals("Binding Member not found for " + name, bindingMemberName, binding.BindingMemberInfo.BindingMember);
		}

		#endregion

		#region FindMenuItem

		protected MenuItem FindMenuItem(ZGrid grid, string itemName)
		{
			MenuItem result = null;
			foreach (MenuItem item in grid.ContextMenu.MenuItems)
			{
				if (item.Text == itemName)
				{
					result = item;
					break;
				}
			}
			return result;
		}

		#endregion
	}
}
