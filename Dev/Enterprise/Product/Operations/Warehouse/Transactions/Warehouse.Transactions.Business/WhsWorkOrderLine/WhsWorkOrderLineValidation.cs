using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsWorkOrderLineValidation : WhsComponentOrderLineValidation
	{
		public WhsWorkOrderLineValidation(WhsWorkOrderLine parent)
			: base(parent)
		{
		}

		protected new WhsWorkOrderLine Parent => (WhsWorkOrderLine)base.Parent;

		#region WE_OP

		protected override void CheckWE_OP()
		{
			base.CheckWE_OP();

			if (!Parent.WE_OPInfo.HasErrors())
			{
				if (!Parent.WE_OPInfo.ReadOnly)
				{
					ListValidation.ErrorIfInvalidPK(Parent.WE_OPInfo, Parent.Lookups.SupplierParts);
					CheckWE_OP_ForProductComponentQuantity();
					CheckWE_OP_ProductComponentHasConvertablePackType();
					CheckWE_OP_ForDisassembly();
					CheckWE_OP_ComponentLinesCreated();
				}
				else if (!(Parent.IsFinalised && (Parent.WorkOrder.Pick?.IsFinalised ?? false)))
				{
					CheckWE_OP_ForBOMExistence();
				}
			}
		}

		void CheckWE_OP_ForProductComponentQuantity()
		{
			if (Parent.SupplierPart != null && Parent.BillOfMaterials.Any(x => x.OE_ComponentQty <= 0))
			{
				Parent.WE_OPInfo.AddError(Res.GetString("559b2ba9-d51b-4c5c-8b99-d815f72ca24a", "This product has a component with an invalid quantity."));
			}
		}

		void CheckWE_OP_ProductComponentHasConvertablePackType()
		{
			foreach (var componentLine in Parent.BOM.ChildComponentLines)
			{
				var componentSupplierPart = componentLine.SupplierPart;
				if (!componentSupplierPart.UnitConverter.Convertible(componentLine.WE_F3_NKPackType, componentSupplierPart.OP_StockKeepingUnit))
				{
					Parent.WE_OPInfo.AddError(Res.GetString("265e700c-c1bc-4445-914b-f125c754dcfb", "Cannot convert component stock keeping unit {0} to {1}. Please add unit conversion for component {2} or change stock keeping unit.",
						componentSupplierPart.OP_StockKeepingUnit, componentLine.WE_F3_NKPackType, componentSupplierPart.OP_PartNum));
				}
			}
		}

		void CheckWE_OP_ForDisassembly()
		{
			if (!Parent.WE_OPInfo.HasErrors() && !Parent.WorkOrder.IsAssembly)
			{
				if (!Parent.SupplierPart.OP_CanDisassembleKit)
				{
					Parent.WE_OPInfo.AddError(Res.GetString("f029b1ef-0152-48e5-aa6b-49373a0d56e0", "This Product is flagged to prevent Disassembly and thus it cannot be Disassembled."));
				}
				else if (IsSecondaryParts(Parent.SupplierPart))
				{
					Parent.WE_OPInfo.AddError(Res.GetString("b5c8f6c3-4b13-4383-b409-a03da501e500", "Cannot select Secondary Product for disassembly work order."));
				}
			}
		}

		bool IsSecondaryParts(OrgSupplierPart product)
		{
			var query = new ZQuery(OrgSecondaryPartBOMSchema.OSB_OP_SecondaryProduct, product.PK);
			return Factory.Exists(typeof(OrgSecondaryPartBOM), query);
		}

		void CheckWE_OP_ComponentLinesCreated()
		{
			if (IsValidatingAll.IsSuspended && !Parent.WE_OPInfo.HasErrors() && Parent.BOM.ChildComponentLines.Count == 0)
			{
				// will not exist if the user selects a non-BOM product, then edits the product to make it a BOM product.
				Parent.WE_OPInfo.AddError(Res.GetString("f6e170ed-afe5-4278-b5d7-a234a8d365c8", "No component lines exist. The product may have been recently edited -- try re-adding this line."));
			}
		}

		void CheckWE_OP_ForBOMExistence()
		{
			if (Parent.ParentLine == null && Parent.SupplierPart != null && Parent.BillOfMaterials.Count == 0)
			{
				Parent.WE_OPInfo.AddError(Res.GetString("e8aa845a-799a-4b2f-b616-93f8dadda04e", "This product does not have components."));
			}
		}

		#endregion

		#region ValidateWE_TransactionQuantity

		protected override void CheckWE_TransactionQuantity()
		{
			base.CheckWE_TransactionQuantity();

			var docket = Parent.Docket;
			if (docket != null && !Parent.WE_TransactionQuantityInfo.HasErrors())
			{
				PartAttributeValidation.CheckQtyForSerialNumber(Parent.Product, docket.Client, (ZPropertyInfoDecimal)Parent.WE_TransactionQuantityInfo, Parent.WE_SerialNumberInfo);
			}
		}

		#endregion

		#region WE_ShortfallQuantityCached

		protected override string ShortfallWarning
		{
			get
			{
				string result;

				if (Parent.BOM.IsTopLevelProduct || Parent.ParentLine == null)
				{
					if (Parent.WorkOrder.IsAssembly)
					{
						result = Res.GetString("21bc278c-45e4-4f33-a7d2-f7e06a158a8d",
							"More components are required to build {0} {1}(s).",
							Parent.WE_TransactionQuantity.ToString(0), Parent.SupplierPart.OP_Desc);
					}
					else // disassembly
					{
						result = Res.GetString("97ef8339-6235-4c1e-8342-eecb8b8f6045",
							"More components are required to disassemble {0} {1}(s).",
							Parent.WE_TransactionQuantity.ToString(0), Parent.SupplierPart.OP_Desc);
					}
				}
				else
				{
					result = Res.GetString("a4b9052e-8ff2-4b76-95f6-6b26dc3d6c7b",
						"There are not enough {0}(s) in stock to build the required number of {1}(s).",
						Parent.SupplierPart.OP_Desc, Parent.ParentLine.SupplierPart.OP_Desc);
				}

				return result;
			}
		}

		#endregion

		#region IsJulianBatchNumberFormatValidationRequired

		protected override bool IsJulianBatchNumberFormatValidationRequired(ZPropertyInfo partAttributeInfo, int attributeNumber)
		{
			return !partAttributeInfo.Value.IsEmpty && Parent.BOM.IsTopLevelProduct;
		}

		#endregion

		#region ValidateAll

		public override void ValidateAll()
		{
			using (new SemaphoreManager(IsValidatingAll))
			{
				base.ValidateAll();
				CheckProductComponentDefinitions();
			}
		}

		void CheckProductComponentDefinitions()
		{
			var line = Parent;
			if (!line.WE_TransactionQuantityInfo.ReadOnly && line.WE_TransactionQuantity != 0)
			{
				if (line.ProductDefinitionDoesNotMatchDocketLineProductDefinition())
				{
					line.AddRowError(Res.GetString("e0acc650-d1fa-40ea-86be-c95fe9a63945", "Component product definition has been changed. Please delete this line and add it again."));
				}
			}
		}

		Semaphore IsValidatingAll
		{
			get { return isValidatingAll ?? (isValidatingAll = new Semaphore()); }
		}

		Semaphore isValidatingAll;

		#endregion
	}
}
