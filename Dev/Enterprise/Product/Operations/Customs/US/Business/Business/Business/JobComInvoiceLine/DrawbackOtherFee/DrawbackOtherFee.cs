using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class DrawbackOtherFee : AutoDrawbackOtherFee
	{
		public DrawbackOtherFee(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Schema

		public new class Schema : AutoDrawbackOtherFee.Schema
		{
			public const string FeeDescription = "FeeDescription";
			public const string FeeAmountPerUnit = "FeeAmountPerUnit";
			public const string ClaimedAmount = "ClaimedAmount";
			public const string _99ClaimedAmount = "_99ClaimedAmount";
			public const string DeclaredAmount = "DeclaredAmount";
			public const string CalculatedAmount = "CalculatedAmount";
			public const string AdjClaimAmount = "AdjClaimAmount";
		}

		#endregion

		#region Override Properties

		[ResourceStringData("Enterprise.Customs.US.Business.DrawbackOtherFee|US_FeeType", Caption = "Fee Type")]
		[ReadOnlyMember(nameof(IsAutoCalculated))]
		public override ZString US_FeeType
		{
			get { return base.US_FeeType; }
			set { base.US_FeeType = value; }
		}

		public override ZDecimal US_FeeAmount
		{
			get => base.US_FeeAmount;
			set
			{
				var oldValue = US_FeeAmount;
				base.US_FeeAmount = value;
				if (!IsCopying && Claims.DrawbackLine.ShouldReCalculateDrawbackData && oldValue != US_FeeAmount)
				{
					Claims.ACEOtherFeeClaim.Default_99ClaimedDutyAndCalculatedAmount();
				}
			}
		}

		#endregion

		#region New Properties

		public new JobComInvoiceLine Parent
		{
			get { return (JobComInvoiceLine)base.Parent; }
		}

		public DrawbackClaims Claims
		{
			get { return fClaims ?? (fClaims = new DrawbackClaims(this)); }
		}
		DrawbackClaims fClaims;

		ZBool IsAutoCalculated
		{
			get { return Parent?.IsAutoCalculated ?? false; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.DrawbackOtherFee|FeeDescription", Caption = "Fee Description")]
		public ZString FeeDescription
		{
			get { return AddInfoLookups.OtherFeeTypes.GetDescriptionFromCode(US_FeeType); }
		}

		public ZPropertyInfo FeeDescriptionInfo
		{
			get { return GetZPropertyInfo(DrawbackOtherFee.Schema.FeeDescription); }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.DrawbackOtherFee|FeeAmountPerUnit", Caption = "Per Unit")]
		public ZDecimal FeeAmountPerUnit
		{
			get { return Claims.ACEOtherFeeClaim.PerUnit; }
		}

		public ZPropertyInfo FeeAmountPerUnitInfo
		{
			get { return GetZPropertyInfo(DrawbackOtherFee.Schema.FeeAmountPerUnit); }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.DrawbackOtherFee|ClaimedAmount", Caption = "Used/Export Amt/Duty")]
		public ZDecimal ClaimedAmount
		{
			get { return Claims.ACEOtherFeeClaim.ClaimedDuty; }
		}

		public ZPropertyInfo ClaimedAmountInfo
		{
			get { return GetZPropertyInfo(DrawbackOtherFee.Schema.ClaimedAmount); }
		}

		[ReadOnlyMember(nameof(IsAutoCalculated))]
		[ResourceStringData("Enterprise.Customs.US.Business.DrawbackOtherFee|_99ClaimedAmount", Caption = "Claimed Amounts 99%")]
		public ZDecimal _99ClaimedAmount
		{
			get { return Claims.ACEOtherFeeClaim._99ClaimedDuty; }
			set { Claims.ACEOtherFeeClaim._99ClaimedDuty = value; }
		}

		public ZPropertyInfo _99ClaimedAmountInfo
		{
			get { return GetWrappedZPropertyInfo(Schema._99ClaimedAmount, x => this.US_99ClaimAmountInfo); }
		}

		[ReadOnlyMember(nameof(IsAutoCalculated))]
		[ResourceStringData("Enterprise.Customs.US.Business.DrawbackOtherFee|DeclaredAmount", Caption = "Fee Amount")]
		public ZDecimal DeclaredAmount
		{
			get { return Claims.ACEOtherFeeClaim.DeclaredAmount; }
			set
			{
				var oldValue = DeclaredAmount;
				Claims.ACEOtherFeeClaim.DeclaredAmount = value;
				DeclaredAmountInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo DeclaredAmountInfo
		{
			get { return GetZPropertyInfo(DrawbackOtherFee.Schema.DeclaredAmount); }
		}

		[ReadOnlyMember(nameof(IsAutoCalculated))]
		[ResourceStringData("Enterprise.Customs.US.Business.DrawbackOtherFee|CalculatedAmount", Caption = "Calculated Amount")]
		public ZDecimal CalculatedAmount
		{
			get { return Claims.ACEOtherFeeClaim.CalculatedAmount; }
			set
			{
				var oldValue = CalculatedAmount;
				Claims.ACEOtherFeeClaim.CalculatedAmount = value;
				CalculatedAmountInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo CalculatedAmountInfo
		{
			get { return GetZPropertyInfo(DrawbackOtherFee.Schema.CalculatedAmount); }
		}

		[ReadOnlyMember(nameof(IsAutoCalculated))]
		[ResourceStringData("Enterprise.Customs.US.Business.DrawbackOtherFee|AdjustedClaimAmount", Caption = "Adjusted Claim Amount")]
		public ZDecimal AdjClaimAmount
		{
			get { return Claims.ACEOtherFeeClaim.AdjClaimAmount; }
			set
			{
				var oldValue = AdjClaimAmount;
				Claims.ACEOtherFeeClaim.AdjClaimAmount = value;
				AdjClaimAmountInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo AdjClaimAmountInfo
		{
			get { return GetZPropertyInfo(DrawbackOtherFee.Schema.AdjClaimAmount); }
		}

		#endregion
	}
}
