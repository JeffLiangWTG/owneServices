using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Agency.Module.Testing
{
	internal class ContainerManagerFilterControlTest : TestCaseWithFactory
	{
		public void TestE9_ColumnShouldExist()
		{
			using (var module = new ContainerManagerModuleForTest())
			using (var filterControl = (ContainerManagerFilterControl)module.GetNewFilterControl())
			{
				var columnNames = new string[] { "LastMovement+E9_LeaseNumber", "LastMovement+RelatedInfo+Origin", "LastMovement+RelatedInfo+Destination", "LastMovement+RelatedInfo+LoadPort", "LastMovement+RelatedInfo+DischargePort" };
				foreach (var columnName in columnNames)
				{
					var col = filterControl.FilteredGrid.ColumnStyles.Cast<Core.Forms.ZGridColumnInfo>().FirstOrDefault(c => c.ColumnName == columnName);
					AssertNotNull(col);
					Assert(!col.IsVisible);
				}
			}
		}

		#region Implementation
		class ContainerManagerModuleForTest : ContainerManagerModule
		{
			public new IFilterControl GetNewFilterControl()
			{
				return base.GetNewFilterControl();
			}
		}
		#endregion
	}
}
