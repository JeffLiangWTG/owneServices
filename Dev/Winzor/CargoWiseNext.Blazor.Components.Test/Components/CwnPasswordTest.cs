using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bunit;
using Microsoft.AspNetCore.Components;

namespace CargoWiseNext.Blazor.Components.Test.Components
{
	public class CwnPasswordTest : BunitTestContext
	{
		[Test]
		public void CwnPassword_RenderTest()
		{
			// Act
			var cut = RenderComponent<CwnPassword>();

			// Assert
			cut.MarkupMatches("<input class=\"\" type=\"Password\"  >\r\n");
		}

		[Test]
		public void CwnPassword_RenderWithPlaceholderTest()
		{
			// Act
			var cut = RenderComponent<CwnPassword>(parameters => parameters.Add(p => p.Placeholder, "Enter Password here"));

			// Assert
			cut.MarkupMatches("<input class=\"\" type=\"Password\" placeholder=\"Enter Password here\"  >\r\n");
		}

		[Test]
		public void CwnPassword_OnInputEventTriggeredTest()
		{
			// Arrange
			string? newValue = null;
			var cut = RenderComponent<CwnPassword>(parameters =>
				parameters.Add(p => p.OnInput, EventCallback.Factory.Create<string?>(this, value => newValue = value)));

			// Act
			cut.Find("input").Input(new ChangeEventArgs { Value = "Password input" });

			// Assert
			Assert.That(newValue, Is.EqualTo("Password input"));
		}
	}
}
