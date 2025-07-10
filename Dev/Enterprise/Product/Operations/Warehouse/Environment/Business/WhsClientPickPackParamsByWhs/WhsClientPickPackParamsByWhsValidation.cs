//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoWhsClientPickPackParamsByWhsValidation
//
//    This class should be used for overriding validation in AutoWhsClientPickPackParamsByWhsValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------
using System.Collections.Immutable;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Environment.Business
{
	public class WhsClientPickPackParamsByWhsValidation : AutoWhsClientPickPackParamsByWhsValidation
	{
		public WhsClientPickPackParamsByWhsValidation(AutoWhsClientPickPackParamsByWhs parent)
			: base(parent)
		{
		}

		#region CheckWPP_F3_NKPackType

		protected override void CheckWPP_F3_NKPackType()
		{
			base.CheckWPP_F3_NKPackType();
			ListValidation.ErrorIfInvalidCode(Parent.WPP_F3_NKPackTypeInfo);
		}

		#endregion

		#region CheckWPP_NumberOfLabelsToPrintOnClose

		protected override void CheckWPP_NumberOfLabelsToPrintOnClose()
		{
			base.CheckWPP_NumberOfLabelsToPrintOnClose();

			MandatoryValidation.CheckNotNegative(Parent.WPP_NumberOfLabelsToPrintOnCloseInfo);
			CheckBothNumberOfLabelsAreNotZeroIfPickAndPackEnabled(Parent.WPP_NumberOfLabelsToPrintOnCloseInfo);
			CheckNumberOfLabelsToPrintDoesNotExceed50(Parent.WPP_NumberOfLabelsToPrintOnCloseInfo, Parent.WPP_NumberOfLabelsToPrintOnClose);
		}

		void CheckBothNumberOfLabelsAreNotZeroIfPickAndPackEnabled(ZPropertyInfo info)
		{
			if (!info.HasErrors() && Parent.WPP_IsPickAndPackEnabled && Parent.WPP_NumberOfLabelsToPrintOnClose == 0 && Parent.WPP_NumberOfLabelsToPrintOnNew == 0)
			{
				info.AddError(Res.GetString("2e811768-2903-4adc-b573-82f4b5a8be4c", "Must print at least one Label for New or Close Package."));
			}
		}

		void CheckNumberOfLabelsToPrintDoesNotExceed50(ZPropertyInfo info, int numberOfLabelsToPrint)
		{
			if (!info.HasErrors() && Parent.WPP_IsPickAndPackEnabled && numberOfLabelsToPrint > 50)
			{
				info.AddError(Res.GetString("c2be6dd6-c228-4b51-a51b-75b5a8b94df0", "Should not print more than 50 Labels."));
			}
		}

		#endregion

		#region CheckWPP_NumberOfLabelsToPrintOnNew

		protected override void CheckWPP_NumberOfLabelsToPrintOnNew()
		{
			base.CheckWPP_NumberOfLabelsToPrintOnNew();

			MandatoryValidation.CheckNotNegative(Parent.WPP_NumberOfLabelsToPrintOnNewInfo);
			CheckBothNumberOfLabelsAreNotZeroIfPickAndPackEnabled(Parent.WPP_NumberOfLabelsToPrintOnNewInfo);
			CheckNumberOfLabelsToPrintDoesNotExceed50(Parent.WPP_NumberOfLabelsToPrintOnNewInfo, Parent.WPP_NumberOfLabelsToPrintOnNew);
		}

		#endregion

		#region CheckWPP_PromptForWeightAndDimensions

		protected override void CheckWPP_PromptForWeightAndDimensions()
		{
			base.CheckWPP_PromptForWeightAndDimensions();

			if (!Parent.WPP_PromptForWeightAndDimensionsInfo.HasErrors() && !Parent.WPP_IsPickAndPackEnabled && Parent.WPP_PromptForWeightAndDimensions)
			{
				Parent.WPP_PromptForWeightAndDimensionsInfo.AddError(Res.GetString("b47dc0ca-0015-4d32-8386-97affcabc6ed", "This setting is only usable if Pick & Pack is enabled."));
			}
		}

		#endregion

		#region CheckWPP_CartoniseByProductOrProductCategory

		protected override void CheckWPP_CartonizeByProduct()
		{
			base.CheckWPP_CartonizeByProduct();
			CheckProductAndProductCategory(Parent.WPP_CartonizeByProductInfo);
		}

		protected override void CheckWPP_CartonizeByProductCategory()
		{
			base.CheckWPP_CartonizeByProductCategory();
			CheckProductAndProductCategory(Parent.WPP_CartonizeByProductCategoryInfo);
		}

		void CheckProductAndProductCategory(ZPropertyInfo zPropertyInfo)
		{
			if (Parent.WPP_CartonizeByProduct && Parent.WPP_CartonizeByProductCategory)
			{
				var errorString = Res.GetString("9F40C54B-7BE9-4169-95E2-7DBC3BE92649", "Cartonize By Product and Cartonize By Product Category should not be selected at the same time.");
				zPropertyInfo.AddError(errorString);
			}
		}

		#endregion

		#region CheckWPP_CycleCountOnShort

		protected override void CheckWPP_CycleCountOnShort()
		{
			base.CheckWPP_CycleCountOnShort();
			CheckCycleCountOnShortNotEnabledWithASalesChannel(Parent.WPP_CycleCountOnShortInfo);
		}

		#endregion

		#region CheckWPP_AllowPickDockDoorLocationOverride

		protected override void CheckWPP_AllowPickDockDoorLocationOverride()
		{
			base.CheckWPP_AllowPickDockDoorLocationOverride();
			CheckAllowPickDockDoorLocationOverrideNotEnabledWithASalesChannel(Parent.WPP_AllowPickDockDoorLocationOverrideInfo);
		}

		#endregion

		#region CheckWPP_WSH_SalesChannel

		protected override void CheckWPP_WSH_SalesChannel()
		{
			base.CheckWPP_WSH_SalesChannel();
			CheckWarehouseAndSalesChannelIsNotSpecifiedTwiceForSameClient(Parent.WPP_WSH_SalesChannelInfo);
			CheckCycleCountOnShortNotEnabledWithASalesChannel(Parent.WPP_WSH_SalesChannelInfo);
			CheckAllowPickDockDoorLocationOverrideNotEnabledWithASalesChannel(Parent.WPP_WSH_SalesChannelInfo);
		}

		void CheckCycleCountOnShortNotEnabledWithASalesChannel(ZPropertyInfo info)
		{
			if (!info.HasErrors()
				&& Parent.WPP_CycleCountOnShort
				&& Parent.WPP_WSH_SalesChannel.IsValid)
			{
				info.AddError(Res.GetString("4a45e059-c5cd-4f6a-aa3b-3690d61042db", "Cycle Count on Short is not valid with a Sales Channel."));
			}
		}

		void CheckAllowPickDockDoorLocationOverrideNotEnabledWithASalesChannel(ZPropertyInfo info)
		{
			if (!info.HasErrors()
				&& Parent.WPP_AllowPickDockDoorLocationOverride
				&& Parent.WPP_WSH_SalesChannel.IsValid)
			{
				info.AddError(Res.GetString("23566631-4234-4D1A-B292-F220F59A7C5F", "Pick Dock Door Override is not valid with a Sales Channel."));
			}
		}

		#endregion

		#region CheckWPP_WW_Warehouse

		protected override void CheckWPP_WW_Warehouse()
		{
			base.CheckWPP_WW_Warehouse();
			CheckWarehouseAndSalesChannelIsNotSpecifiedTwiceForSameClient(Parent.WPP_WW_WarehouseInfo);
		}

		void CheckWarehouseAndSalesChannelIsNotSpecifiedTwiceForSameClient(ZPropertyInfo info)
		{
			if (!info.HasErrors()
				&& Parent.WPP_WW_Warehouse.IsValid
				&& (Parent.WPP_WSH_SalesChannel.IsEmpty || Parent.WPP_WSH_SalesChannel.IsValid))
			{
				var query = new ZQuery();
				query.AddToFilter(WhsClientPickPackParamsByWhsSchema.WPP_OH_Client, Parent.WPP_OH_Client);
				query.AddToFilter(WhsClientPickPackParamsByWhsSchema.WPP_WW_Warehouse, Parent.WPP_WW_Warehouse);
				query.AddEmptyAsNullToFilter(WhsClientPickPackParamsByWhsSchema.WPP_WSH_SalesChannel, Parent.WPP_WSH_SalesChannel);
				query.AddToFilter(WhsClientPickPackParamsByWhsSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);

				if (Parent.Factory.LoadTop1<WhsClientPickPackParamsByWhs>(query) != null)
				{
					info.AddError(Res.GetString("b80b05a0-9ea7-4c3e-9595-a15210fb081b", "The same Warehouse & Sales Channel can only be specified once per Client."));
				}
			}
		}

		#endregion

		#region CheckWPP_UseDirectedPackingConsolidation

		protected override void CheckWPP_UseDirectedPackingConsolidation()
		{
			base.CheckWPP_UseDirectedPackingConsolidation();

			var info = Parent.WPP_UseDirectedPackingConsolidationInfo;
			if (!info.HasErrors() && Parent.WPP_UseDirectedPackingConsolidation && !info.ReadOnly)
			{
				var warehouse = Parent.Factory.Load<WhsWarehouse>(Parent.WPP_WW_Warehouse);
				if (!warehouse?.HasPackingConsolidationLocations ?? false)
				{
					info.AddError(
						Res.GetString(
							"26097e69-273f-42cd-87fa-a64be75ffbdf",
							"Use Directed Packing Consolidation should not be enabled as there are no Packing Consolidation Locations in this Warehouse."));
				}
			}
		}

		#endregion

		#region ShouldValidateFKToCancelledRecord

		protected override bool ShouldValidateFKToCancelledRecord(ZPropertyInfo info)
			=> !FKsToNotValidateForCancelledRecords.Contains(info.Name) && base.ShouldValidateFKToCancelledRecord(info);

		static readonly ImmutableHashSet<string> FKsToNotValidateForCancelledRecords
			= ImmutableHashSet.Create(WhsClientPickPackParamsByWhsSchema.Constants.WPP_WSH_SalesChannel, WhsClientPickPackParamsByWhsSchema.Constants.WPP_F3_NKPackType);

		#endregion
	}
}
