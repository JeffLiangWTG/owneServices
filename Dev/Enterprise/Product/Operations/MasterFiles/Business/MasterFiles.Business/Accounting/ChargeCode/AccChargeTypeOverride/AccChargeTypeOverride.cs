using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	[DependentBusinessObject(typeof(AccChargeCode), "ChargeTypeOverrides")]
	public class AccChargeTypeOverride : AutoAccChargeTypeOverride, IAccChargeTypeOverride
	{
		public AccChargeTypeOverride(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			fAN_AC_ChargeCode = AN_AC_ChargeCode;
		}

#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, System.ComponentModel.PropertyDescriptor[] propertyPath)
		{
			if (AN_InvoiceType.IsEmpty)
			{
				AN_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			}
			base.FillWithValidTestDataCore(kind, propertyPath);
		}

#endif

		#region Properties

		#region AN_MarginPercentage

		protected bool AN_MarginPercentage_ReadOnly
		{
			get { return AN_ChargeType != Constants.ChargeType.Margin; }
		}

		#endregion

		#region AN_JobDirection

		[List("Lookups.DirectionList")]
		public override ZString AN_JobDirection
		{
			get { return base.AN_JobDirection; }
			set { base.AN_JobDirection = value; }
		}

		#endregion

		#region AN_JobType

		[List("Lookups.JobTypes")]
		public override ZString AN_JobType
		{
			get { return base.AN_JobType; }
			set { base.AN_JobType = value; }
		}

		#endregion

		#region AN_InvoiceType

		[List("Lookups.InvoiceTypes")]
		public override ZString AN_InvoiceType
		{
			get { return base.AN_InvoiceType; }
			set { base.AN_InvoiceType = value; }
		}

		#endregion

		#region Charge Types
		[List("Lookups.AC_ChargeType_List")]
		public override ZString AN_ChargeType
		{
			get { return base.AN_ChargeType; }
			set
			{
				base.AN_ChargeType = value;
				if (ChargeCode != null)
				{
					ChargeCode.UpdateChargeTypeDependentReadOnlyInfo();
				}
				SetDefaultMarginPercentage();
			}
		}

		public bool IsDisbursement
		{
			get { return this.AN_ChargeType == Core.Constants.ChargeType.Disbursement; }
		}

		public bool IsRevenue
		{
			get { return this.AN_ChargeType == Core.Constants.ChargeType.Revenue; }
		}

		public bool IsMargin
		{
			get { return this.AN_ChargeType == Core.Constants.ChargeType.Margin; }
		}

		void SetDefaultMarginPercentage()
		{
			if (IsMargin || IsDisbursement)
			{
				AN_MarginPercentage = 100M;
			}
			else
			{
				AN_MarginPercentage = 0M;
			}
		}

		#endregion

		#region HasOveriddenInvoiceType

		public bool HasOveriddenInvoiceType
		{
			get { return AN_InvoiceType != "DEF"; }
		}

		#endregion

		#endregion

		#region Duplicate

		public bool IsDuplicate(AccChargeTypeOverride @override)
		{
			return AN_AC_ChargeCode == @override.AN_AC_ChargeCode &&
				AN_JobDirection == @override.AN_JobDirection &&
				AN_JobType == @override.AN_JobType &&
				AN_InvoiceType == @override.AN_InvoiceType;
		}

		#endregion

		#region Parent Charge Code

		public override ZGuid AN_AC_ChargeCode
		{
			get { return IsDeleted ? fAN_AC_ChargeCode : base.AN_AC_ChargeCode; }
			set
			{
				base.AN_AC_ChargeCode = value;
				fAN_AC_ChargeCode = value;
			}
		}
		ZGuid fAN_AC_ChargeCode;

		public new AccChargeCode ChargeCode
		{
			get { return Factory.Load<AccChargeCode>(AN_AC_ChargeCode); }
		}

		#endregion
	}
}
