using Blazor.Diagrams.Core.Geometry;
using CargoWise.Blazor.Client.Integration.Menus;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using WinzorFramework.JSInterop;

namespace NetworkVisualisation.GUI.Winzor.Test;

public abstract class BunitTestContext : TestContextWrapper
{
	[SetUp]
	public void Setup()
	{
		TestContext = new Bunit.TestContext();
		var rect = new Rectangle(new Point(0, 0), new Size(100, 100));
		JSInterop.SetBoundingClientRect(rect);
		JSInterop.SetupVoid("ZBlazorDiagrams.observe", _ => true);
		JSInterop.Mode = JSRuntimeMode.Loose;

		Services.AddSingleton(Mock.Of<IMenuDisplayer>());
		Services.AddSingleton(Mock.Of<IClipboardJSInterop>());
	}

	[TearDown]
	public void TearDown() => TestContext?.Dispose();
}
