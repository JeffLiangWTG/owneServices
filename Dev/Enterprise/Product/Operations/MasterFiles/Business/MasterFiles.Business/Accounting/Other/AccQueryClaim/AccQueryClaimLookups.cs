//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccQueryClaimLookups
//
//    This class should be used for overriding collections in AutoAccQueryClaimLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------
using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class AccQueryClaimLookups : AutoAccQueryClaimLookups
	{
		public AccQueryClaimLookups(AutoAccQueryClaim parent)
			: base(parent)
		{
			if (!(parent is AccQueryClaim))
			{
				throw new NotSupportedException("Please pass in an AccQuery claim. Currently constructor allows an auto until bug in generator is fixed.");
			}
		}

		AccQueryClaim Claim
		{
			get { return (AccQueryClaim)Parent; }
		}

		#region Collections

		#region Branches

		public override GlbBranchCollection Branches
		{
			get
			{
				GlbBranchNotCurrentCompanyRelatedCollection branchCollection = new GlbBranchNotCurrentCompanyRelatedCollection(Factory, new ZQuery(GlbBranchSchema.GB_IsActive, ZBool.True));
				ZQuery companyFilter = new ZQuery(GlbBranchSchema.GB_GC, Env.CurrentCompany.PK);
				if (Claim.IsIntercompanyClaim)
				{
					companyFilter.AddToFilter(JoinCondition.Or, GlbBranchSchema.GB_GC, Claim.TransactionHeader.Company.PK);
				}
				branchCollection.AdditionalRelationshipFilter = companyFilter;
				return branchCollection;
			}
		}

		#endregion

		#region Contacts

		public new OrgContactDependentCollection Contacts
		{
			get
			{
				OrgContactDependentCollection fContacts;
				fContacts = new OrgContactDependentCollection(Factory.Load<OrgHeader>(Claim.AY_OH_Debtor_Original), Factory);
				fContacts.Load();
				return fContacts;
			}
		}

		#endregion

		#region Debtors

		public override OrgHeaderCollection Debtors
		{
			get { return new DebtorCollection(Factory); }
		}

		#endregion

		#region Creditors

		public OrgHeaderCollection Creditors
		{
			get { return new CreditorCollection(Factory); }
		}

		#endregion

		#region Staff

		public GlbStaffCollection Staff
		{
			get { return new GlbStaffCollection(Factory, new ZQuery(GlbStaffSchema.GS_IsActive, ZBool.True)); }
		}

		#endregion

		#region TransactionHeaders

		protected ZQuery TransactionUniqueFilter
		{
			get
			{
				ZDBOnlySubQuery uniqueSubFilter = new ZDBOnlySubQuery(typeof(AccQueryClaim), AccQueryClaimSchema.AY_AH, true);
				uniqueSubFilter.AddToFilter(AccQueryClaimSchema.AY_AH, SQLComparisonOperator.NotEqual, null);
				uniqueSubFilter.AddToFilter(GetClaimQueryForTransactionUniqueFilter());

				ZDBOnlyQuery uniqueFilter = new ZDBOnlyQuery(typeof(AccTransactionHeader));
				uniqueFilter.AddSubQuery(uniqueSubFilter, JoinCondition.And);

				return uniqueFilter;
			}
		}

		internal ZQuery GetClaimQueryForTransactionUniqueFilter()
		{
			ZQuery claimQuery = new ZQuery();
			claimQuery.AddToFilter(AccQueryClaimSchema.PK, SQLComparisonOperator.NotEqual, Claim.PK);
			claimQuery.AddToFilter(AccQueryClaimSchema.AY_QueryClaimStatus, SQLComparisonOperator.NotEqual, QueryClaimStatusCodeList.Codes.QCStatus3AcceptedAndCreditNoteIssuedAndClosed);
			claimQuery.AddToFilter(AccQueryClaimSchema.AY_QueryClaimStatus, SQLComparisonOperator.NotEqual, QueryClaimStatusCodeList.Codes.QCStatus5RejectedClosed);
			claimQuery.AddToFilter(AccQueryClaimSchema.AY_QueryClaimStatus, SQLComparisonOperator.NotEqual, QueryClaimStatusCodeList.Codes.QCStatus6CancelledAndClosed);

			return claimQuery;
		}

		#endregion

		#endregion

		#region Code Description Pair Lists

		public CodeDescriptionPairList ClaimType
		{
			get { return ObjectFactory.Get<IAccounting>().QueryClaimTypeCodeDescriptionPairList as CodeDescriptionPairList; }
		}

		public CodeDescriptionPairList ClaimReason
		{
			get { return ObjectFactory.Get<IAccounting>().ClaimReasonCodeDescriptionPairList as CodeDescriptionPairList; }
		}

		public virtual CodeDescriptionPairList ClaimStatus
		{
			get { return ObjectFactory.Get<IAccounting>().ClaimStatusCodeDescriptionPairList as CodeDescriptionPairList; }
		}

		#region Hold Options

		public CodeDescriptionPairList HoldOptions
		{
			get
			{
				return AccountingMasterFilesConstants.CreditorGroupConstants.HoldOptions;
			}
		}

		#endregion

		#endregion
	}
}
