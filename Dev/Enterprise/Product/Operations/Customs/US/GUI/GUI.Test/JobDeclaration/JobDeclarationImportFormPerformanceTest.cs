using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.GUI.Testing
{
	sealed class JobDeclarationImportFormPerformanceTest : JobDeclarationFormPerformanceTest
	{
		protected override ZString MessageTypeForFormBashing => JobMessageTypeList.Codes.Import;

		protected override Dictionary<string, int> USLoadEditableChildObjectsExpectedHits => new Dictionary<string, int>
		{
			{ CusAttributeFilterSchema.Constants.TableName, 18 },
		};

		protected override Dictionary<string, int> USValidateAllExpectedHits => new Dictionary<string, int>
		{
			{ StmALogSchema.Constants.TableName, 7 },
			{ OrgCusCodeSchema.Constants.TableName, 7 },
			{ JobDocAddressSchema.Constants.TableName, 10 },
			{ RefDataGroupingSchema.Constants.TableName, 5 },
			{ ZZRefCusCodeListCombinedSchema.Constants.TableName, 21 },
			{ TariffViewSchema.Constants.TableName, 5 },
			{ CusDispositionSchema.Constants.TableName, 19 },
			{ CusAttributeFilterSchema.Constants.TableName, 18 },
		};

		protected override Dictionary<string, int> USLightFormValidationAndSaveExpectedHits => new Dictionary<string, int>
		{
			{ CusAddInfoSchema.Constants.TableName, 22 },
			{ CusCodeDataSchema.Constants.TableName, 8 },
			{ StmALogSchema.Constants.TableName, 7 },
			{ JobDocAddressSchema.Constants.TableName, 8 },
			{ TariffViewSchema.Constants.TableName, 5 },
			{ ZZRefCusCodeListCombinedSchema.Constants.TableName, 5 },
			{ "NonPersitentTable JOBDECLARATION JOIN dbo.GLBBRANCH ON JE_GB = GB_PK JOIN dbo.GLBCOMPANY ON GB_GC = GC_PK", 6 },
			{ CusAttributeFilterSchema.Constants.TableName, 18 },
		};

		protected override Dictionary<string, int> USFormMergeExpectedHits => new Dictionary<string, int>
		{
			{ RefExchangeRateSchema.Constants.TableName, 9 },
			{ StmALogSchema.Constants.TableName, 8 },
			{ StmDocDataOverrideSchema.Constants.TableName, 158 },
			{ StmNoteSchema.Constants.TableName, 62 },
			{ OrgCusCodeSchema.Constants.TableName, 7 },
			{ RefDataGroupingSchema.Constants.TableName, 5 },
			{ ZZRefCusCodeListCombinedSchema.Constants.TableName, 6 },
			{ CusAttributeFilterSchema.Constants.TableName, 18 },
		};

		protected override Dictionary<string, int> USUniversalXMLExportExpectedHits => new Dictionary<string, int>
		{
			{ CusAddInfoSchema.Constants.TableName, 55 },
			{ CusCodeDataSchema.Constants.TableName, 59 },
			{ OrgAddressSchema.Constants.TableName, 8 },
			{ CusAttributeFilterSchema.Constants.TableName, 18 },
		};

		protected override Dictionary<string, int> USUniversalXMLImportUpdateExpectedHits => new Dictionary<string, int>
		{
			{ CusAddInfoSchema.Constants.TableName, 24 },
			{ CusCodeDataSchema.Constants.TableName, 11 },
			{ JobDocAddressSchema.Constants.TableName, 9 },
			{ ProcessTasksSchema.Constants.TableName, 6 },
			{ RefExchangeRateSchema.Constants.TableName, 9 },
			{ RefPacksSchema.Constants.TableName, 7 },
			{ StmNoteSchema.Constants.TableName, 68 },
			{ EDIMessageSchema.Constants.TableName, 9 },
			{ TariffViewSchema.Constants.TableName, 6 },
			{ OrgHeaderSchema.Constants.TableName, 10 },
			{ CusAttributeFilterSchema.Constants.TableName, 18 },
		};

		protected override Dictionary<string, int> USUniversalXMLAddExpectedHits => new Dictionary<string, int>
		{
			{ RefExchangeRateSchema.Constants.TableName, 9 },
			{ RefPacksSchema.Constants.TableName, 7 },
			{ TariffViewSchema.Constants.TableName, 6 },
			{ CusAttributeFilterSchema.Constants.TableName, 18 },
		};

		protected override Dictionary<string, int> USDeleteExpectedHits => new Dictionary<string, int>
		{
			{ StmDocDataOverrideSchema.Constants.TableName, 40 },
			{ StmNoteSchema.Constants.TableName, 36 },
			{ JobDocAddressSchema.Constants.TableName, 8 },
			{ CusAttributeFilterSchema.Constants.TableName, 18 },
		};
	}
}
