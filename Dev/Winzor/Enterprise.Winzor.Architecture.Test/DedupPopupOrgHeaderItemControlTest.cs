using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Tools.DuplicateDetector;
using Enterprise.MasterData.Business;
using Enterprise.MasterData.GUI;
using Enterprise.MasterFiles.Business;
using Microsoft.Playwright;
using NUnit.Framework;
using WTG.PlaywrightTesting;
using static WTG.PlaywrightTesting.PlaywrightTestContext;

namespace Enterprise.Winzor.Architecture.Test;

class DedupPopupOrgHeaderItemControlTest
{
	[Test, WithPlaywrightPage(Headless = false)]
	public async Task TestTempName()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form { Size = new Size(600, 400) };
			var dataSource = GetDedupPopupBizoDataSource();

			var parentControl = new DedupPopupOrgHeaderControl();
			form.Controls.Add(parentControl);
			parentControl.SetDataBinding(dataSource, string.Empty);

			return form;
		});

		var containerInnerPanel = await page.WaitForSelectorAsync("[data-name='confidenceScorePanel']");
		var detailTableLayoutPanel = await page.WaitForSelectorAsync("[data-name='detailTableLayoutPanel']");

		var containerInnerPanelZIndexString = await containerInnerPanel.EvaluateAsync($"e => window.getComputedStyle(e).getPropertyValue('z-index')");
		var detailTableLayoutPanelZIndexString = await detailTableLayoutPanel.EvaluateAsync($"e => window.getComputedStyle(e).getPropertyValue('z-index')");
		var containerInnerPanelZIndex = ZIndexStringToInt(containerInnerPanelZIndexString.Value.ToString());
		var detailTableLayoutPanelZIndex = ZIndexStringToInt(detailTableLayoutPanelZIndexString.Value.ToString());

		Assert.That(detailTableLayoutPanelZIndex, Is.LessThan(containerInnerPanelZIndex));
	}

	int ZIndexStringToInt(string zindex)
	{
		if (!int.TryParse(zindex, out var result))
		{
			result = -2;
		}
		return result;
	}

	DedupPopupBizoDataSource GetDedupPopupBizoDataSource()
	{
		var factory = new BusinessObjectFactory();

		var master = factory.NewWithValidTestData<OrgHeader>();
		master.OH_FullName = "TEST_ORG_MASTER";
		master.OH_Code = "TESORGM";

		var target = factory.NewWithValidTestData<OrgHeader>();
		target.OH_FullName = "TEST_ORG_TARGET";
		target.OH_Code = "TESORGT";

		var scoringResultList = new List<ScoringResult>
		{
			TargetScorerController.Score(new DeduplicationOrgHeader(master), new DeduplicationOrgHeader(target), isStandardizingMaster: true),
		};

		return new DedupPopupBizoDataSource(master, scoringResultList, null);
	}
}
