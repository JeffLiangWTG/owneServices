using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Tools.DuplicateDetector;
using CargoWise.Types;
using Enterprise.MasterData.Common;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class DeduplicationOrganisation : AutoMDMAdminPanelOrganisationView
	{
		public DeduplicationOrganisation(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new abstract class Schema : AutoMDMAdminPanelOrganisationView.Schema
		{
			public const string DOH_CreatedUnderBranch = "DOH_CreatedUnderBranch";
			public const string DOH_CreatedUnderCompany = "DOH_CreatedUnderCompany";
			public const string DOH_ConsolidationsCount = "DOH_ConsolidationsCount";
			public const string DOH_DeclarationsCount = "DOH_DeclarationsCount";
			public const string DOH_ShipmentsCount = "DOH_ShipmentsCount";
			public const string DOH_EnterpriseId = "DOH_EnterpriseId";
			public const string DOH_EnterpriseCode = "DOH_EnterpriseCode";
			public const string DOH_CompanyCode = "DOH_CompanyCode";
			public const string DOH_ProductId = "DOH_ProductId";
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new DeduplicationOrganisationFetchStrategy(this);
		}

		public ZString DOH_CreatedUnderBranch => MasterOrgHeader?.OH_CreatedUnderBranch ?? ZString.Empty;

		public ZString DOH_CreatedUnderCompany => MasterOrgHeader?.OH_CreatedUnderCompany ?? ZString.Empty;

		public int DOH_ConsolidationsCount => MasterOrgHeader?.AssociatedConsols.Count ?? 0;

		public int DOH_DeclarationsCount => MasterOrgHeader?.AssociatedDeclarations.Count ?? 0;

		public int DOH_ShipmentsCount => MasterOrgHeader?.AssociatedShipments.Count ?? 0;

		public OrgHeader MasterOrgHeader => Factory.Load<OrgHeader>(PK);

		PatternMatchingResultCollection matchingResultCollection;
		public PatternMatchingResultCollection MatchingResultCollection
		{
			get
			{
				if (matchingResultCollection == null)
				{
					matchingResultCollection = new PatternMatchingResultCollection(Factory, new ZDBOnlyQuery(typeof(PatternMatchingResult)).AddToFilter(PatternMatchingResultSchema.PMT_MasterPK, PK).AddToFilter(PatternMatchingResultSchema.PMT_Status, PatternMatchingResult.StatusCodes.PotentialDuplicate));
				}
				return matchingResultCollection;
			}
		}

		public event EventHandler<DuplicateSearchingFinishedEventArgs> DuplicateSearchingFinished;

		protected override bool MatchesFilterCore(ZQuery filter, DataRow row, DataTable table, string tableName, string identifier)
		{
			if (filter.LiteralTextADO.Contains("OH_"))
			{
				var query = new ZDBOnlyQuery(typeof(DeduplicationOrganisation));
				var subQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgHeaderSchema.PK);
				subQuery.AddToFilter(filter);
				query.AddSubQuery(subQuery, JoinCondition.And);
				return base.MatchesFilterCore(query, row, table, tableName, identifier);
			}
			return base.MatchesFilterCore(filter, row, table, tableName, identifier);
		}

		public void FindPotentialDuplicates()
		{
			var scoringResults = FindPotentialDuplicatesCore();
			DuplicateSearchingFinished?.Invoke(this, new DuplicateSearchingFinishedEventArgs(scoringResults));
		}

		readonly object lockerObj = new object();

		public void SaveDeduplicationResults(IEnumerable<ScoringResult> scoringResults)
		{
			if (MasterOrgHeader != null)
			{
				using (var inputWithLock = new List<Guid> { MasterOrgHeader.PK.ToGuid() }.ApplyAppLocks(PatternMatchingConstants.AppLockKey))
				{
					if (inputWithLock.ItemsWithLocks.Any())
					{
						DeduplicationHelper.SaveDeduplicationResults(MasterOrgHeader, this, scoringResults, MatchingResultCollection, DOH_Status, lockerObj);
					}
				}
			}
		}

		protected virtual IEnumerable<ScoringResult> FindPotentialDuplicatesCore()
		{
			IEnumerable<ScoringResult> result = null;

			using (Db.DisposableActionForDbConnection())
			{
				var orgHeader = new ReadOnlyBusinessObjectFactory().Load<OrgHeader>(PK);
				((IDeduplicatable)orgHeader).ShouldRunDeduplication = true;
				result = ObjectFactory.Get<IMasterDataProvider>().FindPotentialOrgDuplicatesForAdminPanel(orgHeader);
			}

			return result;
		}
		public void PropagateForcedReloadRequired()
		{
			matchingResultCollection = null;
			MasterOrgHeader.PropagateDeduplicationActionOccurred(DeduplicationAction.ReloadRequired, null, null, null);
		}

		protected virtual ISupportDuplicationFinder GetDuplicationFinder(OrgHeader org) => ObjectFactory.Get<IMasterDataProvider>().GetOrganisationSupportedDuplicationFinders(org, typeof(OrgHeader), null).FirstOrDefault();

		protected override bool IsValidationEnabledCore(ZPropertyInfo propertyInfo) => false;
	}
}
