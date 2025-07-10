using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.GUI.Testing
{
	sealed class JobDeclarationFTZFormPerformanceTest : JobDeclarationFormPerformanceTest
	{
		protected override ZString MessageTypeForFormBashing => JobMessageTypeList.Codes.FTZ;

		protected override Dictionary<string, int> USLoadEditableChildObjectsExpectedHits => new Dictionary<string, int>
		{
			{ CusAddInfoSchema.Constants.TableName, 14 },
			{ CusCodeDataSchema.Constants.TableName, 7 },
			{ CusAttributeFilterSchema.Constants.TableName, 18 },
		};

		protected override Dictionary<string, int> USValidateAllExpectedHits => new Dictionary<string, int>
		{
			{ CusAddInfoSchema.Constants.TableName, 14 },
			{ CusCodeDataSchema.Constants.TableName, 7 },
			{ CusDispositionSchema.Constants.TableName, 11 },
			{ OrgCusCodeSchema.Constants.TableName, 7 },
			{ JobDocAddressSchema.Constants.TableName, 10 },
			{ RefDataGroupingSchema.Constants.TableName, 6 },
			{ ZZRefCusCodeListCombinedSchema.Constants.TableName, 7 },
			{ CusAttributeFilterSchema.Constants.TableName, 18 },
		};

		protected override Dictionary<string, int> USLightFormValidationAndSaveExpectedHits => new Dictionary<string, int>
		{
			{ CusAddInfoSchema.Constants.TableName, 13 },
			{ CusCodeDataSchema.Constants.TableName, 6 },
			{ JobDocAddressSchema.Constants.TableName, 8 },
			{ ZZRefCusCodeListCombinedSchema.Constants.TableName, 5 },
			{ CusAttributeFilterSchema.Constants.TableName, 18 },
		};

		protected override Dictionary<string, int> USFormMergeExpectedHits => new Dictionary<string, int>
		{
			{ CusAddInfoSchema.Constants.TableName, 13 },
			{ CusCodeDataSchema.Constants.TableName, 6 },
			{ RefExchangeRateSchema.Constants.TableName, 9 },
			{ StmDocDataOverrideSchema.Constants.TableName, 90 },
			{ StmNoteSchema.Constants.TableName, 36 },
			{ RefDataGroupingSchema.Constants.TableName, 6 },
			{ ZZRefCusCodeListCombinedSchema.Constants.TableName, 13 },
			{ CusAttributeFilterSchema.Constants.TableName, 18 },
		};

		protected override Dictionary<string, int> USUniversalXMLExportExpectedHits => new Dictionary<string, int>
		{
			{ CusAddInfoSchema.Constants.TableName, 35 },
			{ CusCodeDataSchema.Constants.TableName, 37 },
			{ GenPivotSchema.Constants.TableName, 27 },
			{ OrgAddressSchema.Constants.TableName, 8 },
			{ CusAttributeFilterSchema.Constants.TableName, 18 },
		};

		protected override Dictionary<string, int> USUniversalXMLImportUpdateExpectedHits => new Dictionary<string, int>
		{
			{ CusAddInfoSchema.Constants.TableName, 17 },
			{ CusCodeDataSchema.Constants.TableName, 7 },
			{ EDIMessageSchema.Constants.TableName, 9 },
			{ JobDocAddressSchema.Constants.TableName, 9 },
			{ ProcessTasksSchema.Constants.TableName, 6 },
			{ RefExchangeRateSchema.Constants.TableName, 9 },
			{ RefPacksSchema.Constants.TableName, 7 },
			{ StmDocDataOverrideSchema.Constants.TableName, 26 },
			{ StmNoteSchema.Constants.TableName, 43 },
			{ OrgCusCodeSchema.Constants.TableName, 7 },
			{ OrgHeaderSchema.Constants.TableName, 10 },
			{ CusAttributeFilterSchema.Constants.TableName, 18 },
		};

		protected override Dictionary<string, int> USUniversalXMLAddExpectedHits => new Dictionary<string, int>
		{
			{ CusAddInfoSchema.Constants.TableName, 9 },
			{ CusCodeDataSchema.Constants.TableName, 6 },
			{ EDIMessageSchema.Constants.TableName, 8 },
			{ GenPivotSchema.Constants.TableName, 7 },
			{ OrgAddressSchema.Constants.TableName, 15 },
			{ OrgHeaderSchema.Constants.TableName, 11 },
			{ ProcessJobTriggerLinkSchema.Constants.TableName, 66 },
			{ ProcessTasksSchema.Constants.TableName, 6 },
			{ RefExchangeRateSchema.Constants.TableName, 9 },
			{ RefPacksSchema.Constants.TableName, 7 },
			{ StmDocDataOverrideSchema.Constants.TableName, 22 },
			{ StmNoteSchema.Constants.TableName, 20 },
			{ OrgCusCodeSchema.Constants.TableName, 7 },
			{ JobDocAddressSchema.Constants.TableName, 6 },
			{ JobUSComInvoiceLineSchema.Constants.TableName, 60 },
			{ CusAttributeFilterSchema.Constants.TableName, 18 },
		};

		protected override Dictionary<string, int> USDeleteExpectedHits => new Dictionary<string, int>
		{
			{ CusAddInfoSchema.Constants.TableName, 9 },
			{ StmDocDataOverrideSchema.Constants.TableName, 28 },
			{ StmNoteSchema.Constants.TableName, 25 },
			{ JobDocAddressSchema.Constants.TableName, 8 },
			{ CusDispositionSchema.Constants.TableName, 9 },
			{ CusCodeDataSchema.Constants.TableName, 6 },
			{ CusAttributeFilterSchema.Constants.TableName, 18 },
		};
	}
}
