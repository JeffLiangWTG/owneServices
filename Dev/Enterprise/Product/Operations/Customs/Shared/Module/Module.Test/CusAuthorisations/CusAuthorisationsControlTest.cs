using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.Module.Testing
{
	sealed class CusAuthorisationsControlTest : TestCaseWithFactory
	{
		public void TestEnableAdHocColumn()
		{
			using (var form = new ZForm())
			using (var filterControl = GetNewFilterControl())
			{
				form.Controls.Add(filterControl);
				form.Show();
				var grid = filterControl.Grid;
				var adHoc = grid.GetColumnStyle(CusAuthorisationHeader.Schema.CPH_IsAdHoc);
				AssertNull("AdHoc column should not exist", adHoc);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.France))
			using (var form = new ZForm())
			using (var filterControl = GetNewFilterControl())
			{
				form.Controls.Add(filterControl);
				form.Show();
				var grid = filterControl.Grid;
				var adHoc = grid.GetColumnStyle(CusAuthorisationHeader.Schema.CPH_IsAdHoc);
				AssertNotNull("AdHoc column should exist", adHoc);
				AssertEquals("AdHoc column width", ControlDpiScalingHelper.ScaleToCurrentDpiX(72), adHoc.Width);
				AssertEquals("AdHoc column name", "Ad Hoc", grid.GetColumnCaption(CusAuthorisationHeader.Schema.CPH_IsAdHoc));
			}
		}

		public void TestNewZFilterStrip()
		{
			using (var filterControl = GetNewFilterControl())
			{
				using (var filterStrip = filterControl.NewZFilterStrip())
				{
					AssertType<CusAuthorisationsFilterStrip>(filterStrip);
				}
			}
		}

		public void TestGridColumnNames()
		{
			using (var form = new ZForm())
			using (var filterControl = GetNewFilterControl())
			{
				form.Controls.Add(filterControl);
				form.Show();
				var grid = filterControl.Grid;
				CombineAssertions(() =>
				{
					foreach (var (columnName, columnCaption, _, _) in OrderedColumnDetails)
					{
						AssertEquals(columnName, columnCaption, grid.GetColumnCaption(columnName));
					}
				});
			}
		}

		public void TestGridDefaultColumnOrder()
		{
			using (var form = new ZForm())
			using (var filterControl = GetNewFilterControl())
			{
				form.Controls.Add(filterControl);
				form.Show();
				AssertSequencesEqual(OrderedColumnDetails.Select(x => x.ColumnName), filterControl.Grid.DefaultColumns.Where(x => x.IsVisible).Select(x => x.ColumnName));
			}
		}

		public void TestGridColumnWidths()
		{
			using (var filterControl = GetNewFilterControl())
			{
				var grid = filterControl.Grid;
				CombineAssertions(() =>
				{
					foreach (var (columnName, _, columnWidth, _) in OrderedColumnDetails)
					{
						AssertEquals(columnName, columnWidth, grid.GetColumnStyle(columnName).Width);
					}
				});
			}
		}

		public void TestGridColumnCharacterCasing()
		{
			using (var filterControl = GetNewFilterControl())
			{
				var grid = filterControl.Grid;
				CombineAssertions(() =>
				{
					foreach (var (columnName, _, _, chacterCasing) in OrderedColumnDetails)
					{
						AssertEquals(columnName, chacterCasing, grid.GetColumnStyle(columnName).CharacterCasing);
					}
				});
			}
		}

		CusAuthorisationsControlForTest GetNewFilterControl()
		{
			var collection = new CusAuthorisationHeaderCollection(Factory);
			var filterBizO = new CusAuthorisationsFilterStripBusinessObject();
			return new CusAuthorisationsControlForTest(collection, filterBizO);
		}

		static (string ColumnName, string ColumnCaption, int ColumnWidth, CharacterCasing ColumnCasing)[] OrderedColumnDetails => new[] { (CusAuthorisationHeader.Schema.CPH_Type, "Authorization Type", ControlDpiScalingHelper.ScaleToCurrentDpiX(114), CharacterCasing.Upper), (CusAuthorisationHeader.Schema.CusAuthorisationRuleCodes, "Rule Codes", ControlDpiScalingHelper.ScaleToCurrentDpiX(150), CharacterCasing.Normal), (CusAuthorisationHeader.Schema.CPH_PermitDescription, "Description", ControlDpiScalingHelper.ScaleToCurrentDpiX(150), CharacterCasing.Normal), (CusAuthorisationHeader.Schema.CPH_OH_PermitHolder, "Authorization Holder", ControlDpiScalingHelper.ScaleToCurrentDpiX(122), CharacterCasing.Upper), (CusAuthorisationHeader.Schema.AuthorizationAddress, "Authorization Address", ControlDpiScalingHelper.ScaleToCurrentDpiX(150), CharacterCasing.Normal), (CusAuthorisationHeader.Schema.CPH_Number, "Authorization Number", ControlDpiScalingHelper.ScaleToCurrentDpiX(235), CharacterCasing.Upper), (CusAuthorisationHeader.Schema.CPH_StartDate, "Start Date", ControlDpiScalingHelper.ScaleToCurrentDpiX(72), CharacterCasing.Normal), (CusAuthorisationHeader.Schema.CPH_EndDate, "End Date", ControlDpiScalingHelper.ScaleToCurrentDpiX(68), CharacterCasing.Normal), (CusAuthorisationHeader.Schema.IsCurrent, "Is Current", ControlDpiScalingHelper.ScaleToCurrentDpiX(72), CharacterCasing.Normal) };
		sealed class CusAuthorisationsControlForTest : CusAuthorisationsControl
		{
			public CusAuthorisationsControlForTest(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterStripBusinessObject) : base(gridCollection, filterStripBusinessObject)
			{
			}

			public new ZFilterStrip NewZFilterStrip() => base.NewZFilterStrip();
		}
	}
}
