using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business
{
	public class LockReleaseComplianceBookValidation : ZValidation
	{
		public LockReleaseComplianceBookValidation(LockReleaseComplianceBook parent)
			: base(parent)
		{
			this.parent = parent;
			this.ZValidationInternals = this;
			this.ParentListInternals = parent;
		}

		public void Add(AutoOverallStaffCommissionRuleValidation validation)
		{
			ZValidationInternals.Add(validation);
		}

		public void Remove(AutoOverallStaffCommissionRuleValidation validation)
		{
			ZValidationInternals.Remove(validation);
		}

		public override void ValidateAll()
		{
			using (IDisposable suspender = ParentListInternals.SuspendListChanged())
			{
				ValidateAllCore();
			}
		}
		protected virtual void ValidateAllCore()
		{
			ValidateXD_Calc_PK();
		}

		void ValidateXD_Calc_PK()
		{
			ZValidationInternals.Validate(Parent.XD_Calc_PKInfo, new RunValidationInvoker(this.XD_Calc_PKValidationInvoker));
		}

		void XD_Calc_PKValidationInvoker()
		{
			MandatoryValidation.CheckEntered(Parent.XD_Calc_PKInfo, Res.GetString("91e283d2-2a19-443c-b970-2b5271d2da4d", "Compliance Invoice Book"));

			if (!Parent.XD_Calc_PKInfo.HasErrors())
			{
				CheckComplianceSequenceBoExists();
			}
			if (!Parent.XD_Calc_PKInfo.HasErrors())
			{
				CheckComplianceSequenceBoAllocationLevel();
			}
			if (!Parent.XD_Calc_PKInfo.HasErrors() && Parent.IsLock)
			{
				CheckBookHasBeenLocked();
			}
			if (!Parent.XD_Calc_PKInfo.HasErrors() && Parent.IsLock)
			{
				CheckLockSomeComplianceSubType();
			}
			if (!Parent.XD_Calc_PKInfo.HasErrors() && !Parent.IsLock)
			{
				CheckBookIsNeedRelease();
			}
			if (!Parent.XD_Calc_PKInfo.HasErrors() && !Parent.IsLock)
			{
				CheckReleaseOtherStaffLockedCompliance();
			}
		}

		void CheckComplianceSequenceBoExists()
		{
			if (Parent.ComplianceSequenceBo == null)
			{
				Parent.XD_Calc_PKInfo.AddError(Res.GetString("fdb13aa4-39e6-4d4b-9213-6f703cb8bd88", "This Compliance Invoice Book does not exist in Compliance Sequences."));
			}
		}
		void CheckComplianceSequenceBoAllocationLevel()
		{
			if (Parent.ComplianceSequenceBo.XD_AllocationLevel != Core.Constants.ComplianceBookAllocationLevel.Counter)
			{
				Parent.XD_Calc_PKInfo.AddError(Res.GetString("4cadc10d-a07b-4c27-9ff0-5ad64fef8770", "The selected book cannot be selected. Only ‘CTR’ Allocation Level Compliance Invoice Book can be selected."));
			}
		}

		void CheckBookHasBeenLocked()
		{
			var query = new ZDBOnlyQuery(typeof(AccComplianceSequence));

			var innerSubQuery = Parent.GenAddOnColumnQueryHelper.GetContainsValueForAnyQuery(false, AccComplianceSequence.Schema.XD_LockBy);

			query.AddSubQuery(innerSubQuery, JoinCondition.And);

			var companyFilter = new ZQuery(AccComplianceSequenceSchema.XD_GC_Company, GlbCompany.CurrentCompany.PK);
			query.AddToFilter(companyFilter, JoinCondition.And);

			var bookFilter = new ZQuery(AccComplianceSequenceSchema.PK, SQLComparisonOperator.Equal, Parent.ComplianceSequenceBo.PK);
			query.AddToFilter(bookFilter, JoinCondition.And);

			var complianceSequences = new AccComplianceSequenceCollection(new BusinessObjectFactory(), query);

			if (complianceSequences.Count > 0)
			{
				Parent.XD_Calc_PKInfo.AddError(Res.GetString("483f56bf-67b7-4e3c-88a9-9a43f3420909",
					@"The selected book '{0}' has been locked by '{1}'. Please select another book.", complianceSequences[0].XD_Code, complianceSequences[0].LockBy));
			}
		}

		void CheckBookIsNeedRelease()
		{
			var query = new ZDBOnlyQuery(typeof(AccComplianceSequence));

			query.AddSubQuery(Parent.GenAddOnColumnQueryHelper.GetContainsValueForAnyQuery(true, AccComplianceSequence.Schema.XD_LockBy), JoinCondition.And);

			var companyFilter = new ZQuery(AccComplianceSequenceSchema.XD_GC_Company, GlbCompany.CurrentCompany.PK);
			query.AddToFilter(companyFilter, JoinCondition.And);

			var bookFilter = new ZQuery(AccComplianceSequenceSchema.PK, SQLComparisonOperator.Equal, Parent.ComplianceSequenceBo.PK);
			query.AddToFilter(bookFilter, JoinCondition.And);

			var complianceSequences = new AccComplianceSequenceCollection(new BusinessObjectFactory(), query);

			if (complianceSequences.Count > 0)
			{
				Parent.XD_Calc_PKInfo.AddError(Res.GetString("8df5863e-a0d7-4e57-8385-10dbe9af865c",
					@"The selected book '{0}' is not locked and does not need to be Released. Please select another book.", complianceSequences[0].XD_Code));
			}
		}

		void CheckLockSomeComplianceSubType()
		{
			var query = new ZDBOnlyQuery(typeof(AccComplianceSequence));

			var innerSubQuery = Parent.GenAddOnColumnQueryHelper.GetContainsValueForAnyQuery(false, AccComplianceSequence.Schema.XD_LockBy);
			innerSubQuery.AddToFilter(GenAddOnColumnSchema.XA_Data, SQLComparisonOperator.Equal, GlbStaff.CurrentUser.PK.ToString());

			query.AddSubQuery(innerSubQuery, JoinCondition.And);

			var companyFilter = new ZQuery(AccComplianceSequenceSchema.XD_GC_Company, GlbCompany.CurrentCompany.PK);
			query.AddToFilter(companyFilter, JoinCondition.And);

			var subTypeFilter = new ZQuery(AccComplianceSequenceSchema.XD_SequenceClass, Parent.ComplianceSequenceBo.XD_SequenceClass);
			query.AddToFilter(subTypeFilter, JoinCondition.And);

			var allocationLevelFilter = new ZQuery(AccComplianceSequenceSchema.XD_AllocationLevel, ComplianceBookAllocationLevel.Counter);
			query.AddToFilter(allocationLevelFilter, JoinCondition.And);

			var notThisFilter = new ZQuery(AccComplianceSequenceSchema.PK, SQLComparisonOperator.NotEqual, Parent.ComplianceSequenceBo.PK);
			query.AddToFilter(notThisFilter, JoinCondition.And);

			var complianceSequences = new AccComplianceSequenceCollection(Parent.Factory, query);

			if (complianceSequences.Count > 0)
			{
				Parent.XD_Calc_PKInfo.AddError(Res.GetString("ddb59857-f31f-45f8-b55c-5bd93e40cad1",
					@"'CTR' Allocation Level Compliance Invoice Book for each lock book must be for a different Compliance Sub Type.
Compliance Invoice Book which was locked : 
Sub Type : {0} 
Code : {1}", complianceSequences[0].XD_SequenceClass, complianceSequences[0].XD_Code));
			}
		}

		void CheckReleaseOtherStaffLockedCompliance()
		{
			var query = new ZDBOnlyQuery(typeof(AccComplianceSequence));

			var bookFilter = new ZQuery(AccComplianceSequenceSchema.PK, SQLComparisonOperator.Equal, Parent.ComplianceSequenceBo.PK);
			query.AddToFilter(bookFilter, JoinCondition.And);

			var companyFilter = new ZQuery(AccComplianceSequenceSchema.XD_GC_Company, GlbCompany.CurrentCompany.PK);
			query.AddToFilter(companyFilter, JoinCondition.And);

			var complianceSequences = new AccComplianceSequenceCollection(new BusinessObjectFactory(), query);

			if (complianceSequences.Count > 0 && !Env.Security.ComplianceSequencesModifyReleaseOtherStaff.IsAllowed && complianceSequences[0].XD_LockBy != GlbStaff.CurrentUser.PK)
			{
				Parent.XD_Calc_PKInfo.AddError(Res.GetString("32a46119-b0a7-4f25-b524-2b4aef592e32", @"The selected book '{0}' has been locked by '{1}'. 

If you require access to release ‘CTR’ Allocation Level Compliance Invoice Book locked by other staff, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

{2}", complianceSequences[0].XD_Code, complianceSequences[0].LockBy, Env.Security.ComplianceSequencesModifyReleaseOtherStaff.DisplayTextPathToSecurityRight));
			}
		}

		public override Type AutoValidationType
		{
			get { return typeof(LockReleaseComplianceBook); }
		}

		public LockReleaseComplianceBook Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get
			{
				return parent;
			}
		}

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		readonly LockReleaseComplianceBook parent;

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		readonly IValidationInternals ZValidationInternals;

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		readonly ISingleElementListInternal ParentListInternals;
	}
}
