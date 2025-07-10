using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.TW.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TW.GUI.Testing
{
	abstract class JobDeclarationFormPerformanceAbstractTest : Customs.GUI.Testing.JobDeclarationFormPerformanceAbstractTest
	{
		protected override ZForm GetForm(BusinessObject bizO) => new JobDeclarationForm((JobDeclaration)bizO);
		Dictionary<string, int> TWBaseLoadEditableChildObjectsExpectedHits => new Dictionary<string, int>();
		Dictionary<string, int> TWBaseValidateAllExpectedHits => new Dictionary<string, int>()
		{
			{ OrgAddressSchema.Constants.TableName, 8 },
			{ OrgTranslatedAddressSchema.Constants.TableName, 6 }
		};
		Dictionary<string, int> TWBaseLightFormValidationAndSaveExpectedHits => new Dictionary<string, int>()
		{
			{ OrgAddressSchema.Constants.TableName, 8 },
			{ OrgTranslatedAddressSchema.Constants.TableName, 6 }
		};
		Dictionary<string, int> TWBaseFormMergeExpectedHits => new Dictionary<string, int>();
		Dictionary<string, int> TWBaseUniversalXMLExportExpectedHits => new Dictionary<string, int>();
		Dictionary<string, int> TWBaseUniversalXMLImportUpdateExpectedHits => new Dictionary<string, int>
		{
			{ StmDocDataOverrideSchema.Constants.TableName, 6 },
			{ StmNoteSchema.Constants.TableName, 10 }
		};
		Dictionary<string, int> TWBaseUniversalXMLAddExpectedHits => new Dictionary<string, int>() { };
		Dictionary<string, int> TWBaseDeleteExpectedHits => new Dictionary<string, int>() { { StmDocDataOverrideSchema.Constants.TableName, 8 } };
		protected virtual Dictionary<string, int> TWLoadEditableChildObjectsExpectedHits => new Dictionary<string, int>();
		protected virtual Dictionary<string, int> TWValidateAllExpectedHits => new Dictionary<string, int> { };
		protected virtual Dictionary<string, int> TWLightFormValidationAndSaveExpectedHits => new Dictionary<string, int>() { };
		protected virtual Dictionary<string, int> TWFormMergeExpectedHits => new Dictionary<string, int>();
		protected virtual Dictionary<string, int> TWUniversalXMLExportExpectedHits => new Dictionary<string, int>();
		protected virtual Dictionary<string, int> TWUniversalXMLImportUpdateExpectedHits => new Dictionary<string, int>() { };
		protected virtual Dictionary<string, int> TWUniversalXMLAddExpectedHits => new Dictionary<string, int>() { };
		protected virtual Dictionary<string, int> TWDeleteExpectedHits => new Dictionary<string, int>();
		protected override Dictionary<string, int> LoadEditableChildObjectsExpectedHits => ZipDictionaries(TWBaseLoadEditableChildObjectsExpectedHits, TWLoadEditableChildObjectsExpectedHits);
		protected override Dictionary<string, int> ValidateAllExpectedHits => ZipDictionaries(TWBaseValidateAllExpectedHits, TWValidateAllExpectedHits);
		protected override Dictionary<string, int> LightFormValidationAndSaveExpectedHits => ZipDictionaries(TWBaseLightFormValidationAndSaveExpectedHits, TWLightFormValidationAndSaveExpectedHits);
		protected override Dictionary<string, int> FormMergeExpectedHits => ZipDictionaries(TWBaseFormMergeExpectedHits, TWFormMergeExpectedHits);
		protected override Dictionary<string, int> UniversalXMLExportExpectedHits => ZipDictionaries(TWBaseUniversalXMLExportExpectedHits, TWUniversalXMLExportExpectedHits);
		protected override Dictionary<string, int> UniversalXMLImportUpdateExpectedHits => ZipDictionaries(TWBaseUniversalXMLImportUpdateExpectedHits, TWUniversalXMLImportUpdateExpectedHits);
		protected override Dictionary<string, int> UniversalXMLAddExpectedHits => ZipDictionaries(TWBaseUniversalXMLAddExpectedHits, TWUniversalXMLAddExpectedHits);
		protected override Dictionary<string, int> DeleteExpectedHits => ZipDictionaries(TWBaseDeleteExpectedHits, TWDeleteExpectedHits);
	}
}
