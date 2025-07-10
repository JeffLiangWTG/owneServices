using System;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.GUI.Testing
{
	[TestedType(typeof(AllowSendingBookingConfirmationControl))]
	public class AllowSendingBookingConfirmationControlTest : RegistryZUserControlTestCase
	{
		public void TestPrincipalGridRemoveAction()
		{
			using (var allowSendingBookingConfirmationControl = new AllowSendingBookingConfirmationControl())
			{
				var principalGrid = allowSendingBookingConfirmationControl.Controls.Find("PrincipalGrid", true).First() as ZGrid;

				AssertEquals(RemoveAction.RemoveAndDelete, principalGrid.RemoveAction);
			}
		}

		#region Implementation

		T GetControl<T>(AllowSendingBookingConfirmationControl control, string name)
			where T : Control
		{
			return (T)typeof(AllowSendingBookingConfirmationControl).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(control);
		}

		protected override IBusiness GetNewBusinessEntity()
		{
			var collection = new AllowSendingBookingConfirmationCollection(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), Factory);
			collection.AddNew();
			return collection;
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			var allowSendingBookingConfirmationControl = (AllowSendingBookingConfirmationControl)control;
			ZGrid grid = GetControl<ZGrid>(allowSendingBookingConfirmationControl, "PrincipalGrid");

			return grid.ReadOnly;
		}

		#endregion
	}
}
