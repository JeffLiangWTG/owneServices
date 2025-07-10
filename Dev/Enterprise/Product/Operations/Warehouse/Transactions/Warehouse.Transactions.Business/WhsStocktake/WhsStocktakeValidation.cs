using System;
using System.Collections.Immutable;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsStocktakeValidation : AutoWhsStocktakeValidation
	{
		public WhsStocktakeValidation(AutoWhsStocktake parent)
			: base(parent)
		{
		}

		public new WhsStocktake Parent
		{
			get { return (WhsStocktake)base.Parent; }
		}

		#region CheckWS_ABCAnalysisCategory

		protected override void CheckWS_ABCAnalysisCategory()
		{
			ListValidation.WarnIfInvalidCode(Parent.WS_ABCAnalysisCategoryInfo);
		}

		#endregion

		#region CheckWS_CountEmptyLocationsCategory

		protected override void CheckWS_CountEmptyLocationsCategory()
		{
			base.CheckWS_CountEmptyLocationsCategory();
			if (!Parent.IsDeleted)
			{
				ListValidation.ErrorIfInvalidCode(Parent.WS_CountEmptyLocationsCategoryInfo);
				AddErrorCountEmptyLocationIfIsInvalid(Parent.WS_CountEmptyLocationsCategoryInfo);
			}
		}

		void AddErrorCountEmptyLocationIfIsInvalid(ZPropertyInfo property)
		{
			if (Parent.WS_CountEmptyLocationsCategory == CountEmptyLocationCategory.Codes.IncludeEmptyLocations || Parent.WS_CountEmptyLocationsCategory == CountEmptyLocationCategory.Codes.OnlyCountEmptyLocations)
			{
				AddErrorCannotSelectWhenCountEmptyLocation(property, Parent.WS_OH_ClientInfo.HumanReadableName, () => Parent.Client != null);
				AddErrorCannotSelectWhenCountEmptyLocation(property, Res.GetData("27a333f4-26e9-4c0b-bcab-4e3662a1cd72", "Product").Caption, () => Parent.ProductFilterCollection.Count > 0);
				AddErrorCannotSelectWhenCountEmptyLocation(property, Parent.WS_RH_NKCommodityCodeInfo.HumanReadableName, () => !string.IsNullOrEmpty(Parent.WS_RH_NKCommodityCode));
				AddErrorCannotSelectWhenCountEmptyLocation(property, Parent.WS_ABCAnalysisCategoryInfo.HumanReadableName, () => !string.IsNullOrEmpty(Parent.WS_ABCAnalysisCategory));
				AddErrorCannotSelectWhenCountEmptyLocation(property, Parent.WS_StocktakeCycleInfo.HumanReadableName, () => !string.IsNullOrEmpty(Parent.WS_StocktakeCycle));
				AddErrorCannotSelectWhenCountEmptyLocation(property, Parent.WS_StocktakeTypeInfo.HumanReadableName, () => Parent.WS_StocktakeType != "STD");
			}
		}

		void AddErrorCannotSelectWhenCountEmptyLocation(ZPropertyInfo property, ZString propName, Func<Boolean> conditionToAddError)
		{
			if (conditionToAddError())
			{
				property.AddError(propName + " " + CannotSelectWhenCountEmptyLocations);
			}
		}

		#endregion

		#region CheckWS_StocktakeStatus

		protected override void CheckWS_StocktakeStatus()
		{
			base.CheckWS_StocktakeStatus();
			MandatoryValidation.CheckEntered(Parent.WS_StocktakeStatusInfo);
			ListValidation.ErrorIfInvalidCode(Parent.WS_StocktakeStatusInfo, new StocktakeStatus());
		}

		#endregion

		#region CheckWS_RH_NKCommodityCode

		protected override void CheckWS_RH_NKCommodityCode()
		{
			base.CheckWS_RH_NKCommodityCode();
			ListValidation.ErrorIfInvalidCode(Parent.WS_RH_NKCommodityCodeInfo, Parent.Lookups.CommodityCodes);
		}

		#endregion

		#region CheckWS_StocktakeCycle

		protected override void CheckWS_StocktakeCycle()
		{
			base.CheckWS_StocktakeCycle();
			ListValidation.ErrorIfInvalidCode(Parent.WS_StocktakeCycleInfo, Parent.Lookups.StockTakeCycles);
		}

		#endregion

		#region CheckWS_WW_Whs

		protected override void CheckWS_WW_Whs()
		{
			base.CheckWS_WW_Whs();
			MandatoryValidation.CheckEntered(Parent.WS_WW_WhsInfo);
			if (Parent.Warehouse != null && !Parent.Warehouse.WW_IsActive)
			{
				Parent.WS_WW_WhsInfo.AddError(CannotSelectInactiveWarehouse);
			}
		}

		#endregion

		#region CheckWS_OH_Client

		protected override void CheckWS_OH_Client()
		{
			base.CheckWS_OH_Client();
			if (Parent.Client != null && !Parent.Client.OH_IsActive)
			{
				Parent.WS_OH_ClientInfo.AddError(CannotSelectInactiveClient);
			}
		}

		#endregion

		#region Location

		public void ValidateLocationString()
		{
			ValidateCalculatedProperty(Parent.LocationStringInfo);
		}

		protected void CheckLocationString()
		{
			var parent = Parent;

			if (parent.LocationString != "")
			{
				WhsLocation.ValidateLocation(parent, parent.LocationStringInfo);

				var location = parent.Location;
				if (location != null && location.IsDockDoorLocation)
				{
					parent.LocationStringInfo.AddError(Res.GetString("a3f49f1f-e7db-47d1-bb26-171724516050", "You cannot stocktake a Dock Door Location."));
				}
			}
		}

		#endregion

		#region CheckWS_StocktakeType

		protected override void CheckWS_StocktakeType()
		{
			base.CheckWS_StocktakeType();

			var stocktake = Parent;

			if (stocktake.IsNew && !stocktake.IsAutoCreatingStocktake)
			{
				MandatoryValidation.CheckEntered(stocktake.WS_StocktakeTypeInfo);
				ListValidation.ErrorIfInvalidCode(stocktake.WS_StocktakeTypeInfo, stocktake.Lookups.StocktakeTypes);
			}
		}

		#endregion

		#region ValidateAll

		public override void ValidateAll()
		{
			base.ValidateAll();
			Parent.Lines.ValidateDuplicateLine();
			ValidateLocationString();
		}

		#endregion

		#region Constants

		public static string CannotSelectInactiveWarehouse
		{
			get { return Res.GetString("708ddc16-87cf-4c00-a6cc-a32dc68e1ec6", "This warehouse is set to inactive and cannot be used in transactions"); }
		}
		public static string CannotSelectInactiveClient
		{
			get { return Res.GetString("6eb0578f-29c6-410b-8aa2-85f19c497ccd", "This client is set to inactive and cannot be used in transactions"); }
		}
		public static string CannotSelectWhenCountEmptyLocations
		{
			get { return Res.GetString("d9647a7c-358c-4a97-9576-a4a05259dd88", "Cannot be selected when the count includes empty locations"); }
		}

		#endregion

		#region ShouldValidateFKToCancelledRecord

		protected override bool ShouldValidateFKToCancelledRecord(ZPropertyInfo info)
			=> !FKsToNotValidateForCancelledRecords.Contains(info.Name) && base.ShouldValidateFKToCancelledRecord(info);

		static readonly ImmutableHashSet<string> FKsToNotValidateForCancelledRecords
			= ImmutableHashSet.Create(WhsStocktakeSchema.Constants.WS_WR_Row, WhsStocktakeSchema.Constants.WS_WA_Area, WhsStocktakeSchema.Constants.WS_WL_Location);

		#endregion
	}
}
