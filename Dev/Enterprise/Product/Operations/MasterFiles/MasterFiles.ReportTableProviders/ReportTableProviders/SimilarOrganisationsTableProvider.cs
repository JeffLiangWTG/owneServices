using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.DataProviders;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using SimilarOrgDataTable = Enterprise.MasterFiles.ReportTableProviders.SimilarOrganisationsReportDataset.SimilarOrganisationsDatasetDataTable;

namespace Enterprise.MasterFiles.ReportTableProviders
{
	public class SimilarOrganisationsTableProvider : ParameterisedTableProvider
	{
		protected override DataTable GetDataTable()
		{
			return GetTableForReport(ReportFilterSettings);
		}

		public BusinessObjectFactory Factory = new BusinessObjectFactory();

		#region Create Temp Table For Report

		DataTable GetTableForReport(FilterOptions reportFilterSettings)
		{
			if (reportFilterSettings == null)
			{
				reportFilterSettings = new FilterOptions(Factory);
			}

			SimilarOrgDataTable reportData = new SimilarOrgDataTable();
			OrgMatchThresholds thresholdList = new OrgMatchThresholds();
			ZString threshold = thresholdList.GetCodeFromDescription(reportFilterSettings.MatchLikelihood);
			FilteredBusinessObjectReader reader = new FilteredBusinessObjectReader(reportFilterSettings.ToZQuery(), typeof(OrgHeader));
			reader.FactoryProvider.Current.RefreshEnabled = false;

			foreach (OrgHeader org in reader)
			{
				Dictionary<ZGuid, object> similarOrgsAlreadyShown = new Dictionary<ZGuid, object>();
				org.PatternMatchesForThisOrg.MatchThresholdOverride = threshold;
				AddSimilarOrganisationsForOrg(reportData, org, similarOrgsAlreadyShown);
			}

			return reportData;
		}

		#region Generation Helpers

		protected void AddSimilarOrganisationsForOrg(SimilarOrgDataTable reportData, OrgHeader header, Dictionary<ZGuid, object> similarOrgsAlreadyShown)
		{
			header.SimilarOrgFinder.FindSimilarOrganisations(false);
			if (header.SimilarOrgMatches.Count > 0)
			{
				foreach (OrgPatternMatch currentMatch in header.SimilarOrgMatches)
				{
					if (!similarOrgsAlreadyShown.ContainsKey(currentMatch.Header.PK)) // Dont show the same org twice (cause there are multiple pattern matches per org)
					{
						AddMatchToTempTable(reportData, header, currentMatch);
						similarOrgsAlreadyShown.Add(currentMatch.Header.PK, null);
					}
				}
			}
		}

