using System;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class CusEntryHeaderCharges : TypeSafeCusEntryHeaderCharges, Integration.Customs.US.ICusEntryHeaderCharges, IFee
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new class Schema : TypeSafeCusEntryHeaderCharges.Schema
		{
			public const string ChargeTypeDescription = "ChargeTypeDescription";
		}

		public CusEntryHeaderCharges(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region SupportsCloneCore
		protected override bool SupportsCloneCore()
		{
			return true;
		}
		#endregion

		public CusEntryHeader Parent
		{
			get { return Factory.Load<CusEntryHeader>(C1_CH); }
		}

		[List(nameof(Lookups) + "." + nameof(CusEntryHeaderChargesLookups.C1_ChargeTypeList))]
		public override ZString C1_ChargeType
		{
			get { return base.C1_ChargeType; }
			set { base.C1_ChargeType = value; }
		}

		public bool C1_ChargeType_ReadOnly
		{
			get
			{
				var entryHeader = Parent;
				return entryHeader != null && entryHeader.IsReconImportEntry && !((IReconOriginalChargeParent)entryHeader).DefaultValueForOverridenForNewChild;
			}
		}

		[DecimalPlaces(2)]
		public override ZDecimal C1_ChargeAmount
		{
			get { return base.C1_ChargeAmount; }
			set { base.C1_ChargeAmount = value; }
		}

		public bool C1_ChargeAmount_ReadOnly => C1_ChargeType_ReadOnly && !IsReconMerchandiseProcessingMonthlyFiling;

		public ZString ChargeTypeDescription
		{
			get { return Lookups.C1_ChargeTypeList.GetDescriptionFromCode(C1_ChargeType) ?? ""; }
		}

		public ZPropertyInfo ChargeTypeDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.ChargeTypeDescription); }
		}

		protected override bool ShouldResetDataOnMergingCore
		{
			get { return !EntryHeader?.Declaration?.US_NoDutyCalc ?? true; }
		}

		protected override void CheckChargeTypeUniqueness(ZString value)
		{
		}

		protected override bool ShouldDeleteIfChargeAmountIsZero
		{
			get { return Parent != null && !(Parent.IsReconImportEntry || Parent.IsReconEntry); }
		}

		bool IsReconMerchandiseProcessingMonthlyFiling
		{
			get
			{
				var entryHeader = Parent;
				return entryHeader != null
					&& entryHeader.IsReconImportEntry
					&& C1_ChargeType == Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing
					&& ((IReconOriginalChargeParent)entryHeader).MonthlyFiling;
			}
		}

		#region IFee Members

		ZString IFee.Code
		{
			get { return C1_ChargeType; }
			set { C1_ChargeType = value; }
		}

		ZDecimal IFee.Amount
		{
			get { return C1_ChargeAmount; }
			set { C1_ChargeAmount = value; }
		}

		ZString IFee.SelectedRateType
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		ZBool IFee.IsOverridden => false;

		#endregion
	}
}
