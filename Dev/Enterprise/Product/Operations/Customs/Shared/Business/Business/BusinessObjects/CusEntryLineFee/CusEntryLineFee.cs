using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.ClusterKey;

namespace Enterprise.Customs.Business
{
	[DebuggerDisplay("CusEntryLineFee. {CF_ChargeType}={CF_ChargeAmount}")]
	[DependentBusinessObject(typeof(CusEntryLine), "Fees")]
	public class CusEntryLineFee : AutoCusEntryLineFee, Integration.Customs.ICusEntryLineFee, ITypeDeciderContext, IClusterKeyWorker
	{
		public static readonly CusEntryLineFeeTypeDecider TypeDecider = new CusEntryLineFeeTypeDecider();

		public CusEntryLineFee(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override bool IsSavedByFactory => IsInDatabase || !CF_ChargeAmount.IsEmpty || !ShouldDeleteIfChargeAmountIsZero;

		public override void OnSaving()
		{
			if (ShouldDeleteIfChargeAmountIsZero && CF_ChargeAmount.IsEmpty)
			{
				Delete();
			}
			base.OnSaving();
		}

		public bool ShouldResetDataOnMerging => ShouldResetDataOnMergingCore;

		public CusEntryLine EntryLine => GetEntryLine();

		protected virtual CusEntryLine GetEntryLine() => Factory.Load<CusEntryLine>(CF_CL);

		[RelatedBusinessObject("EntryLine")]
		public override ZGuid CF_CL
		{
			get { return base.CF_CL; }
			set { base.CF_CL = value; }
		}

		[List(nameof(Lookups) + "." + nameof(CusEntryLineFeeLookups.RateOverrideReasonList))]
		public override ZString CF_RateOverrideReasonCode
		{
			get => base.CF_RateOverrideReasonCode;
			set => base.CF_RateOverrideReasonCode = value;
		}

		[List(nameof(Lookups) + "." + nameof(CusEntryLineFeeLookups.MethodOfPaymentList))]
		public override ZString CF_MethodOfPayment
		{
			get => base.CF_MethodOfPayment;
			set => base.CF_MethodOfPayment = value;
		}

		[List(nameof(Lookups) + "." + nameof(CusEntryLineFeeLookups.MethodOfCalculationList))]
		public override ZString CF_MethodOfCalculation
		{
			get => base.CF_MethodOfCalculation;
			set => base.CF_MethodOfCalculation = value;
		}

		[DecimalPlaces(nameof(CF_BaseValueDecimalPlaces))]
		public override ZDecimal CF_BaseValue { get => base.CF_BaseValue; set => base.CF_BaseValue = value; }

		public int CF_BaseValueDecimalPlaces => CF_BaseValueDecimalPlacesCore;

		protected virtual int CF_BaseValueDecimalPlacesCore => ZArchitecture.Schema.CusEntryLineFeeSchema.CF_BaseValue.Scale;

		protected virtual bool ShouldDeleteIfChargeAmountIsZero => true;

		protected virtual bool ShouldResetDataOnMergingCore => true;

		internal protected void ResetData()
		{
			CF_ChargeAmount = 0;
			CF_IsLandedCostOnly = false;
			CF_Rate = 0;
			ResetDataCore();
		}

		public override bool SupportsNotes => false;

		public ZBool IsConfirmed => CF_Source == CusEntryLineFeeSourceCodeList.Codes.CUS;

		protected override bool SupportsCloneCore()
		{
			return !IsConfirmed && base.SupportsCloneCore();
		}

		protected virtual void ResetDataCore() { }

		#region ITypeDeciderContext Members

		string ITypeDeciderContext.Country => (EntryLine as ITypeDeciderContext)?.Country ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

		#endregion

		#region IClusterKeyWorker

		ZPropertyInfoInt IClusterKeyEntity.ClusterKeyPty => (ZPropertyInfoInt)CF_ClusterKeyInfo;
		Type IClusterKeyWorker.ParentBizObjType => typeof(CusEntryLine);
		ZPropertyInfoGuid IClusterKeyWorker.FkToParentPty => (ZPropertyInfoGuid)CF_CLInfo;
		IEnumerable<ClusterKeyChildInfo> IClusterKeyWorker.ClusterKeyChildList => null;

		#endregion
	}
}
