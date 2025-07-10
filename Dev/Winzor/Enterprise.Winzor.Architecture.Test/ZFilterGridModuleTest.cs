using System.Threading.Tasks;
using System.Windows.Forms;
using Bunit;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Winzor.Architecture.Test;

public class ZFilterGridModuleTest
{
	[Test]
	public async Task GridLoadsOnGridModuleForm()
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var module = (DummyFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy);
			var form = new Form();
			form.Controls.Add(module.EmbeddedControl);
			return form;
		});
		Assert.That(rendered.Find(".datagrid"), Is.Not.Null);
	}
}
