using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Test
{
	[TestedType(typeof(CompanyCredentialsLayoutBuilder<GlbCompanyWrapper>))]
	public class CompanyCredentialsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<CompanyCredentialsLayoutBuilder<GlbCompanyWrapper>, GlbCompanyWrapper, CompanyCredentialsControlBag>
	{
		protected override CompanyCredentialsLayoutBuilder<GlbCompanyWrapper> GetColumnLayoutBuilderForTesting() => new CompanyCredentialsLayoutBuilder<GlbCompanyWrapper>();

		protected override ColumnLayoutBuilderCaptionWidthSize ExpectedCaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Medium;

		protected override int ExpectedMaxColumns => 2;
	}
}
