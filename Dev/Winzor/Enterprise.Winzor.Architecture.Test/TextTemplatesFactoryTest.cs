using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Winzor.Architecture.Test;
internal class TextTemplatesFactoryTest
{
	#region Tests

	[TestCaseSource(nameof(BusinessObjectTestCaseData))]
	public async Task ContextIdDoesNotContainHtml(bool bizObjHasCustomTextTemplateContext)
	{
		ZRichTextBox zRichTextBox = null;
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderFormAsync(() => CreateTestZForm(bizObjHasCustomTextTemplateContext, zRichTextBox = new ZRichTextBox()));

		Assert.That(zRichTextBox, Is.Not.Null);
		Assert.That(zRichTextBox.contextMenuManager.TextTemplatesFactory.ContextId, Is.Not.Null.Or.Empty);
		Assert.That(zRichTextBox.contextMenuManager.TextTemplatesFactory.ContextId, Does.Contain("Z0_VarBinaryMax"));
		Assert.That(zRichTextBox.contextMenuManager.TextTemplatesFactory.ContextId, Does.Not.Contain("_HTML"));
	}

	[TestCaseSource(nameof(BusinessObjectTestCaseData)), WithTransaction]
	public async Task GetAllTemplatesForControlReturnsExpectedTemplates(bool bizObjHasCustomTextTemplateContext)
	{
		ZRichTextBox zRichTextBox = null;
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderFormAsync(() => CreateTestZForm(bizObjHasCustomTextTemplateContext, zRichTextBox = new ZRichTextBox()));

		Assert.That(zRichTextBox, Is.Not.Null);

		StmNoteTemplate[] templates = null;
		await zRichTextBox.InvokeWinzorDispatcherAsync(() =>
		{
			zRichTextBox.contextMenuManager.TextTemplatesFactory.GetAllTemplatesForControl().DeleteAll();

			for (var i = 1; i <= 3; i++)
			{
				var template = zRichTextBox.contextMenuManager.TextTemplatesFactory.New();
				template.S8_Description = "Template " + i;
				template.S8_TemplateText = "Cool text! This is template " + i;
				template.Factory.Save();
			}

			templates = zRichTextBox.contextMenuManager?.TextTemplatesFactory?.GetAllTemplatesForControl();
		});

		Assert.That(templates?.Length, Is.EqualTo(3));
		for (var i = 1; i <= 3; i++)
		{
			Assert.That(templates[i - 1].S8_Description.ToString(), Is.EqualTo("Template " + i));
			Assert.That(templates[i - 1].S8_TemplateText.ToString(), Is.EqualTo("Cool text! This is template " + i));
		}
	}

	#endregion

	#region Test Case Data

	public static IEnumerable<TestCaseData> BusinessObjectTestCaseData
	{
		get
		{
			yield return new TestCaseData(false) { TestName = "{m}_StandardBusinessObject" };
			yield return new TestCaseData(true) { TestName = "{m}_BusinessObjectWithCustomTextTemplateContext" };
		}
	}

	#endregion

	#region Helper

	RichTextBoxFormForTest CreateTestZForm(bool bizObjHasCustomTextTemplateContext, ZRichTextBox zRichTextBox)
	{
		return bizObjHasCustomTextTemplateContext ? new RichTextBoxFormForTest(new BusinessObjectFactory().NewWithValidTestData<DummyBizOWithCustomTextTemplateContext>(), zRichTextBox)
			: new RichTextBoxFormForTest(new BusinessObjectFactory().NewWithValidTestData<DummyBusinessObject>(), zRichTextBox);
	}

	class RichTextBoxFormForTest : ZForm
	{
		public RichTextBoxFormForTest(BusinessObject bzo, ZRichTextBox richTextBox)
			: base(bzo)
		{
			BindingSource.SetBindingMember(richTextBox, "Z0_VarBinaryMax");
			Controls.Add(richTextBox);
		}
	}

	class DummyBizOWithCustomTextTemplateContext : DummyBizOWithRelatedNotes, ICustomTextTemplateAlternateContexts, ICustomTextTemplateContext
	{
		public DummyBizOWithCustomTextTemplateContext(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public IReadOnlyCollection<string> GetAlternateTextTemplateContextIDs(object dataSource, KBindingMemberInfo bindingMemberInfo)
		{
			return new string[] { "AlternateId1", "AlternateId2" };
		}

		public BusinessObject[] GetTextTemplateContextBusinessObject(object dataSource, KBindingMemberInfo bindingMemberInfo)
		{
			return new BusinessObject[] { this };
		}

		public string GetTextTemplateContextID(object dataSource, KBindingMemberInfo bindingMemberInfo)
		{
			return "Z0_VarBinaryMax";
		}
	}

	#endregion
}
