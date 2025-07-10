using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.GUI.Testing
{
	sealed class JobDeclarationExportFormPerformanceTest : JobDeclarationFormPerformanceTest
	{
		protected override ZString MessageTypeForFormBashing => JobMessageTypeList.Codes.Export;
		protected override int AcceptableVarianceForXMLImport => 2;

		protected override Dictionary<string, int> USValidateAllExpectedHits => new Dictionary<string, int>
		{
			{ CusAddInfoSchema.Constants.TableName, 24 },
			{ JobRequiredDocumentSchema.Constants.TableName, 7 },
			{ OrgMiscServSchema.Constants.TableName, 8 },
			{ RefPacksSchema.Constants.TableName, 6 },
			{ TariffViewSchema.Constants.TableName, 8 },
			{ JobDocAddressSchema.Constants.TableName, 8 },
			{ OrgAddressSchema.Constants.TableName, 13 },
			{ OrgHeaderSchema.Constants.TableName, 5 },
			{ OrgContactSchema.Constants.TableName, 7 },
			{ OrgCusCodeSchema.Constants.TableName, 7 },
			{ ZZRefCusCodeListCombinedSchema.Constants.TableName, 9 },
			{ CusDispositionSchema.Constants.TableName, 19 },
			{ "NonPersitentTable DBO.JOBCOMINVOICEHEADER LEFT JOIN DBO.JOBDECLARATION ON JZ_JE = JE_PK", 6 },
			{ OrgRelatedPartySchema.Constants.TableName, 7 },
		};

		protected override Dictionary<string, int> USLoadEditableChildObjectsExpectedHits => new Dictionary<string, int>
		{
			{ CusAddInfoSchema.Constants.TableName, 24 },
			{ JobDocAddressSchema.Constants.TableName, 9 },
			{ OrgRelatedPartySchema.Constants.TableName, 6 },
		};

		protected override Dictionary<string, int> USLightFormValidationAndSaveExpectedHits => new Dictionary<string, int>
		{
			{ CusAddInfoSchema.Constants.TableName, 62 },
			{ JobRequiredDocumentSchema.Constants.TableName, 7 },
			{ OrgCusCodeSchema.Constants.TableName, 8 },
			{ OrgMiscServSchema.Constants.TableName, 8 },
			{ RefPacksSchema.Constants.TableName, 6 },
			{ TariffViewSchema.Constants.TableName, 7 },
			{ JobDocAddressSchema.Constants.TableName, 7 },
			{ OrgAddressSchema.Constants.TableName, 13 },
			{ OrgContactSchema.Constants.TableName, 7 },
			{ "NonPersitentTable DBO.JOBCOMINVOICEHEADER LEFT JOIN DBO.JOBDECLARATION ON JZ_JE = JE_PK", 6 },
			{ OrgRelatedPartySchema.Constants.TableName, 7 },
		};

		protected override Dictionary<string, int> USFormMergeExpectedHits => new Dictionary<string, int>
		{
			{ CusEntryHeaderChargesSchema.Constants.TableName, 6 },
			{ CusEntryNumSchema.Constants.TableName, 7 },
			{ StmALogSchema.Constants.TableName, 6 },
			{ JobDocAddressSchema.Constants.TableName, 6 },
			{ GenPivotSchema.Constants.TableName, 7 },
			{ OrgRelatedPartySchema.Constants.TableName, 6 },
		};

		protected override Dictionary<string, int> USUniversalXMLImportUpdateExpectedHits => new Dictionary<string, int>
		{
			{ CusAddInfoSchema.Constants.TableName, 24 },
			{ CusClassificationSchema.Constants.TableName, 19 },
			{ EDIMessageSchema.Constants.TableName, 13 },
			{ OrgContactSchema.Constants.TableName, 6 },
			{ ProcessTasksSchema.Constants.TableName, 5 },
			{ USCTariffSchema.Constants.TableName, 7 },
			{ TariffViewSchema.Constants.TableName, 10 },
			{ RefPacksSchema.Constants.TableName, 7 },
			{ OrgHeaderSchema.Constants.TableName, 12 },
			{ USCTariffDutyRateSchema.Constants.TableName, 5 },
			{ JobDocAddressSchema.Constants.TableName, 16 },
			{ JobDocumentExclusionSchema.Constants.TableName, 7 },
			{ JobShipmentSchema.Constants.TableName, 12 },
			{ OrgAddressSchema.Constants.TableName, 13 },
			{ TariffUOMViewSchema.Constants.TableName, 5 },
			{ JobComInvoiceHeaderSchema.Constants.TableName, 4 },
			{ GenPivotSchema.Constants.TableName, 8 },
			{ StmDocDataOverrideSchema.Constants.TableName, 46 },
			{ StmNoteSchema.Constants.TableName, 74 }
		};

		protected override Dictionary<string, int> USUniversalXMLAddExpectedHits => new Dictionary<string, int>
		{
			{ CusClassificationSchema.Constants.TableName, 19 },
			{ OrgContactSchema.Constants.TableName, 6 },
			{ USCTariffSchema.Constants.TableName, 7 },
			{ TariffViewSchema.Constants.TableName, 10 },
			{ RefPacksSchema.Constants.TableName, 7 },
			{ USCTariffDutyRateSchema.Constants.TableName, 5 },
			{ GlbBranchSchema.Constants.TableName, 6 },
			{ JobShipmentSchema.Constants.TableName, 6 },
			{ OrgHeaderSchema.Constants.TableName, 9 },
			{ TariffUOMViewSchema.Constants.TableName, 5 },
		};

		protected override Dictionary<string, int> USDeleteExpectedHits => new Dictionary<string, int>
		{
			{ ProcessHeaderSchema.Constants.TableName, 9 },
			{ StmALogSchema.Constants.TableName, 6 },
			{ StmDocDataOverrideSchema.Constants.TableName, 40 },
			{ JobDocAddressSchema.Constants.TableName, 15 },
			{ JobDocumentExclusionSchema.Constants.TableName, 8 },
			{ JobShipmentSchema.Constants.TableName, 6 },
			{ StmNoteSchema.Constants.TableName, 40 },
			{ GenPivotSchema.Constants.TableName, 8 },
			{ OrgRelatedPartySchema.Constants.TableName, 6 },
		};

		protected override Dictionary<string, int> USUniversalXMLExportExpectedHits => new Dictionary<string, int>
		{
			{ CusCodeDataSchema.Constants.TableName, 62 },
			{ StmDocDataOverrideSchema.Constants.TableName, 42 },
			{ JobDocAddressSchema.Constants.TableName, 7 },
			{ JobHeaderSchema.Constants.TableName, 5 },
			{ OrgContactSchema.Constants.TableName, 7 },
			{ OrgRelatedPartySchema.Constants.TableName, 6 },
		};
	}
}
