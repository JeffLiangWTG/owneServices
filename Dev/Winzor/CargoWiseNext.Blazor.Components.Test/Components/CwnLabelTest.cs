using Bunit;

namespace CargoWiseNext.Blazor.Components.Test;

public class CwnLabelTest : BunitTestContext
{
	[Test]
	public void CwnLabel_RenderTest()
	{
		var cut = RenderComponent<CwnLabel>();

		cut.MarkupMatches("<label class=\"cwn-label\"/>");
	}

	[Test]
	public void CwnLabel_Text()
	{
		var cut = RenderComponent<CwnLabel>(parameter => parameter
			.Add(p => p.Text, "Field"));

		cut.MarkupMatches("<label class=\"cwn-label\">Field</label>");
	}

	[Test]
	public void CwnLabel_IsRequired()
	{
		var cut = RenderComponent<CwnLabel>(parameter => parameter
			.Add(p => p.IsRequired, true));

		cut.MarkupMatches(@"
			<label class=""cwn-label"">
				<span class=""cwn-label--required"">*</span>
			</label>
		");
	}

	[Test]
	public void CwnLabel_ChildContent()
	{
		var cut = RenderComponent<CwnLabel>(parameter => parameter
			.AddChildContent("<h1>Child Content</h1>"));

		cut.MarkupMatches(@"
			<label class=""cwn-label"">
				<h1>Child Content</h1>
			</label>
		");
	}

	[Test]
	public void CwnLabel_WhenText_WhenIsRequired_WhenChildContent()
	{
		var cut = RenderComponent<CwnLabel>(parameter => parameter
			.Add(p => p.Text, "Required Field")
			.Add(p => p.IsRequired, true)
			.AddChildContent("<h1>Child Content</h1>"));

		cut.MarkupMatches(@"
			<label class=""cwn-label"">
				Required Field <span class=""cwn-label--required"">*</span>
				<h1>Child Content</h1>
			</label>
		");
	}
}
