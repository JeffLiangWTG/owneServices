using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.NZ.Business.MasterFiles
{
	public class ClassificationDataLoad : DataLoad
	{
		public void ImportClassificationData(string dataLocation)
		{
			ImportData(dataLocation, "Classification");
		}

		#region FileValidation

		protected override bool IsFileHeaderValid(OCsvLine line)
		{
			return line.FieldValues.Length == 13
				&& line.FieldValues[00].ToUpperInvariant() == CSVCode.ToUpperInvariant()
				&& line.FieldValues[01].ToUpperInvariant() == CSVDescription.ToUpperInvariant()
				&& line.FieldValues[02].ToUpperInvariant() == CSVTariff.ToUpperInvariant()
				&& line.FieldValues[03].ToUpperInvariant() == CSVConcession.ToUpperInvariant()
				&& line.FieldValues[04].ToUpperInvariant() == CSVPartsOfTariff.ToUpperInvariant()
				&& line.FieldValues[05].ToUpperInvariant() == CSVPermitCode1.ToUpperInvariant()
				&& line.FieldValues[06].ToUpperInvariant() == CSVPermitNo1.ToUpperInvariant()
				&& line.FieldValues[07].ToUpperInvariant() == CSVPermitCode2.ToUpperInvariant()
				&& line.FieldValues[08].ToUpperInvariant() == CSVPermitNo2.ToUpperInvariant()
				&& line.FieldValues[09].ToUpperInvariant() == CSVPermitCode3.ToUpperInvariant()
				&& line.FieldValues[10].ToUpperInvariant() == CSVPermitNo3.ToUpperInvariant()
				&& line.FieldValues[11].ToUpperInvariant() == CSVProhibitedCode1.ToUpperInvariant()
				&& line.FieldValues[12].ToUpperInvariant() == CSVProhibitedCode2.ToUpperInvariant();
		}

		public override string CSVTemplateHeading
		{
			get { return string.Join(",", CSVTemplateHeaders); }
		}

		IEnumerable<string> CSVTemplateHeaders
		{
			get
			{
				yield return CSVCode;
				yield return CSVDescription;
				yield return CSVTariff;
				yield return CSVConcession;
				yield return CSVPartsOfTariff;
				yield return CSVPermitCode1;
				yield return CSVPermitNo1;
				yield return CSVPermitCode2;
				yield return CSVPermitNo2;
				yield return CSVPermitCode3;
				yield return CSVPermitNo3;
				yield return CSVProhibitedCode1;
				yield return CSVProhibitedCode2;
			}
		}

		const string CSVCode = "Code";
		const string CSVDescription = "Description";
		const string CSVTariff = "Tariff";
		const string CSVConcession = "Concession";
		const string CSVPartsOfTariff = "PartsOfTariff";
		const string CSVPermitCode1 = "PermitCode1";
		const string CSVPermitNo1 = "PermitNo1";
		const string CSVPermitCode2 = "PermitCode2";
		const string CSVPermitNo2 = "PermitNo2";
		const string CSVPermitCode3 = "PermitCode3";
		const string CSVPermitNo3 = "PermitNo3";
		const string CSVProhibitedCode1 = "ProhibitedCode1";
		const string CSVProhibitedCode2 = "ProhibitedCode2";

		#endregion

		#region ImportFrom .csv file

		protected override void ProcessDataForThisLine(OCsvLine line)
		{
			Guid transactionPK = Guid.Empty;
			try
			{
				ExtractExcelClassificationData(line);
				transactionPK = CreateClassification();
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				DisplayExcludedRecordMessage(" excluded... data is inconsistent with required format.");
			}

			UpdateAndDisplayIfRequired(transactionPK, CusClassificationSchema.Constants.TableName);
		}

		void ExtractExcelClassificationData(OCsvLine line)
		{
			lookupCode = GetZStringValueIfExists(line.FieldValues, 0, 35);
			description = GetZStringValueIfExists(line.FieldValues, 1, 80);
			tariff = GetZStringValueIfExists(line.FieldValues, 2, 14);
			concession = GetZStringValueIfExists(line.FieldValues, 3, 9);
			partsOfTariff = GetZStringValueIfExists(line.FieldValues, 4, 14);

			permitCode1 = GetZStringValueIfExists(line.FieldValues, 5, 3);
			permitNo1 = GetZStringValueIfExists(line.FieldValues, 6, 12);
			permitCode2 = GetZStringValueIfExists(line.FieldValues, 7, 3);
			permitNo2 = GetZStringValueIfExists(line.FieldValues, 8, 12);
			permitCode3 = GetZStringValueIfExists(line.FieldValues, 9, 3);
			permitNo3 = GetZStringValueIfExists(line.FieldValues, 10, 12);

			prohibitedCode1 = GetZStringValueIfExists(line.FieldValues, 11, 3);
			prohibitedCode2 = GetZStringValueIfExists(line.FieldValues, 12, 3);
		}

		Guid CreateClassification()
		{
			Guid transactionPK = Guid.Empty;
			CusClassification enterpriseClass = LoadClassificationIfItExists();

			if (enterpriseClass == null)
			{
				CusClassification newEnterpriseClass = Factory.New<CusClassification>();
				LoadImportedClassificationValues(newEnterpriseClass);
				newEnterpriseClass.Validation.ValidateAll();
				if (newEnterpriseClass.HasErrors)
				{
					DisplayExcludedRecordMessage(": Record Excluded - Tariff values provided are not valid in " + Core.Constants.ProductName);
					newEnterpriseClass.Delete();
				}
				else
				{
					transactionPK = newEnterpriseClass.PK.ToGuid();
					RunCounters.RecsCreated++;
					RunCounters.RecsToUpdate++;
				}
			}
			else
			{
				DisplayExcludedRecordMessage(": Record Excluded - Classification '" + lookupCode + "' already exists in " + Core.Constants.ProductName);
			}

			return transactionPK;
		}

		void LoadImportedClassificationValues(CusClassification enterpriseClassification)
		{
			enterpriseClassification.CC_LookupCode = lookupCode;
			enterpriseClassification.CC_ClassificationType = CusClassification.ClassificationType.Both;
			enterpriseClassification.CC_TariffNum = tariff;
			enterpriseClassification.CC_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			if (!description.IsEmpty)
			{
				enterpriseClassification.CC_Description = description;
			}

			enterpriseClassification.CC_ConcessionCode = concession;
			enterpriseClassification.CC_PartsOfClassification = partsOfTariff;

			enterpriseClassification.PermitCodes.AddOrUpdateExisting(permitCode1, permitNo1);
			enterpriseClassification.PermitCodes.AddOrUpdateExisting(permitCode2, permitNo2);
			enterpriseClassification.PermitCodes.AddOrUpdateExisting(permitCode3, permitNo3);

			enterpriseClassification.ProhibitedCodes.AddOrUpdateExisting(prohibitedCode1, "");
			enterpriseClassification.ProhibitedCodes.AddOrUpdateExisting(prohibitedCode2, "");
		}

		CusClassification LoadClassificationIfItExists()
		{
			ZQuery classFilter = new ZQuery(CusClassificationSchema.CC_LookupCode, lookupCode);
			classFilter.AddToFilter(CusClassificationSchema.CC_ClassificationType, CusClassification.ClassificationType.Both);
			classFilter.AddToFilter(CusClassificationSchema.CC_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			return Factory.LoadTop1<CusClassification>(classFilter);
		}

		protected ZString GetZStringValueIfExists(string[] fieldValues, int index, int length)
		{
			return (index < fieldValues.Length) ? new ZString(fieldValues[index]).Trim().SubstringSafe(0, length) : ZString.Empty;
		}

		#endregion

		#region Classification Data

		protected ZString lookupCode;
		protected ZString description;
		protected ZString tariff;
		protected ZString concession;
		protected ZString partsOfTariff;
		protected ZString permitCode1;
		protected ZString permitNo1;
		protected ZString permitCode2;
		protected ZString permitNo2;
		protected ZString permitCode3;
		protected ZString permitNo3;
		protected ZString prohibitedCode1;
		protected ZString prohibitedCode2;

		#endregion
	}
}
