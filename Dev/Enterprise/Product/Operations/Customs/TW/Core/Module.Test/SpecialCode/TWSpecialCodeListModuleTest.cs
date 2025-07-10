using CargoWise.EntityFramework;
using Enterprise.Customs.TW.Business;
using Enterprise.Customs.TW.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Module.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Module.Testing
{
	[TestedType(typeof(TWSpecialCodeListModule))]
	public sealed class TWSpecialCodeListModuleTest : ZZRefCusCodeListWrapperModuleTest<TWSpecialCode>
	{
		public void TestModuleAllows()
		{
			using (var module = new TWSpecialCodeListModule())
			{
				AssertEquals("module.AllowDelete", false, module.AllowDelete);
				AssertEquals("module.AllowNew", false, module.AllowNew);
				AssertEquals("module.AllowEdit", false, module.AllowEdit);
			}
		}

		public new void TestGetNewFilterControl()
		{
			using (var testModule = GetNewTestModule())
			{
				Assert(!testModule.ShowRecentItems);
				var testControl = (ZFilterStripControl)testModule.EmbeddedControl;
				AssertEquals(ExpectedCodeCaption, testControl.FilteredGrid.GetColumnStyle(ZZRefCusCodeListWrapper.Schema.Code).Caption);
				AssertEquals(ExpectedCodeCaptionColumnWidth, testControl.FilteredGrid.GetColumnStyle(ZZRefCusCodeListWrapper.Schema.Code).Width);
				AssertEquals(ExpectedDescriptionCaption, testControl.FilteredGrid.GetColumnStyle(ZZRefCusCodeListWrapper.Schema.Description).Caption);
				AssertEquals(ExpectedDescriptionColumnWidth, testControl.FilteredGrid.GetColumnStyle(ZZRefCusCodeListWrapper.Schema.Description).Width);
				AssertEquals(TWSpecialCodeListModule.ListTypeCaption, testControl.FilteredGrid.GetColumnStyle(TWSpecialCode.Schema.SC_CodeType).Caption);
				AssertEquals(70, testControl.FilteredGrid.GetColumnStyle(TWSpecialCode.Schema.SC_CodeType).Width);
				AssertEquals(TWSpecialCodeListModule.ListDescriptionCaption, testControl.FilteredGrid.GetColumnStyle(TWSpecialCode.Schema.SC_CodeTypeDesc).Caption);
				AssertEquals(100, testControl.FilteredGrid.GetColumnStyle(TWSpecialCode.Schema.SC_CodeTypeDesc).Width);
				AssertEquals(TWSpecialCodeListModule.CountryCaption, testControl.FilteredGrid.GetColumnStyle(TWSpecialCode.Schema.SC_Country).Caption);
				AssertEquals(60, testControl.FilteredGrid.GetColumnStyle(TWSpecialCode.Schema.SC_Country).Width);
				AssertEquals(TWSpecialCodeListModule.ControllingAgencyCaption, testControl.FilteredGrid.GetColumnStyle(TWSpecialCode.Schema.SC_ControllingAgency).Caption);
				AssertEquals(120, testControl.FilteredGrid.GetColumnStyle(TWSpecialCode.Schema.SC_ControllingAgency).Width);
				AssertEquals(TWSpecialCodeListModule.ControllingAgencyDescriptionCaption, testControl.FilteredGrid.GetColumnStyle(TWSpecialCode.Schema.SC_ControllingAgencyDescription).Caption);
				AssertEquals(200, testControl.FilteredGrid.GetColumnStyle(TWSpecialCode.Schema.SC_ControllingAgencyDescription).Width);
				AssertEquals(TWSpecialCodeListModule.RemarksCaption, testControl.FilteredGrid.GetColumnStyle(TWSpecialCode.Schema.SC_Remarks).Caption);
				AssertEquals(200, testControl.FilteredGrid.GetColumnStyle(TWSpecialCode.Schema.SC_Remarks).Width);
				AssertEquals(TWSpecialCodeListModule.SourceCaption, testControl.FilteredGrid.GetColumnStyle(TWSpecialCode.Schema.SC_Source).Caption);
				AssertEquals(200, testControl.FilteredGrid.GetColumnStyle(TWSpecialCode.Schema.SC_Source).Width);
			}
		}

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.TW.SpecialCode;
		protected override ZFilterGridModule GetNewTestModule() => new TWSpecialCodeListModule();
		protected override int ExpectedCodeCaptionColumnWidth => 100;
		protected override void AddTestObjects(IBusinessObjectCollection collection)
		{
			base.AddTestObjects(collection);
			TWSpecialCodeTest.CreateTWSpecialCode(collection.Factory, "SP99", "SP99 desc");
			TWSpecialCodeTest.CreateTWSpecialCode(collection.Factory, "ND22", "ND22 desc");
			collection.Factory.Save();
		}
	}
}
