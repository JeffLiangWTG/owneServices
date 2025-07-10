using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.Customs.PL.GUI.PlugIn;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.GUI.Testing;

class ExportSupportingInformationControlTest : TestCaseWithFactory
{
	public void TestAdditionalInfosTabPage()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		using (var userControl = new SupportingInformationControl())
		{
			userControl.SetDataBinding(declaration, string.Empty);
			var tabPage = userControl.FindSingle<ZTabPage>("AdditionalInfoTabPage");
			CombineAssertions(() =>
			{
				AssertEquals("AdditionalInfosTabPage visible", true, tabPage.TabVisible);
				AssertEquals("Caption for export", "[44] Additional Documents", tabPage.CaptionResourceString.Caption);
			});
		}

		using (var userControl = new SupportingInformationControl())
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			userControl.SetDataBinding(declaration, string.Empty);
			var tabPage = userControl.FindSingle<ZTabPage>("AdditionalInfoTabPage");
			AssertEquals("Caption for import", "[44] Additional Info", tabPage.CaptionResourceString.Caption);
		}
	}

	public void TestGetPreviousDocumentsUserControlType()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		using (var control = new SupportingInformationControlForTest())
		{
			control.SetDataBinding(declaration, string.Empty);
			AssertEquals("Export type", typeof(LayoutPreviousDocumentsUserControl), control.GetPreviousDocumentsUserControlType_Exposed());
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			control.SetDataBinding(declaration, string.Empty);
			AssertEquals("Import type", typeof(LayoutPreviousDocumentsUserControl), control.GetPreviousDocumentsUserControlType_Exposed());
		}
	}

	public void TestGetSupportingDocumentsUserControlType()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		using (var control = new SupportingInformationControlForTest())
		{
			control.SetDataBinding(declaration, string.Empty);
			AssertEquals("Export type", typeof(LayoutSupportingDocumentsUserControl), control.GetSupportingDocumentsUserControlType_Exposed());
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			control.SetDataBinding(declaration, string.Empty);
			AssertEquals("Import type", typeof(LayoutSupportingDocumentsUserControl), control.GetSupportingDocumentsUserControlType_Exposed());
		}
	}

	class SupportingInformationControlForTest : SupportingInformationControl
	{
		public Type GetPreviousDocumentsUserControlType_Exposed() => base.GetPreviousDocumentsUserControlType();
		public Type GetSupportingDocumentsUserControlType_Exposed() => base.GetSupportingDocumentsUserControlType();
	}
}
