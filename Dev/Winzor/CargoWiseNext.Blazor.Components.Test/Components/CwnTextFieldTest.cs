using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bunit;
using Microsoft.AspNetCore.Components;

namespace CargoWiseNext.Blazor.Components.Test.Components
{
	public class CwnTextFieldTest : BunitTestContext
	{
		[Test]
		public void CwnTextField_RenderTest()
		{
			// Act
			var cut = RenderComponent<CwnTextField>();

			// Assert
			cut.MarkupMatches("<input class=\"cwn-text-field\" type=\"text\"  >\r\n");
		}

		[Test]
		public void CwnTextField_RenderWithPlaceholderTest()
		{
			// Act
			var cut = RenderComponent<CwnTextField>(parameters => parameters.Add(p => p.Placeholder, "Enter text here"));

			// Assert
			cut.MarkupMatches("<input class=\"cwn-text-field\" type=\"text\" placeholder=\"Enter text here\"  >\r\n");
		}

		[Test]
		public void CwnTextField_RenderWithCustomInputTypeTest()
		{
			// Act
			var cut = RenderComponent<CwnTextField>(parameters => parameters.Add(p => p.InputType, InputType.Password));

			// Assert
			cut.MarkupMatches("<input class=\"cwn-text-field\" type=\"password\"  >\r\n");
		}

		[Test]
		public void CwnTextField_OnInputEventTriggeredTest()
		{
			// Arrange
			string? newValue = null;
			var cut = RenderComponent<CwnTextField>(parameters =>
				parameters.Add(p => p.OnInput, EventCallback.Factory.Create<string?>(this, value => newValue = value)));
			var component = cut.Instance;

			// Act
			cut.Find("input").Input(new ChangeEventArgs { Value = "Test input" });
			Assert.That(component.Value, Is.Null);

			// Assert
			cut.Find("input").Change(new ChangeEventArgs { Value = "Test input" });
			Assert.That(newValue, Is.EqualTo("Test input"));
		}
	}
}
