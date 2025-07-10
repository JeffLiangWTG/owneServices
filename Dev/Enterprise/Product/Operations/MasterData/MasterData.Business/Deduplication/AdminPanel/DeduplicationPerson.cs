using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Tools.DuplicateDetector;
using CargoWise.Types;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterData.Business
{
	public class DeduplicationPerson : AutoMDMAdminPanelPersonView, IDeduplicationPerson
	{
		public DeduplicationPerson(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public GlbPerson MasterGlbPerson => Factory.Load<GlbPerson>(PK);

		public ZString Workplace => MasterGlbPerson.PrimaryWorkplace;
		public ZString WorkplaceCode => MasterGlbPerson.PrimaryWorkplaceCode;
		public ZString Location => MasterGlbPerson.PrimaryUNLOCO;
		public ZString JobTitle => MasterGlbPerson.PrimaryJobTitle;

		public new abstract class Schema : AutoMDMAdminPanelPersonView.Schema
		{
			public const string Workplace = "Workplace";
			public const string WorkplaceCode = "WorkplaceCode";
			public const string Location = "Location";
			public const string JobTitle = "JobTitle";
		}

		PatternMatchingResultCollection matchingResultCollection;
		public PatternMatchingResultCollection MatchingResultCollection
		{
			get
			{
				if (matchingResultCollection == null)
				{
					matchingResultCollection = new PatternMatchingResultCollection(Factory, new ZQuery(PatternMatchingResultSchema.PMT_MasterPK, PK).AddToFilter(PatternMatchingResultSchema.PMT_Status, PatternMatchingResult.StatusCodes.PotentialDuplicate));
				}

				return matchingResultCollection;
			}
		}

		public event EventHandler<DuplicateSearchingFinishedEventArgs> DuplicateSearchingFinished;

		protected override bool MatchesFilterCore(ZQuery filter, DataRow row, DataTable table, string tableName, string identifier)
		{
			if (filter.LiteralTextADO.Contains("PER_"))
			{
				var query = new ZDBOnlyQuery(typeof(DeduplicationPerson));
				var subQuery = new ZDBOnlySubQuery(typeof(GlbPerson), GlbPersonSchema.PK);
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

		protected virtual IEnumerable<ScoringResult> FindPotentialDuplicatesCore()
		{
			IEnumerable<ScoringResult> result = null;

			using (Db.DisposableActionForDbConnection())
			{
				var glbPerson = new ReadOnlyBusinessObjectFactory().Load<GlbPerson>(PK);
				((IDeduplicatable)glbPerson).ShouldRunDeduplication = true;
				result = ObjectFactory.Get<IMasterDataProvider>().FindPotentialPersonDuplicatesForAdminPanel(glbPerson);
			}

			return result;
		}

		readonly object lockerObj = new object();

		public void SaveDeduplicationResults(IEnumerable<ScoringResult> scoringResults)
		{
			DeduplicationHelper.SaveDeduplicationResults(MasterGlbPerson, this, scoringResults, MatchingResultCollection, DPE_Status, lockerObj);
		}

		protected virtual ISupportDuplicationFinder GetDuplicationFinder(GlbPerson person) => ObjectFactory.Get<IMasterDataProvider>().GetPersonSupportedDuplicationFinders(person, null).FirstOrDefault();

		protected override bool IsValidationEnabledCore(ZPropertyInfo propertyInfo) => false;

		[DocumentEngineIntegration.DocumentParsing.DocumentMacroIgnore]
		public override ZBlob DPE_PasswordHash { get => base.DPE_PasswordHash; set => base.DPE_PasswordHash = value; }

		[DocumentEngineIntegration.DocumentParsing.DocumentMacroIgnore]
		public override ZBlob DPE_PasswordSalt { get => base.DPE_PasswordSalt; set => base.DPE_PasswordSalt = value; }
	}
}
