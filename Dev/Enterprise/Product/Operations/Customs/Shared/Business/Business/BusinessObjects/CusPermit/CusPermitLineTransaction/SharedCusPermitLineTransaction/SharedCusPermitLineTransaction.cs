using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.Business
{
	[SystemDefinedValues]
	public abstract class SharedCusPermitLineTransaction : AutoCusPermitLineTransaction
	{
		protected SharedCusPermitLineTransaction(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static readonly SharedCusPermitLineTransactionTypeDecider TypeDecider = new SharedCusPermitLineTransactionTypeDecider();

		#region Override Properties

		[RelatedBusinessObject("PermitHeader")]
		public override ZGuid CPL_CPH_PermitHeader
		{
			get { return base.CPL_CPH_PermitHeader; }
			set
			{
				var hasChanged = CPL_CPH_PermitHeader != value;
				base.CPL_CPH_PermitHeader = value;
				if (hasChanged)
				{
					DefaultTransactionType();
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(SharedCusPermitLineTransactionLookups.PermitTransactionCategories))]
		public override ZString CPL_TransactionCategory
		{
			get { return base.CPL_TransactionCategory; }
			set { base.CPL_TransactionCategory = value; }
		}

		[List(nameof(Lookups) + "." + nameof(SharedCusPermitLineTransactionLookups.PermitTransactionTypes))]
		public override ZString CPL_TransactionType
		{
			get { return base.CPL_TransactionType; }
			set { base.CPL_TransactionType = value; }
		}

		[ReadOnly(true)]
		[List(nameof(Lookups) + "." + nameof(SharedCusPermitLineTransactionLookups.PermitTransactionStatuses))]
		public override ZString CPL_TransactionStatus
		{
			get { return base.CPL_TransactionStatus; }
			set { base.CPL_TransactionStatus = value; }
		}

		public bool CPL_TransactionType_ReadOnly => true;

		public bool CPL_TransactionDate_ReadOnly => true;

		public override bool ReadOnly
		{
			get { return base.ReadOnly || this.IsInDatabase; }
			set { base.ReadOnly = value; }
		}

		public override ZDecimal CPL_TranQty
		{
			get { return base.CPL_TranQty; }
			set
			{
				var hasChanged = CPL_TranQty != value;
				base.CPL_TranQty = value;
				if (hasChanged)
				{
					PermitHeader?.QuantityBalanceInfo.RefreshBinding();
				}
			}
		}

		public override ZDecimal CPL_TranValue
		{
			get { return base.CPL_TranValue; }
			set
			{
				var hasChanged = CPL_TranValue != value;
				base.CPL_TranValue = value;
				if (hasChanged)
				{
					PermitHeader?.ValueBalanceInfo.RefreshBinding();
				}
			}
		}

		#endregion

		#region New Properties

		public bool IsPending => CPL_TransactionStatus == PermitTransactionStatusList.Codes.Pending;

		public ZString TransactionTypeDescription => Lookups.PermitTransactionTypes.GetDescriptionFromCode(CPL_TransactionType);

		public SharedCusPermitHeader PermitHeader => Factory.Load<SharedCusPermitHeader>(CPL_CPH_PermitHeader);

		public ZString TransactionStatusDescription
		{
			get
			{
				var status = CPL_TransactionStatus;
				var statusList = Lookups.PermitTransactionStatuses;
				var list = statusList.ContainsCode(status) ? statusList : Factory.GetCachedValue<PermitTransactionStatusList>();
				return list.GetDescriptionFromCode(status);
			}
		}

		#endregion

		#region Business Object Overrides

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		protected override ZString HumanReadableNameCore
		{
			get { return Res.GetString("9E59C347-1EB1-4F80-9097-60737E1E519D", "{0} {1}", ShortName, CPL_Reference).Trim(); }
		}

		protected abstract ZString ShortName { get; }

		#endregion

		#region Implementation

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CPL_TransactionDate = ZDateTime.Now;
		}

		protected override void OnCreateAutoAdminLog()
		{
			base.OnCreateAutoAdminLog();

			var autoCreatedLog = Logs.AutoCreatedLog;
			if (autoCreatedLog != null && autoCreatedLog.SL_SE_NKEvent == AutoEvents.AddedARecordToTheSystem.Code)
			{
				autoCreatedLog.Delete();
			}
		}

		protected void DefaultTransactionType()
		{
			if (CPL_TransactionType.IsEmpty)
			{
				CPL_TransactionType = ((PermitHeader?.GetTransactions().Count() ?? -1) == 0) ? PermitTransactionTypeList.Codes.OBL : PermitTransactionTypeList.Codes.ADJ;
			}
		}

		#endregion

		#region Validation and Lookups

		public new SharedCusPermitLineTransactionLookups Lookups => (SharedCusPermitLineTransactionLookups)base.Lookups;

		public new SharedCusPermitLineTransactionValidation Validation => (SharedCusPermitLineTransactionValidation)base.Validation;

		#endregion Validation and Lookups
	}
}
