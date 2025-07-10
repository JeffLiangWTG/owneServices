namespace Enterprise.Freight.Agency.DataTransfer.Universal
{
	using System.Collections.Generic;
	using System.Linq;
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.Freight.Agency.Business;
	using Enterprise.Freight.DataTransfer.Universal;
	using Enterprise.UniversalDataBuss.Integration;
	using Enterprise.UniversalDataBuss.Management;
	using UniversalEvent = UniversalDataBuss.DataObjects.Universal.Event;

	public class AgencyShipmentContainerEventParentFinder : EventParentFinder
	{
		public AgencyShipmentContainerEventParentFinder(BusinessObjectFactory factory, IEventDataContextManager manager, IXmlImportLogger logger)
			: base(factory, manager, logger)
		{
		}

		protected override BusinessObject[] GetLogParentsForEventUsingContext(UniversalEvent xmlEvent)
		{
			IXmlEventValueObject eventValueObject = xmlEvent;

			if (eventValueObject.Context == null)
			{
				return null;
			}

			var isContainerDataContext = IsAgencyContainerShipmentDataContext(eventValueObject.Context);
			var isTopLevelPackDataContext = eventValueObject.Context.Values.Any(
				val => val.Key.Type == nameof(UniversalEvent.ContextTypes.GoodsItemID));

			var result = new List<BusinessObject>();

			if (isContainerDataContext && isTopLevelPackDataContext)
			{
				logger.Log(Enterprise.Integration.LogType.Error, Res.GetString("14641dad-090b-41e8-8aa6-e9085c8e4a16", "Both Goods Item ID and Container ID are used"));
			}
			else if (isContainerDataContext || isTopLevelPackDataContext)
			{
				var parentMatches = GetBestParentMatches(eventValueObject.Context);

				if (parentMatches.Length == 1)
				{
					var parentMatch = parentMatches.Single();

					if (isContainerDataContext)
					{
						foreach (var reference in parentMatch.References)
						{
							var containerMatcher = new AgencyShipmentContainerMatcher(parentMatch.Parent, reference);
							var bestMatch = containerMatcher.GetBestMatch();

							if (bestMatch != null)
							{
								result.Add(bestMatch);
							}
						}
					}
					else
					{
						var containersByNumber = parentMatch.Parent.ShippingContainers.Cast<AgencyShipmentContainer>()
							.Where(c => c.JC_ContainerNum == parentMatch.References.First().GoodsItemID && c.IsTopLevelPack)
							.ToArray();

						if (containersByNumber.Length == 1)
						{
							result.Add(containersByNumber.Single());
						}
					}
				}
				else if (parentMatches.Length > 1)
				{
					logger.Log(Enterprise.Integration.LogType.Error, Res.GetString("14641dad-090b-41e8-8aa6-e9085c8e4a16", "Both Goods Item ID and Container ID are used"));
				}
			}

			return result.Any()
				? result.ToArray()
				: null;
		}

		ContainerParentMatch<AgencyShipment, AgencyShipmentContainerReferences>[] GetBestParentMatches(IXmlEventValueObjectContextValueList context)
		{
			var result = new Dictionary<ZGuid, ContainerParentMatch<AgencyShipment, AgencyShipmentContainerReferences>>();

			var containerNumbers = context.ContainerNumbers?.AsEnumerable() ?? new[] { ZString.Empty };

			foreach (var containerNumber in containerNumbers)
			{
				var references = new AgencyShipmentContainerReferences(context, containerNumber);
				var matcher = new AgencyShipmentMatcher<AgencyShipment>(factory, references, logger);
				var match = matcher.GetBestMatch();

				if (match == null)
				{
					continue;
				}

				ContainerParentMatch<AgencyShipment, AgencyShipmentContainerReferences> parentMatch = null;

				if (result.TryGetValue(match.PK, out parentMatch))
				{
					parentMatch.AddReference(references);
				}
				else
				{
					parentMatch = new ContainerParentMatch<AgencyShipment, AgencyShipmentContainerReferences>(match, references);
					result[match.PK] = parentMatch;
				}
			}

			return result.Values.ToArray();
		}

		bool IsAgencyContainerShipmentDataContext(IXmlEventValueObjectContextValueList context)
		{
			var containerValueTypes = new[]
			{
				nameof(UniversalEvent.ContextTypes.ContainerNumber),
				nameof(UniversalEvent.ContextTypes.ContainerISOCode),
				nameof(UniversalEvent.ContextTypes.ContainerReleaseNumber)
			};

			var receivedContextValueTypes = context.Values.Select(val => val.Key.Type.ToString());

			return receivedContextValueTypes.Intersect(containerValueTypes).Any();
		}
	}
}



