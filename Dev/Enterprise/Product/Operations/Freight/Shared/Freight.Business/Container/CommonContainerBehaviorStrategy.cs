using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;

namespace Enterprise.Freight.Business
{
	public class CommonContainerBehaviorStrategy
	{
		public CommonContainerBehaviorStrategy()
		{
		}

		#region RefContainer

		public virtual void RefContainerAttached(CommonContainer container)
		{
			DefaultContainerDimensionsFromRefContainer(container);

			if (!container.IsSettingWeightsSuspendedFromUniversalShipmentReader)
			{
				DefaultTareWeight(container);
			}

			DefaultAtmosphere(container);
		}

		public virtual void RefContainerRemoved(CommonContainer container)
		{
		}

		void DefaultContainerDimensionsFromRefContainer(CommonContainer container)
		{
			if (container.Container != null)
			{
				container.JC_TotalLength = container.Container.RC_Length;
				container.JC_TotalHeight = container.Container.RC_Height;
				container.JC_TotalWidth = container.Container.RC_Width;
			}
		}

		void DefaultTareWeight(CommonContainer container)
		{
			if (container.Container != null)
			{
				decimal weightInKgs = container.Container.RC_TareWeight * container.JC_ContainerCount;
				if (Core.Constants.Weight.ContainsCode(container.JC_GrossWeightUQ))
				{
					container.JC_TareWeight = Core.Constants.Weight.Convert(weightInKgs, Core.Constants.Weight.Kilograms, container.JC_GrossWeightUQ);
				}
				else
				{
					container.JC_TareWeight = weightInKgs;
				}
			}
		}

		void DefaultAtmosphere(CommonContainer container)
		{
			if (container.Container != null && !((IBusinessObjectInternals)container).IsCopying)
			{
				container.JC_IsControlledAtmosphere = container.Container.RC_ContainerType == Constants.ContainerTypes.Refrigerated;
			}
		}

		#endregion

		#region ContainerCount

		public void ContainerCountChanged(CommonContainer container)
		{
			if (!container.IsSettingWeightsSuspendedFromUniversalShipmentReader)
			{
				DefaultTareWeight(container);
			}
		}

		#endregion

		#region EmptyReturnedBy

		public virtual void EmptyReturnedByChanged(CommonContainer container, ZDateTime originalValue)
		{
		}

		#endregion

		#region EmptyRequired

		public virtual void EmptyRequiredChanged(CommonContainer container, ZDateTime originalValue)
		{
		}

		#endregion

		#region ArrivalSlotDateTime

		public virtual void ArrivalSlotDateOrReferenceChanged(CommonContainer container, ZDateTime originalValue)
		{
		}

		#endregion

		#region DepartureSlotDateTime

		public virtual void DepartureSlotDateOrReferenceChanged(CommonContainer container, ZDateTime originalValue)
		{
		}

		#endregion

		#region Validation

		public virtual CommonContainerValidation GetNewValidation(CommonContainer container)
		{
			return new CommonContainerValidation(container);
		}

		#endregion
	}
}
