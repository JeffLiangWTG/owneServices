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
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.GUI.Test.Registry.BillsOfLading
{
	[TestedType(typeof(BillOfLadingImageControl))]
	public class BillOfLadingImageControlTest : RegistryZUserControlTestCase
	{
		public void TestPrincipalGridRemoveAction()
		{
			using (var billOfLadingImageControl = new BillOfLadingImageControl())
			{
				var principalGrid = billOfLadingImageControl.Controls.Find("PrincipalGrid", true).First() as ZGrid;

				AssertEquals(RemoveAction.RemoveAndDelete, principalGrid.RemoveAction);
			}
		}

		#region Implementation

		T GetControl<T>(BillOfLadingImageControl control, string name)
			where T : Control
		{
			return (T)typeof(BillOfLadingImageControl).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(control);
		}

		protected override IBusiness GetNewBusinessEntity()
		{
			var collection = new BillOfLadingImageCollection(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), Factory);
			collection.AddNew();
			return collection;
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			BillOfLadingImageControl billOfLadingImageControl = (BillOfLadingImageControl)control;
			ZGrid grid = GetControl<ZGrid>(billOfLadingImageControl, "PrincipalGrid");
			ImageSelectionControl imageSelection = GetControl<ImageSelectionControl>(billOfLadingImageControl, "ImageSelectionControl");

			return grid.ReadOnly && imageSelection.ReadOnly;
		}

		#endregion
	}
}
