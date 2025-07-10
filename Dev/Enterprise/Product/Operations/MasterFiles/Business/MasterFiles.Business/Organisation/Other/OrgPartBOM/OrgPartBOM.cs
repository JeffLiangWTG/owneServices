using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	[CodeProperty(nameof(BOMDescription)), DescriptionProperty(nameof(BOMDescription))]
	public class OrgPartBOM : AutoOrgPartBOM
	{
		public OrgPartBOM(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			OE_ComponentQty = 1;
		}

		[List("Lookups.SubParts")]
		public override ZGuid OE_OP_Component
		{
			get { return base.OE_OP_Component; }
			set
			{
				base.OE_OP_Component = value;
				if (Component != null)
				{
					OE_F3_NKPackType = Component.OP_StockKeepingUnit;
				}
			}
		}

		[RelatedBusinessObjectTestExclude("This property is a ZString. However it's used as a foreign key. This attribute is added to pass UTs of OrgPartBomView ComponentPack")]
		[List("Lookups.ProductUQList")]
		public override ZString OE_F3_NKPackType
		{
			get
			{
				return base.OE_F3_NKPackType;
			}
			set
			{
				base.OE_F3_NKPackType = value;
			}
		}

		public ZString BOMDescription => $"{Component?.OP_PartNum} - {OE_F3_NKPackType}";

		[ResourceStringData("OrgPartBOM|ComponentDescription", Caption = "Description")]
		public ZString ComponentDescription => Component?.OP_Desc ?? ZString.Empty;

		public ZPropertyInfo ComponentDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(ComponentDescription)); }
		}

		public ZString ChildrenBomIndicator
		{
			get { return Component != null && Component.BillOfMaterials.Count > 0 ? "+" : ""; }
		}

		public ZPropertyInfo ChildrenBomIndicatorInfo
		{
			get { return GetZPropertyInfo(nameof(ChildrenBomIndicator)); }
		}

		public override ZPropertyInfo OE_F3_NKPackTypeInfo
		{
			get
			{
				base.OE_F3_NKPackTypeInfo.HumanReadableName = Res.GetString("a843ceb1-250e-435c-a269-816ed9427714", "Stock Unit");
				return base.OE_F3_NKPackTypeInfo;
			}
		}

		public override ZPropertyInfo OE_ComponentQtyInfo
		{
			get
			{
				base.OE_ComponentQtyInfo.HumanReadableName = Res.GetString("a00692af-504a-4770-ae9f-05754f46208a", "Quantity Per");
				return base.OE_ComponentQtyInfo;
			}
		}

		public override ZBool OE_ExcludeForVirtualWarehouse
		{
			get => base.OE_ExcludeForVirtualWarehouse;
			set
			{
				base.OE_ExcludeForVirtualWarehouse = value;
				if (!IsValidationSuspended)
				{
					var partBomsToCheck = MainProduct?.BillOfMaterials?.Where(p => p.PK != PK) ?? Enumerable.Empty<OrgPartBOM>();
					foreach (var partBom in partBomsToCheck)
					{
						partBom.Validation.ValidateOE_ExcludeForVirtualWarehouse();
					}
				}
			}
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new Strategy(this);
		}

		class Strategy : EnterpriseBusinessObjectFetchStrategy
		{
			public Strategy(OrgPartBOM component)
				: base(component)
			{
			}

			OrgPartBOM Component
			{
				get { return (OrgPartBOM)BusinessObject; }
			}

			protected override void FetchForLoadCore()
			{
				base.FetchForLoadCore();
				Factory.AddFetchHint(typeof(OrgSupplierPart), Component.OE_OP_MainProduct);
			}
		}
		#region Lookups

		protected override OrgPartBOMLookups GetNewLookups()
		{
			return new OrgPartBOMLookups(this);
		}

		#endregion

		public override bool CanDelete => base.CanDelete
			&& (!IsInDatabase || !OrgPartBOMValidation.IsKitBuiltOnSalesOrder(Factory, OE_OP_MainProduct));

		public override MultilingualString ReasonForNotAbleToDelete => OrgPartBOMValidation.PickOnSalesOrderDetected;
	}
}
