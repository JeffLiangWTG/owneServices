using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class ReconOriginalEntryHeaderLookups
	{
		public ReconOriginalEntryHeaderLookups(ReconOriginalEntryHeader reconOriginalEntry)
		{
			this.reconOriginalEntry = reconOriginalEntry;
			this.factory = reconOriginalEntry.Factory;
		}

		readonly ReconOriginalEntryHeader reconOriginalEntry;
		readonly BusinessObjectFactory factory;

		public ZZRefCusCodeListCombinedCollection SchDPortList
		{
			get { return ZZRefCusCodeListCombinedCollection.GetCachedCollection(factory, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today); }
		}

		public ModuleEntryHeaderCollection Entries
		{
			get
			{
				ModuleEntryHeaderCollection result = new ModuleEntryHeaderCollection(reconOriginalEntry.Factory);

				FilterBusinessObjectDefault filterBODefault = new FilterBusinessObjectDefault(ModuleEntryHeaderCollection.USFilterConstants.ReconIssue, "Property", reconOriginalEntry.ReconDeclaration.US_IssueCode);
				result.FilterBusinessObjectDefaults.Add(filterBODefault);
				ZQuery query = new ZQuery(QueryHelper.GetQueryWithSubQueryOnGenAddOnColumn(USAddInfoSchema.Constants.US_OtherReconIndicator, reconOriginalEntry.ReconDeclaration.US_IssueCode));

				filterBODefault = new FilterBusinessObjectDefault(ModuleEntryHeaderCollection.USFilterConstants.SuretyCode, "Property", reconOriginalEntry.ReconDeclaration.US_SuretyCode);
				result.FilterBusinessObjectDefaults.Add(filterBODefault);
				query.AddToFilter(QueryHelper.GetQueryWithSubQueryOnGenAddOnColumn(USAddInfoSchema.Constants.US_SuretyCode, reconOriginalEntry.ReconDeclaration.US_SuretyCode));

				if (reconOriginalEntry.ReconDeclaration.IOROrgPK.IsValid)
				{
					filterBODefault = new FilterBusinessObjectDefault(ModuleEntryHeaderCollection.USFilterConstants.ImporterOfRecord, "Property", reconOriginalEntry.ReconDeclaration.IOROrgPK);
					result.FilterBusinessObjectDefaults.Add(filterBODefault);
					query.AddToFilter(GetIOROrgPKQuery());
				}

				filterBODefault = new FilterBusinessObjectDefault("Exclude IOR Filing Their Own Recon", "Property0", ZBool.True);
				result.FilterBusinessObjectDefaults.Add(filterBODefault);
				query.AddToFilter(GetExcludeIORFilingTheirOwnRecQuery());

				filterBODefault = new FilterBusinessObjectDefault("Reconciliation", "Property0", ZBool.True);
				result.FilterBusinessObjectDefaults.Add(filterBODefault);
				query.AddToFilter(GetNotReconciledQuery());

				filterBODefault = new FilterBusinessObjectDefault(ModuleEntryHeaderCollection.USFilterConstants.ImportSource, "Property", reconOriginalEntry.ReconDeclaration.US_ImportEntrySource);
				result.FilterBusinessObjectDefaults.Add(filterBODefault);
				query.AddToFilter(GetImportSourceQuery());

				result.AdditionalFilter = query;

				if (reconOriginalEntry.CH_OrigEntryReference.Length > 3)
				{
					filterBODefault = new FilterBusinessObjectDefault(ModuleEntryHeaderCollection.USFilterConstants.EntryNumber, "Property", reconOriginalEntry.CH_OrigEntryReference.SubstringSafe(3));
					result.FilterBusinessObjectDefaults.Add(filterBODefault);
				}
				result.AddNotificationWhenAdditionalFilterNotMetOverride = AddNotificationWhenAdditionalFilterNotMetOverride;
				result.GetNotificationTypeWhenAdditionalFilterNotMetOverride = GetNotificationTypeWhenAdditionalFilterNotMetOverride;
				return result;
			}
		}

		void AddNotificationWhenAdditionalFilterNotMetOverride(StringCollectionX errors, BusinessObject selectedBusinessObject)
		{
			errors.Add("This Entry cannot be chosen as it does not match the required filters.\r\neg.the Recon Issue, the Surety Code, the Importer of Record, etc.\r\nPlease choose another Entry.");
		}

		CargoWise.ComponentModel.INotificationType GetNotificationTypeWhenAdditionalFilterNotMetOverride()
		{
			return NotificationType.Warning;
		}

		ZQuery GetImportSourceQuery()
		{
			switch (reconOriginalEntry.ReconDeclaration.US_ImportEntrySource)
			{
				case ReconciliationImportEntrySourceList.Codes.PuertoRico:
					return QueryHelper.GetQueryWithSubQueryOnGenAddOnColumn(USAddInfoSchema.Constants.US_SchDEntry, SQLComparisonOperator.StartsWith, "49");
				case ReconciliationImportEntrySourceList.Codes.VirginIslands:
					return QueryHelper.GetQueryWithSubQueryOnGenAddOnColumn(USAddInfoSchema.Constants.US_SchDEntry, SQLComparisonOperator.StartsWith, "51");
				case ReconciliationImportEntrySourceList.Codes.FiftyStates:
					ZQuery query = new ZQuery(QueryHelper.GetQueryWithSubQueryOnGenAddOnColumn(USAddInfoSchema.Constants.US_SchDEntry, SQLComparisonOperator.DoesNotStartWith, "49"));
					query.AddToFilter(QueryHelper.GetQueryWithSubQueryOnGenAddOnColumn(USAddInfoSchema.Constants.US_SchDEntry, SQLComparisonOperator.DoesNotStartWith, "51"));
					return query;
				default:
					return new ZQuery();
			}
		}

		ZQuery GetNotReconciledQuery()
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(CusEntryHeader));
			result.AddToFilter(CusEntryHeaderSchema.CH_MessageType, CusEntryHeaderMessageTypeList.Codes.EntrySummary);

			var helper = new GenAddOnColumnQueryHelper(typeof(JobDeclaration));
			var subQuery = helper.GetContainsValueForAnyQuery(false, USAddInfoSchema.US_OtherReconIndicator.Name, USAddInfoSchema.US_NAFTAReconIndicator.Name);

			ZDBOnlySubQuery declarationQuery = new ZDBOnlySubQuery(typeof(JobDeclaration), CusEntryHeaderSchema.CH_JE);
			declarationQuery.AddSubQuery(subQuery, JoinCondition.And);
			result.AddSubQuery(declarationQuery, JoinCondition.And);

			//Recon Entry exists?
			ZDBOnlySubQuery reconEntryQuery = new ZDBOnlySubQuery(typeof(CusEntryHeader), CusEntryHeaderSchema.CH_CH_PrimeEntry, true);
			reconEntryQuery.AddToFilter(CusEntryHeaderSchema.CH_MessageType, CusEntryHeaderMessageTypeList.Codes.ReconOriginalEntry);
			reconEntryQuery.AddToFilter(CusEntryHeaderSchema.CH_CH_PrimeEntry, SQLComparisonOperator.NotEqual, DBNull.Value);
			result.AddSubQuery(reconEntryQuery, JoinCondition.And);

			return result;
		}

		ZDBOnlyQuery GetExcludeIORFilingTheirOwnRecQuery()
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(CusEntryHeader));

			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(JobDeclaration), CusEntryHeaderSchema.CH_JE);
			subQuery.AddToFilter(JobDeclarationSchema.JE_AddInfo, SQLComparisonOperator.NotContains, USAddInfoSchema.Constants.US_FileTheirOwnRecon.Substring(3) + "=Y");
			result.AddSubQuery(subQuery, JoinCondition.And);

			return result;
		}

		ZDBOnlyQuery GetIOROrgPKQuery()
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(CusEntryHeader));

			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(JobDeclaration), CusEntryHeaderSchema.CH_JE);
			subQuery.AddToFilter(JobDeclarationSchema.JE_OA_DeclarantAddress, SQLComparisonOperator.Equal, reconOriginalEntry.ReconDeclaration.ImporterOfRecord.MainAddress.PK);
			result.AddSubQuery(subQuery, JoinCondition.And);

			return result;
		}

		GenAddOnColumnQueryHelper QueryHelper
		{
			get { return queryHelper ?? (queryHelper = new GenAddOnColumnQueryHelper(typeof(CusEntryHeader), typeof(JobDeclaration), CusEntryHeaderSchema.CH_JE)); }
		}
		GenAddOnColumnQueryHelper queryHelper;

		public CodeDescriptionPairList EntryTypeList
		{
			get
			{
				return factory.GetCachedValue<CodeDescriptionPairList>("EntryTypeListForRecon", delegate
				{
					EntryTypeList result = new EntryTypeList();
					result.RemoveDrawbackSummaryEntryTypes();
					result.RemoveExWarehouseEntryTypes();
					result.RemoveInBondEntryTypes();
					result.RemoveLiquidationEntryTypes();
					result.Sort();
					return result;
				}
				);
			}
		}

		public CodeDescriptionPairList YesNoList
		{
			get
			{
				return factory.GetCachedValue("YesNoList", delegate
				{
					CodeDescriptionPairList result = new YesNoDefaultList();
					result.RemoveCode(YesNoDefaultList.Codes.Default);
					return result;
				});
			}
		}

		public PendingActionIDTypeList PendingActionTypeList => factory.GetCachedValue<PendingActionIDTypeList>();

		public TransportModeCodes TransportModeList
		{
			get { return factory.GetCachedValue<TransportModeCodes>(); }
		}

		public CodeDescriptionPairList MessagingModeList
		{
			get { return factory.GetCachedValue<JobApplicationCodeList>(); }
		}
	}
}
