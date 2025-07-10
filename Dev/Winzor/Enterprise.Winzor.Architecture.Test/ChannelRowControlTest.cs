using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using AngleSharp.Css.Dom;
using Bunit;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.GUI;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.VisualBoards.Business.Test;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Moq;
using NUnit.Framework;
using WinzorFramework.JSInterop;

namespace Enterprise.Winzor.Architecture.Test;

class ChannelRowControlTest
{
	[Test]
	public async Task PrimaryChannelsBorderStyle()
	{
		using var env = EnvProxy.SetTemporaryEnvForTest(CargoWise.Application.ObjectFactory.Get<IEnv>());
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var factory = new BusinessObjectFactory();

			var jobHeader = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(factory);
			var workflow = VisualBoardsTestHelper.CreateWorkflow(jobHeader, completionStatement: "Container");

			var tagDef = VisualBoardsTestHelper.CreateTagDefinition(factory, "bla");
			var tagMagnitude = VisualBoardsTestHelper.CreateTagMagnitude(tagDef, "dem");

			workflow.AddTag(tagMagnitude);

			return GenerateTestForm(factory, workflow);
		});

		var actualStyle = rendered.Find(".channelrowcontrol").GetAttribute("style");
		Assert.That(actualStyle, Contains.Substring("border: 1px solid gray; border-collapse: collapse;"));
	}

	[Test]
	public async Task TestDragAndDrop()
	{
		using var ctx = new EnterpriseTestContext();
		var services = ctx.DefaultClientServices;
		var eventService = new Mock<IClientEventService>();
		var parentMouseEvent = new Mock<IJSObjectReference>();
		var documentMouseEvent = new Mock<IJSObjectReference>();
		await using var registerClientEvent = new RegisteredClientEvent(parentMouseEvent.Object);
		eventService.Setup(s => s.RegisterMouseEventListenerAsync(It.IsAny<Func<WebMouseEventArgs, Task>>(),
			ClientMouseEvent.MouseMove, It.IsAny<ElementReference>()))
			.ReturnsAsync(registerClientEvent);
		await using var registeredClientEvent = new RegisteredClientEvent(documentMouseEvent.Object);
		eventService.Setup(s => s.RegisterGlobalMouseEventListenerAsync(It.IsAny<Func<WebMouseEventArgs, Task>>(),
			ClientMouseEvent.MouseUp)).ReturnsAsync(registeredClientEvent);
		var interop = new Mock<IElementJSInterop>();
		interop.Setup(i => i.GetOffsetTopAsync(It.IsAny<ElementReference>())).ReturnsAsync(10d);
		ctx.Services.AddScoped(_ => interop.Object);

		ChannelRowControl channelRow = null;
		services.ClientEventService = eventService.Object;
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var panel = new ZPanel();
			form.Controls.Add(panel);
			channelRow = new ChannelRowControl();
			panel.Controls.Add(channelRow);
			return form;
		}, services);

		var row = rendered.Find(".channelrowcontrol");

		var startMouseArg = new WebMouseEventArgs() { PageY = 20 };
		row.MouseDown(startMouseArg);
		Assert.That(row.GetStyle().GetZIndex(), Is.EqualTo("1"));
		eventService.Verify(s => s.RegisterMouseEventListenerAsync(channelRow.DragMoveAsync, ClientMouseEvent.MouseMove, It.IsAny<ElementReference>()), Times.Once());
		eventService.Verify(s => s.RegisterGlobalMouseEventListenerAsync(channelRow.DragEndAsync, ClientMouseEvent.MouseUp), Times.Once());

		var moveMouseArg = new WebMouseEventArgs() { PageY = 50 };
		await ctx.Renderer.Dispatcher.InvokeAsync(async () => await channelRow.DragMoveAsync(moveMouseArg));
		rendered.Render();
		row = rendered.Find(".channelrowcontrol");
		Assert.That(row.GetStyle().GetTop(), Is.EqualTo("40px"));

		var dragEndEvent = ctx.Renderer.Dispatcher.InvokeAsync(async () =>
		{
			await Task.Delay(10);
			await channelRow.DragEndAsync(moveMouseArg);
		});

		// Test when DragEndAsync is queued and a click event is fired
		eventService.Invocations.Clear();
		row.MouseDown(startMouseArg);
		var mouseUpEvent = ctx.Renderer.Dispatcher.InvokeAsync(async () => await channelRow.DragEndAsync(moveMouseArg));
		await dragEndEvent;
		await mouseUpEvent;

		eventService.Verify(s => s.RegisterMouseEventListenerAsync(channelRow.DragMoveAsync, ClientMouseEvent.MouseMove, It.IsAny<ElementReference>()), Times.Never(), "MouseDown event when dragging should not attach another MouseMove listener");
		eventService.Verify(s => s.RegisterGlobalMouseEventListenerAsync(channelRow.DragEndAsync, ClientMouseEvent.MouseUp), Times.Never(), "MouseDown event when dragging should not attach another MouseUp listener");

		Assert.That(channelRow.Top, Is.EqualTo(40));
		parentMouseEvent.Verify(e => e.DisposeAsync(), Times.Once());
		documentMouseEvent.Verify(e => e.DisposeAsync(), Times.Once());
	}

	[Test]
	public async Task PreloadElementJSInterop()
	{
		using var ctx = new EnterpriseTestContext();
		var services = ctx.MockCargoWiseClientServices;
		var interop = new Mock<IElementJSInterop>();
		interop.Setup(i => i.PreloadInterop());
		ctx.Services.AddScoped(_ => interop.Object);

		var rendered = await ctx.RenderControlOnFormAsync(() => new ChannelRowControl());

		interop.Verify(e => e.PreloadInterop(), Times.Once());
	}

	ZForm GenerateTestForm(BusinessObjectFactory factory, ProcessHeader workflow)
	{
		var system = VisualBoardsTestHelper.CreateSystem(factory, "XYZ");
		var bucket = VisualBoardsTestHelper.CreateBucket(system);
		var section = VisualBoardsTestHelper.CreateBoardSection(bucket);
		var viewModel = new BMBoardSectionChannelsViewModel(section.SectionConfiguration, ChannelAxis.Primary);

		var primaryChannel = VisualBoardsTestHelper.CreatePrimaryChannelForSection(section);
		ChannelAxis axis = ChannelAxis.Primary;
		ChannelRowControl channelRowControl = new ChannelRowControl(viewModel, primaryChannel, axis);
		var form = new ZForm(system);
		form.Controls.Add(channelRowControl);
		return form;
	}
}
