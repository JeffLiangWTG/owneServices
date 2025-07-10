using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Warehouse;

namespace Enterprise.Warehouse.Transactions.Business
{
	public abstract class WhsPickableDocketLineValidation : WhsDocketLineValidation
	{
		protected WhsPickableDocketLineValidation(WhsPickableDocketLine parent)
			: base(parent)
		{
		}

		#region Parent

		public new WhsPickableDocketLine Parent
		{
			get { return (WhsPickableDocketLine)base.Parent; }
		}

		#endregion

		// Calculated

		#region ValidatePickGroup

		public void ValidatePickGroup()
		{
			ValidateCalculatedProperty(Parent.PickGroupForBindingInfo);
		}

		protected virtual void CheckPickGroupForBinding()
		{
		}

		#endregion

		#region ValidateWE_ShortfallQuantityCached

		public void ValidateWE_ShortfallQuantityCached_WithoutUpdatingCache()
		{
			using (new SemaphoreManager(ShortfallQtyCacheUpdateSemaphore))
			{
				ValidateWE_ShortfallQuantityCached();
			}
		}

		public void ValidateWE_ShortfallQuantityCached()
		{
			ValidateCalculatedProperty(Parent.WE_ShortfallQuantityCachedInfo);
		}

		protected virtual void CheckWE_ShortfallQuantityCached()
		{
			if (ShortfallQtyCacheUpdateSemaphore.IsSuspended) // don't update the shortfall qty cached value
			{
				if (Parent.SupplierPart != null && Parent.GetShortfallExistsStatus_WithoutUpdatingCache())
				{
					AddShortfallWarning();
				}
			}
			else if (!Parent.IsDeleted && Parent.SupplierPart != null && Parent.GetShortfallExistsStatus())
			{
				AddShortfallWarning();
			}
		}

		void AddShortfallWarning()
		{
			Parent.WE_ShortfallQuantityCachedInfo.AddWarning(ShortfallWarning);
		}

		protected abstract string ShortfallWarning { get; }

		Semaphore ShortfallQtyCacheUpdateSemaphore
		{
			get { return shortfallQtyCacheUpdateSemaphore ?? (shortfallQtyCacheUpdateSemaphore = new Semaphore()); }
		}

		Semaphore shortfallQtyCacheUpdateSemaphore;

		#endregion

		#region ValidateSumOfUnitsMet

		public void ValidateSumOfUnitsMet()
		{
			if (!Parent.ReadOnly)
			{
				ValidateCalculatedProperty(Parent.SumOfUnitsMetInfo);
			}
		}

		protected void CheckSumOfUnitsMet()
		{
			var pickableDocket = Parent.PickableDocket;
			if (pickableDocket?.IsAttachedToPick ?? false)
			{
				if (Parent.SumOfUnitsMet < Parent.WE_TransactionQuantity)
				{
					SumOfUnitsNotMetValidationCore(pickableDocket);
				}
			}
		}

		protected virtual void SumOfUnitsNotMetValidationCore(WhsPickableDocket docket)
		{
			Parent.SumOfUnitsMetInfo.AddWarning(Res.GetString("52c35d33-3578-4e1f-91dd-1575fbe30c73", "Shortfall"));
		}

		#endregion

		// Persistent

		#region ValidateWE_TransactionQuantity

		protected override void CheckWE_TransactionQuantity()
		{
			base.CheckWE_TransactionQuantity();

			if (Parent.WE_TransactionQuantity < 0m)
			{
				Parent.WE_TransactionQuantityInfo.AddError(Res.GetString("425535f9-bce2-481c-85c3-f4ef0523f81d", "Please enter a value greater than zero"));
			}
			else if (Parent.WE_TransactionQuantity == 0m)
			{
				Parent.WE_TransactionQuantityInfo.AddWarning(Res.GetString("5e463f6e-f50d-4403-be6a-81c2f614dd05", "Lines with zero quantities will not be picked"));
			}
			else if (WarehouseDataRegistry.Instance.EnableSchemaRedesignChanges.Value)
			{
				PartAttributeValidation.CheckQtyForSerialNumber(Parent.Product, Parent.Docket.Client, (ZPropertyInfoDecimal)Parent.WE_TransactionQuantityInfo, Parent.WE_SerialNumberInfo);
			}
		}

		#endregion

		#region ValidateLocationString

		protected override void CheckLocationString()
		{
			// location is not used by pickable docket lines.
		}

		#endregion

		#region CheckHeldCode

		protected override void CheckHeldCode(string status, ZPropertyInfo propertyInfo)
		{
			MandatoryValidation.CheckNotEntered(propertyInfo);
		}

		#endregion

		// Validate All

		#region ValidateAll

		public override void ValidateAll()
		{
			base.ValidateAll();

			ValidateWE_ShortfallQuantityCached_WithoutUpdatingCache();
			ValidateSumOfUnitsMet();
			ValidatePickGroup();
		}

		#endregion
	}
}
