using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.ZA.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ZA.GUI.Testing
{
	class JobDeclarationFormPerformanceTest : Customs.GUI.Testing.JobDeclarationFormPerformanceAbstractTest
	{
		protected override ZForm GetForm(BusinessObject bizO) => new JobDeclarationForm((JobDeclaration)bizO);

		Dictionary<string, int> ZABaseLoadEditableChildObjectsExpectedHits => new Dictionary<string, int>
		{
			{ CusLineTariffDetailSchema.Constants.TableName, 60 }
		};
		Dictionary<string, int> ZABaseValidateAllExpectedHits => new Dictionary<string, int>
		{
			{ OrgCusCodeSchema.Constants.TableName, 9 },
			{ JobDeclarationSchema.Constants.TableName, 11 }, // ValidationHelper.GetJobNumberOfDuplicateUCR
			{ RefCusProcedureSchema.Constants.TableName, 6 } // CheckJI_Procedure(JobComInvoiceLineLookups.Procedures) and CheckJI_BondedWhsQuantity(IsBondedWhsQuantityRequired->invoiceLine.CusProcedure)
		};
		Dictionary<string, int> ZABaseLightFormValidationAndSaveExpectedHits => new Dictionary<string, int>
		{
			{ OrgCusCodeSchema.Constants.TableName, 8 },
			{ JobDeclarationSchema.Constants.TableName, 11 }, // ValidationHelper.GetJobNumberOfDuplicateUCR
			{ RefCusProcedureSchema.Constants.TableName, 6 }, // CheckJI_Procedure(JobComInvoiceLineLookups.Procedures) and CheckJI_BondedWhsQuantity(IsBondedWhsQuantityRequired->invoiceLine.CusProcedure)
			{ JobDocAddressSchema.Constants.TableName, 60 } // CusEntryInstructionValidation.ValidateOH_SubContractor()
		};
		Dictionary<string, int> ZABaseFormMergeExpectedHits => new Dictionary<string, int>
		{
			{ JobComInvHeaderChargeSchema.Constants.TableName, 7 },
			{ RefCusProcedureSchema.Constants.TableName, 51 },
			{ JobDocAddressSchema.Constants.TableName, 60 }, // CusEntryInstructionValidation.ValidateOH_SubContractor()
			{ CusVehicleSchema.Constants.TableName, 60 }
		};
		Dictionary<string, int> ZABaseUniversalXMLExportExpectedHits => new Dictionary<string, int>
		{
			{ JobDocAddressSchema.Constants.TableName, 61 }, // CusEntryInstructionValidation.ValidateOH_SubContractor()
			{ CusVehicleSchema.Constants.TableName, 60 }
		};
		Dictionary<string, int> ZABaseUniversalXMLImportUpdateExpectedHits => new Dictionary<string, int>
		{
			{ OrgAddressSchema.Constants.TableName, 10 },
			{ OrgHeaderSchema.Constants.TableName, 11 },
			{ RefCusProcedureSchema.Constants.TableName, 6 } // Default CEI_Style(ProcedureGroup) and JI_CEI(SetDefaultPreviousProcedureCode)
		};
		Dictionary<string, int> ZABaseUniversalXMLAddExpectedHits => new Dictionary<string, int>
		{
			{ OrgAddressSchema.Constants.TableName, 10 },
			{ OrgHeaderSchema.Constants.TableName, 12 },
			{ RefCusProcedureSchema.Constants.TableName, 6 } // Default CEI_Style(ProcedureGroup) and JI_CEI(SetDefaultPreviousProcedureCode)
		};
		Dictionary<string, int> ZABaseDeleteExpectedHits => new Dictionary<string, int>
		{
			{ GenAddOnColumnSchema.Constants.TableName, 67 },
			{ StmNoteSchema.Constants.TableName, 13 },
			{ StmDocDataOverrideSchema.Constants.TableName, 8 }
		};

		protected virtual Dictionary<string, int> ZALoadEditableChildObjectsExpectedHits => new Dictionary<string, int>();
		protected virtual Dictionary<string, int> ZAValidateAllExpectedHits => new Dictionary<string, int>();
		protected virtual Dictionary<string, int> ZALightFormValidationAndSaveExpectedHits => new Dictionary<string, int>();
		protected virtual Dictionary<string, int> ZAFormMergeExpectedHits => new Dictionary<string, int>();
		protected virtual Dictionary<string, int> ZAUniversalXMLExportExpectedHits => new Dictionary<string, int>();
		protected virtual Dictionary<string, int> ZAUniversalXMLImportUpdateExpectedHits => new Dictionary<string, int>();
		protected virtual Dictionary<string, int> ZAUniversalXMLAddExpectedHits => new Dictionary<string, int>();
		protected virtual Dictionary<string, int> ZADeleteExpectedHits => new Dictionary<string, int>();

		protected override Dictionary<string, int> LoadEditableChildObjectsExpectedHits => ZipDictionaries(ZABaseLoadEditableChildObjectsExpectedHits, ZALoadEditableChildObjectsExpectedHits);
		protected override Dictionary<string, int> ValidateAllExpectedHits => ZipDictionaries(ZABaseValidateAllExpectedHits, ZAValidateAllExpectedHits);
		protected override Dictionary<string, int> LightFormValidationAndSaveExpectedHits => ZipDictionaries(ZABaseLightFormValidationAndSaveExpectedHits, ZALightFormValidationAndSaveExpectedHits);
		protected override Dictionary<string, int> FormMergeExpectedHits => ZipDictionaries(ZABaseFormMergeExpectedHits, ZAFormMergeExpectedHits);
		protected override Dictionary<string, int> UniversalXMLExportExpectedHits => ZipDictionaries(ZABaseUniversalXMLExportExpectedHits, ZAUniversalXMLExportExpectedHits);
		protected override Dictionary<string, int> UniversalXMLImportUpdateExpectedHits => ZipDictionaries(ZABaseUniversalXMLImportUpdateExpectedHits, ZAUniversalXMLImportUpdateExpectedHits);
		protected override Dictionary<string, int> UniversalXMLAddExpectedHits => ZipDictionaries(ZABaseUniversalXMLAddExpectedHits, ZAUniversalXMLAddExpectedHits);
		protected override Dictionary<string, int> DeleteExpectedHits => ZipDictionaries(ZABaseDeleteExpectedHits, ZADeleteExpectedHits);

		protected override string[] GetTablesToGatherHitQuery()
		{
			var tablesToGatherHitQuery = base.GetTablesToGatherHitQuery();
			return tablesToGatherHitQuery.Append(CusEntryHeaderSchema.Constants.TableName,
				CusEntryLineSchema.Constants.TableName,
				RefCusProcedureSchema.Constants.TableName,
				CusEntryHeaderChargesSchema.Constants.TableName,
				CusEntryPayInfoSchema.Constants.TableName,
				CusEntryNumSchema.Constants.TableName,
				CusEntryInstructionSchema.Constants.TableName).ToArray();
		}

		protected override void DecorateDeclaration(BaseJobDeclaration declaration)
		{
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		}

		protected override void AddEntryInstructions(BaseJobDeclaration declaration, int numberOfEntryInstructions)
		{
			for (int index = 1; index <= numberOfEntryInstructions; index++)
			{
				var instruction = declaration.CustomsEntryInstructions.AddNew();
				instruction.CEI_Description = index.ToString();
				instruction.CEI_Style = "A" + index;
			}
		}

		protected override void DecorateInvoiceHeader(BaseJobComInvoiceHeader invoice)
		{
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.SouthAfrica;
		}

		protected override void DecorateInvoiceLine(BaseJobComInvoiceLine invoiceLine)
		{
			invoiceLine.JI_CEI = invoiceLine.Declaration.CustomsEntryInstructions[invoiceLine.JI_LineNo - 1].PK;
		}
	}
}
