using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Bunit;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules.Testing;
using Microsoft.AspNetCore.Components;
using NUnit.Framework;
using WinzorTestFramework;

namespace Enterprise.Winzor.Architecture.Test;

class ZGuidFindBoxColumnStyleTest
{
	[TestCase("1234", "S234", "1234", "S234", TestName ="GuidTestWithInLength")]
	[TestCase("1234", "QWEQW", "1234", "QWEQW", TestName = "GuidTestExceedMaxLength")]
	[TestCase("aaaaa", "bbbbb", "AAAAA", "BBBBB", TestName = "GuidTestWithLowerCaseLetters")]
	[TestCase("*&^*&", "*(&(*", "*&^*&", "*(&(*", TestName = "GuidTestWithSpecialCharacters")]
	[TestCase("1-a-3", "xx-a1", "1-A-3", "XX-A1", TestName = "GuidTestWithSpecialCharactersAndLowerCase")]
	public async Task CheckZGuidFindBoxColumnDisplayTest(string guid, string newGuid, string expectedGuid, string expectedNewGuid)
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new WinzorTestForm();
			form.BindingSource.DataSourceType = typeof(DummyBusinessObject);
			var grid = new ZGrid();
			form.BindingSource.SetBindingMember(grid, "Collection");
			var columnStyleInfo = new ZCodeFindBoxColumnStyleInfo();
			columnStyleInfo.ModuleID = DummyModuleIDs.Dummy;
			columnStyleInfo.ColumnName = "Z0_Code";
			columnStyleInfo.BindToList = "Lookups+DummyList";
			grid.ColumnStyles.Add(columnStyleInfo);
			form.Controls.Add(grid);
			form.DataSourceType = typeof(DummyBusinessObject);
			var factory = new BusinessObjectFactory();
			var dummyBizo = factory.New<DummyBusinessObject>();
			var child = factory.New<DummyEnterpriseBusinessObject>();
			child.Z0_Code = guid;
			dummyBizo.Collection.Add(child);
			form.SetDataBinding(dummyBizo, "");
			return form;
		});

		var input = rendered.FindAll("input").Cast<IHtmlInputElement>().Single();
		Assert.That(input.Value, Is.EqualTo(expectedGuid));

		await input.TriggerEventAsync("oninput", new ChangeEventArgs { Value = newGuid });
		await input.FocusOutAsync();
		Assert.That(rendered.FindAll("tbody>tr")[0].ChildNodes[1].TextContent, Is.EqualTo(expectedNewGuid), rendered.Markup);
		await rendered.FindAll("tbody>tr")[0].Children[1].MouseDownAsync(new WebMouseEventArgs());
		Assert.That(input.Value, Is.EqualTo(expectedGuid));
	}

	[TestCase("<br>i", "&lt;br&gt;i")]
	[TestCase("</br>", "&lt;/br&gt;")]
	[TestCase("<p>", "&lt;p&gt;")]
	[TestCase("<hr>x", "&lt;hr&gt;x")]
	[TestCase("<td>a", "&lt;td&gt;a")]
	[TestCase("<tr>/", "&lt;tr&gt;/")]
	[TestCase("<li><", "&lt;li&gt;&lt;")]
	[TestCase("<img>", "&lt;img&gt;")]
	public async Task ZGuidFindBoxColumnHtmlEncodedTest(string htmlString, string expectedHtmlEncodedString)
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new WinzorTestForm();
			form.BindingSource.DataSourceType = typeof(DummyBusinessObject);
			var grid = new ZGrid();
			form.BindingSource.SetBindingMember(grid, "Collection");
			var columnStyleInfo = new ZCodeFindBoxColumnStyleInfo() { CharacterCasing = CharacterCasing.Normal };
			columnStyleInfo.ModuleID = DummyModuleIDs.Dummy;
			columnStyleInfo.ColumnName = "Z0_Code";
			columnStyleInfo.BindToList = "Lookups+DummyList";
			grid.ColumnStyles.Add(columnStyleInfo);
			form.Controls.Add(grid);
			form.DataSourceType = typeof(DummyBusinessObject);
			var factory = new BusinessObjectFactory();
			var dummyBizo = factory.New<DummyBusinessObject>();
			var child = factory.New<DummyEnterpriseBusinessObject>();
			child.Z0_Code = htmlString;
			dummyBizo.Collection.Add(child);
			form.SetDataBinding(dummyBizo, "");
			return form;
		});
		await rendered.Find("input").FocusOutAsync();
		Assert.That((rendered.FindAll("tbody>tr")[0].ChildNodes[1] as IElement).InnerHtml, Is.EqualTo($"{expectedHtmlEncodedString}"), rendered.Markup);
	}
}
