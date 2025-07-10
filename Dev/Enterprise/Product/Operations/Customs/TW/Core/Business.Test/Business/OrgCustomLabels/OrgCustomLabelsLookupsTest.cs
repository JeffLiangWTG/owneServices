using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class OrgCustomLabelsLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestFieldNameList()
		{
			var org = Factory.New<OrgHeader>();
			var orgWrapper = OrgHeaderWrapper.New(org);
			var orgCustomLabels = orgWrapper.ExportCustomDocumentLabels.AddNew();
			var lookup = orgCustomLabels.Lookups;
			orgCustomLabels.OT_Type = OrgConstants.CustomLabelType.OverrideExportDoc;
			AssertType<ExportDeclarationDocumentFieldList>(lookup.FieldNameList);

			orgCustomLabels.OT_Type = OrgConstants.CustomLabelType.OverrideImportDoc;
			AssertType<ImportDeclarationDocumentFieldList>(lookup.FieldNameList);

			orgCustomLabels.OT_Type = CargoWise.Types.ZString.Empty;
			AssertType<CodeDescriptionPairList>(lookup.FieldNameList);
		}
	}
}