		protected void AddMatchToTempTable(SimilarOrgDataTable reportData, OrgHeader mainOrg, OrgPatternMatch matchToAdd)
		{
			SimilarOrganisationsReportDataset.SimilarOrganisationsDatasetRow newMatchRow = reportData.NewSimilarOrganisationsDatasetRow();

			// Add Main Org Fields
			newMatchRow.MainOrgCode = mainOrg.OH_Code;
			newMatchRow.MainOrgName = mainOrg.OH_FullName;
			newMatchRow.MainBusinessRegNo = mainOrg.PrimaryRegistrationNumber.Number;
			newMatchRow.MainUNLOCO = mainOrg.OH_RL_NKClosestPort;
			newMatchRow.MainAddress1 = mainOrg.MainAddress.OA_Address1;
			newMatchRow.MainAddress2 = mainOrg.MainAddress.OA_Address2;
			newMatchRow.MainCity = mainOrg.MainAddress.OA_City;
			newMatchRow.MainState = mainOrg.MainAddress.OA_State;
			newMatchRow.MainPostCode = mainOrg.MainAddress.OA_PostCode;
			newMatchRow.MainPhone = mainOrg.MainAddress.OA_Phone;
			newMatchRow.MainFax = mainOrg.MainAddress.OA_Fax;
			newMatchRow.MainEmail = mainOrg.MainAddress.OA_Email;
			newMatchRow.MainWeb = mainOrg.MainWebURL.PU_URL;

			// Add Similar Org Fields
			newMatchRow.MatchLikelihood = matchToAdd.MatchLikelihood;
			newMatchRow.OS_Score = matchToAdd.OS_Score.ToString();
			newMatchRow.SimilarOrgCode = matchToAdd.Header.OH_Code;
			newMatchRow.SimilarOrgName = matchToAdd.Header.OH_FullName;
			newMatchRow.SimilarBusinessRegNo = matchToAdd.OS_BusinessRegNo;
			newMatchRow.SimilarUNLOCO = matchToAdd.Header.OH_RL_NKClosestPort;
			newMatchRow.SimilarAddress1 = matchToAdd.Address.OA_Address1;
			newMatchRow.SimilarAddress2 = matchToAdd.Address.OA_Address2;
			newMatchRow.SimilarCity = matchToAdd.Address.OA_City;
			newMatchRow.SimilarState = matchToAdd.Address.OA_State;
			newMatchRow.SimilarPostCode = matchToAdd.Address.OA_PostCode;
			newMatchRow.SimilarPhone = matchToAdd.Address.OA_Phone;
			newMatchRow.SimilarFax = matchToAdd.Address.OA_Fax;
			newMatchRow.SimilarEmail = matchToAdd.Address.OA_Email;
			newMatchRow.SimilarWeb = matchToAdd.Header.MainWebURL.PU_URL;

			reportData.Rows.Add(newMatchRow);
		}

		#endregion

		#endregion

		#region Search Filter Parameters

		protected override void SetupParameterValues(object[] values)
		{
			ReportFilterSettings = new FilterOptions(Factory,
				new ZGuid(values[0]),       // Arg 1: Filter By Org
				new ZGuid(values[1]),       // Arg 2: Filter By Branch
				new ZGuid(values[2]),       // Arg 3: Filter By UNLOCO
				new ZGuid(values[3]),       // Arg 4: Filter By Country
				!string.IsNullOrEmpty((string)values[4]) ? ZBool.True : ZBool.False,     // Arg 5: Filter By National Account
				!string.IsNullOrEmpty((string)values[5]) ? ZBool.True : ZBool.False,     // Arg 6: Filter By Temporary Account
				(string)values[6],              // Arg 7: Filter By Organisation Name starting with
				(string)values[7],              // Arg 8: Filter by Match Likelihood
				new ZGuid(values[8]),               // Arg 9: Filter by AR Group
				new ZGuid(values[9]),           // Arg 10: Filter by AP Group
				(string)values[10],             // Arg 11: Filter By Account Type
				(ZDateTime)values[11],          // Arg 12: Filter Org added from date
				(ZDateTime)values[12]           // Arg 13: Filter Org added to date

			);
		}

		protected override Parameter[] ExpectedParameters()
		{
			return new Parameter[]
			{
				new Parameter("MainOrgPK", typeof(Guid)),
				new Parameter("MainBranchPK", typeof(Guid)),
				new Parameter("MainUNLOCOPK", typeof(Guid)),
				new Parameter("MainCountryPK", typeof(Guid)),
				new Parameter("IsNational", typeof(string)),
				new Parameter("IsTemporary", typeof(string)),
				new Parameter("OrgNameStartsWith", typeof(string)),
				new Parameter("MatchLikelihood", typeof(string)),
				new Parameter("ARGroupPK", typeof(Guid)),
				new Parameter("APGroupPK", typeof(Guid)),
				new Parameter("AccountType", typeof(string)),
				new Parameter("OrgAddedFromDate", typeof(ZDateTime)),
				new Parameter("OrgAddedToDate", typeof(ZDateTime))
			};
		}

		#region FilterOptions

