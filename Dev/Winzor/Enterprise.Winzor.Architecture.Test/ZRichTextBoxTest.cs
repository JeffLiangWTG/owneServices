using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Bunit;
using CargoWise.Blazor.Client.Integration.Menus;
using CargoWise.Types;
using Enterprise.EConversation.Business;
using Enterprise.EConversation.GUI;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Microsoft.Playwright;
using Moq;
using NUnit.Framework;
using WinzorFramework.JSInterop;
using WinzorTestFramework;
using WTG.PlaywrightTesting;

namespace Enterprise.Winzor.Architecture.Test;

class ZRichTextBoxTest
{
	/// <summary>
	/// This test provides a viewable reproduction of:
	///		CS01767694 - WISGLOSYD - \n characters appear differently in native/web version (https://svc-ediprod.wtg.zone/Services/link/ShowEditForm/SupportIncident/4399e65e-c83d-4031-919d-0279661d23bb?lang=en-gb)
	///	The pathological behaviour (i.e. from before this WI) was that (some) of the newline patterns were added to the HTML as entities instead of all being treated as paragraph seperations.
	/// </summary>
	/// <returns></returns>
	[Test, WithPlaywrightPage]
	public async Task TestMixedNewlinesPlainTextIsConvertedCorrectly()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var input =
			"auto converting 'sg_strtgc_gds_ctrl_reg'\r\nbuild: https://contentservice.wtg.zone/job/build_upload/6889/console\nwebwatcher: https://webwatcher.wtg.zone/search?contentId=sg_strtgc_gds_ctrl_reg\rwebwatcher diff: https://webwatcher.wtg.zone/api/v1/content/extracted/html-diff/sg_strtgc_gds_ctrl_reg/124/125\ndiff: https://contentservice.wtg.zone/job/diff/5235/artifact/ws/diff.html\nfriendly diff: https://contentservice.wtg.zone/job/diff/5235/artifact/ws/diff_friendly.html\n";
		var expected = $"{WrapInP("auto converting &#39;sg_strtgc_gds_ctrl_reg&#39;")}<p>{WrapInSpan("build: ")}<a href=\"https://contentservice.wtg.zone/job/build_upload/6889/console\" target=\"_blank\" {standardStyle}>https://contentservice.wtg.zone/job/build_upload/6889/console</a></p><p>{WrapInSpan("webwatcher: ")}<a href=\"https://webwatcher.wtg.zone/search?contentId=sg_strtgc_gds_ctrl_reg\" target=\"_blank\" {standardStyle}>https://webwatcher.wtg.zone/search?contentId=sg_strtgc_gds_ctrl_reg</a></p><p>{WrapInSpan("webwatcher diff: ")}<a href=\"https://webwatcher.wtg.zone/api/v1/content/extracted/html-diff/sg_strtgc_gds_ctrl_reg/124/125\" target=\"_blank\" {standardStyle}>https://webwatcher.wtg.zone/api/v1/content/extracted/html-diff/sg_strtgc_gds_ctrl_reg/124/125</a></p><p>{WrapInSpan("diff: ")}<a href=\"https://contentservice.wtg.zone/job/diff/5235/artifact/ws/diff.html\" target=\"_blank\" {standardStyle}>https://contentservice.wtg.zone/job/diff/5235/artifact/ws/diff.html</a></p><p>{WrapInSpan("friendly diff: ")}<a href=\"https://contentservice.wtg.zone/job/diff/5235/artifact/ws/diff_friendly.html\" target=\"_blank\" {standardStyle}>https://contentservice.wtg.zone/job/diff/5235/artifact/ws/diff_friendly.html</a></p><p></p>";
		ZRichTextBox textBox = null!;
		var page = await ctx.LoadControlOnFormAsync(
			() =>
			{
				textBox = new ZRichTextBox() { Width = 1000, Height = 1000, Font = standardFont };
				textBox.Html = ORtfTextUtil.RtfToHtml(input);
				return textBox;
			}
		);
		Assert.That(textBox.Html, Is.EqualTo(expected));
	}

	[Test, WithPlaywrightPage]
	public async Task TestClickOnPopupButtonWorks()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new ZForm();
			var textBox = new ZRichTextBox() { Width = 1000 };

			form.Controls.Add(textBox);
			return form;
		});

		var toolbar = page.GetByRole(AriaRole.Toolbar);
		var btn = toolbar.GetByRole(AriaRole.Button, new() { Name = "Bold" });
		await Assertions.Expect(btn).ToBeVisibleAsync();

		await btn.ClickAsync();
	}

	[Test, WithPlaywrightPage]
	public async Task TestClickOnLinkShouldOpenNewWindow()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			var richTextBox = new ZRichTextBox()
			{
				Width = 500,
				Height = 500,
				Html = "<p><a href=\"https://proget.wtg.zone/2FDocuments%2FMicrosoft%20Teams%20Chat%20Files%2F2024%2D07%2D10%2012%2D26%2D48.mkv\">ProGet</a></p>"
			};
			form.Controls.Add(richTextBox);
			return form;
		});

		var client = await RichTextBoxClient.GetClientAsync();
		var editorBody = await client.GetEditorBodyAsync();
		var editorAnchor = page.Locator(".richtextbox__editoranchor");

		await editorAnchor.WaitForAsync();
		await editorAnchor.FocusAsync();

		var link = await editorBody.WaitForSelectorAsync("a:first-of-type");
		var popup = await page.RunAndWaitForPopupAsync(async () => await link.ClickAsync());
		Assert.That(popup.Url, Is.EqualTo("https://proget.wtg.zone/2FDocuments%2FMicrosoft%20Teams%20Chat%20Files%2F2024%2D07%2D10%2012%2D26%2D48.mkv"));
	}

	[Test, WithPlaywrightPage]
	[TestCase("<p><span style=\"font-family: 'Arial', sans-serif; font-size: 32pt;\">Size32Arial</span><span style=\"font-family: 'Calibri', sans-serif; font-size: 10pt;\">ShouldNotChooseThisText</span></p>", "Arial", "32", TestName = "{m}_NonDefaultFontName_NonDefaultFontSize")]
	[TestCase("<p><span style=\"font-family: 'Microsoft Sans Serif', sans-serif; font-size: 10pt;\">Size10MSS</span><span style=\"font-family: 'Calibri', sans-serif; font-size: 10pt;\">ShouldNotChooseThisText</span></p>", "Microsoft Sans Serif", "10", TestName = "{m}_DefaultFontName_DefaultFontSize")]
	[TestCase("<p><span style=\"font-family: 'Arial', sans-serif; font-size: 10pt;\">Size10Arial</span><span style=\"font-family: 'Calibri', sans-serif; font-size: 10pt;\">ShouldNotChooseThisText</span></p>", "Arial", "10", TestName = "{m}_NonDefaultFontName_DefaultFontSize")]
	[TestCase("<p><span style=\"font-family: 'Microsoft Sans Serif', sans-serif; font-size: 32pt;\">Size32MSS</span><span style=\"font-family: 'Calibri', sans-serif; font-size: 10pt;\">ShouldNotChooseThisText</span></p>", "Microsoft Sans Serif", "32", TestName = "{m}_DefaultFontName_NonDefaultFontSize")]
	public async Task TestZRichTextBoxAtTheTop_ShouldHaveCorrectValues_ForToolbarFontNameAndFontSize(string html, string resultFontName, string resultFontSize)
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			var richTextBox = new ZRichTextBox()
			{
				Width = 500,
				Height = 500,
				Html = html
			};
			form.Controls.Add(richTextBox);
			return form;
		});

		var fontSize = page.GetByLabel("Font sizes", new() { Exact = true });
		Assert.That(async () => await fontSize.InputValueAsync(), Is.EqualTo(resultFontSize).After(3000, 100));

		var fontName = page.GetByLabel("Fonts", new() { Exact = true });
		Assert.That(async () => await fontName.InputValueAsync(), Is.EqualTo(resultFontName).After(3000, 100));
	}

	[Test, WithPlaywrightPage]
	[TestCase("<p><span style=\"font-family: 'Arial', sans-serif; font-size: 32pt;\">Size32Arial</span><span style=\"font-family: 'Calibri', sans-serif; font-size: 10pt;\">ShouldNotChooseThisText</span></p>", TestName = "{m}_NonDefaultFontName_NonDefaultFontSize")]
	[TestCase("<p><span style=\"font-family: 'Microsoft Sans Serif', sans-serif; font-size: 10pt;\">Size10MSS</span><span style=\"font-family: 'Calibri', sans-serif; font-size: 10pt;\">ShouldNotChooseThisText</span></p>", TestName = "{m}_DefaultFontName_DefaultFontSize")]
	[TestCase("<p><span style=\"font-family: 'Arial', sans-serif; font-size: 10pt;\">Size10Arial</span><span style=\"font-family: 'Calibri', sans-serif; font-size: 10pt;\">ShouldNotChooseThisText</span></p>", TestName = "{m}_NonDefaultFontName_DefaultFontSize")]
	[TestCase("<p><span style=\"font-family: 'Microsoft Sans Serif', sans-serif; font-size: 32pt;\">Size32MSS</span><span style=\"font-family: 'Calibri', sans-serif; font-size: 10pt;\">ShouldNotChooseThisText</span></p>", TestName = "{m}_DefaultFontName_NonDefaultFontSize")]
	public async Task TestZRichTextBoxToolbarValues_ShouldNotBeChanged_ByNoneTextNode(string html)
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			var button = new TextBox() { Text = "Dummy button" };
			form.Controls.Add(button);
			var richTextBox = new ZRichTextBox()
			{
				Width = 500,
				Height = 500,
				Html = html
			};
			form.Controls.Add(richTextBox);
			return form;
		});
		var editorAnchor = page.Locator(".richtextbox__editoranchor");
		await editorAnchor.WaitForAsync();
		var toolbar = page.GetByRole(AriaRole.Toolbar);
		await toolbar.WaitForAsync();	
		// this test is used to simiulate this behaviour: open a work item > go to workflow & tracking tab > go back to Details tab
		// the toolbar font size should not be 8, which is the size of the tab node.
		var currentSelectionNodeType =
			await page.EvaluateAsync<string>("() => window.getSelection().anchorNode.nodeType");
		Assert.That(currentSelectionNodeType, Is.Not.EqualTo("3")); // not of Node.TEXT_NODE

		var fontName = page.GetByLabel("Fonts", new() { Exact = true });
		Assert.That(async () => await fontName.InputValueAsync(), Is.EqualTo("Microsoft Sans Serif").After(3000, 100));
		// NOTE: currently, this test assert these toolbar values to their default values.
		// If we find a way to set the font and fontsize on toolbar to where the caret was when we just back from another tab or ZRichtTextBoxPopupForm, we can change the assert accordingly
		var fontSize = page.GetByLabel("Font sizes", new() { Exact = true });
		Assert.That(async () => await fontSize.InputValueAsync(), Is.EqualTo("10").After(3000, 100));
	}

	[Test]
	public async Task ZRichTextBoxHasCorrectOverflow()
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() => new ZRichTextBox());
		var richTextBox = rendered.Find("[data-name=\"ZRichTextBox\"]");

		Assert.That(richTextBox.GetAttribute("style"), Does.Contain("overflow:hidden"));
	}

	[Test]
	public async Task ZRichTextBoxHasPopupButtonRendered()
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var richTextBox = new ZRichTextBox();
			return richTextBox;
		});
		var popupButton = rendered.Find(".button--standard");

		Assert.That(popupButton, Is.Not.Null);
	}

	[Test]
	public async Task ZRichTextBox_PopupButton_ReadonlyShouldMatchRichTextBox([Values] bool isReadOnly)
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var richTextBox = new ZRichTextBox();
			richTextBox.ReadOnly = isReadOnly;
			Assert.That(richTextBox.PopupButton.ReadOnly, Is.EqualTo(isReadOnly));
			return richTextBox;
		});
	}

	[Test, WithPlaywrightPage]
	public async Task ZRichTextBoxPopupFormHasPopupButtonRendered()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		ZRichTextBox richTextBox = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new WinzorTestForm { Width = 650, Height = 500 };
			richTextBox = new ZRichTextBox()
			{
				Width = 650,
				Height = 500
			};
			richTextBox.IsPopupButtonVisible = false;
			form.Controls.Add(richTextBox);
			return form;
		});

		var toolbarLocator = page.Locator(".richtextbox__toolbar");
		await toolbarLocator.WaitForAsync();

		var popupButton = await page.WaitForSelectorAsync(".button.button--standard", new() { State = WaitForSelectorState.Detached, Timeout = 1000 });
		Assert.That(popupButton, Is.Null);
	}

	[Test, WithPlaywrightPage]
	[TestCase(true, 0, TestName = "{m}_Toolbar_NoExtraSpace")]
	[TestCase(false, 0, TestName = "{m}_NoToolbar_NoExtraSpace")]
	[TestCase(false, 32, TestName = "{m}_NoToolbar_ExtraSpace")]
	public async Task ZRichTextBoxToolBarTests(bool isToolBarVisible, int top)
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new ZForm();
			var textBox = new ZRichTextBox()
			{
				Top = top,
				IsToolBarVisible = isToolBarVisible
			};
			form.Controls.Add(textBox);
			return form;
		});

		var richEdit = await page.WaitForSelectorAsync(".richtextbox");
		var richEditBoundingBox = await richEdit.BoundingBoxAsync();
		var toolbar = page.GetByRole(AriaRole.Toolbar);

		if (isToolBarVisible)
		{
			await Assertions.Expect(toolbar).ToBeVisibleAsync();
		}
		else
		{
			await Assertions.Expect(toolbar).ToBeHiddenAsync();
		}

		Assert.That(richEditBoundingBox.Y, Is.EqualTo(top));
	}

	[Test, WithPlaywrightPage]
	public async Task ZRichTextBox_PopupButton_AboveToolbar()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new ZForm();
			var textBox = new ZRichTextBox() { Width = 1000 };
			form.Controls.Add(textBox);
			return form;
		});

		var client = await RichTextBoxClient.GetClientAsync();
		await client.FocusEditorAsync();

		var toolbar = page.Locator(".richtextbox__toolbar");
		await Assertions.Expect(toolbar).ToBeAttachedAsync();
		var toolbarZIndexIsInteger = int.TryParse(await toolbar.GetComputedStyleAsync("z-index"), out int toolbarZIndex);

		var popupButton = page.Locator(".button--standard");
		Assert.That(popupButton, Is.Not.Null);
		var popupZIndex = int.Parse((await popupButton.GetComputedStyleAsync("z-index")).Raw);

		// Currently, the toolbarZIndexString == auto, so this if will never be entered.
		// Instead the else will be entered, and we want to ensure that the popupZIndex > 0 (as auto is 0)
		// The if else will be left here for any future refactor.
		if (toolbarZIndexIsInteger)
		{
			Assert.That(popupZIndex, Is.GreaterThan(toolbarZIndex));
		}
		else
		{
			Assert.That(popupZIndex, Is.GreaterThan(0));
		}
	}

	[Test, WithPlaywrightPage]
	public async Task ZRichTextBoxDataInserterInsertsAtSelection()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		ZRichTextBox richTextBox = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new WinzorTestForm();
			richTextBox = new ZRichTextBox()
			{
				Width = 500,
				Height = 500
			};
			richTextBox.RichEdit.Text = "Test Content";
			form.Controls.Add(richTextBox);
			return form;
		});

		var contentLocator = page.Locator(".richtextbox__editoranchor");
		await contentLocator.WaitForAsync();

		var client = await RichTextBoxClient.GetClientAsync();
		Assert.That(client.GetActiveTextAsync, Is.EqualTo("Test Content"));

		await richTextBox.InvokeWinzorDispatcherAsync(() =>
		{
			richTextBox.SelectionStart = 4;
			richTextBox.SelectionLength = 1;
		});

		var editorContent = await client.EvaluateAsync<string>("client => client.getEditorContent()");
		Assert.That(editorContent, Is.EqualTo("<p>Test Content</p>"));

		Assert.That(async () => await client.EvaluateAsync<int>($"client => client.getSelection().start;"), Is.EqualTo(4).After(3000, 100));
		Assert.That(async () => await client.EvaluateAsync<int>($"client => client.getSelection().end;"), Is.EqualTo(5).After(3000, 100));

		await richTextBox.InvokeWinzorDispatcherAsync(() =>
		{
			richTextBox.InsertObject(new DataObject(DataFormats.Rtf, @"{\rtf\line inserted\line}"));
		});

		Assert.That(async () => await client.EvaluateAsync<string>("client => client.getEditorText();"), Is.EqualTo("Test\n\n\ninserted\n\n\n\nContent").After(3000, 100));
		Assert.That(async () => await client.EvaluateAsync<int>($"client => client.getSelection().start;"), Is.EqualTo(15).After(3000, 100));
		Assert.That(async () => await client.EvaluateAsync<int>($"client => client.getSelection().end;"), Is.EqualTo(15).After(3000, 100));
	}

	[Test]
	public async Task ZRichTextBox_Toolbar_Visible([Values] bool isToolBarVisible)
	{
		using var ctx = new EnterpriseTestContext();
		_ = await ctx.RenderControlOnFormAsync(() =>
		{
			var richTextBox = new ZRichTextBox();
			Assert.That(richTextBox.RichEdit.IsToolBarVisible, Is.True, "Default value is True");
			richTextBox.IsToolBarVisible = isToolBarVisible;
			Assert.That(richTextBox.RichEdit.IsToolBarVisible, Is.EqualTo(isToolBarVisible));
			return richTextBox;
		});
	}

	class TextModel : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;
		void NotifyPropertyChanged(string propertyName = "")
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		ZBlob name = ZBlob.Empty;
		public ZBlob Name_HTML
		{
			get
			{
				return this.name;
			}

			set
			{
				if (value != this.name)
				{
					this.name = value;
					NotifyPropertyChanged();
				}
			}
		}
	}

	[Test, WithPlaywrightPage]
	public async Task TextBindingChangeShouldClearUndoManager()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		ZRichTextBox richTextBox = null;
		var textModel = new TextModel { Name_HTML = ZBlob.FromAscii("1") };

		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			richTextBox = new ZRichTextBox()
			{
				Width = 500,
				Height = 500,
			};
			richTextBox.SetDataBinding(textModel, "Name");
			return richTextBox;
		});

		var client = await RichTextBoxClient.GetClientAsync();
		Assert.That(client.GetActiveTextAsync, Is.EqualTo("1"));

		await richTextBox.InvokeWinzorDispatcherAsync(() => textModel.Name_HTML = ZBlob.FromAscii("2"));
		Assert.That(client.GetActiveTextAsync, Is.EqualTo("2"));

		var editorBody = await client.GetEditorBodyAsync();
		await editorBody.PressAsync("Control+Z");
		Assert.That(client.GetActiveTextAsync, Is.EqualTo("2"));
	}

	[Test, WithPlaywrightPage, WithTransaction]
	public async Task ZRichTextBoxSupportF5InsertTimestamp()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new ZForm();
			var textBox = new ZRichTextBox() { Width = 1000 };
			form.Controls.Add(textBox);
			return form;
		});
		var client = await RichTextBoxClient.GetClientAsync();
		var editorBody = await client.GetEditorBodyAsync();
		await client.FocusEditorAsync();
		await editorBody.PressAsync("F5");
		Assert.That(async () => (await editorBody.InnerTextAsync()).Trim(), Is.EqualTo(EnvProxy.Instance.CurrentUser.InitialsAndDateTimeGmt.Trim()).After(500, 50));
	}

	[Test, WithPlaywrightPage, WithTransaction]
	public async Task ZRichTextBoxSupportF5InsertTimestampWhenDisplayGMTOffsetOnF5UserInfoIsFalse()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			RawDataRegistry.Instance.DisplayGMTOffsetOnF5UserInfo.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var form = new ZForm();
			var textBox = new ZRichTextBox() { Width = 1000 };
			form.Controls.Add(textBox);
			return form;
		});
		var client = await RichTextBoxClient.GetClientAsync();
		var editorBody = await client.GetEditorBodyAsync();
		await client.FocusEditorAsync();
		await editorBody.PressAsync("F5");
		Assert.That(async () => (await editorBody.InnerTextAsync()).Trim(), Is.EqualTo(EnvProxy.Instance.CurrentUser.InitialsAndDateTime.Trim()).After(500, 50));
	}

	[WithPlaywrightPage, WithTransaction]
	[TestCase("plaintext-only")]
	[TestCase("false")]
	[TestCase("true")]
	public async Task ZRichTextBoxSupportF5InsertTimestampWhenContentEditableNotFalse(string contentEditable)
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			RawDataRegistry.Instance.DisplayGMTOffsetOnF5UserInfo.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var form = new ZForm();
			var textBox = new ZRichTextBox() { Width = 1000 };
			form.Controls.Add(textBox);
			return form;
		});
		var client = await RichTextBoxClient.GetClientAsync();
		var editorBody = await client.GetEditorBodyAsync();
		var editorAnchor = page.Locator(".richtextbox__editoranchor");
		await editorAnchor.WaitForAsync();
		await editorAnchor.EvaluateAsync($"e => e.contentEditable = '{contentEditable}'");
		Assert.That(await editorAnchor.GetAttributeAsync("contentEditable"), Is.EqualTo(contentEditable));

		await client.FocusEditorAsync();
		await editorBody.PressAsync("F5");
		if (contentEditable == "false")
		{
			Assert.That(async () => (await editorBody.InnerTextAsync()).Trim(), Is.EqualTo(string.Empty).After(1000, 100));
		}
		else
		{
			Assert.That(async () => (await editorBody.InnerTextAsync()).Trim(), Is.EqualTo(EnvProxy.Instance.CurrentUser.InitialsAndDateTime.Trim()).After(1000, 100));
		}
	}

	[Test, WithPlaywrightPage, WithTransaction]
	public async Task ZRichTextBoxSupportF5InsertTimestampOnceAfterSwitchTab()
	{
		ZRichTextBox richtextbox1 = null;
		ZRichTextBox richtextbox2 = null;
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			RawDataRegistry.Instance.DisplayGMTOffsetOnF5UserInfo.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var form = new Form();
			var tabControl = new ZTabControl();
			form.Controls.Add(tabControl);

			var tabPage1 = new ZTabPage() { Name = "Tab1", Text = "Tab 1" };
			richtextbox1 = new ZRichTextBox()
			{
				Width = 700,
				Height = 500
			};
			tabPage1.Controls.Add(richtextbox1);
			var tabPage2 = new ZTabPage() { Name = "Tab2", Text = "Tab 2" };
			richtextbox2 = new ZRichTextBox()
			{
				Width = 700,
				Height = 500
			};
			tabPage2.Controls.Add(richtextbox2);
			tabControl.Controls.Add(tabPage1);
			tabControl.Controls.Add(tabPage2);
			return form;
		});

		var contentLocator = page.Locator(".richtextbox__editoranchor");

		var tab2Button = page.Locator("button", new PageLocatorOptions() { HasTextString = "Tab 2" });
		await tab2Button.ClickAsync();
		await contentLocator.WaitForAsync();

		var tab1Button = page.Locator("button", new PageLocatorOptions() { HasTextString = "Tab 1" });
		await tab1Button.ClickAsync();
		await contentLocator.WaitForAsync();

		await Assertions.Expect(contentLocator).ToHaveCountAsync(1);

		var client1 = await RichTextBoxClient.GetClientAsync(richtextbox1.RichEdit);
		var editorBodyInClient1 = await client1.GetEditorBodyAsync();
		await client1.FocusEditorAsync();
		await editorBodyInClient1.PressAsync("F5");

		var initialText = EnvProxy.Instance.CurrentUser.InitialsAndDateTime.Trim();
		await Assertions.Expect(contentLocator).ToHaveTextAsync(initialText);

		await tab2Button.ClickAsync();
		var client2 = await RichTextBoxClient.GetClientAsync(richtextbox2.RichEdit);
		await client2.FocusEditorAsync();
		var editorBodyInClient2 = await client2.GetEditorBodyAsync();
		await editorBodyInClient2.PressAsync("F5");

		var initialText2 = EnvProxy.Instance.CurrentUser.InitialsAndDateTime.Trim();
		await Assertions.Expect(contentLocator).ToHaveTextAsync(initialText2);
	}

	[TestCase(3, 1)]
	[TestCase(3, 3)]
	[TestCase(3, 2)]
	[WithPlaywrightPage, WithTransaction]
	public async Task F5InsertTimestampOnDifferentLine(int numberOfLines, int expectedStampline)
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var focusTriggered = false;
		var timestamp = string.Empty;

		var page = await ctx.LoadFormAsync(() =>
		{
			RawDataRegistry.Instance.DisplayGMTOffsetOnF5UserInfo.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var form = new ZForm();
			var textBox = new ZRichTextBox() { Width = 1000 };
			textBox.RichEdit.LostFocus += (s, e) =>
			{
				focusTriggered = true;
			};
			form.Controls.Add(textBox);
			form.Controls.Add(new TextBox());
			return form;
		});
		var client = await RichTextBoxClient.GetClientAsync();
		var editorBody = await client.GetEditorBodyAsync();
		await client.FocusEditorAsync();

		for (var i = 0; i < numberOfLines; i++)
		{
			if (i == expectedStampline - 1)
			{
				await editorBody.PressAsync("F5");
				timestamp = EnvProxy.Instance.CurrentUser.InitialsAndDateTime.Trim();
			}
			else
			{
				await page.Keyboard.TypeAsync($"Line{i + 1}");
			}
			await page.WaitForTimeoutAsync(500);
			await page.Keyboard.PressAsync("Enter");
		}

		await page.Locator(".textbox").FocusAsync();

		Assert.That(() => focusTriggered, Is.True.After(500, 100));

		var paragraphs = await page.QuerySelectorAllAsync("p");
		var expectedText = string.Empty;

		await Assert.MultipleAsync(async () =>
		{
			for (var i = 0; i < numberOfLines; i++)
			{
				if (i == expectedStampline - 1)
				{
					expectedText = timestamp;
				}
				else
				{
					expectedText = $"Line{i + 1}";
				}
				Assert.That((await paragraphs[i].InnerTextAsync()).TrimEnd(), Is.EqualTo(expectedText));
			}
		});
	}

	[Test, WithPlaywrightPage]
	public async Task ZRichTextBoxSupportShortcutsWhenContentEditable()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var richTextBox = default(ZRichTextBox);
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new ZForm() { Width = 700, Height = 500 };
			richTextBox = new ZRichTextBox() { Width = 700, Height = 500 };
			form.Controls.Add(richTextBox);
			return form;
		});
		var client = await RichTextBoxClient.GetClientAsync();
		var editorBody = await client.GetEditorBodyAsync();
		var editor = page.Locator(".richtextbox__editoranchor");
		Assert.That(await editor.GetAttributeAsync("contenteditable"), Is.EqualTo("true"));

		await editor.FocusAsync();
		await page.Keyboard.TypeAsync("some text");
		await editorBody.PressAsync("Control+A");
		await editorBody.PressAsync("Control+B");
		Assert.That(async () => await editorBody.InnerHTMLAsync(), Does.Match("<p><span style=\"font-weight: bold;\">some text</span></p>"));

		await editorBody.PressAsync("Control+I");
		Assert.That(async () => await editorBody.InnerHTMLAsync(), Does.Match("<p><span style=\"font-weight: bold; font-style: italic;\">some text</span></p>"));

		await editorBody.PressAsync("Control+U");
		Assert.That(async () => await editorBody.InnerHTMLAsync(), Does.Match("<p><span style=\"font-weight: bold; font-style: italic; text-decoration-line: underline;\">some text</span></p>"));
	}

	[Test, WithPlaywrightPage]
	public async Task ShiftTabOutOfRichTextBox()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		TextBox textBox = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new ZForm();
			textBox = new TextBox() { Top = 0, TabIndex = 0 };
			form.Controls.Add(textBox);
			form.Controls.Add(new ZRichTextBox() { Top = 50, TabIndex = 1 });
			return form;
		});
		var client = await RichTextBoxClient.GetClientAsync();
		var editorBody = await client.GetEditorBodyAsync();
		await client.FocusEditorAsync();
		await editorBody.PressAsync("Shift+Tab");

		Assert.That(() => textBox.Focused, Is.True.After(2000, 100));
	}

	[Test, WithPlaywrightPage, WithTransaction]
	public async Task F5InsertOnSelection()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			RawDataRegistry.Instance.DisplayGMTOffsetOnF5UserInfo.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var form = new ZForm();
			var textBox = new ZRichTextBox() { Width = 1000 };
			form.Controls.Add(textBox);
			return form;
		});

		var client = await RichTextBoxClient.GetClientAsync();
		var editorBody = await client.GetEditorBodyAsync();
		await client.FocusEditorAsync();

		await page.Keyboard.TypeAsync("its a good day today");
		await editorBody.PressAsync("Control+A");
		await editorBody.PressAsync("F5");

		var timestamp = EnvProxy.Instance.CurrentUser.InitialsAndDateTime.Trim();

		Assert.That(await editorBody.InnerHTMLAsync(), Is.EqualTo($"<p>{timestamp} </p>"));
	}

	[Test, WithPlaywrightPage, WithTransaction]
	public async Task F5ShouldNotTriggerSyncOfClientContent()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var bindingObj = new TextModel { Name_HTML = ZBlob.Empty };
		ZRichTextBox richTextBox = null;
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			RawDataRegistry.Instance.DisplayGMTOffsetOnF5UserInfo.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			richTextBox = new ZRichTextBox() { Font = standardFont };
			richTextBox.SetDataBinding(bindingObj, "Name");
			return richTextBox;
		});

		// If the focus out event is fired on the server, it will trigger a validating event
		var validatingRaised = new TaskCompletionSource();
		richTextBox.Validating += (_, _) => validatingRaised.SetResult();

		var editor = page.Locator(".richtextbox__editoranchor");
		await editor.FocusAsync();

		await page.Keyboard.TypeAsync("Hello!");
		Assert.That(async () => await editor.InnerTextAsync(), Is.EqualTo("Hello!").After(3000, 100));
		Assert.That(() => richTextBox.RichEdit.ContentIsOutOfSyncWithClient, Is.True.After(3000, 100));

		var timestamp = EnvProxy.Instance.CurrentUser.InitialsAndDateTime;
		await editor.PressAsync("F5");

		Assert.That(async () => await editor.InnerTextAsync(), Is.EqualTo($"Hello!{timestamp}").After(3000, 100));
		Assert.That(await validatingRaised.Task.WithTimeout(TimeSpan.FromSeconds(3)), Is.False);
		Assert.That(() => richTextBox.RichEdit.ContentIsOutOfSyncWithClient, Is.True.After(3000, 100));

		await page.Keyboard.TypeAsync("Goodbye!");
		Assert.That(async () => await editor.InnerTextAsync(), Is.EqualTo($"Hello!{timestamp}Goodbye!").After(3000, 100));
		Assert.That(() => richTextBox.RichEdit.ContentIsOutOfSyncWithClient, Is.True.After(3000, 100));

		// Simulate a Ctrl+S action and check all the content is saved correctly
		await richTextBox.InvokeWinzorDispatcherAsync(() => richTextBox.FindForm().Validate());
		Assert.That(bindingObj.Name_HTML.ToUTF8().ToString(), Is.EqualTo(WrapInP($"Hello!{timestamp}Goodbye!")));
	}

	[Test, WithPlaywrightPage, WithTransaction]
	public async Task CtrlSShouldSaveContentWithLongContent()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var bindingObj = new TextModel { Name_HTML = ZBlob.Empty };
		ZRichTextBox richTextBox = null;

		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			RawDataRegistry.Instance.DisplayGMTOffsetOnF5UserInfo.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			richTextBox = new ZRichTextBox();
			richTextBox.Font = standardFont;
			richTextBox.SetDataBinding(bindingObj, "Name");
			return richTextBox;
		});

		var validatingRaised = new TaskCompletionSource();
		richTextBox.Validating += (_, _) => validatingRaised.SetResult();

		var editor = page.Locator(".richtextbox__editoranchor");
		await editor.FocusAsync();

		string longContent = new string('A', 10000);
		await page.Keyboard.TypeAsync(longContent);

		Assert.That(async () => await editor.InnerTextAsync(), Is.EqualTo(longContent).After(3000, 100));

		Assert.That(() => richTextBox.RichEdit.ContentIsOutOfSyncWithClient, Is.True.After(3000, 100));

		await richTextBox.InvokeWinzorDispatcherAsync(() => richTextBox.FindForm().Validate());

		Assert.That(bindingObj.Name_HTML.ToUTF8().ToString(), Is.EqualTo(WrapInP(longContent)).After(3000, 100));
	}

	[WithPlaywrightPage]
	[TestCase(true, false, false)]
	[TestCase(false, true, false)]
	[TestCase(false, false, true)]
	public async Task RTBDoesNotInsertUserInfoWhenPressingF5WithModKeys(bool hasCtrl, bool hasAlt, bool hasShift)
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadControlOnFormAsync(() => new ZRichTextBox());

		var client = await RichTextBoxClient.GetClientAsync();
		var editorBody = await client.GetEditorBodyAsync();
		await editorBody.PressAsync(GetKeyCombinationString(hasCtrl, hasAlt, hasShift));
		await Task.Delay(100);
		var content = (await editorBody.InnerTextAsync()).Trim();
		Assert.That(content, Is.Empty);
	}

	[Test, WithPlaywrightPage, WithTransaction]
	public async Task RTBShouldInsertCorrectFontStyleWhenPressingF5()
	{
		ZRichTextBox richTextBox = null;
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			RawDataRegistry.Instance.DisplayGMTOffsetOnF5UserInfo.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			richTextBox = new ZRichTextBox()
			{
				Html = @"<p><span style=""font-family: Lucida Handwriting, sans-serif; font-size: 20px;"">abc</span><span style=""font-family: Jokerman, sans-serif; font-size: 20px;"">def</span></p>"
			};
			return richTextBox;
		});

		Assert.That(richTextBox, Is.Not.Null);
		var client = await RichTextBoxClient.GetClientAsync();
		var editorBody = await client.GetEditorBodyAsync();
		await client.FocusEditorAsync();

		var userAndTimestamp = EnvProxy.Instance.CurrentUser.InitialsAndDateTime;

		await editorBody.PressAsync("F5");
		Assert.That(async () => await editorBody.InnerHTMLAsync(), Does.Match(@$"<p><span style=""font-family: 'Lucida Handwriting', sans-serif; font-size: 20px;"">{userAndTimestamp}abc</span><span style=""font-family: Jokerman, sans-serif; font-size: 20px;"">def</span></p>").After(3000, 100));

		await page.Keyboard.PressAsync("Control+End");
		await editorBody.PressAsync("F5");
		Assert.That(async () => await editorBody.InnerHTMLAsync(), Does.Match(@$"<p><span style=""font-family: 'Lucida Handwriting', sans-serif; font-size: 20px;"">{userAndTimestamp}abc</span><span style=""font-family: Jokerman, sans-serif; font-size: 20px;"">def{userAndTimestamp}</span></p>").After(3000, 100));

		var text = (await editorBody.InnerTextAsync()).Trim();
		await client.EvaluateAsync($"client => client.setSelection({{ start: {text.Length / 2 + 1}, end: {text.Length / 2 + 1} }});");
		await editorBody.PressAsync("F5");
		Assert.That(async () => await editorBody.InnerHTMLAsync(), Does.Match(@$"<p><span style=""font-family: 'Lucida Handwriting', sans-serif; font-size: 20px;"">{userAndTimestamp}abc{userAndTimestamp}</span><span style=""font-family: Jokerman, sans-serif; font-size: 20px;"">def{userAndTimestamp}</span></p>").After(3000, 100));
	}

	string GetKeyCombinationString(bool hasCtrl, bool hasAlt, bool hasShift)
	{
		var sb = new StringBuilder();
		if (hasCtrl)
		{
			sb.Append("Control+");
		}
		if (hasAlt)
		{
			sb.Append("Alt+");
		}
		if (hasShift)
		{
			sb.Append("Shift+");
		}
		sb.Append("F5");
		return sb.ToString();
	}

	[Test, WithPlaywrightPage]
	public async Task SetFontSizeDoesNotSetFontSizeForNonRtbElement()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		ZRichTextBox richTextBox = null!;
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			var codeFindBox = new ZCodeFindBox();
			richTextBox = new ZRichTextBox() { Width = 500, Height = 500, Text = "Enlarge me please :)", Top = 50 };
			form.Controls.Add(codeFindBox);
			form.Controls.Add(richTextBox);
			return form;
		});

		var client = await RichTextBoxClient.GetClientAsync();
		await client.FocusEditorAsync();
		var sizeInput = page.GetByLabel("Font sizes", new() { Exact = true });
		await page.ClickAsync(".richtextbox__editoranchor");
		await page.Keyboard.TypeAsync("Enlarge me please :)");
		await page.Keyboard.PressAsync("Control+A");
		await sizeInput!.FillAsync("32");
		await page.Keyboard.PressAsync("Enter");
		Assert.That(await client.GetActiveHtmlAsync(), Is.EqualTo("<p><span style=\"font-size: 32pt;\">Enlarge me please :)</span></p>"));

		var codeFindBox = page.Locator("input").First;
		await codeFindBox.FocusAsync();
		await page.Keyboard.TypeAsync("Don't enlarge me please :(");
		await sizeInput!.FillAsync("96");
		await page.Keyboard.PressAsync("Enter");

		// The buggy setFontSize modified the bounding div's font size,
		// rather than just the focused element.
		var form = page.Locator("div.form");
		Assert.That(async () => await form.EvaluateAsync<string>("(e) => e.style.fontSize"), Is.EqualTo("8pt").After(2000, 100));
		Assert.That(await client.GetActiveHtmlAsync(), Is.EqualTo("<p><span style=\"font-size: 96pt;\">Enlarge me please :)</span></p>"));
	}

	[Test, WithPlaywrightPage]
	public async Task ZRichTextBoxEnterKeyPreventedWhileContextMenuOpened()
	{
		ILocator editor = null;
		var clientServiceProvider = new MockCargoWiseClientSeviceProvider();
		var contextMenuOpened = new TaskCompletionSource();
		clientServiceProvider.MockMenuDisplayer
			.Setup(o => o.SendShowMenuRequestAsync(It.IsAny<MenuInteropModel>(), It.IsAny<Func<SubMenuLoadRequest, Task<MenuItemInteropModel[]>>>(), It.IsAny<Func<MenuClosedResult, Task>>()))
			.Callback<MenuInteropModel, Func<SubMenuLoadRequest, Task<MenuItemInteropModel[]>>, Func<MenuClosedResult, Task>>((menu, _, _) =>
			{
				if (menu.MenuType == MenuType.ContextMenu)
				{
					// Simulate the context menu opening effect in the client app, since we can't use menu interop to open context menu in Playwright.
					editor.BlurAsync().GetAwaiter().GetResult();
					contextMenuOpened.SetResult();
				}
			});

		await using var ctx = new InMemoryAppServerTestContext(clientServiceProvider);
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new ZForm();
			var textBox = new ZRichTextBox() { Width = 1000, Height = 1000, ContextMenuStrip = new ContextMenuStrip() };
			form.Controls.Add(textBox);
			return form;
		});

		editor = await WaitForEditorAsync(page, "Start!\n");

		await page.Mouse.ClickAsync(100, 100, new MouseClickOptions() { Button = MouseButton.Left });

		await page.WaitForTimeoutAsync(200);
		await page.Keyboard.TypeAsync("Hello");

		await page.WaitForTimeoutAsync(200);
		await page.Keyboard.PressAsync("Enter");
		await page.Mouse.ClickAsync(100, 100, new MouseClickOptions() { Button = MouseButton.Right });

		await contextMenuOpened.Task;
		await page.Keyboard.PressAsync("Enter");

		await page.WaitForTimeoutAsync(200);
		await page.Keyboard.PressAsync("Enter");
		await page.Mouse.ClickAsync(100, 100, new MouseClickOptions() { Button = MouseButton.Left });

		await page.WaitForTimeoutAsync(200);
		await page.Keyboard.TypeAsync("Hello 2");

		await Assertions.Expect(editor.Locator("p").Nth(1)).ToHaveTextAsync("Hello");
		await Assertions.Expect(editor.Locator("p").Nth(2)).ToHaveTextAsync("Hello 2");
	}

	static IEnumerable<TestCaseData> PasteTextOnlyTestCases
	{
		get
		{
			yield return new TestCaseData(new[] {
				new JSClipboardData("text/html", "<p>This is some HTML content which should NOT be pasted</p>"),
				new JSClipboardData("text/plain", "This is the plain text content which should be pasted"),
			}, "insertHTML", "<p>This is the plain text content which should be pasted</p>")
			{ TestName = "{m}_WithHTMLandText_TextShouldBeInserted" };
			yield return new TestCaseData(new[] {
				new JSClipboardData("text/html", "<p><span style=\"font-size: 13.3333px; font-style: italic; font-weight: 700; text-align: left;\">This is text with some formatting which should NOT be pasted!</span></p>"),
				new JSClipboardData("text/plain", "This is the plain text content which should be pasted"),
			}, "insertHTML", "<p>This is the plain text content which should be pasted</p>")
			{ TestName = "{m}_WithFormattedHTMLandText_TextShouldBeInserted" };
			yield return new TestCaseData(new[] {
				new JSClipboardData("text/html", "<p><span style=\"font-size: 13.3333px; font-style: italic; font-weight: 700; text-align: left;\">This is text with some formatting which will be removed :(</span></p>"),
			}, "insertHTML", "<p>This is text with some formatting which will be removed :(</p>")
			{ TestName = "{m}_WithHTML_HTMLShouldBeInsertedAsPlainText" };
			yield return new TestCaseData(new[] {
				new JSClipboardData("text/plain", "This is the plain text which should be pasted"),
			}, "insertHTML", "<p>This is the plain text which should be pasted</p>")
			{ TestName = "{m}_WithText_TextShouldBeInserted" };
			yield return new TestCaseData(new[] {
				new JSClipboardData("text/html", "<p>Line 1</p><p>Line 2</p>"),
				new JSClipboardData("text/plain", "Line 1\r\n\r\nLine 2"),
			}, "insertHTML", "<p>Line 1</p><p>Line 2</p>")
			{ TestName = "{m}_WithMultipleLines" };
		}
	}

	[TestCaseSource(nameof(PasteTextOnlyTestCases)), WithPlaywrightPage]
	public async Task ZRichTextBoxPasteTextOnly(JSClipboardData[] clipboardData, string expectedCommand, string expectedContent)
	{
		string initialText = WrapInP("Lorem ipsum dolor sit amet, consectetur adipiscing elit.") + WrapInP("Mauris varius lectus at enim gravida accumsan.");

		await using var ctx = new InMemoryAppServerTestContext();
		ZRichTextBox richTextBox = null!;
		var page = await ctx.LoadControlOnFormAsync(() => richTextBox = new ZRichTextBox { Font = standardFont, Html = initialText });
		Assert.That(richTextBox, Is.Not.Null);

		var editor = page.Locator(".richtextbox__editoranchor");
		Assert.That(async () => await editor.InnerHTMLAsync(), Is.EqualTo(initialText).After(3000, 100));

		// Mock the clipboard read API as clipboard is unreliable in DAT
		await page.MockClipboardRead(clipboardData);
		await page.AttachExecCommandListener();

		await editor.FocusAsync();
		await editor.SelectTextAsync();
		await richTextBox.InvokeWinzorDispatcherAsync(() => richTextBox.PasteTextOnly());
		Assert.That(async () => await page.ExecCommandInvocations(), Has.One.EqualTo(expectedCommand).After(2000, 100));
		Assert.That(async () => await editor.InnerHTMLAsync(), Is.EqualTo(expectedContent));
	}

	static IEnumerable<TestCaseData> SetFontSizeOnMultipleElementsTestData()
	{
		yield return new TestCaseData(
			$"<p>{WrapInSpan("line one")}</p><p><br></p><ul><li>{WrapInSpan("item one")}</li></ul><p><br></p>",
			5, 15, 96,
			$"<p>{WrapInSpan("line ")}<span style=\"font-family: Tahoma, sans-serif; font-size: 96pt;\">one</span></p><p><span style=\"font-size: 96pt;\"><br></span></p><ul><li><span style=\"font-family: Tahoma, sans-serif; font-size: 96pt;\">item </span>{WrapInSpan("one")}</li></ul><p><br></p>")
		{
			TestName = "{m}_SameFontSize"
		};
		yield return new TestCaseData(
			$"<p>{WrapInSpan("a")}<span style=\"font-family: Tahoma, sans-serif; font-size:13px;\">b</span><span style=\"font-family: Tahoma, sans-serif; font-size:16px;\">a</span><span style=\"font-family: Tahoma, sans-serif; font-size:9px;\">c</span><span style=\"font-family: Tahoma, sans-serif; font-size:16px;\">u</span>{WrapInSpan("s ")}<br></p>",
			0, 6, 10,
			$"<p><span style=\"font-size: 10pt;\"><span style=\"font-family: Tahoma, sans-serif;\">abacus</span></span>{WrapInSpan(" ")}</p>")
		{
			TestName = "{m}_DifferentFontSize"
		};
	}

	[WithPlaywrightPage]
	[TestCaseSource(nameof(SetFontSizeOnMultipleElementsTestData))]
	public async Task SetFontSizeOnMultipleElements(string originalHtml, int selectionStart, int selectionEnd, int newFontSize, string expectedHtml)
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadControlOnFormAsync(() => new ZRichTextBox()
		{
			Width = 500,
			Height = 500,
			Html = originalHtml,
			IsToolBarVisible = true
		});

		var sizeSelector = page.GetByLabel("Font sizes", new() { Exact = true });
		await sizeSelector.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
		Assert.That(async () => await sizeSelector.InputValueAsync(), Is.Not.EqualTo(string.Empty).After(3000, 100));

		var client = await RichTextBoxClient.GetClientAsync();
		await client.FocusEditorAsync();

		await client.EvaluateAsync($@"client => {{client.setSelection({{ start: {selectionStart}, end: {selectionEnd}}});}}");
		await sizeSelector!.FillAsync(newFontSize.ToString());
		await sizeSelector!.PressAsync("Enter");
		Assert.That(client.GetEditorHtmlAsync, Is.EqualTo(expectedHtml).After(1000, 100));
	}

	[WithPlaywrightPage]
	[TestCase("Microsoft Sans Serif", "Microsoft Sans Serif", TestName = "{m}_MicrosoftSansSerif")]
	[TestCase("Microsoft Serif", "Microsoft Sans Serif", TestName = "{m}_MicrosoftSerif")]
	[TestCase("Courier New", "Courier New", TestName = "{m}_CourierNew")]
	[TestCase("Courier", "Microsoft Sans Serif", TestName = "{m}_Courier")]
	public async Task SetFontNameNotOnDropdownList(string newFontName, string expectedFontName)
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadControlOnFormAsync(() => new ZRichTextBox()
		{
			Width = 500,
			Height = 500,
			Html = "<p>line one</p><p><br></p><ul><li>item one</li></ul><p><br></p>",
			IsToolBarVisible = true
		});

		var fonts = page.GetByLabel("Fonts", new() { Exact = true });
		Assert.That(fonts, Is.Not.Null);

		var client = await RichTextBoxClient.GetClientAsync();
		await client.FocusEditorAsync();

		await client.EvaluateAsync("client => {client.setSelection({ start: 5, end: 15});}");
		await fonts!.FillAsync(newFontName);
		await fonts!.PressAsync("Enter");

		Assert.That(await fonts.InputValueAsync(), Is.EqualTo(expectedFontName));
	}

	[WithPlaywrightPage]
	[TestCase("<p><span style=\"font-size:21px;\">Single element </span></p>", 5, 5, "15.8", TestName = "{m}_SelectEmptyRange")]
	[TestCase("<ul style=\"font-size:10pt\"><li style=\"font-size:16px;\">Single element</li></ul>", 0, 14, "12", TestName = "{m}_SelectWholeElement")]
	[TestCase("<p><span style=\"font-size:12pt;\">Single element</span></p>", 4, 8, "12", TestName = "{m}_SelectPartialElement")]
	[TestCase("<p><br></p><p style=\"font-size: 12pt;\"><br></p>", 1, 1, "12", TestName = "{m}_SelectAfterLineBreak")]
	[TestCase("<ul style=\"font-size:21px;\"><li></li></ul>", 0, 0, "15.8", TestName = "{m}_SelectAfterEmptyLi")]
	[TestCase("<ol><li style=\"font-size:12pt;\">Element 1 </li><li style=\"font-size:12pt;\">Element 2</li></ol><p><a href=\"https://devops.wisetechglobal.com\" style=\"font-size: 12pt;\" target=\"_blank\" rel=\"noopener\">A link</a><br></p>", 0, 27, "12", TestName = "{m}_SelectWholeMultipleElements")]
	[TestCase("<p><span style=\"font-size:12pt;\">Element 1 </span></p><p><span style=\"font-size:12pt;\">Element 2</span></p>", 5, 15, "12", TestName = "{m}_SelectPartialMultipleElements")]
	[TestCase("<p><span style=\"font-size:16pt;\">Parent <span style=\"font-size:12pt;\">Child</span></span></p><ul style=\"font-size:16pt;\"><li style=\"font-size:12pt;\">List item 1</li><li style=\"font-size:12pt;\">List item 2</li></ul>", 7, 24, "12", TestName = "{m}_SelectWholeNestedElements")]
	[TestCase("<p><span style=\"font-size:16pt;\">Parent 1 <span style=\"font-size:12pt;\">Child 1 </span></span></p><p><span style=\"font-size:16pt;\"><span style=\"font-size:12pt;\">Child 2 </span> Parent 2</span></p>", 10, 20, "12", TestName = "{m}_SelectPartialNestedElements")]
	public async Task SelectionHasSameFontSize_ShouldUpdateFontSizeToolbar(string originalHtml, int selectionStart, int selectionEnd, string expectedFontSizeInPt)
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadControlOnFormAsync(() => new ZRichTextBox()
		{
			Width = 500,
			Height = 500,
			Html = originalHtml,
			IsToolBarVisible = true
		});
		var sizeSelector = page.GetByLabel("Font sizes", new() { Exact = true });
		await sizeSelector.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
		Assert.That(async () => await sizeSelector.InputValueAsync(), Is.Not.EqualTo(string.Empty).After(3000, 100));
		var client = await RichTextBoxClient.GetClientAsync();
		await client.FocusEditorAsync();

		await client.EvaluateAsync($@"client => {{client.setSelection({{ start: {selectionStart}, end: {selectionEnd}}});}}");
		await Assertions.Expect(sizeSelector).ToHaveValueAsync(expectedFontSizeInPt);
	}

	[Test, WithPlaywrightPage]
	public async Task TestSelectFontsAndFontSize()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadControlOnFormAsync(() => new ZRichTextBox()
		{
			Width = 500,
			Height = 500,
			Html = WrapInP("Single element "),
			IsToolBarVisible = true
		});
		var fontSizeInput = page.GetByLabel("Font sizes", new() { Exact = true });
		await fontSizeInput.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
		Assert.That(async () => await fontSizeInput.InputValueAsync(), Is.Not.EqualTo(string.Empty).After(1000, 100));

		var editor = page.Locator(".richtextbox__editoranchor");
		await editor.FocusAsync();

		var client = await RichTextBoxClient.GetClientAsync();
		await client.EvaluateAsync("client => { client.setSelection({ start: 0, end: 13}); }");

		var sizeSelector = page.GetByLabel("Open font sizes menu", new() { Exact = true });
		await sizeSelector.ClickAsync();
		var sizeOption = page.Locator("button.combobox__dropdown-item").Nth(17);
		await sizeOption.ClickAsync();
		var fontSelector = page.GetByLabel("Open fonts menu", new() { Exact = true });
		await fontSelector.ClickAsync();
		var fontsOption = page.Locator(".popup").GetByText("Courier New", new() { Exact = true });
		await fontsOption.ClickAsync();
		Assert.That(await editor.InnerHTMLAsync(), Is.EqualTo(@$"<p><span style=""font-size: 24pt; font-family: &quot;Courier New&quot;, monospace;"">Single elemen</span>{WrapInSpan("t ")}</p>"));
	}

	[WithPlaywrightPage]
	[TestCase("<p>a<span style=\"font-size:13px;\">b</span><span style=\"font-size:16px;\">a</span><span style=\"font-size:9px;\">c</span><span style=\"font-size:16px;\">u</span>s <br></p>", 0, 6, TestName = "{m}_SelectWholeElements")]
	[TestCase("<p><span style=\"font-size:16pt;\">Element 1 </span><span style=\"font-size:12pt;\">Element 2</span></p><p><a href=\"https://devops.wisetechglobal.com\" style=\"font-size: 16pt;\" target=\"_blank\" rel=\"noopener\">A link</a><br></p>", 5, 21, TestName = "{m}_SelectPartialElements")]
	[TestCase("<p><span style=\"font-size:12pt;\">Parent 1 <span style=\"font-size:12pt;\">Child 1 </span></span></p><p><span style=\"font-size:12pt;\"><span style=\"font-size:16pt;\">Child 2 </span> Parent 2</span></p>", 10, 20, TestName = "{m}_SelectNestedElements")]
	[TestCase("<p><span style=\"font-size:12pt;\">Element 1 </span><span style=\"font-size:14pt\"><br></span><span style=\"font-size:12pt;\">Element 2</span></p>", 5, 15, TestName = "{m}_LineBreakHasDifferentFontSize")]
	[TestCase("<p><span style=\"font-size:12pt;\">Element 1 </span></p><ul><li></li></ul><p><span style=\"font-size:12pt;\">Element 2</span></p>", 5, 15, TestName = "{m}_EmptyElementHasDifferentFontSize")]
	[TestCase("<p><span style=\"font-size:18pt;\">Parent <span style=\"font-size:14pt;\">Nested element </span> parent</span></p>", 3, 16, TestName = "{m}_ChildElementHasDifferentFontSize")]
	public async Task SelectionHasDifferentFontSize_ShouldClearFontSizeToolbar(string originalHtml, int selectionStart, int selectionEnd)
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadControlOnFormAsync(() => new ZRichTextBox()
		{
			Width = 500,
			Height = 500,
			Html = originalHtml,
			IsToolBarVisible = true
		});
		var sizeSelector = page.GetByLabel("Font sizes", new() { Exact = true });
		await sizeSelector.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
		Assert.That(async () => await sizeSelector.InputValueAsync(), Is.Not.EqualTo(string.Empty).After(1000, 100));

		var client = await RichTextBoxClient.GetClientAsync();
		await client.FocusEditorAsync();
		await client.EvaluateAsync($@"client => {{client.setSelection({{ start: {selectionStart}, end: {selectionEnd}}});}}");

		await Assertions.Expect(sizeSelector).ToBeEmptyAsync();
	}

	[Test, WithPlaywrightPage]
	public async Task ToolBarHasItsParentBackColor()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new ZForm();
			var textBox = new ZRichTextBox() { Width = 1000, Height = 1000, BackColor = Color.Red };
			form.Controls.Add(textBox);
			return form;
		});

		var toolbar = page.GetByRole(AriaRole.Toolbar);
		await Assertions.Expect(toolbar).ToHaveCSSAsync("background-color", "rgb(255, 0, 0)");
	}

	[Test, WithPlaywrightPage]
	public async Task StrikethroughOnMultipleFontSizes()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form() { Width = 700 };
			var richTextBox = new ZRichTextBox()
			{
				Width = 700,
				Height = 500,
				Html = @$"<p>{WrapInSpan("abc")}<span style=""font-family: Tahoma, sans-serif; font-size: 32pt;"">def</span><span style=""font-family: Tahoma, sans-serif; font-size: 48pt;"">ghi</span></p>"
			};
			form.Controls.Add(richTextBox);
			return form;
		});
		var client = await RichTextBoxClient.GetClientAsync();
		await client.FocusEditorAsync();
		var toolbar = page.GetByRole(AriaRole.Toolbar);
		var strikethrough = toolbar.GetByRole(AriaRole.Button, new() { Name = "Strikethrough" });
		Assert.That(strikethrough, Is.Not.Null);

		await client.EvaluateAsync($@"client => {{client.setSelection({{ start: 0, end: 9}});}}");
		await strikethrough!.ClickAsync();
		Assert.That(await client.GetEditorHtmlAsync(), Is.EqualTo(@$"<p><span style=""{standardFontStyle} text-decoration-line: line-through;"">abc</span><span style=""font-family: Tahoma, sans-serif; font-size: 32pt; text-decoration-line: line-through;"">def</span><span style=""font-family: Tahoma, sans-serif; font-size: 48pt; text-decoration-line: line-through;"">ghi</span></p>"));

		await client.EvaluateAsync($@"client => {{client.setSelection({{ start: 2, end: 7}});}}");
		await strikethrough!.ClickAsync();
		Assert.That(await client.GetEditorHtmlAsync(), Is.EqualTo(@$"<p><span style=""{standardFontStyle} text-decoration-line: line-through;"">ab</span>{WrapInSpan("c")}<span style=""font-family: Tahoma, sans-serif; font-size: 32pt;"">def</span><span style=""font-family: Tahoma, sans-serif; font-size: 48pt;"">g</span><span style=""font-family: Tahoma, sans-serif; font-size: 48pt; text-decoration-line: line-through;"">hi</span></p>"));
		await strikethrough!.ClickAsync();
		Assert.That(await client.GetEditorHtmlAsync(), Is.EqualTo(@$"<p><span style=""{standardFontStyle} text-decoration-line: line-through;"">abc</span><span style=""font-family: Tahoma, sans-serif; font-size: 32pt; text-decoration-line: line-through;"">def</span><span style=""font-family: Tahoma, sans-serif; font-size: 48pt; text-decoration-line: line-through;"">ghi</span></p>"));

		await client.EvaluateAsync($@"client => {{client.setSelection({{ start: 0, end: 9}});}}");
		await strikethrough!.ClickAsync();
		Assert.That(await client.GetEditorHtmlAsync(), Is.EqualTo(@$"<p>{WrapInSpan("abc")}<span style=""font-family: Tahoma, sans-serif; font-size: 32pt;"">def</span><span style=""font-family: Tahoma, sans-serif; font-size: 48pt;"">ghi</span></p>"));
	}

	[Test, WithPlaywrightPage]
	public async Task SwitchTabShouldNotLoseContent()
	{
		ZRichTextBox richtextbox1 = null;
		ZRichTextBox richtextbox2 = null;
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			var tabControl = new ZTabControl();
			form.Controls.Add(tabControl);

			var tabPage1 = new ZTabPage() { Name = "Tab1", Text = "Tab 1" };
			richtextbox1 = new ZRichTextBox()
			{
				Width = 700,
				Height = 500,
				Html = @"<p>tab page 1</p>"
			};
			tabPage1.Controls.Add(richtextbox1);
			var tabPage2 = new ZTabPage() { Name = "Tab2", Text = "Tab 2" };
			richtextbox2 = new ZRichTextBox()
			{
				Width = 700,
				Height = 500,
				Html = @"<p>tab page 2</p>"
			};
			tabPage2.Controls.Add(richtextbox2);
			tabControl.Controls.Add(tabPage1);
			tabControl.Controls.Add(tabPage2);
			return form;
		});

		var client1 = await RichTextBoxClient.GetClientAsync(richtextbox1.RichEdit);
		var editorBodyInClient1 = await client1.GetEditorBodyAsync();
		Assert.That(editorBodyInClient1.InnerTextAsync, Is.EqualTo("tab page 1"));

		var tab2Button = page.Locator("button", new PageLocatorOptions() { HasTextString = "Tab 2" });
		await tab2Button.ClickAsync();
		var client2 = await RichTextBoxClient.GetClientAsync(richtextbox2.RichEdit);
		var editorBodyInClient2 = await client2.GetEditorBodyAsync();
		Assert.That(editorBodyInClient2.InnerTextAsync, Is.EqualTo("tab page 2"));

		var tab1Button = page.Locator("button", new PageLocatorOptions() { HasTextString = "Tab 1" });
		await tab1Button.ClickAsync();

		var contentLocator = page.Locator(".richtextbox__editoranchor");
		await contentLocator.WaitForAsync(new LocatorWaitForOptions() { Timeout = 500 });

		Assert.That(editorBodyInClient1.InnerTextAsync, Is.EqualTo("tab page 1"));
	}

	static IEnumerable<TestCaseData> StrikethroughAndUnderlineIsNotDamagedBySetFontSizeTestData()
	{
		yield return new TestCaseData(
			@"<p><span style=""font-family: Tahoma, sans-serif; font-size: 96pt;""><span style=""text-decoration-line: line-through;"">hell</span><span style=""text-decoration-line: line-through;"">oooo</span></span></p>",
			@"<p><span style=""text-decoration-line: line-through; font-family: Tahoma, sans-serif; font-size: 12pt;"">he</span><span style=""text-decoration-line: line-through; font-family: Tahoma, sans-serif; font-size: 96pt;"">lloooo</span></p>")
		{
			TestName = "{m}_Strikethrough_WithEmptyWrappingSpan"
		};

		yield return new TestCaseData(
			@"<p><span style=""font-family: Tahoma, sans-serif; font-size: 96pt;font-weight: bold;""><span style=""text-decoration-line: line-through;"">hell</span><span style=""text-decoration-line: line-through;"">oooo</span></span></p>",
			@"<p><strong><span style=""text-decoration-line: line-through; font-family: Tahoma, sans-serif; font-size: 12pt;"">he</span><span style=""text-decoration-line: line-through; font-family: Tahoma, sans-serif; font-size: 96pt;"">lloooo</span></strong></p>")
		{
			TestName = "{m}_Strikethrough_WithoutEmptyWrappingSpan"
		};

		yield return new TestCaseData(
			@"<p><span style=""font-family: Tahoma, sans-serif; font-size: 96pt;""><span style=""text-decoration-line: underline;"">hell</span><span style=""text-decoration-line: underline;"">oooo</span></span></p>",
			@"<p><span style=""text-decoration-line: underline; font-family: Tahoma, sans-serif; font-size: 12pt;"">he</span><span style=""text-decoration-line: underline; font-family: Tahoma, sans-serif; font-size: 96pt;"">lloooo</span></p>")
		{
			TestName = "{m}_Underline_WithEmptyWrappingSpan"
		};

		yield return new TestCaseData(
			@"<p><span style=""font-family: Tahoma, sans-serif; font-size: 96pt; font-weight: bold;""><span style=""text-decoration-line: underline;"">hell</span><span style=""text-decoration-line: underline;"">oooo</span></span></p>",
			@"<p><strong><span style=""text-decoration-line: underline; font-family: Tahoma, sans-serif; font-size: 12pt;"">he</span><span style=""text-decoration-line: underline; font-family: Tahoma, sans-serif; font-size: 96pt;"">lloooo</span></strong></p>")
		{
			TestName = "{m}_Underline_WithoutEmptyWrappingSpan"
		};
	}

	[WithPlaywrightPage]
	[TestCaseSource(nameof(StrikethroughAndUnderlineIsNotDamagedBySetFontSizeTestData))]
	public async Task StrikethroughAndUnderlineIsNotDamagedBySetFontSize(string initialHtml, string expectedHtml)
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new ZForm() { Width = 700 };
			var richTextBox = new ZRichTextBox()
			{
				Width = 700,
				Height = 500,
				Html = initialHtml
			};
			form.Controls.Add(richTextBox);
			return form;
		});
		var sizeSelector = page.GetByLabel("Font sizes", new() { Exact = true });
		await sizeSelector.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
		Assert.That(async () => await sizeSelector.InputValueAsync(), Is.Not.EqualTo(string.Empty).After(1000, 100));

		var client = await RichTextBoxClient.GetClientAsync();
		await client.FocusEditorAsync();

		var toolbarLocator = page.Locator(".richtextbox__toolbar");
		await toolbarLocator.WaitForAsync();

		await client.EvaluateAsync($@"client => {{client.setSelection({{ start: 0, end: 2}});}}");

		await sizeSelector!.FillAsync("12");
		await sizeSelector!.PressAsync("Enter");
		Assert.That(await client.GetEditorHtmlAsync(), Is.EqualTo(expectedHtml));
	}

	[Test, WithPlaywrightPage]
	public async Task StrikethroughAndUnderlineOnComplicatedHtml()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		ZRichTextBox richTextBox = null!;
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new ZForm() { Width = 700, Height = 500 };
			richTextBox = new ZRichTextBox()
			{
				Width = 700,
				Height = 500,
				IsToolBarVisible = true,
				Html = @$"<p><span style=""font-family: Tahoma, sans-serif; font-size: 22pt;"">h<span style=""font-style: italic;"">e</span></span><span style=""font-style: italic;""><span style=""font-family: Tahoma, sans-serif; font-size: 48pt;"">ll</span>{WrapInSpan("oooo")}</span></p>"
			};
			form.Controls.Add(richTextBox);
			return form;
		});

		var client = await RichTextBoxClient.GetClientAsync();
		var editor = page.Locator(".richtextbox__editoranchor");
		await editor.FocusAsync();

		var toolbarLocator = page.Locator(".richtextbox__toolbar");
		await toolbarLocator.WaitForAsync();

		await client.EvaluateAsync("client => { client.setSelection({ start: 0, end: 8}); }");

		var strikethrough = toolbarLocator.GetByLabel("Strikethrough");
		await strikethrough.ClickAsync();
		Assert.That(await editor.InnerHTMLAsync(), Is.EqualTo(@$"<p><span style=""font-family: Tahoma, sans-serif; font-size: 22pt; text-decoration-line: line-through;"">h</span><em><span style=""font-family: Tahoma, sans-serif; font-size: 22pt;"">e</span></em><em><span style=""font-family: Tahoma, sans-serif; font-size: 48pt;"">ll</span></em><em>{WrapInSpan("oooo")}</em></p>"));

		var underline = toolbarLocator.GetByLabel("Underline");
		await underline.ClickAsync();
		Assert.That(await editor.InnerHTMLAsync(), Is.EqualTo(@$"<p><span style=""font-family: Tahoma, sans-serif; font-size: 22pt; text-decoration-line: underline line-through;"">h</span><em><span style=""font-family: Tahoma, sans-serif; font-size: 22pt;"">e</span></em><em><span style=""font-family: Tahoma, sans-serif; font-size: 48pt;"">ll</span></em><em>{WrapInSpan("oooo")}</em></p>"));

		var sizeSelector = toolbarLocator.GetByLabel("Font sizes", new() { Exact = true });
		await sizeSelector.FillAsync("32");
		await sizeSelector.PressAsync("Enter");
		Assert.That(await editor.InnerHTMLAsync(), Is.EqualTo(@$"<p><span style=""font-size: 32pt;""><span style=""font-family: Tahoma, sans-serif; text-decoration-line: underline line-through;"">h</span><em><span style=""font-family: Tahoma, sans-serif;"">e</span></em><em><span style=""font-family: Tahoma, sans-serif;"">ll</span></em><em><span style=""font-family: Tahoma, sans-serif;"">oooo</span></em></span></p>"));

		await underline.ClickAsync();
		Assert.That(await editor.InnerHTMLAsync(), Is.EqualTo(@$"<p><span style=""font-size: 32pt;""><span style=""font-family: Tahoma, sans-serif; text-decoration-line: line-through;"">h</span><em><span style=""font-family: Tahoma, sans-serif;"">e</span></em><em><span style=""font-family: Tahoma, sans-serif;"">ll</span></em><em><span style=""font-family: Tahoma, sans-serif;"">oooo</span></em></span></p>"));

		await strikethrough.ClickAsync();
		Assert.That(await editor.InnerHTMLAsync(), Is.EqualTo(@$"<p><span style=""font-size: 32pt;""><span style=""font-family: Tahoma, sans-serif;"">h</span><em><span style=""font-family: Tahoma, sans-serif;"">e</span></em><em><span style=""font-family: Tahoma, sans-serif;"">ll</span></em><em><span style=""font-family: Tahoma, sans-serif;"">oooo</span></em></span></p>"));
	}

	[Test]
	public async Task ZRichTextBoxShouldUpdateBindingWhenValidatingWhileOutOfSyncWithClientChanges()
	{
		using var ctx = new EnterpriseTestContext();
		var path = "/_content/WinzorFramework/js/module/richTextBox.js";
		var richTextBoxModule = ctx.JSInterop.SetupModule(path);
		richTextBoxModule.Setup<EditorContent>("getEditorContent", new InvocationMatcher(_ => true)).SetResult(new EditorContent(WrapInP("This is some text I added without focusing out"), null, 0));

		var bindingObj = new TextModel { Name_HTML = ZBlob.FromUTF8(WrapInP("This is the old content")) };
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var richTextBox = new ZRichTextBox();
			richTextBox.SetDataBinding(bindingObj, "Name");
			return richTextBox;
		});
		var richTextBox = rendered.GetControl<ZRichTextBox>();

		await rendered.Find(".richtextbox").TriggerEventAsync("onmodified", new EventArgs());

		Assert.That(bindingObj.Name_HTML.ToUTF8().ToString(), Is.EqualTo(WrapInP("This is the old content")));
		Assert.That(ctx.JSInterop.Invocations.Select(i => i.Identifier), Has.Exactly(0).EqualTo("getEditorContent"));

		await richTextBox.InvokeWinzorDispatcherAsync(() => richTextBox.FindForm().Validate());

		Assert.That(bindingObj.Name_HTML.ToUTF8().ToString(), Is.EqualTo(WrapInP("This is some text I added without focusing out")));
		Assert.That(ctx.JSInterop.Invocations.Select(i => i.Identifier), Has.Exactly(1).EqualTo("getEditorContent"));
	}

	[Test, WithPlaywrightPage]
	public async Task EnterKeyOnFormWithAcceptRichTextBoxBeingFocused_ShouldNotFireAcceptButton()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var acceptButtonIsClicked = false;
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			var acceptButton = new Button();
			acceptButton.Click += (_, _) => { acceptButtonIsClicked = true; };

			form.Controls.Add(new ZRichTextBox());
			form.Controls.Add(acceptButton);
			form.AcceptButton = acceptButton;
			return form;
		});

		var rtb = page.Locator(".richtextbox__editoranchor");
		await rtb.WaitForAsync();
		await rtb.FocusAsync();

		await page.Keyboard.PressAsync("Enter");
		Assert.That(() => acceptButtonIsClicked, Is.False.After(3000));
	}

	[Test, WithPlaywrightPage]
	public async Task TestInsertDangerousOrLargeFilesShouldShowWarningBox()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var richTextBox = default(ZRichTextBox);
		var form = default(ZForm);

		var page = await ctx.LoadFormAsync(() =>
		{
			form = new ZForm();
			richTextBox = new ZRichTextBox();
			form.Controls.Add(richTextBox);
			return form;
		});

		var fileServiceMock = new Mock<IFileService>();
		fileServiceMock
			.Setup(i => i.UploadFilesToServerAsync(It.IsAny<BrowserFile[]>(), It.IsAny<long>(), It.IsAny<CancellationToken>()))
			.Returns<BrowserFile[], long, CancellationToken>((files, _, _) => Task.FromResult(files.Select(f => f.Name).ToArray()));
		form.CargoWiseClientServices.FileService = fileServiceMock.Object;

		var files = new[]
		{
			new BrowserFile { Name = "LargeFile.txt", Size = long.MaxValue },
			new BrowserFile { Name = "DangerousFile.exe", Size = 1024 }
		};
		await richTextBox.RichEdit.OnInsertImagesAsync(files, 0, 0);
		Assert.That(UnitTestUserNotification.Instance.LastMessage.Text, Is.EqualTo("The following files were not added because they are potentially dangerous file types:\r\nDangerousFile.exe\r\n\r\nThe following files are larger than the maximum file size (10MB) specified in the registry 'System -> DocManager -> eDocs Maximum File Size':\r\nLargeFile.txt\r\n"));
	}

	[Test, WithPlaywrightPage]
	public async Task ConversationMessagesWithLongContentDisplayCorrectly()
	{
		const string testMessage = "wGSjUv6ESt7wW3VsovDnBO1kmamHs1vTiu9ZhCEzVlK8mzFa94khm066rKyIwG9Co2Uz53Ui0lpGNzUuYcNndRxuqheSnN4L8lAkMfKd7P3LPGqakW0BgxDBDEuG5" +
			"xbEKA2L3i6Nqsar0qkqyzREoWkE5m2mFwTIxfH7r5HNN3y4WYKwottSornzDfzQGAANdO8YK57akdFEaJyQnolVacKOdYAaOyo0mLC81miIrlFy2Me4lUuEzihpySDR4JaxKykdWgws" +
			"UQRHgKezMT0cXDOXfeTqWIk5ARIQMQ1GHNT0r28F7bLo6b3sOxSlMFSl5o3KIfnHa3r3lNhUnWpoUPZtil1wJKrP0tbviXGGnT8NFiNqOpc1hN49apIKuWF1csU0xgQT3ZWj2rcgqq2" +
			"uDYglEi2WugnJVrXZcul8NY747YvGEY3LAxsGDZgT5vsyt1yRC67z4SEcwyTvFtNSL59zcYzz1gMquAWLoGpp9epmROWL2rb5xo9nVq8MRse3foFBex6mHfqWXl5iPC4KjPdYAfkw1x" +
			"XuPpE9spClyYp4CgipOkNNX6KmrGvcdGYF7cEECO4rUoBKf3yh0RshFOC2HF5BrjwTchNjogYcTMed5WBh2RwIOOWo7upCCEPRCCYfzUZtznj4pA0WwsfNdlgT3m63OZuTAEk5g6ghj" +
			"7zmrzCHq1nJ6a7i0YL4m9VsHUo51VzqsG9wi6z6XE1quP5gV2cMRvgghlSFSc4hprBEHTr8WHy14blrTOaHy1bK5UAk2YeffGDGy0mGyWJ0ronwEUMUQsCJadUZXH45lMWULH84isgh" +
			"qYtlfUIoZmXCiLnIMG9CQArIYg7xCqXbvbjIbhYJUiURDYhx4QiyZyJMPuPHt76uziZlKdp1OGONb1CrDoRMJ37JPaPJzko26WT5cmxmzvY20VPwSDNvBv5xAMZdK1lSU9RCmkGhjkh" +
			"TbhLANaru9ePjB1JGBRkzj7NiFncShkZmfTV5GQ06zYfiYD0";
		const string senderName = "Sender Jack";

		Form form = null;
		ConversationMessageUserControl conversationMessageUserControl = null;
		EConversationMessageListUserControl conversationMessageListUserControl = null;
		await using var ctx = new InMemoryAppServerTestContext();
		var mockMessage = CreateMockMessage(senderName, testMessage);
		var page = await ctx.LoadFormAsync(() =>
		{
			form = new Form { Height = 200, Width = 600 };
			conversationMessageUserControl = new ConversationMessageUserControl(mockMessage.Object, false);
			conversationMessageUserControl.bodyTextBox.ScrollBars = RichTextBoxScrollBars.None;
			conversationMessageListUserControl = new EConversationMessageListUserControl();
			conversationMessageListUserControl.messagesLayoutPanel.Children.Add(conversationMessageUserControl);
			form.Controls.Add(conversationMessageListUserControl);
			return form;
		});

		var messageControls = page.Locator(".richtextbox__data");
		await Assertions.Expect(messageControls).ToHaveCountAsync(1);
		await Assertions.Expect(messageControls.First).ToHaveTextAsync(testMessage);

		await form.InvokeWinzorDispatcherAsync(() =>
		{
			var message2 = CreateMockMessage(senderName, testMessage);
			var messageUserControl2 = new ConversationMessageUserControl(message2.Object, false);
			conversationMessageListUserControl.messagesLayoutPanel.Children.Add(messageUserControl2);

			var message3 = CreateMockMessage(senderName, testMessage);
			var messageUserControl3 = new ConversationMessageUserControl(message3.Object, false);
			conversationMessageListUserControl.messagesLayoutPanel.Children.Add(messageUserControl3);
		});

		await Assertions.Expect(messageControls).ToHaveCountAsync(3);
		await Assertions.Expect(messageControls.Nth(1)).ToHaveTextAsync(testMessage);
		await Assertions.Expect(messageControls.Nth(2)).ToHaveTextAsync(testMessage);
	}

	Mock<IConversationMessage> CreateMockMessage(string senderName, string messageBody)
	{
		var mockMessage = new Mock<IConversationMessage>();
		mockMessage.SetupGet(x => x.SenderDisplayName).Returns(senderName);
		mockMessage.SetupGet(x => x.Body).Returns(messageBody);
		return mockMessage;
	}

	readonly Font standardFont = new Font("Tahoma", 20, GraphicsUnit.Pixel);
	const string standardFontStyle = "font-family: Tahoma, sans-serif; font-size: 20px;";
	const string standardStyle = $"style=\"{standardFontStyle}\"";
	static string WrapInSpan(string s) => $"<span {standardStyle}>{s}</span>";
	static string WrapInP(string s) => $"<p><span {standardStyle}>{s}</span></p>";

	Task<ILocator> WaitForEditorAsync(IPage page, string textContent = "") => WaitForEditorAsync(page, 0, textContent);

	async Task<ILocator> WaitForEditorAsync(IPage page, int i = 0, string textContent = "")
	{
		await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
		await page.WaitForSelectorAsync(".richtextbox");
		var richTextBox = page.Locator(".richtextbox").Nth(i);
		var editor = richTextBox.Locator(".richtextbox__editoranchor");
		await editor.WaitForAsync(new LocatorWaitForOptions() { State = WaitForSelectorState.Attached });
		await editor.FillAsync(textContent);
		return editor;
	}

	[Test, WithPlaywrightPage, WithTransaction]
	[SetCulture("en-AU")]
	public async Task EmailDropBubblesToHostControl()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new CommunicationForm();

			return form;
		});
		var client = await RichTextBoxClient.GetClientAsync();
		var editorBody = await client.GetEditorBodyAsync();
		await client.FocusEditorAsync();

		// Mock MSGReader function on the page to simulate the parsed message body
		await page.EvaluateAsync(@"() => {
			window.MSGReader = class {
				constructor(arrayBuffer) {}
				getFileData() {
					return { body: 'This is the email body from a .msg file' };
				}
			};
		}");

		bool dropBubbled = false;
		await page.ExposeFunctionAsync("dropBubbledToHost", () =>
		{
			dropBubbled = true;
			return Task.CompletedTask;
		});

		// Simulate drop event with the .msg file
		await page.EvaluateAsync(@"(element) => {
		const dataTransfer = new DataTransfer();
		
		const dropEvent = new DragEvent('drop', { dataTransfer: dataTransfer, bubbles: true, cancelable: true });
		element.dispatchEvent(dropEvent);
	}", editorBody);

		Assert.That(dropBubbled, Is.True, "Drop event did not bubble to host.");
	}
}
