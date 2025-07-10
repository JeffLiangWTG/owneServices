using System.Linq;
using Enterprise.Core.Forms;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Agency.Business.Testing;

namespace Enterprise.Freight.Agency.Module.Testing
{
	internal class BillContainersFilterControlTest : BaseAgencyTest
	{
		public void TestAdditionalReferenceColumn()
		{
			var containers = new BillOfLadingContainerCollection(Factory);
			using (var filterControl = new BillContainersFilterControl(containers, new BillContainersFilterStrip()))
			{
				bool columnExistsAndNotVisible = filterControl.FilteredGrid.ColumnStyles.Cast<ZGridColumnInfo>().Any((c) => c.ColumnName == "AdditionalReferenceNumbersAsString" && !c.IsVisible);
				Assert("Additional Reference column should exist and should NOT be visible", columnExistsAndNotVisible);
			}
		}

		public void TestEstimatedTimeDepartureColumn()
		{
			var containers = new BillOfLadingContainerCollection(Factory);
			using (var filterControl = new BillContainersFilterControl(containers, new BillContainersFilterStrip()))
			{
				var columnInfo = filterControl.FilteredGrid.ColumnStyles
						.Cast<ZGridColumnInfo>()
						.FirstOrDefault((c) => c.ColumnName == "Booking+Sailing+JX_JA_E_DEP");
				AssertNotNull("ETD column should exist", columnInfo);
				AssertEquals("ETD column caption", "ETD", columnInfo.CaptionResourceString.Caption);
				AssertEquals("ETD column default visibility is false", false, columnInfo.IsVisible);
			}
		}

		public void TestEstimatedTimeArrivaleColumn()
		{
			var containers = new BillOfLadingContainerCollection(Factory);
			using (var filterControl = new BillContainersFilterControl(containers, new BillContainersFilterStrip()))
			{
				var columnInfo = filterControl.FilteredGrid.ColumnStyles
						.Cast<ZGridColumnInfo>()
						.FirstOrDefault((c) => c.ColumnName == "Booking+Sailing+JX_JB_E_ARV");
				AssertNotNull("ETA column should exist", columnInfo);
				AssertEquals("ETA column caption", "ETA", columnInfo.CaptionResourceString.Caption);
				AssertEquals("ETA column default visibility is false", false, columnInfo.IsVisible);
			}
		}
	}
}
