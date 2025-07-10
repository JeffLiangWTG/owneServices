using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Module.Testing
{
	sealed class RefCusTradeGroupFilterControlTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestColumns()
		{
			using (var form = new ZForm())
			using (var filterControl = new RefCusTradeGroupFilterControl(new RefCusTradeGroupCollection(Factory), new RefCusTradeGroupFilterBusinessObject()))
			{
				form.Controls.Add((filterControl));
				form.Show();

				var gird = filterControl.Grid;
				CombineAssertions(() =>
				{
					AssertColumn(gird.GetColumnStyle(RefCusTradeGroupSchema.ZZA_TradeGroup.Name), width: 100, CharacterCasing.Upper);
					AssertColumn(gird.GetColumnStyle(RefCusTradeGroupSchema.ZZA_Description.Name), width: 300, CharacterCasing.Normal);
				});
			}

			void AssertColumn(ZGridColumnInfo info, int width, CharacterCasing casing)
			{
				AssertEquals(info.ColumnName + " width", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(width), info.Width);
				AssertEquals(info.ColumnName + " casing", casing, info.CharacterCasing);
			}
		}
	}
}
