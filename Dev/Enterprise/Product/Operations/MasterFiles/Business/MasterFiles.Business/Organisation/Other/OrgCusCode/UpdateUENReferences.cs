using System;
using CargoWise.BrandManager;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class UpdateUENReferences : DataLoad
	{
		public UpdateUENReferences()
		{
		}

		public void UpdateUENReferenceData(string dataLocation)
		{
			ImportData(dataLocation, (NoResString)"Organisation UEN Mapping Data record");
		}

		#region DataFields UENData

		public class CsvOrg
		{
			public CsvOrg()
			{
			}

			public ZString OrgCRN;
			public ZString OrgUEN;
			public ZString OrgEntityName;
		}

		public class CsvOrgDataError
		{
			public string ErrorMsg;
		}

		CsvOrgDataError DataError;

		#endregion

		#region ImportFrom .csv file

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
		protected override void ProcessDataForThisLine(OCsvLine line)
		{
			Guid transPK = Guid.Empty;

			if (RunCounters.CurrentRow == 2 && line.FieldValues[0] == "Previous Entity Registration No")
			{
				// Exclude this row as UEN Mapping file has two heading rows.
			}
			else if (line.FieldValues[0].StartsWith((NoResString)"Total Count of records processed:"))
			{
				// Exclude this row as UEN Mapping file has a total processed row at the end of the values.
			}
			else
			{
				CsvOrg orgData = ExtractOrgReferenceNos(line);

				if (orgData != null)
				{
					if (!orgData.OrgUEN.IsEmpty)
					{
						transPK = ProcessExtractedData(orgData);
					}
					else
					{
						RunCounters.RecsExcluded++;
						DisplayLogMessage(Res.GetString("0abec0a2-7f41-46cc-ba31-716f68aca791", "Row {0} excluded... UEN value was not returned. CRN: {1}", RunCounters.CurrentRow.ToString(), orgData.OrgCRN));
					}
				}
				else
				{
					RunCounters.RecsExcluded++;
					DisplayLogMessage(Res.GetString("ab1ff172-1ef4-42ce-8c90-0baa8a486dde", "Row {0} excluded... data is inconsistent with required format {1}", RunCounters.CurrentRow.ToString(), DataError.ErrorMsg));
				}
			}

			UpdateAndDisplayIfRequired(transPK, OrgCusCodeSchema.Constants.TableName);
		}

		protected Guid ProcessExtractedData(CsvOrg orgData)
		{
			Guid transPK = Guid.Empty;
			OrgCusCode[] cRNCusCodes = FindCRN(orgData.OrgCRN);
			if (cRNCusCodes.Length > 0)
			{
				try
				{
					foreach (OrgCusCode crnCusCode in cRNCusCodes)
					{
						if (transPK != Guid.Empty)
						{
							UpdateAndDisplayIfRequired(transPK, OrgCusCodeSchema.Constants.TableName);
							transPK = Guid.Empty;
						}

						ZQuery uENCodeFilter = new ZQuery(OrgCusCodeSchema.OK_CodeType, OrgCusCode.SingaporeCodeTypes.UniqueEntityNumber);
						uENCodeFilter.AddToFilter(OrgCusCodeSchema.OK_RN_NKCodeCountry, crnCusCode.OK_RN_NKCodeCountry);
						uENCodeFilter.AddToFilter(OrgCusCodeSchema.OK_OH, crnCusCode.OK_OH);
						OrgCusCode uENOrgCusCode = (OrgCusCode)Factory.LoadTop1(typeof(OrgCusCode), uENCodeFilter);
						if (uENOrgCusCode == null)
						{
							OrgCusCode uenCusCode = Factory.New<OrgCusCode>();
							uenCusCode.OK_CodeType = OrgCusCode.SingaporeCodeTypes.UniqueEntityNumber;
							uenCusCode.OK_CustomsRegNo = orgData.OrgUEN;
							uenCusCode.OK_OH = crnCusCode.OK_OH;
							uenCusCode.OK_RN_NKCodeCountry = crnCusCode.OK_RN_NKCodeCountry;

							OrgHeader orgBeingUpdated = Factory.Load<OrgHeader>(crnCusCode.OK_OH);
							DisplayLogMessage(Res.GetString("e0d084cc-ba9b-4206-844a-5b5af49688b4", "{0} Organization (Code/Name): {1} / {2} - UEN entity: {3}, updated with UEN: {4}", BrandingFactory.Instance.ProductName, orgBeingUpdated.OH_Code, orgBeingUpdated.OH_FullNameTruncated, orgData.OrgEntityName, orgData.OrgUEN));

							RunCounters.RecsToUpdate++;
							RunCounters.RecsUpdated++;
							transPK = uenCusCode.PK.ToGuid();
						}
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					RunCounters.RecsExcluded++;
					DisplayFormattedLogMessage(orgData.OrgEntityName, ex.Message);
				}
			}
			else
			{
				RunCounters.RecsExcluded++;
				DisplayLogMessage(Res.GetString("c08e050a-9e16-40c2-9f39-11960e2547e6", "CRN reference {0} was no longer found in {1} tables. UEN details returned: UEN{2}, Entity Name: {3}", orgData.OrgCRN, BrandingFactory.Instance.ProductName, orgData.OrgUEN, orgData.OrgEntityName));
			}

			return transPK;
		}

		#endregion

		#region ExtractData

		protected CsvOrg ExtractOrgReferenceNos(OCsvLine line)
		{
			try
			{
				return ExtractCSVOrgData(line);
			}
			catch (Exception e)
			{
				if (e.IsCriticalException())
				{ throw; }
				return null;
			}
		}

		protected CsvOrg ExtractCSVOrgData(OCsvLine line)
		{
			CsvOrg orgData = new CsvOrg();
			DataError = new CsvOrgDataError();
			orgData.OrgCRN = TrimmedZString(line.FieldValues[0]);
			orgData.OrgEntityName = TrimmedZString(line.FieldValues[1]);
			orgData.OrgUEN = TrimmedZString(line.FieldValues[2]);

			return orgData;
		}

		ZString TrimmedZString(string value)
		{
			return new ZString(value).Trim();
		}

		#endregion

		#region Utilities

		#region Notifications

		void DisplayFormattedLogMessage(ZString entityName, string detailedExceptionMessage)
		{
			var ouputRowNo = Res.GetString("c955e1e4-eb8c-40ac-b748-46f14ecc8bdb", "Line {0}:", RunCounters.CurrentRow.ToString());
			var logMessage = Res.GetString("e1eab876-4499-4d4a-81f4-60ca18735454", "{0} Entity: {1}  {2}", ouputRowNo, entityName, detailedExceptionMessage);

			DisplayLogMessage(logMessage);
		}

		protected override void OutputFinalTotals(string dataType)
		{
			DisplayLogMessage("\r\n" + Res.GetString("54ea2c04-44cc-4969-b68e-190c1843b853", "T O T A L : Organization Config references updated with UEN value = {0}, UEN mapping record's excluded = {1}", RunCounters.RecsUpdated, RunCounters.RecsExcluded) + "\r\n");
		}

		#endregion

		#region Org Handling

		OrgCusCode[] FindCRN(string cRN)
		{
			ZQuery currentCRNQuery = new ZQuery(OrgCusCodeSchema.OK_CodeType, OrgCusCode.SingaporeCodeTypes.CentralRegistrationNumber);
			currentCRNQuery.AddToFilter(OrgCusCodeSchema.OK_RN_NKCodeCountry, Core.Constants.CountryCodes.Singapore);
			currentCRNQuery.AddToFilter(OrgCusCodeSchema.OK_CustomsRegNo, cRN);
			OrgCusCode[] cRNCusCodes = Factory.Load<OrgCusCode>(new ZQuery(currentCRNQuery));
			return cRNCusCodes;
		}

		#endregion

		#endregion

		#region CSV File Header

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded file header")]
		readonly string[] ValidFileHeader = new string[] {
													"Previous Entity Registration No Mappings",
												};

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded column headers")]
		readonly string[] ValidFileHeaderColumns = new string[] {
													"Previous Entity Registration No",
													"Entity Name",
													"UEN",
													"UEN Status",
													"Issuance Agency",
													"Entity Type",
													"Registered Address Street Name",
													"Remarks",
												};

		#endregion

		#region Validation

		protected override bool IsFileHeaderValid(OCsvLine line)
		{
			return ValidFileHeader[0].ToUpper() == line.FieldValues[0].ToUpper();
		}

		public override string CSVTemplateHeading
		{
			get
			{
				return
					String.Join(",", ValidFileHeader) +
					System.Environment.NewLine +
					String.Join(",", ValidFileHeaderColumns);
			}
		}

		#endregion
	}
}
