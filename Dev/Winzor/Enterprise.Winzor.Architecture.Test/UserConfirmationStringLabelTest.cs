using System.Threading.Tasks;
using Bunit;
using Enterprise.Core.Environment;
using NUnit.Framework;

namespace Enterprise.Winzor.Architecture.Test;

class UserConfirmationStringLabelTest
{
	// input less than expected label text length
	[TestCase("yes", "linear-gradient(90deg,rgba(0,128,0,0.2)72.727272727272727272727272730%,rgba(255,255,255,0)0%)")]
	[TestCase("ye", "linear-gradient(90deg,rgba(0,128,0,0.2)50.0%,rgba(255,255,255,0)0%)")]
	[TestCase("sbc", "linear-gradient(90deg,rgba(255,0,0,0.2)72.727272727272727272727272730%,rgba(255,255,255,0)0%)")]
	[TestCase("ys", "linear-gradient(90deg,rgba(0,128,0,0.2)27.272727272727272727272727270%,rgba(255,0,0,0.2)0% 77.272727272727272727272727270%,rgba(255,255,255,0)0%)")]
	// input above expected label text length wrong part will be expect string which is yes
	[TestCase("yyes", "linear-gradient(90deg,rgba(0,128,0,0.2)27.272727272727272727272727270%,rgba(255,0,0,0.2)0% 77.272727272727272727272727270%,rgba(255,255,255,0)0%)")]
	[TestCase("yessss", "linear-gradient(90deg,rgba(255,0,0,0.2)72.727272727272727272727272730%,rgba(255,255,255,0)0%)")]
	// input is space will be all red or input is empty should be white
	[TestCase("", "linear-gradient(90deg,rgba(255,255,255,0)100%,rgba(255,255,255,0)0%")]
	[TestCase("   ", "linear-gradient(90deg,rgba(255,0,0,0.2)72.727272727272727272727272730%,rgba(255,255,255,0)0%)")]
	[TestCase("y  ", "linear-gradient(90deg,rgba(0,128,0,0.2)27.272727272727272727272727270%,rgba(255,0,0,0.2)0% 77.272727272727272727272727270%,rgba(255,255,255,0)0%)")]
	public async Task ConfirmationStringLabelWithHighlightColorStyle(string inputText, string expectedResult)
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var stringLabel = new UserConfirmationStringLabel();
			stringLabel.Text = "yes";
			stringLabel.UpdateInput(inputText);
			return stringLabel;
		});

		var renderedLabel = rendered.Find(".label");
		var renderedLabelStyle = renderedLabel.GetAttribute("style");
		Assert.That(renderedLabelStyle, Does.Contain($"background: {expectedResult}"));
	}
}
