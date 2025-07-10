using System.Collections.Generic;
using System.Linq;
using CargoWise.DataTransfer.Ratings;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business
{
	[CalculatorProperty(CartageCalculator.Items.EquipmentType, RateLineItem.Schema.TM_Text, IsMandatory = true, MapTo = "String1", RelatedTo = "EquipmentType")]
	[CalculatorProperty(RateLine.Schema.TL_ConversionFactorString, MapTo = "String2")]
	public class CartageCalculator : BaseCombinedCalculator
	{
		public CartageCalculator(IRateLine master)
			: base(master)
		{
		}

		public const string Code = RatingCalculatorCodes.Cartage;

		#region Initialisation

		protected override IEnumerable<IRateLineItem> CheckOrCreateItems()
		{
			var itemList = base.CheckOrCreateItems();
			SetEquipmentDefaults(itemList);

			return itemList;
		}

		#endregion

		#region Properties

		public override ZString EquipmentType
		{
			get { return (ZString)this[CartageCalculator.Items.EquipmentType]; }
			set { this[CartageCalculator.Items.EquipmentType] = value; }
		}

		#region SetEquipmentDefaults

		void SetEquipmentDefaults(IEnumerable<IRateLineItem> itemList)
		{
			if (Line?.ParentRateEntry?.ParentRatingHeader?.Header == null)
			{
				return;
			}

			var equipmentLine = itemList.FirstOrDefault(item => item.TM_Type == CartageCalculator.Items.EquipmentType) as RateLineItem;
			if (equipmentLine == null || !equipmentLine.TM_Text.IsEmpty)
			{
				return;
			}

			using (equipmentLine.SuspendSettingHasChanges())
			using (equipmentLine.GetValidationSuspender())
			{
				equipmentLine.TM_Text = DetermineDefaultEquipment();
			}
		}

		protected ZString DetermineDefaultEquipment()
		{
			var result = ZString.Empty;

			var parentRateEntry = Line.ParentRateEntry;
			if (parentRateEntry.IsOriginEntry())
			{
				result = GetEquipmentForOriginEntry();
			}
			else if (parentRateEntry.IsDestinationEntry())
			{
				result = GetEquipmentForDestinationEntry();
			}
			else if (parentRateEntry.IsAir() || parentRateEntry.IsFCL() || parentRateEntry.IsLCL())
			{
				result = GetEquipmentForEntryUsingAddress(null);
			}

			return result;
		}

		#region Helper Methods

		ZString GetEquipmentForDestinationEntry()
		{
			var result = ZString.Empty;

			if (RateLineBizO.Parent.Consignee != null)
			{
				if (RateLineBizO.Parent.CartageDeliveryAddressOverride != null)
				{
					result = GetEquipmentForEntryUsingAddress(RateLineBizO.Parent.CartageDeliveryAddressOverride);
				}
				else
				{
					result = GetEquipmentForEntryUsingAddress(RateLineBizO.Parent.Consignee.Addresses.DefaultAddressOfType(OrgAddressType.Delivery, false));
				}
			}
			else
			{
				result = GetEquipmentForEntryUsingAddress(RateLineBizO.Parent.Parent.Header.Addresses.DefaultAddressOfType(OrgAddressType.Delivery, false));
			}

			return result;
		}

		ZString GetEquipmentForOriginEntry()
		{
			var result = ZString.Empty;

			if (RateLineBizO.Parent.Consignor != null)
			{
				if (RateLineBizO.Parent.CartagePickupAddressOverride != null)
				{
					result = GetEquipmentForEntryUsingAddress(RateLineBizO.Parent.CartagePickupAddressOverride);
				}
				else
				{
					result = GetEquipmentForEntryUsingAddress(RateLineBizO.Parent.Consignor.Addresses.DefaultAddressOfType(OrgAddressType.Pickup, false));
				}
			}
			else
			{
				result = GetEquipmentForEntryUsingAddress(RateLineBizO.Parent.Parent.Header.Addresses.DefaultAddressOfType(OrgAddressType.Pickup, false));
			}

			return result;
		}

		ZString GetEquipmentForEntryUsingAddress(OrgAddress address)
		{
			var result = ZString.Empty;
			if (RateLineBizO.Parent.IsAir())
			{
				result = address != null ? address.OA_AIREquipmentNeeded : new ZString(OrganisationsDataRegistry.Instance.RequiredCartageEquipmentAIR.Value);
			}
			else if (RateLineBizO.Parent.IsFCL())
			{
				result = address != null ? address.OA_FCLEquipmentNeeded : new ZString(OrganisationsDataRegistry.Instance.RequiredCartageEquipmentFCL.Value);
			}
			else if (RateLineBizO.Parent.IsLCL())
			{
				result = address != null ? address.OA_LCLEquipmentNeeded : new ZString(OrganisationsDataRegistry.Instance.RequiredCartageEquipmentLCL.Value);
			}

			return result;
		}

		#endregion

		#endregion

		protected override bool ShowEquipmentTypeInternal
		{
			get { return true; }
		}

		protected override bool HasDuplicateEquipmentType(RateLine line)
		{
			var hasDifferentConditions = line.TL_Condition != Line.TL_Condition ||
				(line.TL_Condition == Line.TL_Condition && Line.TL_Condition == RateLineConditions.UserDefined && Line.TL_ConditionalExpression != line.TL_ConditionalExpression);

			var allowDuplicate = EquipmentType == Core.Constants.EquipmentNeeded.Any
				? hasDifferentConditions || line.TL_WeightVolume != Line.TL_WeightVolume
				: hasDifferentConditions;

			return base.HasDuplicateEquipmentType(line) && !allowDuplicate;
		}

		public override bool SupportsProductLineUnitFactor => true;

		public override bool SupportsPackageLineUnitFactor => true;

		#endregion

		#region Lists

		protected CodeDescriptionPairList CartageConversionFactorList
		{
			get
			{
				return Line.ConversionFactorForBinding.Lookups.ConversionFactors;
			}
		}

		public override CodeDescriptionPairList List1
		{
			get { return RateLineBizO.Lookups.EquipmentTypes; }
		}

		public override CodeDescriptionPairList List2
		{
			get { return CartageConversionFactorList; }
		}

		#endregion

		#region Quotation Lines

		protected override QuotationLine GetBreakMinimumQuotationLine(IRateLineItem item)
		{
			var breakMinimumLine = QuotationLine.NewWithValue(Line, 0, item.TM_BreakMinimum, Res.GetString("ef589a65-f4a4-4f9c-a023-896cec3fde44", "Minimum"), (NoResString)ZString.Empty);
			if (breakMinimumLine != null)
			{
				breakMinimumLine.Shift();
			}

			return breakMinimumLine;
		}

		#endregion

		public override bool CanBePrintedUsing6StandardOperatorColumnHeaders
		{
			get { return false; }
		}
	}
}

