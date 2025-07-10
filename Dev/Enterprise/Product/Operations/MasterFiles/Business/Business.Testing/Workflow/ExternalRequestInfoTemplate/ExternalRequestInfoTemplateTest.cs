using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ExternalRequestInfoTemplate))]
	internal class ExternalRequestInfoTemplateTest : EnterpriseBusinessObjectTestCase
	{
		#region Implementation

		public override BusinessObject GetNewBusinessObjectSafeSaving()
		{
			return externalRequestInfoTemplate;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return externalRequestInfoTemplate;
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return externalRequestInfoTemplate;
		}

		protected override void SetUp()
		{
			base.SetUp();

			externalRequestInfoTemplate = Factory.NewWithValidTestData<ExternalRequestInfoTemplate>();
			externalRequestInfoTemplate.RIT_Code = "XXX";
			externalRequestInfoTemplate.RIT_Description = "XXX1 DESC";
			externalRequestInfoTemplate.RIT_JobType = ExternalRequestTypeJobTypes.Codes.ORD;
			externalRequestInfoTemplate.RIT_IsActive = true;
		}

		ExternalRequestInfoTemplate externalRequestInfoTemplate;

		#endregion

		public void TestHtmlProperty()
		{
			var template = (ExternalRequestInfoTemplate)GetNewBusinessObject();
			AssertEquals(ZBlob.Empty, template.RIT_Template);
			AssertEquals(ZBlob.Empty, template.RIT_Template_HTML);

			template.RIT_Template_HTML = ZBlob.FromUTF8("<p>123</p>");

			AssertEquals(@"{\rtf1\ansi\ansicpg1252\deflang3081\nouicompat\uc0{\fonttbl}{\colortbl}{{123}\par}}", ORtfTextUtil.GeneratorInfoRegex.Replace(template.RIT_Template.ToUTF8(), string.Empty));
			AssertEquals("<p>123</p>", template.RIT_Template_HTML.ToUTF8());
		}

		public void TestHtmlFromTextProperty()
		{
			var template = (ExternalRequestInfoTemplate)GetNewBusinessObject();
			AssertEquals(ZBlob.Empty, template.RIT_Template);
			AssertEquals(ZBlob.Empty, template.RIT_Template_HTML);

			template.RIT_Template = ZBlob.FromUTF8("1234\r\n5678");

			AssertEquals("<p>1234</p><p>5678</p>", template.RIT_Template_HTML.ToUTF8());

			template.RIT_Template = ZBlob.FromUTF8("{\\rtf1\\test\\ansi\\ansicpg1252\\nouicompat\\deflang3081\r\n{\\*\\generator Riched20 10.0.19041}\\viewkind4\\uc1 \\pard rtf\\par\r\n}\r\n");

			AssertEquals("<p>rtf</p>", template.RIT_Template_HTML.ToUTF8());
		}
	}
}
