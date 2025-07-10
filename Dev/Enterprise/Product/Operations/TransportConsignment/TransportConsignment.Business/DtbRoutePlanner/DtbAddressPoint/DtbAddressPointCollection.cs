using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.TransportConsignment.Integration;

namespace Enterprise.TransportConsignment.Business
{
	public class DtbAddressPointCollection : NonPersistentBusinessObjectCollection<DtbAddressPoint>, IBusinessObjectCollection
	{
		public DtbAddressPointCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region AllowNew / AllowRemove

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override bool AllowRemoveCore
		{
			get { return false; }
		}

		#endregion

		#region LoadAndConsolidateFromConfirmations

		public bool LoadAndConsolidateFromConfirmations(ZQuery confirmationsQuery, int maximumConfirmations, DtbRoutePlannerViewMode viewMode = DtbRoutePlannerViewMode.None)
		{
			this.RemoveAll();

			// load confirmations
			confirmationsQuery.MaximumRows = maximumConfirmations + 1;
			var confirmations = Factory.Load<DtbConsignmentConfirmation>(confirmationsQuery);
			foreach (var confirmation in confirmations)
			{
				Factory.AddFetchHint(typeof(DtbConsignmentInstruction), confirmation.KK_KN_BookingInstruction);
			}

			// only consolidate if we did not exceed the max
			var isConfirmationCountWithinMax = (confirmations.Length <= maximumConfirmations);
			if (isConfirmationCountWithinMax)
			{
				var allAddressPointConfirmations = new List<DtbConsignmentConfirmation>(confirmations.Length);

				if (viewMode == DtbRoutePlannerViewMode.Direct)
				{
					ConsolidateFromConfirmationsForDirectMode(confirmations, allAddressPointConfirmations);
				}
				else
				{
					ConsolidateFromConfirmations(confirmations, allAddressPointConfirmations);
				}

				// store confirmations in the factory cache
				const string AddressPointsKey = "DtbAddressPointCollectionKey";
				Factory.InitialiseConfirmationsTotalsCache(AddressPointsKey, allAddressPointConfirmations);
			}

			return isConfirmationCountWithinMax;
		}

		#region ConsolidateFromConfirmations

		void ConsolidateFromConfirmations(DtbConsignmentConfirmation[] confirmations, List<DtbConsignmentConfirmation> consolidatedConfirmations)
		{
			var confirmationsGroupedByOrgAddress = new Dictionary<IDocAddress, DtbAddressPoint>();
			var confirmationsGroupedByDocAddress = new Dictionary<string, DtbAddressPoint>();

			foreach (var confirmation in confirmations)
			{
				var jda = confirmation.Instruction.Address;
				if (jda.E2_AddressOverride) // consolidate by DocAddress (company name + address fields, no fuzzy matching)
				{
					var key = jda.GetAddressUniqueKey();
					if (!string.IsNullOrEmpty(key))
					{
						AddConfirmationToCollection(confirmationsGroupedByDocAddress, consolidatedConfirmations, key, jda, confirmation);
					}
				}
				else // consolidate by org address
				{
					var address = jda.Address;
					if (address != null) // null if instruction had no actual address, for now we will not allow these to be allocated
					{
						AddConfirmationToCollection(confirmationsGroupedByOrgAddress, consolidatedConfirmations, address, address, confirmation);
					}
				}
			}
		}

		void AddConfirmationToCollection<T>(Dictionary<T, DtbAddressPoint> dictionary, List<DtbConsignmentConfirmation> allAddressPointConfirmations, T key, IDocAddress address, DtbConsignmentConfirmation confirmation)
		{
			DtbAddressPoint addressPoint;
			if (!dictionary.TryGetValue(key, out addressPoint))
			{
				addressPoint = new DtbAddressPoint(address);
				dictionary.Add(key, addressPoint);
			}

			AddConfirmationToAddressPoint(allAddressPointConfirmations, addressPoint, confirmation);
		}

		#endregion

		#region ConsolidateFromConfirmationsForDirectMode

		void ConsolidateFromConfirmationsForDirectMode(DtbConsignmentConfirmation[] confirmations, List<DtbConsignmentConfirmation> allAddressPointConfirmations)
		{
			var confirmationPairs = confirmations.ToLookup(c => c.ConsignmentID);
			var confirmationsGroupedByAddress = new Dictionary<PickupAndDeliveryPair, DtbAddressPoint>();

			foreach (var lookupKey in confirmationPairs.Select(l => l.Key))
			{
				ConsolidateAndAddDirectToCollection(allAddressPointConfirmations, confirmationsGroupedByAddress, confirmationPairs[lookupKey]);
			}
		}