		public class FilterOptions
		{
			public FilterOptions(BusinessObjectFactory factory, ZGuid mainOrgPK, ZGuid mainBranchPK, ZGuid mainUNLOCOPK, ZGuid mainCountryPK, ZBool isNational, ZBool isTemporary, ZString orgNameStartsWith, ZString matchLikelihood, ZGuid aRGroup, ZGuid aPGroup, ZString accountType, ZDateTime orgAddedFromDate, ZDateTime orgAddedToDate)
			{
				this.Factory = factory;
				this.MainOrgPK = mainOrgPK;
				this.MainBranchPK = mainBranchPK;
				this.MainUNLOCOPK = mainUNLOCOPK;
				this.MainCountryPK = mainCountryPK;
				this.IsNational = isNational;
				this.IsTemporary = isTemporary;
				this.OrgNameStartsWith = orgNameStartsWith;
				this.MatchLikelihood = matchLikelihood;
				this.APGroupPK = aPGroup;
				this.ARGroupPK = aRGroup;
				this.AccountType = accountType;
				this.OrgAddedFromDate = orgAddedFromDate;
				this.OrgAddedToDate = orgAddedToDate;
			}

			/// <summary>
			/// Empty Filter
			/// </summary>
			public FilterOptions(BusinessObjectFactory factory)
			{
				this.Factory = factory;
			}

			#region ToZQuery

			public ZQuery ToZQuery()
			{
				ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(OrgHeader));

				result.AddToFilter(MainOrgFilter);
				result.AddToFilter(OrgNameStartsWithFilter);
				result.AddToFilter(MainUNLOCOPKFilter);
				result.AddToFilter(MainCountryPKFilter);
				result.AddToFilter(IsNationalFilter);
				result.AddToFilter(IsTemporaryFilter);
				result.AddToFilter(AccountTypeCNECNR);
				CompanyDataSubQuery.AddToFilter(ARGroupFilter);
				CompanyDataSubQuery.AddToFilter(APGroupFilter);
				CompanyDataSubQuery.AddToFilter(AccountTypeARAP);
				CompanyDataSubQuery.AddToFilter(MainBranchPKFilter);

				result.AddSubQuery(CompanyDataSubQuery, JoinCondition.And);

				ZDBOnlySubQuery orgAddedEventLogSubQuery = GetOrgAddedEventLogSubQuery();
				if (orgAddedEventLogSubQuery != null)
				{
					result.AddSubQuery(orgAddedEventLogSubQuery, JoinCondition.And);
				}

				return result;
			}

			#region Filters

			#region OrgHeader Filters

			protected ZQuery MainOrgFilter
			{
				get { return MainOrgPK.IsValid ? new ZQuery(OrgHeaderSchema.PK, MainOrgPK) : new ZQuery(); }
			}

			protected ZQuery OrgNameStartsWithFilter
			{
				get { return !OrgNameStartsWith.IsEmpty ? new ZQuery(OrgHeaderSchema.OH_FullName, SQLComparisonOperator.StartsWith, OrgNameStartsWith) : new ZQuery(); }
			}

			protected ZQuery MainUNLOCOPKFilter
			{
				get
				{
					ZQuery result = new ZQuery();
					if (MainUNLOCOPK.IsValid)
					{
						string uNLOCOCode = ((RefUNLOCO)Factory.Load(typeof(RefUNLOCO), MainUNLOCOPK)).RL_Code;
						result.AddToFilter(OrgHeaderSchema.OH_RL_NKClosestPort, uNLOCOCode);
					}
					return result;
				}
			}

			protected ZQuery MainCountryPKFilter
			{
				get
				{
					ZQuery result = new ZQuery();
					if (MainCountryPK.IsValid)
					{
						string countryCode = ((RefCountry)Factory.Load(typeof(RefCountry), MainCountryPK)).RN_Code;
						result.AddToFilter(OrgHeaderSchema.OH_RL_NKClosestPort, SQLComparisonOperator.StartsWith, countryCode);
					}
					return result;
				}
			}

			protected ZQuery IsNationalFilter
			{
				get { return IsNational ? new ZQuery(OrgHeaderSchema.OH_IsNationalAccount, IsNational) : new ZQuery(); }
			}

