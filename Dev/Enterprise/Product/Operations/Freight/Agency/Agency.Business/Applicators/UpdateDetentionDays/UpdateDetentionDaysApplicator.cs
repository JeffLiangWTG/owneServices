using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Freight.Agency.Business
{
	public sealed class UpdateDetentionDaysApplicator : AutoUpdateDetentionDaysApplicator
	{
		public UpdateDetentionDaysApplicator()
			: base(Res.GetString("1b8c3bdb-c175-44c3-bc38-6e8d16973b80", "Update Detention Days")) { }

		protected override void ApplyCore(IOperationalActionSectionLog log, BusinessObject[] targets)
		{
			foreach (KeyValuePair<ZGuid, List<ContainerMovement>> pair in GroupMovements(targets))
			{
				RefContainerStock stock = pair.Value[0].Factory.Load<RefContainerStock>(pair.Key);

				log.NotifyFormat(
					OperationalActionLogErrorLevel.Informational,
					Res.GetString("9b737dd0-431e-4f4e-aebc-42ac535601e0", "Updating movements for {1}."),
					null,
					HyperlinkHelper.Link(stock)
					);

				foreach (ContainerMovement movement in pair.Value)
				{
					UpdateDetentionDays(log, movement);
				}
			}
		}

		static IDictionary<ZGuid, List<ContainerMovement>> GroupMovements(IEnumerable<BusinessObject> targets)
		{
			Dictionary<ZGuid, List<ContainerMovement>> lookup = new Dictionary<ZGuid, List<ContainerMovement>>();
			List<ContainerMovement> list;

			foreach (ContainerMovement movement in targets)
			{
				if (movement != null)
				{
					if (!lookup.TryGetValue(movement.E9_R6, out list))
					{
						list = new List<ContainerMovement>();
						lookup.Add(movement.E9_R6, list);
					}

					list.Add(movement);
				}
			}
			return lookup;
		}

		static void NotifyUnsupportedMovementType(IOperationalActionSectionLog log, ContainerMovement movement)
		{
			log.NotifyFormat(
				OperationalActionLogErrorLevel.Informational,
				Res.GetString("742ed478-9612-45b1-9083-2c44686d216b", "The {1} movement does not support detentions."),
				null,
				HyperlinkHelper.Link(movement)
				);
		}

		static void NotifyUnchanged(IOperationalActionSectionLog log, ContainerMovement movement)
		{
			log.NotifyFormat(
				OperationalActionLogErrorLevel.Informational,
				Res.GetString("ff17f896-6017-4a62-98c4-9655e0b21e9c", "The number of detention days for {1} is already set to {2} and does not need to be updated."),
				null,
				HyperlinkHelper.Link(movement),
				movement.E9_DetentionDays
				);
		}

		static void NotifyPosted(IOperationalActionSectionLog log, ContainerMovement movement, short newDays)
		{
			log.NotifyFormat(
				OperationalActionLogErrorLevel.Warning,
				Res.GetString("E729A0F3-0CAC-4A25-A543-5BF28CCE1930", "Cannot update the detention days for the {1} movement from {2} to {3} as it already has a posted invoice on {4}."),
				null,
				HyperlinkHelper.Link(movement),
				movement.E9_DetentionDays,
				newDays,
				HyperlinkHelper.Link(movement.Detention)
				);
		}

		static void NotifyInvoiced(IOperationalActionSectionLog log, ContainerMovement movement, short newDays)
		{
			log.NotifyFormat(
				OperationalActionLogErrorLevel.Warning,
				Res.GetString("38B82C24-7F16-4695-BDB1-84385C72E895", "Updating the number of detention days for {1} from {2} to {3} however it is currently attached to the detention invoice {4}."),
				null,
				HyperlinkHelper.Link(movement),
				movement.E9_DetentionDays,
				newDays,
				HyperlinkHelper.Link(movement.Detention)
				);
		}

		static void NotifyUpdated(IOperationalActionSectionLog log, ContainerMovement movement, short newDays)
		{
			log.NotifyFormat(
				OperationalActionLogErrorLevel.Informational,
				Res.GetString("6ca4461e-d163-4586-a317-22e326c71a50", "Updating the number of detention days for {1} from {2} to {3}."),
				null,
				HyperlinkHelper.Link(movement),
				movement.E9_DetentionDays,
				newDays
				);
		}

		void UpdateDetentionDays(IOperationalActionSectionLog log, ContainerMovement movement)
		{
			string detentionType = ContainerMovementTypes.GetDetentionCalculation(movement.E9_MovementType);

			if (string.IsNullOrEmpty(detentionType))
			{
				NotifyUnsupportedMovementType(log, movement);
			}
			else
			{
				short newDays = GetDetentionStrategy(detentionType).GetDefaultDetentionDays(movement).GetValueOrDefault(0);

				if (newDays == movement.E9_DetentionDays)
				{
					NotifyUnchanged(log, movement);
				}
				else if (movement.DetentionPosted)
				{
					NotifyPosted(log, movement, newDays);
				}
				else if (movement.DetentionInvoiced)
				{
					NotifyInvoiced(log, movement, newDays);
					movement.E9_DetentionDays = newDays;
				}
				else
				{
					NotifyUpdated(log, movement, newDays);
					movement.E9_DetentionDays = newDays;
				}
			}
		}

		DetentionStrategy GetDetentionStrategy(string code)
		{
			DetentionStrategy result;

			if (detentionStrategies == null)
			{
				detentionStrategies = new Dictionary<string, DetentionStrategy>();
				result = DetentionStrategy.New(code);
				detentionStrategies.Add(code, result);
			}
			else if (!detentionStrategies.TryGetValue(code, out result))
			{
				result = DetentionStrategy.New(code);
				detentionStrategies.Add(code, result);
			}

			return result;
		}

		Dictionary<string, DetentionStrategy> detentionStrategies;
	}
}
