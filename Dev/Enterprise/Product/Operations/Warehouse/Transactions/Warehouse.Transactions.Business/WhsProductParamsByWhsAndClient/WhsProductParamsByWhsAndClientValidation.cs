using System.Collections.Immutable;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsProductParamsByWhsAndClientValidation : AutoWhsProductParamsByWhsAndClientValidation
	{
		public WhsProductParamsByWhsAndClientValidation(AutoWhsProductParamsByWhsAndClient parent)
			: base(parent)
		{
		}

		#region Parent

		protected new WhsProductParamsByWhsAndClient Parent
		{
			get { return (WhsProductParamsByWhsAndClient)base.Parent; }
		}

		#endregion

		#region ValidateAll

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidatePickGroup();
		}

		#endregion

		// Calculated

		#region ValidatePickGroup

		public void ValidatePickGroup()
		{
			ValidateCalculatedProperty(Parent.PickGroupForBindingInfo);
		}

		protected void CheckPickGroupForBinding()
		{
			if (!Parent.W3_PickGroup.IsEmpty && !Parent.Lookups.PickGroups.ContainsCode(Parent.W3_PickGroup))
			{
				var errorMessage = ListValidation.GetNotificationMessage(Parent.W3_PickGroupInfo).ToString();
				Parent.PickGroupForBindingInfo.AddError(errorMessage);
			}
		}

		#endregion

		// Persistent

		#region W3_OH

		protected override void CheckW3_OH()
		{
			base.CheckW3_OH();
			ListValidation.ErrorIfInvalidPK(Parent.W3_OHInfo);
			CheckDuplicateEntries(Parent.W3_OHInfo);
			CheckW3_OH_HaveRelationship();
			Helper.CheckCannotBeModifiedIfJulianBatchNumberAttributeUsedAndHasStock(Parent, Parent.W3_OHInfo);

			ValidateW3_WW();
		}

		void CheckW3_OH_HaveRelationship()
		{
			var supplierPart = Parent.SupplierPart;
			if (supplierPart != null)
			{
				if (supplierPart.RelatedOrganisations.FindByOrganisationPKAndRelationship(Parent.W3_OH, OrgPartRelation.RelationshipTypes.Owner) == null)
				{
					Parent.W3_OHInfo.AddError(Res.GetString("3e2eb80d-1045-475c-85be-fcc2eb09205a", "This organization has no relationship with this product. You can create relationships in the Related Organizations tab on this form"));
				}
			}
		}

		#endregion

		#region W3_WW

		protected override void CheckW3_WW()
		{
			base.CheckW3_WW();
			ListValidation.ErrorIfInvalidPK(Parent.W3_WWInfo);
			MandatoryValidation.CheckEntered(Parent.W3_WWInfo);
			CheckDuplicateEntries(Parent.W3_WWInfo);
			Helper.CheckCannotBeModifiedIfJulianBatchNumberAttributeUsedAndHasStock(Parent, Parent.W3_WWInfo);
			ValidateW3_OH();
		}

		#endregion

		#region W3_OP

		protected override void CheckW3_OP()
		{
			base.CheckW3_OP();
			MandatoryValidation.CheckEntered(Parent.W3_OPInfo);
		}

		#endregion

		#region W3_StockTakeCycle

		protected override void CheckW3_StockTakeCycle()
		{
			base.CheckW3_StockTakeCycle();
			ListValidation.ErrorIfInvalidCode(Parent.W3_StockTakeCycleInfo, Parent.Lookups.StockTakeCycles);
		}

		#endregion

		#region W3_EconomicQuantity

		protected override void CheckW3_EconomicQuantity()
		{
			base.CheckW3_EconomicQuantity();
			MandatoryValidation.CheckNotNegative(Parent.W3_EconomicQuantityInfo);
			if (!Parent.W3_EconomicQuantityInfo.HasErrors())
			{
				if (Parent.W3_ReplenishmentMinimum > 0m && Parent.W3_EconomicQuantity <= Parent.W3_ReplenishmentMinimum)
				{
					Parent.W3_EconomicQuantityInfo.AddError(Res.GetString("90accbcd-8275-4985-87f4-bbb1fb1bd319", "Economic Quantity must be greater than or equal to Replenishment Minimum"));
				}
				else if (Parent.W3_EconomicQuantity < Parent.W3_ReplenishmentMultiple)
				{
					Parent.W3_EconomicQuantityInfo.AddError(Res.GetString("dc772855-5368-448c-acc8-903799bbbca7", "Economic Quantity must be greater than or equal to Replenishment Multiple"));
				}
			}
		}

		#endregion

		#region W3_ReplenishmentMinimum

		protected override void CheckW3_ReplenishmentMinimum()
		{
			base.CheckW3_ReplenishmentMinimum();
			MandatoryValidation.CheckNotNegative(Parent.W3_ReplenishmentMinimumInfo);
			if (!Parent.W3_ReplenishmentMinimumInfo.HasErrors() && Parent.W3_ReplenishmentMinimum > 0m && Parent.W3_ReplenishmentMinimum >= Parent.W3_EconomicQuantity)
			{
				Parent.W3_ReplenishmentMinimumInfo.AddError(Res.GetString("c94cc07b-0c2c-49df-a743-5d151dac7748", "Replenishment Minimum must be less than Economic Quantity"));
			}
		}

		#endregion

		#region W3_ReplenishmentMultiple

		protected override void CheckW3_ReplenishmentMultiple()
		{
			base.CheckW3_ReplenishmentMultiple();
			MandatoryValidation.CheckNotNegative(Parent.W3_ReplenishmentMultipleInfo);
			if (!Parent.W3_ReplenishmentMinimumInfo.HasErrors())
			{
				if (Parent.W3_EconomicQuantity > 0 && Parent.W3_ReplenishmentMultiple == 0)
				{
					Parent.W3_ReplenishmentMultipleInfo.AddError(Res.GetString("fe24b86e-9dd3-4dfd-b068-2f3b5f080344", "Please enter a Replenishment Multiple greater than or equal to 1"));
				}
				else if (Parent.W3_ReplenishmentMultiple > Parent.W3_EconomicQuantity)
				{
					Parent.W3_ReplenishmentMultipleInfo.AddError(Res.GetString("dc772855-5368-448c-acc8-903799bbbca7", "Economic Quantity must be greater than or equal to Replenishment Multiple"));
				}
			}
		}

		#endregion

		#region W3_ExpiryNotificationPeriod

		protected override void CheckW3_ExpiryNotificationPeriod()
		{
			base.CheckW3_ExpiryNotificationPeriod();
			MandatoryValidation.CheckNotNegative(Parent.W3_ExpiryNotificationPeriodInfo);
		}

		#endregion

		#region W3_WA_StagingLocationBOM

		protected override void CheckW3_WL_StagingLocationBOM()
		{
			base.CheckW3_WL_StagingLocationBOM();

			var stagingLocationBOM = Parent.StagingLocationBOM;
			var warehouse = Parent.Warehouse;
			if (stagingLocationBOM != null && warehouse != null)
			{
				if (stagingLocationBOM.WLV_WW_Whs != warehouse.PK)
				{
					AddLocationNotInWarehouseError(Parent.W3_WL_StagingLocationBOMInfo, stagingLocationBOM, warehouse);
				}
				else if (stagingLocationBOM.IsDockDoorLocation)
				{
					Parent.W3_WL_StagingLocationBOMInfo.AddError(
						Res.GetString("1945C33C-B86D-4B0B-9600-DC55319C530E", "Please enter a non Dock Door Location."));
				}
				else if (stagingLocationBOM.IsInBondedArea)
				{
					Parent.W3_WL_StagingLocationBOMInfo.AddError(
						Res.GetString("5054A441-675C-4F86-ACD1-84F25F6B621A", "Staging Locations cannot be in Bonded Areas."));
				}
				else if (stagingLocationBOM.IsInInwardProcessingArea)
				{
					Parent.W3_WL_StagingLocationBOMInfo.AddError(
						Res.GetString("8716bfe8-bbe4-4f6e-8b32-5c3a9497388b", "Staging Locations cannot be in Inward Processing Areas."));
				}
			}
		}

		static void AddLocationNotInWarehouseError(ZPropertyInfo info, WhsLocation stagingLocationBOM, WhsWarehouse warehouse)
		{
			info.AddError(Res.GetString("F9462138-B932-4DE2-960D-3BEE0E54D70B",
					"Location {0} does not exist in Warehouse {1}.", stagingLocationBOM.WLV_LocationString, warehouse.WW_WarehouseNameMultilingual));
		}

		#endregion

		protected override void CheckW3_WL_InwardsProcessingStagingLocationBOM()
		{
			base.CheckW3_WL_InwardsProcessingStagingLocationBOM();

			var inwardStagingLocationBOM = Parent.InwardProcessingStagingLocationBOM;
			var warehouse = Parent.Warehouse;
			if (inwardStagingLocationBOM != null && warehouse != null)
			{
				if (inwardStagingLocationBOM.WLV_WW_Whs != warehouse.PK)
				{
					AddLocationNotInWarehouseError(Parent.W3_WL_InwardsProcessingStagingLocationBOMInfo, inwardStagingLocationBOM, warehouse);
				}
				else if (!inwardStagingLocationBOM.IsInInwardProcessingArea)
				{
					Parent.W3_WL_InwardsProcessingStagingLocationBOMInfo.AddError(
						Res.GetString("706d35e6-85a7-4673-b980-fda1948d9568", "Inward Processing Staging Locations must be in Inward Processing Areas."));
				}
			}
		}

		#region W3_WA_DynamicPickFaceArea

		protected override void CheckW3_WA_DynamicPickFaceArea()
		{
			base.CheckW3_WA_DynamicPickFaceArea();

			if (Parent.DynamicPickFaceArea != null && Parent.Warehouse != null)
			{
				if (Parent.DynamicPickFaceArea.Warehouse != Parent.Warehouse)
				{
					Parent.W3_WA_DynamicPickFaceAreaInfo.AddError(Res.GetString("00a64a1e-4db1-4695-a3e4-6d9015f46cf7",
						"Area {0} does not exist in Warehouse {1}.", Parent.DynamicPickFaceArea.WA_NameMultilingual, Parent.Warehouse.WW_WarehouseNameMultilingual));
				}
				else if (!Parent.DynamicPickFaceArea.WA_AreaType.Equals(Environment.CodeLists.AreaTypes.Codes.DynamicPickFace))
				{
					Parent.W3_WA_DynamicPickFaceAreaInfo.AddError(Res.GetString("2a359972-868d-4b96-99ad-17a2fa63c77e",
						"Area {0} is not a dynamic picking area.", Parent.DynamicPickFaceArea.WA_NameMultilingual));
				}
				else if (Parent.Product.PickFaces.Any(pf => pf.Location != null && pf.Location.Warehouse.PK == Parent.Warehouse.PK && pf.Client.PK == Parent.Client.PK))
				{
					Parent.W3_WA_DynamicPickFaceAreaInfo.AddError(Res.GetString("f1c56263-0d78-4752-bf0a-ab13aa71a780",
						"Product is also assigned to a fixed pick face."));
				}
				else if (Parent.DynamicPickFaceArea.PickLocations.Count == 0)
				{
					Parent.W3_WA_DynamicPickFaceAreaInfo.AddError(Res.GetString("f11f5c19-0f21-41bd-82a5-c98433100117",
						"Dynamic area {0} contains no locations.", Parent.DynamicPickFaceArea.WA_NameMultilingual));
				}
			}
		}

		#endregion

		#region W3_F3_NKReceivedPackType, W3_F3_NKReleasedPackType

		protected override void CheckW3_F3_NKReceivedPackType()
		{
			base.CheckW3_F3_NKReceivedPackType();
			ListValidation.ErrorIfInvalidCode(Parent.W3_F3_NKReceivedPackTypeInfo);
			CheckThereIsAValidUnitConverterForPackType(Parent.W3_F3_NKReceivedPackTypeInfo);
		}

		protected override void CheckW3_F3_NKReleasedPackType()
		{
			base.CheckW3_F3_NKReleasedPackType();
			ListValidation.ErrorIfInvalidCode(Parent.W3_F3_NKReleasedPackTypeInfo);
			{
				CheckThereIsAValidUnitConverterForPackType(Parent.W3_F3_NKReleasedPackTypeInfo);
			}
		}

		void CheckThereIsAValidUnitConverterForPackType(ZPropertyInfo info)
		{
			ZString packType = info.Value.ToString();

			if (!packType.IsEmpty && Parent.SupplierPart != null)
			{
				if (!Parent.SupplierPart.UnitConverter.Convertible(Parent.SupplierPart.OP_StockKeepingUnit, packType))
				{
					info.AddWarning(OrgPartRelation.NoClientUQConverterError.Replace("Client UQ", info.Description));
				}
			}
		}

		#endregion

		#region CheckW3_MaximumShelfLife

		protected override void CheckW3_MaximumShelfLife()
		{
			base.CheckW3_MaximumShelfLife();

			MandatoryValidation.CheckNotNegative(Parent.W3_MaximumShelfLifeInfo);

			CheckW3_MaximumShelfLife_JulianBatchNumberUsedAndHasStock();
			CheckW3_MaximumShelfLife_CannotBeLessThenConsigneeMinShelfLifeAccepted();
		}

		#region CheckW3_MaximumShelfLife_CannotBeZeroIfJulianBatchNumberUsedAndHasStock

		void CheckW3_MaximumShelfLife_JulianBatchNumberUsedAndHasStock()
		{
			if (!Parent.W3_MaximumShelfLifeInfo.HasErrors())
			{
				var errorMsg = Helper.CheckMaximumShelfLifeIsValidWhenHasStock(Parent, Parent.W3_MaximumShelfLife);
				if (!errorMsg.IsEmpty)
				{
					Parent.W3_MaximumShelfLifeInfo.AddError(errorMsg);
				}
			}
		}

		#endregion

		#region CheckW3_MaximumShelfLife_CannotBeLessThenConsigneeMinShelfLifeAccepted

		void CheckW3_MaximumShelfLife_CannotBeLessThenConsigneeMinShelfLifeAccepted()
		{
			if (!Parent.W3_MaximumShelfLifeInfo.HasErrors() && Parent.W3_MaximumShelfLife > 0 && Parent.Client != null)
			{
				var consigneeMinShelfLifeAccepted = Parent.SupplierPart.RelatedOrganisations.FindByOrganisationPKAndRelationship(Parent.Client.PK, OrgPartRelation.RelationshipTypes.Owner)?.OU_ConsigneeMinShelfLifeAccepted;
				if (consigneeMinShelfLifeAccepted != null)
				{
					var errorMsg = Helper.CheckMaximumShelfLifeIsLessThanConsigneeMinShelfLifeAccepted(consigneeMinShelfLifeAccepted.Value, Parent.W3_MaximumShelfLife);
					if (!errorMsg.IsEmpty)
					{
						Parent.W3_MaximumShelfLifeInfo.AddError(errorMsg);
					}
				}
			}
		}

		#endregion

		#endregion

		#region CheckDuplicateEntries

		void CheckDuplicateEntries(ZPropertyInfo info)
		{
			if (Parent.SupplierPart != null)
			{
				foreach (WhsProductParamsByWhsAndClient param in Parent.Product.ParamsByWhsAndClient)
				{
					if (param.PK != Parent.PK && param.W3_OH == Parent.W3_OH && param.W3_WW == Parent.W3_WW)
					{
						info.AddError(Res.GetString("3a297ced-fea4-446e-814a-9d8f4a80bbc1", "This is a duplicate entry. There is already an entry for this Client / Warehouse"));
						break;
					}
				}
			}
		}

		#endregion

		#region ShouldValidateFKToCancelledRecord

		protected override bool ShouldValidateFKToCancelledRecord(ZPropertyInfo info)
			=> !FKsToNotValidateForCancelledRecords.Contains(info.Name) && base.ShouldValidateFKToCancelledRecord(info);

		static readonly ImmutableHashSet<string> FKsToNotValidateForCancelledRecords
			= ImmutableHashSet.Create(WhsProductParamsByWhsAndClientSchema.Constants.W3_WA_DynamicPickFaceArea,
				WhsProductParamsByWhsAndClientSchema.Constants.W3_WL_StagingLocationBOM,
				WhsProductParamsByWhsAndClientSchema.Constants.W3_WPG_PutawayGroup,
				WhsProductParamsByWhsAndClientSchema.Constants.W3_WL_InwardsProcessingStagingLocationBOM,
				WhsProductParamsByWhsAndClientSchema.Constants.W3_F3_NKReceivedPackType,
				WhsProductParamsByWhsAndClientSchema.Constants.W3_F3_NKReleasedPackType);

		#endregion

		#region Helper

		WhsProductParamsByWhsAndClientValidationHelper Helper => helper ?? (helper = new WhsProductParamsByWhsAndClientValidationHelper());
		WhsProductParamsByWhsAndClientValidationHelper helper;

		#endregion
	}
}
