using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	/// <summary>
	/// Only for a module grid for EntryHeaderModule. It does not get refreshed even if there is a change made on CusEntryHeader in the passed factory
	/// In the context of module, business objects should have been saved and F3 uses a different factory
	/// </summary>
	public class ModuleEntryHeaderCollection : ActiveBusinessObjectCollection<CusEntryHeader>
	{
		public ModuleEntryHeaderCollection(BusinessObjectFactory factory, GlbCompany company)
			: this(factory, company, new ZQuery())
		{
		}

		public ModuleEntryHeaderCollection(BusinessObjectFactory factory, GlbCompany company, ZQuery filter)
			: base(factory, filter?.AddToFilter(GetBGMReferenceOrEntryNumberFilter()))
		{
			this.company = company;
		}

		public ModuleEntryHeaderCollection(BusinessObjectFactory factory, ICollectionRelationship relationship)
			: base(factory, relationship)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter types")]
		public static class FilterConstants
		{
			public const string EntryNumber = "Entry Number";
			public const string JobNumber = "Job Number";
			public const string ReferenceNumber = "Reference Number";
			public const string SubmissionDate = "Submission Date";
			public const string ReleaseDate = "Release Date";
			public const string EntryStatus = "Entry Status";
			public const string MessageStatus = "Message Status";
			public const string WarehouseTransactionStatus = "WHS Trans. Status";
			public const string MessageType = "Message Type";
		}

		static ZQuery GetBGMReferenceOrEntryNumberFilter()
		{
			var filter = new ZQuery(CusEntryHeaderSchema.CH_BGMReference, SQLComparisonOperator.NotEqual, ZString.Empty);

			var entryHeaderQuery = new ZDBOnlyQuery(typeof(CusEntryHeader));
			var entryNumQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
			entryNumQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, SQLComparisonOperator.Equal, CusEntryNumberTypes.Standard.MovementReferenceNumber);
			entryNumQuery.AddToFilter(CusEntryNumSchema.CE_Category, SQLComparisonOperator.Equal, "CUS");
			entryNumQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, SQLComparisonOperator.NotEqual, ZString.Empty);
			entryNumQuery.AddToFilter(CusEntryNumSchema.CE_ParentTable, SQLComparisonOperator.Equal, CusEntryHeaderSchema.Constants.TableName);
			entryHeaderQuery.AddSubQuery(entryNumQuery, JoinCondition.And);

			filter.AddToFilter(entryHeaderQuery, JoinCondition.Or);

			return filter;
		}

		readonly GlbCompany company;

		protected override object[] GetCollectionState()
		{
			return new object[] { company };
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			var result = base.CreateRelationshipFilter();

			var entryHeaderQuery = new ZDBOnlyQuery(typeof(CusEntryHeader));

			var declarationQuery = new ZDBOnlySubQuery(typeof(BaseJobDeclaration), CusEntryHeaderSchema.CH_JE);
			ZDBOnlySubQuery branchQuery = new ZDBOnlySubQuery(typeof(GlbBranch), JobDeclarationSchema.JE_GB);
			branchQuery.AddToFilter(GlbBranchSchema.GB_GC, company.PK);
			declarationQuery.AddSubQuery(branchQuery, JoinCondition.And);
			entryHeaderQuery.AddSubQuery(declarationQuery, JoinCondition.And);

			result.AddToFilter(entryHeaderQuery);

			return result;
		}

		protected override bool AllowNew
		{
			get { return false; }
		}

		public override void Delete(CusEntryHeader businessObject)
		{
			throw new NotSupportedException();
		}
	}
}
