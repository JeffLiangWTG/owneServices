using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.NZ.GUI.Declaration.Testing
{
	class JobDeclarationFormPerformanceTest : Customs.GUI.Testing.JobDeclarationFormPerformanceAbstractTest
	{
		protected override ZForm GetForm(BusinessObject bizO) => new DeclarationBasherForm((JobDeclaration)bizO);
		Dictionary<string, int> NZBaseLoadEditableChildObjectsExpectedHits => new Dictionary<string, int> { { CusCodeDataSchema.Constants.TableName, 5 } };
		Dictionary<string, int> NZBaseValidateAllExpectedHits => new Dictionary<string, int> { { CusCodeDataSchema.Constants.TableName, 6 }, { GlbBranchSchema.Constants.TableName, 5 }, { OrgCusCodeSchema.Constants.TableName, 8 }, { OrgHeaderSchema.Constants.TableName, 5 } };
		Dictionary<string, int> NZBaseLightFormValidationAndSaveExpectedHits => new Dictionary<string, int> { { OrgCusCodeSchema.Constants.TableName, 8 } };
		Dictionary<string, int> NZBaseFormMergeExpectedHits => new Dictionary<string, int> { { OrgHeaderSchema.Constants.TableName, 7 } };
		Dictionary<string, int> NZBaseUniversalXMLExportExpectedHits => new Dictionary<string, int> { };
		Dictionary<string, int> NZBaseUniversalXMLImportUpdateExpectedHits => new Dictionary<string, int> { { CusCodeDataSchema.Constants.TableName, 5 }, { StmNoteSchema.Constants.TableName, 13 }, { OrgCusCodeSchema.Constants.TableName, 8 } };
		Dictionary<string, int> NZBaseUniversalXMLAddExpectedHits => new Dictionary<string, int> { { OrgCusCodeSchema.Constants.TableName, 8 } };
		Dictionary<string, int> NZBaseDeleteExpectedHits => new Dictionary<string, int>();
		protected virtual Dictionary<string, int> NZLoadEditableChildObjectsExpectedHits => new Dictionary<string, int>();
		protected virtual Dictionary<string, int> NZValidateAllExpectedHits => new Dictionary<string, int>();
		protected virtual Dictionary<string, int> NZLightFormValidationAndSaveExpectedHits => new Dictionary<string, int>();
		protected virtual Dictionary<string, int> NZFormMergeExpectedHits => new Dictionary<string, int>();
		protected virtual Dictionary<string, int> NZUniversalXMLExportExpectedHits => new Dictionary<string, int>();
		protected virtual Dictionary<string, int> NZUniversalXMLImportUpdateExpectedHits => new Dictionary<string, int>();
		protected virtual Dictionary<string, int> NZUniversalXMLAddExpectedHits => new Dictionary<string, int>();
		protected virtual Dictionary<string, int> NZDeleteExpectedHits => new Dictionary<string, int>();
		protected override Dictionary<string, int> LoadEditableChildObjectsExpectedHits => ZipDictionaries(NZBaseLoadEditableChildObjectsExpectedHits, NZLoadEditableChildObjectsExpectedHits);
		protected override Dictionary<string, int> ValidateAllExpectedHits => ZipDictionaries(NZBaseValidateAllExpectedHits, NZValidateAllExpectedHits);
		protected override Dictionary<string, int> LightFormValidationAndSaveExpectedHits => ZipDictionaries(NZBaseLightFormValidationAndSaveExpectedHits, NZLightFormValidationAndSaveExpectedHits);
		protected override Dictionary<string, int> FormMergeExpectedHits => ZipDictionaries(NZBaseFormMergeExpectedHits, NZFormMergeExpectedHits);
		protected override Dictionary<string, int> UniversalXMLExportExpectedHits => ZipDictionaries(NZBaseUniversalXMLExportExpectedHits, NZUniversalXMLExportExpectedHits);
		protected override Dictionary<string, int> UniversalXMLImportUpdateExpectedHits => ZipDictionaries(NZBaseUniversalXMLImportUpdateExpectedHits, NZUniversalXMLImportUpdateExpectedHits);
		protected override Dictionary<string, int> UniversalXMLAddExpectedHits => ZipDictionaries(NZBaseUniversalXMLAddExpectedHits, NZUniversalXMLAddExpectedHits);
		protected override Dictionary<string, int> DeleteExpectedHits => ZipDictionaries(NZBaseDeleteExpectedHits, NZDeleteExpectedHits);
	}
}
