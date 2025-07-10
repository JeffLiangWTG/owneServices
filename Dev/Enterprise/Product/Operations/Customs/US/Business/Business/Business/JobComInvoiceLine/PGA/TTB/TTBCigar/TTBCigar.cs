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
	public class TTBCigar : AutoTTBCigar, ITTBCigar
	{
		public TTBCigar(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : AutoTTBCigar.Schema
		{
			public const string IsMaximumRate = "IsMaximumRate";
		}

		public new TTBLine Parent
		{
			get { return base.Parent as TTBLine; }
		}

		#region Override Properties

		public const decimal MaximuSalePrice = 76.322m;

		[ResourceStringData("Enterprise.Customs.US.Business.TTBCigar|IsMaximumRate", Caption = "Is Maximum Rate", MediumCaption = "Maximum Rate", ShortCaption = "Is Max")]
		[ReadOnlyMember(nameof(IsMaximumRate_ReadOnly))]
		public ZBool IsMaximumRate
		{
			get { return US_UnitPrice.Round(3) == MaximuSalePrice; }
			set
			{
				var oldValue = IsMaximumRate;
				if (!IsCopying && IsMaximumRate != value)
				{
					US_UnitPrice = value ? MaximuSalePrice : 0m;
				}
				IsMaximumRateInfo.RefreshBinding(oldValue);
			}
		}

		bool IsMaximumRate_ReadOnly
		{
			get { return US_IsSmall; }
		}

		public ZPropertyInfo IsMaximumRateInfo
		{
			get { return GetZPropertyInfo(Schema.IsMaximumRate); }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.TTBCigar|US_IsSmall", Caption = "Is Small Cigar", MediumCaption = "Small Cigar", ShortCaption = "Small")]
		public override ZBool US_IsSmall
		{
			get { return base.US_IsSmall; }
			set
			{
				var oldValue = US_IsSmall;
				base.US_IsSmall = value;
				if (!IsCopying && oldValue != US_IsSmall && US_IsSmall)
				{
					IsMaximumRate = ZBool.False;
					US_UnitPrice = ZDecimal.Zero;
				}
			}
		}

		[ResourceStringData("Enterprise.Customs.US.Business.TTBCigar|US_Quantity", Caption = "Quantity", FullDescription = "Large Cigar Sale Quantity")]
		public override ZInt US_Quantity
		{
			get { return base.US_Quantity; }
			set { base.US_Quantity = value; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.TTBCigar|US_UnitPrice", Caption = "Unit Price (c)", FullDescription = "Large Cigar Sale Unit Price")]
		public override ZDecimal US_UnitPrice
		{
			get { return base.US_UnitPrice; }
			set { base.US_UnitPrice = value; }
		}
		#endregion

		#region ITTBCigar Members

		ZBool ITTBCigar.IsSmall
		{
			get { return US_IsSmall; }
		}

		ZInt ITTBCigar.Quantity
		{
			get { return US_Quantity; }
		}

		ZDecimal ITTBCigar.UnitPrice
		{
			get { return US_UnitPrice; }
		}

		#endregion

		public void UpdateAddInfoProperties()
		{
			if (Data != null && HasChanges)
			{
				Data.UpdateRelatedPropertyInfo();
			}
		}
	}
}
