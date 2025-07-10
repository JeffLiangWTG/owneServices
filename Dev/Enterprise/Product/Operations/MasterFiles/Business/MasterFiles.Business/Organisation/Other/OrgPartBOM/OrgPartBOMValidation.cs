//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoOrgPartBOMValidation
//
//    This class should be used for overriding validation in AutoOrgPartBOMValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------
using System.Collections;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Integration.Warehouse;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class OrgPartBOMValidation : AutoOrgPartBOMValidation
	{
		public OrgPartBOMValidation(AutoOrgPartBOM parent)
			: base(parent)
		{
		}

		#region Properties

		#region IsSameBOMExists

		bool IsSameBOMExists
		{
			get
			{
				if (bom.MainProduct != null)
				{
					foreach (var bomPart in bom.MainProduct.BillOfMaterials)
					{
						if (bomPart.OE_OP_Component == bom.OE_OP_Component && bomPart.OE_F3_NKPackType == bom.OE_F3_NKPackType && bom.PK != bomPart.PK)
						{
							return true;
						}
					}
				}
				return false;
			}
		}

		#endregion

		#region bom

		OrgPartBOM bom => (OrgPartBOM)Parent;

		#endregion

		#endregion

		#region CheckOE_ComponentQty

		protected override void CheckOE_ComponentQty()
		{
			base.CheckOE_ComponentQty();
			MandatoryValidation.CheckEntered(bom.OE_ComponentQtyInfo, Res.GetString("62709a9a-2e72-4451-aa90-98bb490c1c05", "Quantity Per"));
			MandatoryValidation.CheckNotNegative(bom.OE_ComponentQtyInfo, Res.GetString("62709a9a-2e72-4451-aa90-98bb490c1c05", "Quantity Per"));
			CheckIfMasterProductBuiltOnSalesOrder(bom.OE_ComponentQtyInfo);
		}

		#endregion

		#region CheckOE_F3_NKPackType

		protected override void CheckOE_F3_NKPackType()
		{
			base.CheckOE_F3_NKPackType();
			MandatoryValidation.CheckEntered(bom.OE_F3_NKPackTypeInfo, Res.GetString("1c882065-2561-404b-ac34-8882ec35c859", "Stock Unit"));
			ListValidation.ErrorIfInvalidCode(bom.OE_F3_NKPackTypeInfo, ResString.GetMultilingualString("1c882065-2561-404b-ac34-8882ec35c859", "Stock Unit"));
			AddErrorIfSameBOM(bom.OE_F3_NKPackTypeInfo);

			if (!bom.OE_F3_NKPackTypeInfo.ReadOnly && !bom.OE_F3_NKPackTypeInfo.HasErrors())
			{
				if (bom.Component != null) // when component product is invalid
				{
					if (!bom.Component.UnitConverter.Convertible(bom.OE_F3_NKPackType, bom.Component.OP_StockKeepingUnit))
					{
						bom.OE_F3_NKPackTypeInfo.AddError(Res.GetString("7bc3600c-fc83-48b2-9417-bf7a7b26023e", "Please specify a unit conversion for this pack type."));
					}
				}
				CheckIfMasterProductBuiltOnSalesOrder(bom.OE_F3_NKPackTypeInfo);
			}
		}

		#endregion

		#region CheckOE_OP_Component

		protected override void CheckOE_OP_Component()
		{
			base.CheckOE_OP_Component();

			var component = bom.Component;
			if (component != null)
			{
				var mainProduct = bom.MainProduct;
				if (IsCircularReference(new Stack(), mainProduct))
				{
					bom.OE_OP_ComponentInfo.AddError(circularReferenceError);
				}

				if ((OrgPartBOMValidationHelper.DoesProductHaveOwner(mainProduct) || OrgPartBOMValidationHelper.DoesProductHaveOwner(component))
					&& !OrgPartBOMValidationHelper.DoCommonOwnersExist(mainProduct, component))
				{
					bom.OE_OP_ComponentInfo.AddError(Res.GetString("c7a3ee7f-a666-4412-8105-d8748cf22a8e", "All Components added to this Product should have at least one common owner, or neither the Component nor the Product should have an owner relationship specified."));
				}

				AddErrorIfSameBOM(bom.OE_OP_ComponentInfo);

				if (OrgPartBOMValidationHelper.IsProductComponentOnMainProductWithIsPickOnOrder(mainProduct))
				{
					bom.OE_OP_ComponentInfo.AddError(ParentHasIsPickOnOrderParent);
				}

				if (mainProduct.OP_IsComponentPickedOnSalesOrder && component.BillOfMaterials.Count > 0)
				{
					bom.OE_OP_ComponentInfo.AddError(CannotAddThisProductAsComponent);
				}
			}
			CheckIfMasterProductBuiltOnSalesOrder(bom.OE_OP_ComponentInfo);
		}

		static bool IsCircularReference(Stack references, OrgSupplierPart part)
		{
			if (part != null)
			{
				if (references.Contains(part))
				{
					return true;
				}
				else
				{
					references.Push(part);

					foreach (OrgPartBOM bom in part.BillOfMaterials)
					{
						if (IsCircularReference(references, bom.Component))
						{
							return true;
						}
					}

					references.Pop();
				}
			}

			return false;
		}

		#endregion

		protected override void CheckOE_ExcludeForVirtualWarehouse()
		{
			base.CheckOE_ExcludeForVirtualWarehouse();
			if (Parent.OE_ExcludeForVirtualWarehouse
				&& !Parent.OE_ExcludeForVirtualWarehouseInfo.HasErrors()
				&& (Parent?.MainProduct?.BillOfMaterials.All(p => p.OE_ExcludeForVirtualWarehouse) ?? false))
			{
				Parent.OE_ExcludeForVirtualWarehouseInfo.AddError(Res.GetString("b98faf6c-a7d8-41c0-86f3-c7502426ac42", "Cannot have all Components excluded in the BOM composition for Virtual Warehouse."));
			}
		}

		#region AddErrorIfSameBOM

		void AddErrorIfSameBOM(ZPropertyInfo propertyInfo)
		{
			if (IsSameBOMExists)
			{
				propertyInfo.AddError(Res.GetString("3a5df103-2efe-456c-9b9a-d9b2ed37d44d", "This component already exists in the BOM list."));
			}
		}

		#endregion

		#region circularReferenceError

		public static string circularReferenceError => Res.GetString("09aeacb5-8e16-48b8-8135-d989b764013e", "Circular reference error. There are sub-part(s) that reference this Component.");

		#endregion

		void CheckIfMasterProductBuiltOnSalesOrder(ZPropertyInfo propertyInfo)
		{
			if (!propertyInfo.HasErrors() && (propertyInfo.HasChanges || !Parent.IsInDatabase))
			{
				var mainProduct = Parent.MainProduct;
				if (mainProduct != null)
				{
					if (IsKitBuiltOnSalesOrder(Parent.Factory, mainProduct.PK))
					{
						propertyInfo.AddError(PickOnSalesOrderDetected);
					}
				}
			}
		}

		public static bool IsKitBuiltOnSalesOrder(BusinessObjectFactory factory, ZGuid mainProductPK)
		{
			return ObjectFactory.Get<IWhsPickOnSalesOrderDetector>().IsKitBuiltOnSalesOrder(factory, mainProductPK);
		}

		public static bool IsComponentUsedToBuiltKitOnSalesOrder(BusinessObjectFactory factory, ZGuid productPK)
		{
			return ObjectFactory.Get<IWhsPickOnSalesOrderDetector>().IsComponentUsedToBuiltKitOnSalesOrder(factory, productPK);
		}

		#region Error messages

		public static MultilingualString ParentHasIsPickOnOrderParent => ResString.GetMultilingualString("d5ae67cf-5382-4b48-a8c3-5eab52b881b3", "Component can not be added to this product, because its main product is set to 'Can Pick without Work Order'");

		public static MultilingualString CannotAddThisProductAsComponent => ResString.GetMultilingualString("74f283cf-ac7a-4b70-924a-702dbb9811b7", "Component can not be added to this product, because it has child components.");

		public static MultilingualString PickOnSalesOrderDetected => ResString.GetMultilingualString("d2becaf9-c201-42c3-8f7b-2d8a16396bbb", "Cannot change the BOM composition for this product.\r\nThis product has kits that were picked on sales orders without Work Orders and are not yet finalized. To ensure the integrity of these kits, the BOM composition cannot be changed, please finalize these Picks first.");

		#endregion
	}
}
