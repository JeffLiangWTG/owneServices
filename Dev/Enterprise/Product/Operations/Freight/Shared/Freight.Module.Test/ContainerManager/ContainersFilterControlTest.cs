using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Module.Testing
{
	sealed class ContainersFilterControlTest : TestCaseWithFactory
	{
		public void TestZFilterStrip_Is_WorkflowFilterStrip()
		{
			var containers = new ContainerNonDependentCollection(Factory);
			var containersFilterBO = new ContainerManagerFilterStrip();

			using (var form = new ZForm())
			{
				var filterControl = new ContainersFilterControl(containers, containersFilterBO);
				form.Controls.Add(filterControl);
				form.Show();

				Assert("Must return WorkflowFilterStrip (or descendants) so that workflow filter strips would work property",
					filterControl.AddNewFilterStrip() is ContainerModuleStrip);
			}
		}

		public void TestVGMStatusColumnExisted()
		{
			var containers = new ContainerNonDependentCollection(Factory);
			var containersFilterBO = new ContainerManagerFilterStrip();

			using (var form = new ZForm())
			{
				var filterControl = new ContainersFilterControl(containers, containersFilterBO);
				form.Controls.Add(filterControl);
				form.Show();

				var vgmStatusColumn = filterControl.Grid.ColumnStyles.Cast<ZGridColumnInfo>().FirstOrDefault(c => c.ColumnName == "JC_GrossWeightVerificationStatus");
				AssertNotNull(vgmStatusColumn);
				Assert(!vgmStatusColumn.IsVisible);
			}
		}

		public void TestAdditionalReferenceColumn()
		{
			var containers = new ContainerNonDependentCollection(Factory);
			var containersFilterBO = new ContainerManagerFilterStrip();

			using (var filterControl = new ContainersFilterControl(containers, containersFilterBO))
			{
				bool columnExistsAndNotVisible = filterControl.FilteredGrid.ColumnStyles
					.Cast<ZGridColumnInfo>()
					.Any(col => col.ColumnName == "AdditionalReferenceNumbersAsString" && !col.IsVisible);

				Assert("Additional Reference column should exist and should NOT be visible.", columnExistsAndNotVisible);
			}
		}
	}
}
