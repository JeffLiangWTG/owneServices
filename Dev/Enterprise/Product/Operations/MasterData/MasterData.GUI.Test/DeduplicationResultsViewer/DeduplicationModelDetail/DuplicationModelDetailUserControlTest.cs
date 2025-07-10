using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.MasterData.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterData.GUI.Test
{
	public class DuplicationModelDetailUserControlTest : TestCaseWithFactory
	{
		public void TestGridColumns()
		{
			var allColumns = new[]
			{
				"Similarity",
				"ConfidenceScore",
				"Source",
				"Active",
				"PrimaryWorkplace",
				"Name",
				"Email",
				"Birthday",
				"Website",
				"Number",
				"Domain",
				"Type",
				"Code",
				"Country",
				"Address",
				"UNLOCO",
				"City",
				"State",
				"Coordinates",
				"Brand",
				"ContactPhone",
				"AddressPhone",
				"ContactMobile",
				"AddressMobile",
				"OtherPhone",
				"HomePhone",
				"ContactFax",
				"AddressFax",
				"CusCodeCustomsRegNo",
				"PersonPhone",
				"PersonWorkPhone",
				"PersonMobile",
				"PersonFax"
			};

			var group = new DuplicationModelDetailGroup(
				null,
				Enumerable.Empty<DuplicationModelDetail>(),
				Enumerable.Empty<DuplicationModelDetail>(),
				allColumns,
				Enumerable.Empty<string>());

			using (var form = new ZForm())
			{
				var control = new DuplicationModelDetailUserControlForTest(group, withConfidence: false);
				form.Controls.Add(control);
				form.Show();

				var columnInfos = control.ModelDetailGrid.ColumnStyles.Cast<ZGridColumnInfo>();
				var columnNames = columnInfos.Select(x => x.ColumnName);

				Assert(columnInfos.All(x => x.IsSortable is false));
				AssertContainsExactElementsInExactOrder(allColumns, columnNames);
			}
		}
	}

	class DuplicationModelDetailUserControlForTest : DuplicationModelDetailUserControl
	{
		public DuplicationModelDetailUserControlForTest(DuplicationModelDetailGroup modelDetailGroup, bool withConfidence)
			: base(modelDetailGroup, withConfidence)
		{
		}

		public new ZLabel ModelDetailLabel => base.ModelDetailLabel;
		public new ZGrid ModelDetailGrid => base.ModelDetailGrid;
	}
}
