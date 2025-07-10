using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.ZArchitecture.Business.ClusterKey;

namespace Enterprise.Customs.Business
{
	[DebuggerDisplay("CusEntryHeaderCharges. {C1_ChargeType}={C1_ChargeAmount}")]
	public class CusEntryHeaderCharges : AutoCusEntryHeaderCharges, Integration.Customs.ICusEntryHeaderCharges, ITypeDeciderContext, IClusterKeyWorker
	{
		public static readonly CusEntryHeaderChargesTypeDecider TypeDecider = new CusEntryHeaderChargesTypeDecider();

		public CusEntryHeaderCharges(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public bool ShouldResetDataOnMerging
		{
			get { return ShouldResetDataOnMergingCore; }
		}

		public override void OnSaving()
		{
			if (ShouldDeleteIfChargeAmountIsZero && C1_ChargeAmount == 0m)
			{
				Delete();
			}
			base.OnSaving();
		}

		public override ZString C1_ChargeType
		{
			get { return base.C1_ChargeType; }
			set
			{
				var oldValue = C1_ChargeType;
				base.C1_ChargeType = value;
				if (!IsCopying && value != oldValue)
				{
					CheckChargeTypeUniqueness(value);
				}
			}
		}

		[RelatedBusinessObject("EntryHeader")]
		public override ZGuid C1_CH
		{
			get { return base.C1_CH; }
			set { base.C1_CH = value; }
		}

		[List(nameof(Lookups) + "." + nameof(CusEntryHeaderChargesLookups.PaymentMethodsList))]
		[MaxLength(3)]
		[ResourceStringData("534E9729-24C4-401F-8A66-9372DDA027AC", Caption = "Payment Method", ShortCaption = "Payment Method")]
		public override ZString C1_MethodOfPayment
		{
			get => base.C1_MethodOfPayment;
			set => base.C1_MethodOfPayment = value;
		}

		public CusEntryHeader EntryHeader => Factory.Load<CusEntryHeader>(C1_CH);

		protected virtual void CheckChargeTypeUniqueness(ZString value)
		{
			var entryHeader = EntryHeader;
			if (entryHeader != null)
			{
				var isConfirmed = IsConfirmed;
				var charges = isConfirmed ? entryHeader.ConfirmedCharges.Cast<CusEntryHeaderCharges>() : entryHeader.Charges.Cast<CusEntryHeaderCharges>();
				var collectionName = isConfirmed ? "ConfirmedCusEntryHeaderChargesCollection" : "CusEntryHeaderChargesCollection";

				if (charges.Any(x => x != this && x.C1_ChargeType == value && !x.C1_IsLandedCostOnly))
				{
					if (entryHeader.Declaration.IsImportingData)
					{
						throw new DataObjectReadFailureException(Res.GetString("180BF29F-B335-4D37-A04E-9EA620B427D3", "The charge code {0} is not unique in this {1}.", value, collectionName));
					}

					if (isConfirmed)
					{
						ErrorReporter.ReportOnce($"Not unique in this {collectionName}.", value + $" is not unique in this {collectionName}.");
					}
				}
			}
		}

		public ZBool IsConfirmed => C1_Source == CusEntryLineFeeSourceCodeList.Codes.CUS;

		protected virtual bool ShouldDeleteIfChargeAmountIsZero => true;

		protected virtual bool ShouldResetDataOnMergingCore => true;

		protected override bool SupportsCloneCore() => !IsConfirmed && base.SupportsCloneCore();

		#region ITypeDeciderContext Members

		string ITypeDeciderContext.Country => (EntryHeader as ITypeDeciderContext)?.Country ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

		#endregion

		#region IClusterKeyWorker

		ZPropertyInfoInt IClusterKeyEntity.ClusterKeyPty => (ZPropertyInfoInt)C1_ClusterKeyInfo;
		Type IClusterKeyWorker.ParentBizObjType => typeof(CusEntryHeader);
		ZPropertyInfoGuid IClusterKeyWorker.FkToParentPty => (ZPropertyInfoGuid)C1_CHInfo;
		IEnumerable<ClusterKeyChildInfo> IClusterKeyWorker.ClusterKeyChildList => null;

		#endregion
	}
}
