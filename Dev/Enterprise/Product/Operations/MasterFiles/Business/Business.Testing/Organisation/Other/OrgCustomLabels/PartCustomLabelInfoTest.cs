using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class PartCustomLabelInfoTest : TestCaseWithFactory
	{
		public void TestPartInfo()
		{
			PartCustomLabelInfo partInfo = new PartCustomLabelInfo("", "", "", null, (NoResString)"", null, Factory);
			AssertEquals("Should have empty caption", "", partInfo.Caption);
			Assert("Is Enabled should be false", !partInfo.IsEnabled);

			partInfo = new PartCustomLabelInfo("32r", "", "", null, (NoResString)"Default Caption", GlbCompany.CurrentCompany.OrgProxy, Factory);
			ZString test = partInfo.Caption;
			AssertEquals("32r", partInfo.Caption);
			Assert("Should have IsEnabled true", partInfo.IsEnabled);
			Assert("IsMandatory should be false", !partInfo.IsMandatory);
			ErrorReporter.Clear();

			partInfo = new PartCustomLabelInfo("Part", PartAttributeTypeList.Codes.Mandatory, "", null, (NoResString)"Default Caption", GlbCompany.CurrentCompany.OrgProxy, Factory);
			AssertEquals("Part", partInfo.Caption);
			Assert("Is Mandatory should be false", !partInfo.IsMandatory);
			AssertEquals("Hint should equal caption", partInfo.Hint, partInfo.Caption);
		}
	}
}