		void ConsolidateAndAddDirectToCollection(
			List<DtbConsignmentConfirmation> allAddressPointConfirmations,
			Dictionary<PickupAndDeliveryPair, DtbAddressPoint> confirmationsGroupedByAddress,
			IEnumerable<DtbConsignmentConfirmation> consignmentConfirmations)
		{
			var picConfirmation = consignmentConfirmations.FirstOrDefault(c => c.IsPickUp && !c.Instruction.IsMulti);
			var dlvConfirmation = consignmentConfirmations.FirstOrDefault(c => c.IsDelivery && !c.Instruction.IsMulti);

			if (picConfirmation == null && dlvConfirmation != null)
			{
				picConfirmation = dlvConfirmation.GetRelatedConfirmation();
				if (picConfirmation.Instruction.ServiceExists != dlvConfirmation.Instruction.ServiceExists)
				{
					picConfirmation = null;
				}
			}

			if (picConfirmation != null && dlvConfirmation == null)
			{
				dlvConfirmation = picConfirmation.GetRelatedConfirmation();
				if (dlvConfirmation.Instruction.ServiceExists != picConfirmation.Instruction.ServiceExists)
				{
					dlvConfirmation = null;
				}
			}

			if (dlvConfirmation != null && picConfirmation != null)
			{
				AddDirectToCollectionIfValidAddresses(allAddressPointConfirmations, confirmationsGroupedByAddress, picConfirmation, dlvConfirmation);
			}
		}

		void AddDirectToCollectionIfValidAddresses(List<DtbConsignmentConfirmation> allAddressPointConfirmations, Dictionary<PickupAndDeliveryPair, DtbAddressPoint> confirmationsGroupedByAddress, DtbConsignmentConfirmation picConfirmation, DtbConsignmentConfirmation dlvConfirmation)
		{
			var deliveryAddress = GetAddressFromJobDocAddress(dlvConfirmation.Instruction.Address);
			var pickupAddress = GetAddressFromJobDocAddress(picConfirmation.Instruction.Address);

			if (deliveryAddress != null && pickupAddress != null) // null if instruction had no actual address, for now we will not allow these to be allocated
			{
				var pickupDeliveryPair = new PickupAndDeliveryPair(pickupAddress, deliveryAddress);
				if (!pickupDeliveryPair.PickupKey.IsEmpty && !pickupDeliveryPair.DeliveryKey.IsEmpty)
				{
					AddConfirmationToCollection(confirmationsGroupedByAddress, allAddressPointConfirmations, pickupDeliveryPair, picConfirmation);
					AddConfirmationToCollection(confirmationsGroupedByAddress, allAddressPointConfirmations, pickupDeliveryPair, dlvConfirmation);
				}
			}
		}

		void AddConfirmationToCollection(Dictionary<PickupAndDeliveryPair, DtbAddressPoint> dictionary, List<DtbConsignmentConfirmation> consolidatedConfirmations, PickupAndDeliveryPair addressPair, DtbConsignmentConfirmation confirmation)
		{
			DtbAddressPoint addressPoint;
			if (!dictionary.TryGetValue(addressPair, out addressPoint))
			{
				addressPoint = new DtbAddressPoint(addressPair);
				dictionary.Add(addressPair, addressPoint);
			}

			AddConfirmationToAddressPoint(consolidatedConfirmations, addressPoint, confirmation);
		}

		IDocAddress GetAddressFromJobDocAddress(JobDocAddress jobDocAddress)
		{
			return jobDocAddress.E2_AddressOverride ? jobDocAddress : jobDocAddress.Address; // null if instruction had no actual address
		}

		#endregion

		#region AddConfirmationToAddressPoint

		void AddConfirmationToAddressPoint(List<DtbConsignmentConfirmation> allAddressPointConfirmations, DtbAddressPoint addressPoint, DtbConsignmentConfirmation confirmation)
		{
			Add(addressPoint);

			// We suspend CountChanged to prevent firing ListChanged via RefreshBinding(). If we do not do this, 
			// Direct Mode properties may (because of a race condition on whether the pickup or delivery confirmation
			// get refreshed first) show incorrect results.
			using (addressPoint.SuspendConfirmationsCountChanged())
			{
				addressPoint.Confirmations.Add(confirmation);
			}

			allAddressPointConfirmations.Add(confirmation);
		}

		#endregion

		#endregion

		#region CreateNonPersistentBusinessObject

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return DtbAddressPoint.GetNewFakeDtbAddressPointForBinding(Factory);
		}

		#endregion
	}
}
