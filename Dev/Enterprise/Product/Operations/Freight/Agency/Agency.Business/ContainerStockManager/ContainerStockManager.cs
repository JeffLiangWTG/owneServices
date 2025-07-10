using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Common.Business;

namespace Enterprise.Freight.Agency.Business
{
	internal abstract class ContainerStockManager
	{
		public static ContainerStockManager New(AgencyShipmentContainer container)
		{
			return new LicencedContainerStockManager(container);
		}

		protected ContainerStockManager(AgencyShipmentContainer container)
		{
			if (container == null)
			{
				throw new ArgumentNullException(nameof(container));
			}

			this.container = container;
		}

		public void ContainerNumberChanging(ZString oldValue, ZString newValue)
		{
			if (oldValue != newValue)
			{
				ContainerNumberChangingCore(oldValue, newValue);
			}
		}
		public void ContainerTypeChanging(ZGuid oldValue, ZGuid newValue)
		{
			if (oldValue != newValue)
			{
				ContainerTypeChangingCore(oldValue, newValue);
			}
		}
		public void IsShipperOwnedChanging(ZBool oldValue, ZBool newValue)
		{
			if (oldValue != newValue)
			{
				IsShipperOwnedChangingCore(oldValue, newValue);
			}
		}
		public JobContainerValidation CreateExtraValidation()
		{
			return CreateExtraValidationCore();
		}

		#region Implementation

		protected AgencyShipmentContainer Container
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return container; }
		}
		protected BusinessObjectFactory Factory
		{
			get { return container.Factory; }
		}

		#endregion

		protected abstract void ContainerNumberChangingCore(ZString oldValue, ZString newValue);
		protected abstract void ContainerTypeChangingCore(ZGuid oldValue, ZGuid newValue);
		protected abstract void IsShipperOwnedChangingCore(ZBool oldValue, ZBool newValue);
		protected abstract JobContainerValidation CreateExtraValidationCore();

		readonly AgencyShipmentContainer container;
	}
}


