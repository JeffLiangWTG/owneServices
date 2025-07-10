using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.SG.V4.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.SG.V4.GUI.Testing
{
	internal abstract class JobDeclarationFormPerformanceAbstractTest : Customs.GUI.Testing.JobDeclarationFormPerformanceAbstractTest
	{
		protected override ZForm GetForm(BusinessObject bizO) => new JobDeclarationForm((JobDeclaration)bizO);
		Dictionary<string, int> SGBaseLoadEditableChildObjectsExpectedHits => new Dictionary<string, int>();
		Dictionary<string, int> SGBaseValidateAllExpectedHits => new Dictionary<string, int> { { GlbBranchSchema.Constants.TableName, 5 } };
		Dictionary<string, int> SGBaseLightFormValidationAndSaveExpectedHits => new Dictionary<string, int>();
		Dictionary<string, int> SGBaseFormMergeExpectedHits => new Dictionary<string, int>();
		Dictionary<string, int> SGBaseUniversalXMLExportExpectedHits => new Dictionary<string, int> { { JobHeaderSchema.Constants.TableName, 5 } };
		Dictionary<string, int> SGBaseUniversalXMLImportUpdateExpectedHits => new Dictionary<string, int> { { StmNoteSchema.Constants.TableName, 7 } };
		Dictionary<string, int> SGBaseUniversalXMLAddExpectedHits => new Dictionary<string, int>()
		{ { OrgHeaderSchema.Constants.TableName, 11 } };
		Dictionary<string, int> SGBaseDeleteExpectedHits => new Dictionary<string, int> { { OrgHeaderSchema.Constants.TableName, 11 }, { StmDocDataOverrideSchema.Constants.TableName, 10 }, { StmNoteSchema.Constants.TableName, 5 } };
		protected virtual Dictionary<string, int> SGLoadEditableChildObjectsExpectedHits => new Dictionary<string, int>();
		protected virtual Dictionary<string, int> SGValidateAllExpectedHits => new Dictionary<string, int>();
		protected virtual Dictionary<string, int> SGLightFormValidationAndSaveExpectedHits => new Dictionary<string, int>();
		protected virtual Dictionary<string, int> SGFormMergeExpectedHits => new Dictionary<string, int>();
		protected virtual Dictionary<string, int> SGUniversalXMLExportExpectedHits => new Dictionary<string, int>();
		protected virtual Dictionary<string, int> SGUniversalXMLImportUpdateExpectedHits => new Dictionary<string, int>();
		protected virtual Dictionary<string, int> SGUniversalXMLAddExpectedHits => new Dictionary<string, int>();
		protected virtual Dictionary<string, int> SGDeleteExpectedHits => new Dictionary<string, int>();
		protected override Dictionary<string, int> LoadEditableChildObjectsExpectedHits => ZipDictionaries(SGBaseLoadEditableChildObjectsExpectedHits, SGLoadEditableChildObjectsExpectedHits);
		protected override Dictionary<string, int> ValidateAllExpectedHits => ZipDictionaries(SGBaseValidateAllExpectedHits, SGValidateAllExpectedHits);
		protected override Dictionary<string, int> LightFormValidationAndSaveExpectedHits => ZipDictionaries(SGBaseLightFormValidationAndSaveExpectedHits, SGLightFormValidationAndSaveExpectedHits);
		protected override Dictionary<string, int> FormMergeExpectedHits => ZipDictionaries(SGBaseFormMergeExpectedHits, SGFormMergeExpectedHits);
		protected override Dictionary<string, int> UniversalXMLExportExpectedHits => ZipDictionaries(SGBaseUniversalXMLExportExpectedHits, SGUniversalXMLExportExpectedHits);
		protected override Dictionary<string, int> UniversalXMLImportUpdateExpectedHits => ZipDictionaries(SGBaseUniversalXMLImportUpdateExpectedHits, SGUniversalXMLImportUpdateExpectedHits);
		protected override Dictionary<string, int> UniversalXMLAddExpectedHits => ZipDictionaries(SGBaseUniversalXMLAddExpectedHits, SGUniversalXMLAddExpectedHits);
		protected override Dictionary<string, int> DeleteExpectedHits => ZipDictionaries(SGBaseDeleteExpectedHits, SGDeleteExpectedHits);
	}
}
