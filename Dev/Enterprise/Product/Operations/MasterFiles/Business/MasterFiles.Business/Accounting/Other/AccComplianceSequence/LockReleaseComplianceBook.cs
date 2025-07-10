using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public abstract class LockReleaseComplianceBook : NonPersistentBusinessObject
	{
		protected LockReleaseComplianceBook(BusinessObjectFactory factory) : base(factory)
		{
		}

		internal AccComplianceSequence ComplianceSequenceBo
		{
			get { return complianceSequenceBo; }
			set
			{
				if (value != complianceSequenceBo)
				{
					complianceSequenceBo = value;
				}
			}
		}
		AccComplianceSequence complianceSequenceBo;

		internal GenAddOnColumnQueryHelper GenAddOnColumnQueryHelper
		{
			get { return helper ?? (helper = new GenAddOnColumnQueryHelper(typeof(AccComplianceSequence))); }
		}
		GenAddOnColumnQueryHelper helper;

		[List("AccComplianceSequences")]
		public ZGuid XD_Calc_PK
		{
			get
			{
				return XD_Calc_PKField;
			}
			set
			{
				if (value != XD_Calc_PKField)
				{
					XD_Calc_PKField = value;
					ComplianceSequenceBo = Factory.Load<AccComplianceSequence>(value);
					XD_Calc_PKInfo.RefreshBinding();
				}
			}
		}
		ZGuid XD_Calc_PKField;

		public ZPropertyInfo XD_Calc_PKInfo
		{
			get { return GetZPropertyInfo(nameof(XD_Calc_PK)); }
		}

		public virtual bool IsLock
		{
			get;
		}

		#region Lookup

		public virtual AccComplianceSequenceCollection AccComplianceSequences
		{
			get
			{
				if (fAccComplianceSequences == null)
				{
					var query = GetComplianceSequenceQuery();
					fAccComplianceSequences = new AccComplianceSequenceCollection(Factory, query);
				}
				return fAccComplianceSequences;
			}
		}

		AccComplianceSequenceCollection fAccComplianceSequences;

		protected virtual ZQuery GetComplianceSequenceQuery()
		{
			var query = new ZDBOnlyQuery(typeof(AccComplianceSequence));

			var companyFilter = new ZQuery(AccComplianceSequenceSchema.XD_GC_Company, GlbCompany.CurrentCompany.PK);
			query.AddToFilter(companyFilter, JoinCondition.And);

			return query;
		}

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			Validation.ValidateAll();
			base.RunPreSaveValidationCore();
		}
		public LockReleaseComplianceBookValidation Validation
		{
			get
			{
				return GetNewValidation();
			}
		}
		protected virtual LockReleaseComplianceBookValidation GetNewValidation()
		{
			return new LockReleaseComplianceBookValidation(this);
		}

		#endregion

	}
}
