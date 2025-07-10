using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;

namespace Enterprise.Customs.Universal.Module.Testing
{
	public abstract class ZZRefCusCodeListWrapperModuleTest<T> : ZModuleBasherTest where T : ZZRefCusCodeListWrapper
	{
		public void TestGetNewFilterControl()
		{
			using (var testModule = GetNewTestModule())
			{
				var testControl = (ZFilterStripControl)testModule.EmbeddedControl;
				AssertEquals(ExpectedCodeCaption, testControl.FilteredGrid.GetColumnStyle(ZZRefCusCodeListWrapper.Schema.Code).Caption);
				AssertEquals(ExpectedDescriptionCaption, testControl.FilteredGrid.GetColumnStyle(ZZRefCusCodeListWrapper.Schema.Description).Caption);
				AssertEquals(ExpectedCodeCaptionColumnWidth, testControl.FilteredGrid.GetColumnStyle(ZZRefCusCodeListWrapper.Schema.Code).Width);
				AssertEquals(ExpectedDescriptionColumnWidth, testControl.FilteredGrid.GetColumnStyle(ZZRefCusCodeListWrapper.Schema.Description).Width);
			}
		}

		public void TestGetNewGridCollection()
		{
			using (var testModule = GetNewTestModule())
			{
				AssertNotNull("GridCollection Type", testModule.GridCollection as ZZRefCusCodeListWrapperCollection<T>);
			}
		}

		public void TestGetNewFilterBusinessObject()
		{
			using (var testModule = GetNewTestModule())
			{
				AssertNotNull("FilterBusinessObject Type", testModule.FilterBusinessObject as ZZRefCusCodeListWrapperFilterStripBusinessObject);
			}
		}

		protected virtual string ExpectedCodeCaption => "Code";
		protected virtual string ExpectedDescriptionCaption => "Description";
		protected virtual int ExpectedCodeCaptionColumnWidth => 80;
		protected virtual int ExpectedDescriptionColumnWidth => 300;
		protected abstract ZFilterGridModule GetNewTestModule();
	}
}
