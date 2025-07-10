using System.Threading.Tasks;
using Bunit;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Winzor.Architecture.Test;

class ZNumberRangeControlTest
{
	[Test]
	public async Task FromCalcEditShouldOverlapToCalcEditByHavingZIndexEqualToOne()
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var stripBizO = new MockFilterStripBizO();
			var form = new WinzorTestForm();
			var factory = new BusinessObjectFactory();
			var dummy = new DummyBusinessObjectCollection(factory);
			var dataSource = new MockFilterStripForm(stripBizO).DataSource;
			var filterStripControl = new ZFilterStripControlForTest(dummy, dataSource);
			var columnStyleInfo = new ZTextBoxColumnStyleInfo();
			columnStyleInfo.ColumnName = DummyBizoSchema.Constants.Z0_VarCharMax;

			var filterStripBizO = dataSource.FilterStrips.AddNew();
			filterStripBizO.FilterDescription = "Number Range (Decimal)";
			filterStripControl.AddFilterStrip(filterStripBizO);
			filterStripControl.FilteredGrid.BindTo = "Dummies";
			filterStripControl.FilteredGrid.ColumnStyles.Add(columnStyleInfo);

			var panel = filterStripControl.Controls.Find("FilterStripsPanel", false)[0];
			var filterStrip = (ZFilterStrip)panel.Controls[panel.Controls.Count - 1];
			var numberRangeControl = (ZNumberRangeControl)filterStrip.Controls[5];
			var propertySearchDropEdit = (ZFilterStripDropEdit)numberRangeControl.Controls[0];
			propertySearchDropEdit.SelectItem("Between");
			propertySearchDropEdit.CommitBoundValue();

			form.Controls.Add(numberRangeControl);
			return form;
		});

		var fromCalcEdit = rendered.Find("[data-type='Enterprise.ZArchitecture.GUI.Internal.ZNumberRangeControl'] > input");
		Assert.That(fromCalcEdit.GetAttribute("style"), Does.Contain("z-index:1"));
	}
}
