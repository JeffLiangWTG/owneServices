using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Common.Business
{
	public class LegTypeCollectionStrategy
	{
		public enum LegBookingType
		{
			Booking,
			NonBooking,
			All
		}

		public enum LegContainerMode
		{
			Containerized,
			Loose,
			All
		}

		public LegTypeCollectionStrategy(LegBookingType bookingMode, LegContainerMode containerMode)
		{
			switch (bookingMode)
			{
				case LegBookingType.Booking:
					IsBooking = true;
					break;
				case LegBookingType.NonBooking:
					IsBooking = false;
					break;
				default:
					IsBooking = null;
					break;
			}

			switch (containerMode)
			{
				case LegContainerMode.Containerized:
					ContainerMode = Constants.ContainerModes.Containerised;
					break;
				case LegContainerMode.Loose:
					ContainerMode = Constants.ContainerModes.Loose;
					break;
				default:
					ContainerMode = "";
					break;
			}
		}
		internal string ContainerMode;
		internal bool? IsBooking;

		internal ZQuery Filter
		{
			get
			{
				var result = new ZQuery();

				if (IsBooking != null)
				{
					result.AddToFilter(LocalCartageJobLegTypeSchema.E4_IsBooking, IsBooking);
				}

				if (!string.IsNullOrWhiteSpace(ContainerMode))
				{
					result.AddToFilter(LocalCartageJobLegTypeSchema.E4_ContainerMode, ContainerMode);
				}

				return result;
			}
		}
	}

	public class CommonCartageLegTypeCollection : ActiveBusinessObjectCollection<CommonCartageLegType>
	{
		public CommonCartageLegTypeCollection(CommonCartageType cartageType, LegTypeCollectionStrategy strategy)
			: base(cartageType.Factory, cartageType, strategy.Filter, LocalCartageJobLegTypeSchema.E4_E3)
		{
			this.strategy = strategy;
			CartageType = cartageType;
		}

		public CommonCartageLegTypeCollection(CommonCartageType cartageType)
			: base(cartageType.Factory, cartageType, null, LocalCartageJobLegTypeSchema.E4_E3)
		{
			CartageType = cartageType;
		}

#if DEBUG
		[CargoWise.EntityFramework.Testing.SuppressCollectionStateTest]
#endif
		readonly CommonCartageType CartageType;

#if DEBUG
		[CargoWise.EntityFramework.Testing.SuppressCollectionStateTest]
#endif
		readonly LegTypeCollectionStrategy strategy;

		#region HasContainerisedLeg

		/// <summary>
		/// By now, ContainerModes on the legs of a jobtype have to be the same. 
		/// Hopefully in future we can have different ContainerModes on JobLegs.
		/// </summary>
		public bool HasContainerisedLeg
		{
			get
			{
				foreach (CommonCartageLegType leg in this)
				{
					if (leg.E4_ContainerMode == Core.Constants.ContainerModes.Containerised)
					{
						return true;
					}
				}
				return false;
			}
		}

		#endregion

		#region Overrides

		protected override bool AllowNew
		{
			get { return !CartageType.E3_IsSystem; }
		}

		protected override void SetDefaultsForNewElementCore(CommonCartageLegType newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);
			if (strategy != null)
			{
				if (strategy.IsBooking != null)
				{
					newElement.E4_IsBooking = strategy.IsBooking.Value;
				}

				if (!string.IsNullOrWhiteSpace(strategy.ContainerMode))
				{
					newElement.E4_ContainerMode = strategy.ContainerMode;
				}

				if (strategy.IsBooking != null && !strategy.IsBooking.Value)
				{
					newElement.E4_DisplayOrder = Convert.ToSByte(newElement.CommonCartageType.AllCartageLegTypes.Count + 1);
				}
			}
		}

		#endregion

		#region RemoveByOrganisationType

		public void RemoveByCartageOrg(ZGuid cartageOrgPK)
		{
			List<CommonCartageLegType> toBeRemoved = new List<CommonCartageLegType>();

			foreach (CommonCartageLegType legType in this)
			{
				if (legType.E4_E5_FromOrg == cartageOrgPK ||
					legType.E4_E5_WaitPointOrg == cartageOrgPK ||
					legType.E4_E5_ToOrg == cartageOrgPK)
				{
					toBeRemoved.Add(legType);
				}
			}

			foreach (CommonCartageLegType legTypeToRemove in toBeRemoved)
			{
				legTypeToRemove.Delete();
			}
		}

		#endregion
	}
}
