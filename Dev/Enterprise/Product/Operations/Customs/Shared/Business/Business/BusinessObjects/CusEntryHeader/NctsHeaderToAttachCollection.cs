using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class NctsHeaderToAttachCollection : BusinessObjectCollection<CusInBondHeader>
	{
		public NctsHeaderToAttachCollection(CusEntryHeader entryHeader)
			: base(entryHeader.Factory)
		{
			this.entryHeader = Argument.NotNull(entryHeader, nameof(CusEntryHeader));

			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(ShowOnlyJobsWithMRNFilterName, "Property0", ZBool.True, true));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter Name, should not be translated")]
		const string ShowOnlyJobsWithMRNFilterName = "Show only jobs with an MRN?";

		readonly CusEntryHeader entryHeader;

		protected override ZQuery CreateRelationshipFilter()
		{
			var result = base.CreateRelationshipFilter();
			var query = new ZDBOnlyQuery(typeof(CusInBondHeader));
			query.AddToFilter(CusInBondHeaderSchema.BH_IsActive, true);
			query.AddToFilter(CusInBondHeaderSchema.BH_HeaderType, SQLComparisonOperator.Like, "D");
			query.AddToFilter(CusInBondHeaderSchema.BH_ApplicationCode, new[] { CusInBondApplicationCodeList.Codes.NCTS4, CusInBondApplicationCodeList.Codes.NCTS5 });

			var branchQuery = new ZDBOnlySubQuery(typeof(GlbBranch), GlbBranchSchema.PK, CusInBondHeaderSchema.BH_GB);
			branchQuery.AddToFilter(GlbBranchSchema.GB_GC, entryHeader.Declaration.JE_GC);
			query.AddSubQuery(branchQuery, JoinCondition.And);

			result.AddToFilter(query);
			return result;
		}

		protected override bool AllowNewCore => false;
	}
}
