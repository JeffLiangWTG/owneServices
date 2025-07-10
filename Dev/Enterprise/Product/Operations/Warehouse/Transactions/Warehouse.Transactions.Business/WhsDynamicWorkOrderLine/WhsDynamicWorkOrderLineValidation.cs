using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsDynamicWorkOrderLineValidation : WhsComponentOrderLineValidation
	{
		public WhsDynamicWorkOrderLineValidation(WhsComponentOrderLine parent)
			: base(parent)
		{
		}

		#region Parent

		public new WhsDynamicWorkOrderLine Parent => (WhsDynamicWorkOrderLine)base.Parent;

		#endregion

		#region Docket

		WhsDynamicWorkOrder Docket => (WhsDynamicWorkOrder)Parent?.Docket;

		#endregion

		#region CheckWE_OP

		protected override void CheckWE_OP()
		{
			base.CheckWE_OP();
			CheckPartMustNotBeBOM();
			CheckNoDuplicationOfComponents();
		}

		void CheckPartMustNotBeBOM()
		{
			if (!Parent.WE_OPInfo.HasErrors())
			{
				if (!Parent.WE_WE_ParentDocketLine.IsValid
					&& Parent.IsBOMProduct)
				{
					var customsData = Parent.CustomsData;
					if (customsData.WB_IsMainInwardsProcessedItem)
					{
						Parent.WE_OPInfo.AddError(Res.GetString("b55c7786-8623-47c2-8410-1ff3beb66a7a", "Main Product cannot be a Bill of Materials."));
					}
					else if (customsData.WB_IsSecondaryInwardsProcessedItem)
					{
						Parent.WE_OPInfo.AddError(Res.GetString("cddd66b2-2095-472b-98da-8afb9c1347db", "Secondary Product cannot be a Bill of Materials."));
					}
				}
			}
		}

		void CheckNoDuplicationOfComponents()
		{
			if (!Parent.WE_OPInfo.HasErrors()
				&& Parent.WE_WE_ParentDocketLine.IsValid
				&& Parent.ParentLine.ChildComponentLinesCollection.Any(l => l.PK != Parent.PK && l.WE_OP == Parent.WE_OP))
			{
				Parent.WE_OPInfo.AddError(ComponentLineMustHaveUniqueProductErrorMessage);
			}
		}

		public static string ComponentLineMustHaveUniqueProductErrorMessage => Res.GetString("d111a765-fad5-4f49-ab19-f8469a91ed6a", "Each component line must have a unique product.");

		#endregion

		#region CheckWE_TransactionQuantity

		protected override void CheckWE_TransactionQuantity()
		{
			if (!Parent.WE_TransactionQuantityInfo.HasErrors())
			{
				if (Parent.WE_TransactionQuantity <= 0m)
				{
					Parent.WE_TransactionQuantityInfo.AddError(DynamicWorkOrderLineQuantityMustBeGreaterZero);
				}
				else if (Parent.WE_TransactionQuantity % 1 != 0 && Parent.IsMainInwardProcessedItem)
				{
					Parent.WE_TransactionQuantityInfo.AddError(MainProductLineQuantityMustBeAnInteger);
				}
				else
				{
					CheckComponentSumsAreCorrect();
				}
			}
		}

		void CheckComponentSumsAreCorrect()
		{
			if (Parent.WE_WE_ParentDocketLine.IsValid)
			{
				var docket = Docket;
				if (docket != null)
				{
					var validationCache = docket.GetPreSaveValidationCache();
					if (validationCache != null && validationCache.IsSecondaryComponentSumMoreThanMainComponent(Parent.WE_OP))
					{
						Parent
							.WE_TransactionQuantityInfo
							.AddError(DynamicWorkOrderComponentsSumIncorrect);
					}
				}
			}
		}

		public static string MainProductLineQuantityMustBeAnInteger => Res.GetString("1dd02ebf-720b-4695-9fe3-bd9d07e5ac74", "Quantity of main product must be an integer.");

		public static string DynamicWorkOrderLineQuantityMustBeGreaterZero => Res.GetString("28e679cc-26f1-4f26-a177-30d60f6defba", "Quantity must be greater than zero.");

		public static string DynamicWorkOrderComponentsSumIncorrect => Res.GetString("86f1dab8-4dc9-43b8-9b92-96193dca38f2", "The sum of a component across all secondary products must be less than or equal to the quantity of that component on the main product.");

		#endregion

		#region SumOfUnits

		protected override void SumOfUnitsNotMetValidationCore(WhsPickableDocket docket)
		{
			if (Parent.WE_WE_ParentDocketLine.IsValid && Parent.ParentLine.IsMainInwardProcessedItem)
			{
				if (docket.IsFinalising)
				{
					Parent
						.SumOfUnitsMetInfo
						.AddError(Res.GetString("79a7760e-7138-45d4-b014-51671bbf4a72", "To finalize a Dynamic Work Order, main product component lines must be fully allocated."));
				}
				else
				{
					base.SumOfUnitsNotMetValidationCore(docket);
				}
			}
			else if (!Docket.IsAssembly)
			{
				if (docket.IsFinalising && Parent.SumOfUnitsMet % 1 != 0)
				{
					Parent
						.SumOfUnitsMetInfo
						.AddError(Res.GetString("5f5154bd-a0d9-4ca8-9231-6eb435a2e6ca", "Allocated quantity of kit having assembly reverted must be an integer."));
				}
				else
				{
					base.SumOfUnitsNotMetValidationCore(docket);
				}
			}
		}

		#endregion

		#region CheckWE_ShortfallQuantityCached

		protected override void CheckWE_ShortfallQuantityCached() { }
		protected override string ShortfallWarning => string.Empty;

		#endregion

		#region IsJulianBatchNumberFormatValidationRequired

		protected override bool IsJulianBatchNumberFormatValidationRequired(ZPropertyInfo partAttributeInfo, int attributeNumber)
			=> !partAttributeInfo.Value.IsEmpty && Parent.WE_WE_ParentDocketLine.IsEmpty;

		#endregion
	}
}
