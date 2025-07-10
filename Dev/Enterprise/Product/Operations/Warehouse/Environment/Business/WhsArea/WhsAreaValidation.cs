using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Environment.Business
{
	public class WhsAreaValidation : AutoWhsAreaValidation
	{
		#region Constructor

		public WhsAreaValidation(AutoWhsArea parent)
			: base(parent)
		{
			this.Parent = (WhsArea)parent;
		}

		#endregion

		#region Parent

		protected readonly new WhsArea Parent;

		#endregion

		// non persistent

		#region ValidateWA_CalcMaxWeight

		public void ValidateWA_CalcMaxWeight()
		{
			ValidateCalculatedProperty(Parent.WA_CalcMaxWeightInfo);
		}

		protected virtual void CheckWA_CalcMaxWeight()
		{
			if (Parent.WeightAndVolumeHelper.HasMaxWeightWarningMessage)
			{
				Parent.WA_CalcMaxWeightInfo.AddWarning(Res.GetString("df25e4d4-d8cc-4087-bc3e-ec70930fc92a",
					"Cannot calculate Max Weight because the following Location(s) have invalid units:\r\n{0}", Parent.WeightAndVolumeHelper.MaxWeightWarningMessage));
			}
		}

		#endregion

		#region ValidateWA_CalcMaxVolume

		public void ValidateWA_CalcMaxVolume()
		{
			ValidateCalculatedProperty(Parent.WA_CalcMaxVolumeInfo);
		}

		protected virtual void CheckWA_CalcMaxVolume()
		{
			if (Parent.WeightAndVolumeHelper.HasMaxVolumeWarningMessage)
			{
				Parent.WA_CalcMaxVolumeInfo.AddWarning(Res.GetString("519a2d92-87bd-4e6e-9ce9-91243ef9fb71",
					"Cannot calculate Max Volume because the following Location(s) have invalid units:\r\n{0}", Parent.WeightAndVolumeHelper.MaxVolumeWarningMessage));
			}
		}

		#endregion

		#region ValidateWA_CalcCurrentWeight

		public void ValidateWA_CalcCurrentWeight()
		{
			ValidateCalculatedProperty(Parent.WA_CalcCurrentWeightInfo);
		}

		protected virtual void CheckWA_CalcCurrentWeight()
		{
			if (Parent.WeightAndVolumeHelper.HasWeightWarningMessage)
			{
				Parent.WA_CalcCurrentWeightInfo.AddWarning(Res.GetString("91d0d89c-fe6e-4e44-8925-1fd0fe261417",
					"Cannot calculate Weight because the following Product(s) have invalid units:\r\n{0}", Parent.WeightAndVolumeHelper.WeightWarningMessage));
			}
		}

		#endregion

		#region ValidateWA_CalcCurrentVolume

		public void ValidateWA_CalcCurrentVolume()
		{
			ValidateCalculatedProperty(Parent.WA_CalcCurrentVolumeInfo);
		}

		protected virtual void CheckWA_CalcCurrentVolume()
		{
			if (Parent.WeightAndVolumeHelper.HasVolumeWarningMessage)
			{
				Parent.WA_CalcCurrentVolumeInfo.AddWarning(Res.GetString("b6dceaba-7d73-4b37-a139-603507c779fc",
					"Cannot calculate Volume because the following Product(s) have invalid units:\r\n{0}", Parent.WeightAndVolumeHelper.VolumeWarningMessage));
			}
		}

		#endregion

		#region ValidateRFPickPackPrinterPK

		public void ValidateRFPickPackPrinterPK()
		{
			ValidateCalculatedProperty(Parent.RFPickPackPrinterPKInfo);
		}

		protected void CheckRFPickPackPrinterPK()
		{
			TypeValidation.CheckValidGuid(Parent.RFPickPackPrinterPKInfo);
		}

		#endregion

		#region Persistent

		#region CheckWA_WW_Whs

		protected override void CheckWA_WW_Whs()
		{
			base.CheckWA_WW_Whs();

			CheckIsUpdatingWarehouseForSavedWhsArea(Parent.WA_WW_WhsInfo);
			CheckAttachedWarehouseLocations(Parent.WA_WW_WhsInfo, Parent);
		}

		void CheckIsUpdatingWarehouseForSavedWhsArea(ZPropertyInfo info)
		{
			if (!info.HasErrors() && info.HasChanges)
			{
				info.AddError(Res.GetString("c6825f04-70bb-4125-82f3-8ebc3d47ad44", "Cannot update Warehouse for existing Areas."));
			}
		}

		static void CheckAttachedWarehouseLocations(ZPropertyInfo info, WhsArea parent)
		{
			if (!info.HasErrors())
			{
				if (parent.PickLocations.Cast<WhsLocation>().Any(l => l.WLV_WW_Whs != parent.WA_WW_Whs))
				{
					info.AddError(LocationsAttachedFromAnotherWarehouseErrorMsg);
				}
			}
		}

		#endregion

		#region CheckWA_Name

		protected override void CheckWA_Name()
		{
			base.CheckWA_Name();
			MandatoryValidation.CheckEntered(Parent.WA_NameInfo);
			TranslatableDataFieldAttribute.Validate(Parent.WA_NameInfo);
		}

		#endregion

		#region CheckWA_AreaType

		protected override void CheckWA_AreaType()
		{
			base.CheckWA_AreaType();
			MandatoryValidation.CheckEntered(Parent.WA_AreaTypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.WA_AreaTypeInfo, Parent.Lookups.AreaTypes);
			CheckWA_AreaType_DynamicPF_OnlyContainsDynamicLocations();
			CheckWA_AreaType_DyanmicPF_CannotBeChangedIfProductsAssigned();
			CheckWA_AreaType_Bonded_CannotBeChangedIfSOHOrHasTransitPackages();
			CheckWA_AreaType_InwardProcessing();
			CheckWA_AreaType_VATFiscal();
		}

		void CheckWA_AreaType_Bonded_CannotBeChangedIfSOHOrHasTransitPackages()
		{
			var previousType = (ZString)Parent.WA_AreaTypeInfo.OriginalValue;
			if (previousType != Parent.WA_AreaType && previousType == AreaTypes.Codes.Bonded)
			{
				var areaLocations = GetAreaLocationPKs().ToArray();
				if (ValidationHelper.CheckIfLocationsHaveCustomsStockOnHand(Parent.Factory, areaLocations))
				{
					Parent.WA_AreaTypeInfo.AddError(Res.GetString("492D992D-C142-43E8-BEFE-2FCEF93A489A", "Area Type cannot be changed from Bonded Area Type when the Area already contains inventory."));
				}
				if (ValidationHelper.CheckIfLocationsHaveTransitPackage(Parent.Factory, areaLocations))
				{
					Parent.WA_AreaTypeInfo.AddError(Res.GetString("3A17A5EA-80D7-403B-A845-D9EFA7129DD6", "Area Type cannot be changed from Bonded Area Type when the Area already contains packages."));
				}
			}
		}

		IEnumerable<ZGuid> GetAreaLocationPKs() => Parent.PickLocations.Concat(Parent.PutawayLocations).Select(location => location.PK).Distinct();

		void CheckWA_AreaType_DynamicPF_OnlyContainsDynamicLocations()
		{
			if (Parent.WA_AreaType == AreaTypes.Codes.DynamicPickFace
				&& Parent.PickLocations.Any(l => l.LocationType.WLT_LocationClass != AreaTypes.Codes.DynamicPickFace))
			{
				Parent.WA_AreaTypeInfo.AddError(Res.GetString("C0F6D38F-E9EE-486B-A9AD-344E1E8F8EB8", "Dynamic Pick Face Areas must only contain locations with DPF Location Class."));
			}
		}

		void CheckWA_AreaType_DyanmicPF_CannotBeChangedIfProductsAssigned()
		{
			var previousType = (ZString)Parent.WA_AreaTypeInfo.OriginalValue;
			if (previousType != Parent.WA_AreaType
				&& previousType == AreaTypes.Codes.DynamicPickFace
				&& Parent.Factory.LoadTop1<IWhsProductParamsByWhsAndClient>(new ZQuery(WhsProductParamsByWhsAndClientSchema.W3_WA_DynamicPickFaceArea, Parent.PK)) != null)
			{
				Parent.WA_AreaTypeInfo.AddError(Res.GetString("4FBEAD8E-2E22-4F98-A54B-E36B2C4CB94A", "Cannot change area type from {0} to {1}, as dynamic products have already been assigned.", previousType, Parent.WA_AreaType));
			}
		}

		void CheckWA_AreaType_InwardProcessing()
		{
			if (Parent.WA_AreaType == AreaTypes.Codes.InwardProcessing && !(Parent.Warehouse?.WW_IsVirtualWarehouse ?? false))
			{
				Parent.WA_AreaTypeInfo.AddError(Res.GetString("165c0045-6e09-4b9c-b0a4-e14082b8d943", "Inward Processing Areas must only be used in Virtual Warehouses."));
			}

			var previousType = (ZString)Parent.WA_AreaTypeInfo.OriginalValue;
			if (!Parent.WA_AreaTypeInfo.HasErrors() && previousType != Parent.WA_AreaType)
			{
				if (previousType == AreaTypes.Codes.InwardProcessing)
				{
					Parent.WA_AreaTypeInfo.AddError(Res.GetString("1ec41ea6-60c4-4bc0-b54a-1883f2118b1a", "Area Type cannot be changed from Inward Processing Area Type."));
				}
				else if (Parent.WA_AreaType == AreaTypes.Codes.InwardProcessing)
				{
					Parent.WA_AreaTypeInfo.AddError(Res.GetString("e48dae29-e1db-4af7-a3a7-69eb533d172d", "Area Type cannot be changed to Inward Processing Area Type."));
				}
			}
		}

		void CheckWA_AreaType_VATFiscal()
		{
			if (Parent.WA_AreaType == AreaTypes.Codes.VATFiscal)
			{
				if (Parent.Warehouse is WhsWarehouse warehouse &&
					!ObjectFactory.Get<Enterprise.Integration.Customs.Shared.IEuropeanUnionCustomsMembersProvider>().
					IsInEuropeanCustomsUnionOrInheritsFromEU(Enterprise.Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(warehouse.CountryCode)))
				{
					Parent.WA_AreaTypeInfo.AddError(Res.GetString("798C55B5-CD14-4A75-B57E-85AB6D69D214", "VAT Fiscal Areas must only be used in EU Warehouses."));
				}
			}
		}

		#endregion

		#region CheckWA_IsPickingAreaAndCheckWA_IsPutawayArea

		#region CheckWA_IsPickingArea

		protected override void CheckWA_IsPickingArea()
		{
			base.CheckWA_IsPickingArea();

			var info = Parent.WA_IsPickingAreaInfo;
			CheckBothFlagsAreDisabled(info, Parent.WA_IsPutawayArea);
			CheckReferencedObjectExistsForArea<WhsLocation>(info, WhsLocationViewSchema.WLV_WA_PickingArea,
				Res.GetString("c84448ff-ff0f-48e7-aaee-fd19fdfe7366", "Area is used as a picking area in location(s) hence cannot change it to a non-pick area."));
			CheckWA_IsPicking_DynamicPickFaceArea();
			CheckPickAreaAndDefaultPickAreaNotConflicting(info, Parent);
		}

		#endregion

		#region CheckWA_IsPutawayArea

		protected override void CheckWA_IsPutawayArea()
		{
			base.CheckWA_IsPutawayArea();

			var info = Parent.WA_IsPutawayAreaInfo;
			CheckBothFlagsAreDisabled(info, Parent.WA_IsPickingArea);
			CheckReferencedObjectExistsForArea<WhsLocation>(info, WhsLocationViewSchema.WLV_WA_PutawayArea,
				Res.GetString("6ede742e-54b2-440e-93cf-8ba85ce6e5dd", "Area is used as a putaway area in location(s) hence cannot change it to a non-putaway area."));
			CheckWA_IsPutAway_DynamicPickFaceArea();
			CheckPutAwayAreaAndDefaultPutAwayAreaNotConflicting(info, Parent);
		}

		#endregion

		#region CheckWA_IsDefaultPickArea

		protected override void CheckWA_IsDefaultPickArea()
		{
			base.CheckWA_IsDefaultPickArea();

			CheckPickAreaAndDefaultPickAreaNotConflicting(Parent.WA_IsDefaultPickAreaInfo, Parent);
		}

		#endregion

		#region CheckWA_IsDefaultPutawayArea

		protected override void CheckWA_IsDefaultPutawayArea()
		{
			base.CheckWA_IsDefaultPutawayArea();

			CheckPutAwayAreaAndDefaultPutAwayAreaNotConflicting(Parent.WA_IsDefaultPutawayAreaInfo, Parent);
		}

		#endregion

		static void CheckBothFlagsAreDisabled(ZPropertyInfo info, ZBool isOtherFlagEnabled)
		{
			if (!info.HasErrors() && !(ZBool)info.Value && !isOtherFlagEnabled)
			{
				info.AddError(Res.GetString("942340B4-8238-46CE-9F16-2684AC9EB432", "Area must at least be picking or putaway area."));
			}
		}

		void CheckReferencedObjectExistsForArea<T>(ZPropertyInfo info, SchemaGuidColumn column, string errorMessage) where T : class
		{
			if (!info.HasErrors() && !(ZBool)info.Value && Parent.Factory.LoadTop1<T>(new ZQuery(column, Parent.PK)) != null)
			{
				info.AddError(errorMessage);
			}
		}

		static void CheckPickAreaAndDefaultPickAreaNotConflicting(ZPropertyInfo info, WhsArea parent)
		{
			if (!info.HasErrors() && !parent.WA_IsPickingArea && parent.WA_IsDefaultPickArea)
			{
				info.AddError(Res.GetString("c568cacd-5454-4fd5-b9a9-6371c9df02c4", "Pick Area must be enabled when Default Pick Area is enabled."));
			}
		}

		static void CheckPutAwayAreaAndDefaultPutAwayAreaNotConflicting(ZPropertyInfo info, WhsArea parent)
		{
			if (!info.HasErrors() && !parent.WA_IsPutawayArea && parent.WA_IsDefaultPutawayArea)
			{
				info.AddError(Res.GetString("be8aa0f3-2408-42f2-88ee-c0e3764d4e26", "Putaway Area must be enabled when Default Putaway Area is enabled."));
			}
		}

		#region CheckWA_IsPicking_DynamicPickFaceArea

		public void CheckWA_IsPicking_DynamicPickFaceArea()
		{
			if (Parent.WA_AreaType == AreaTypes.Codes.DynamicPickFace && !Parent.WA_IsPickingArea)
			{
				Parent.WA_IsPickingAreaInfo.AddError(Res.GetString("1930473b-a6ae-4890-8aaf-002e6128c1d7", "Dynamic Pick Face Areas must be a picking area."));
			}
		}

		#endregion

		#region CheckWA_IsPutAway_DynamicPickFaceArea

		public void CheckWA_IsPutAway_DynamicPickFaceArea()
		{
			if (Parent.WA_AreaType == AreaTypes.Codes.DynamicPickFace && Parent.WA_IsPutawayArea)
			{
				Parent.WA_IsPutawayAreaInfo.AddError(Res.GetString("c6c398ed-52ed-472f-ab64-5cba0e2657b3", "Dynamic Pick Face Areas cannot be a putaway area."));
			}
		}

		#endregion

		#endregion

		#region LocationsAttachedFromAnotherWarehouseErrorMsg

		public static string LocationsAttachedFromAnotherWarehouseErrorMsg
		{
			get { return Res.GetString("5f6411ea-86b2-4e0f-9d46-a46eb5ee1dbb", "There are Locations attached to this Area which belong to another Warehouse. Detach these Locations before trying to change this Area's Warehouse. If you do not know which these Locations are, use the Locations search and filter by Area."); }
		}

		#endregion

		#endregion

		#region ValidateAll

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateWA_CalcCurrentWeight();
			ValidateWA_CalcCurrentVolume();
			ValidateWA_CalcMaxWeight();
			ValidateWA_CalcMaxVolume();
			ValidateRFPickPackPrinterPK();
		}

		#endregion
	}
}

