using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Winzor.Architecture.Test;

class ZDynamicMultilineTextBoxFormTest
{
	[Test]
	public async Task UseParentDivForLayoutShouldBeFalse()
	{
		using var ctx = new EnterpriseTestContext();
		ZDynamicMultilineTextBoxForm multilineTextBoxForm = null;
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var textBox = new ZTextBox { IsDynamicMultiline = true, MaxLength = 3, CharacterCasing = CharacterCasing.Normal };
			multilineTextBoxForm = new ZDynamicMultilineTextBoxForm(textBox, new Size(5, 5));
			return multilineTextBoxForm;
		});
		Assert.That(multilineTextBoxForm.UseParentDivForLayout, Is.False);
	}
}