			protected ZQuery IsTemporaryFilter
			{
				get { return IsTemporary ? new ZQuery(OrgHeaderSchema.OH_IsTempAccount, IsTemporary) : new ZQuery(); }
			}

			protected ZQuery AccountTypeCNECNR
			{
				get
				{
					ZQuery result = new ZQuery();

					if (AccountType == "CNE")
					{
						result.AddToFilter(OrgHeaderSchema.OH_IsConsignee, ZBool.True);
					}
					else if (AccountType == "CNR")
					{
						result.AddToFilter(OrgHeaderSchema.OH_IsConsignor, ZBool.True);
					}

					return result;
				}
			}

			#endregion

			#region OrgCompanyData Filters

			protected ZQuery ARGroupFilter
			{
				get { return ARGroupPK.IsValid ? new ZQuery(OrgCompanyDataSchema.OB_OJ_ARDebtorGroup, ARGroupPK) : new ZQuery(); }
			}

			protected ZQuery APGroupFilter
			{
				get { return APGroupPK.IsValid ? new ZQuery(OrgCompanyDataSchema.OB_OG_APCreditorGroup, APGroupPK) : new ZQuery(); }
			}

			protected ZQuery AccountTypeARAP
			{
				get
				{
					ZQuery result = new ZQuery();

					if (AccountType == "AR")
					{
						result.AddToFilter(OrgCompanyDataSchema.OB_IsDebtor, ZBool.True);
					}
					else if (AccountType == "AP")
					{
						result.AddToFilter(OrgCompanyDataSchema.OB_IsCreditor, ZBool.True);
					}

					return result;
				}
			}

			protected ZQuery MainBranchPKFilter
			{
				get { return MainBranchPK.IsValid ? new ZQuery(OrgCompanyDataSchema.OB_GB_ControllingBranch, MainBranchPK) : new ZQuery(); }
			}

			#endregion

			#endregion

			#region Sub-Queries

			#region CompanyData

			ZDBOnlySubQuery CompanyDataSubQuery
			{
				get
				{
					if (fCompanyDataSubQuery == null)
					{
						fCompanyDataSubQuery = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.OB_OH);
					}
					return fCompanyDataSubQuery;
				}
			}

			ZDBOnlySubQuery fCompanyDataSubQuery;

			#endregion

			protected ZDBOnlySubQuery GetOrgAddedEventLogSubQuery()
			{
				if (!OrgAddedFromDate.IsEmpty || !OrgAddedToDate.IsEmpty)
				{
					ZDBOnlySubQuery orgAddedEventLogSubQuery = new ZDBOnlySubQuery(typeof(StmALog), StmALogSchema.SL_Parent);
					orgAddedEventLogSubQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.AddedARecordToTheSystem.Code);

					if (!OrgAddedFromDate.IsEmpty)
					{
						orgAddedEventLogSubQuery.AddToFilter(StmALogSchema.SL_PostedTimeUtc, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, OrgAddedFromDate);
					}

					if (!OrgAddedToDate.IsEmpty)
					{
						orgAddedEventLogSubQuery.AddToFilter(StmALogSchema.SL_PostedTimeUtc, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, OrgAddedToDate);
					}

					return orgAddedEventLogSubQuery;
				}
				else
				{
					return null;
				}
			}

			#endregion

			#endregion

			public readonly ZString MatchLikelihood;

			protected readonly ZGuid MainOrgPK;
			protected readonly ZGuid MainBranchPK;
			protected readonly ZGuid MainUNLOCOPK;
			protected readonly ZGuid MainCountryPK;
			protected readonly ZBool IsNational;
			protected readonly ZBool IsTemporary;
			protected readonly ZString OrgNameStartsWith;
			protected readonly ZGuid ARGroupPK;
			protected readonly ZGuid APGroupPK;
			protected readonly ZString AccountType;
			protected readonly ZDateTime OrgAddedFromDate;
			protected readonly ZDateTime OrgAddedToDate;

			readonly BusinessObjectFactory Factory;
		}

		#endregion

		protected FilterOptions ReportFilterSettings;

		#endregion
	}
}
