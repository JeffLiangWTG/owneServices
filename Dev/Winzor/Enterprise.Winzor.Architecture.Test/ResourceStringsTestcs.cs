using System.Threading.Tasks;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using Res = CargoWiseOne.ResourceStrings.Res;

namespace Enterprise.Winzor.Architecture.Test;

class ResourceStringsTestcs
{
	[Test]
	public async Task ResourceStringAnalyzerEnabledOnZArchitectureGUI()
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			ctx.Using(Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified));
			return new TextTemplateForm();
		});
		Assert.That(rendered.Markup, Does.Contain("模板名称"));
	}
}
