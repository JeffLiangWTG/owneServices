using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Agency.Module.Testing
{
	internal class ContainerMoveFilterControlTest : TestCaseWithFactory
	{
		public void TestE9_ColumnShouldExist()
		{
			using (var module = new ContainerMoveModuleForTest())
			using (var filterControl = (ContainerMoveFilterControl)module.GetNewFilterControl())
			{
				var columnNames = new string[] { "E9_LeaseNumber", "RelatedInfo+Origin", "RelatedInfo+Destination", "RelatedInfo+LoadPort", "RelatedInfo+DischargePort" };
				foreach (var columnName in columnNames)
				{
					var col = filterControl.FilteredGrid.ColumnStyles.Cast<Core.Forms.ZGridColumnInfo>().FirstOrDefault(c => c.ColumnName == columnName);
					AssertNotNull(col);
					Assert(!col.IsVisible);
				}
			}
		}

		#region Implementation
		class ContainerMoveModuleForTest : ContainerMoveModule
		{
			public new IFilterControl GetNewFilterControl()
			{
				return base.GetNewFilterControl();
			}
		}
		#endregion
	}
}
