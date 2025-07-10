using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.DataTransfer.Universal
{
	class ConsolContainerLinker : IContainerLinker
	{
		public ConsolContainerLinker(CommonConsol consol, IXmlImportLogger logger)
		{
			this.consol = Argument.NotNull(consol, "consol");
			this.logger = logger;
		}

		readonly CommonConsol consol;
		readonly IXmlImportLogger logger;

		public CommonContainer[] GetLogParent(IXmlEventValueObject xmlEvent)
		{
			Argument.NotNull(xmlEvent, "xmlEvent");

			var allContainerIDs = xmlEvent.Context.MAWBNumber.IsEmpty
				? xmlEvent.Context.ContainerNumbers?.Select(x => x.ToUpper()).ToList()
				: xmlEvent.Context.ULDIdentifications?.Select(x => x.ToUpper()).ToList();

			if (allContainerIDs != null && allContainerIDs.Any())
			{
				var containerIDs = allContainerIDs.ToLookup(l => l);
				foreach (var group in containerIDs)
				{
					var count = group.Count();
					if (count > 1)
					{
						logger?.LogBoth(LogType.Warning, Res.GetString("6649abc8-bc51-4b78-80e2-695df2ea1bc9", "Ignoring {0} duplicate(s) of Container Number [{1}]", count - 1, group.Key));
					}
				}

				var distinctContainerIDs = containerIDs.Select(g => g.Key).ToList();
				var consolContainers = consol.Containers.Cast<CommonContainer>().Where(c => containerIDs.Contains(c.JC_ContainerNum)).ToArray();
				if (consolContainers.Any())
				{
					if (consol.IsAir)
					{
						var containerIDsNeedToCreate = distinctContainerIDs.Where(c => !consolContainers.Any(container => c == container.JC_ContainerNum)).ToList();
						UpdateContainers(xmlEvent, containerIDsNeedToCreate);

						consolContainers = consol.Containers.Cast<CommonContainer>().Where(c => containerIDs.Contains(c.JC_ContainerNum)).ToArray();
					}

					return consolContainers;
				}
				else
				{
					return UpdateContainers(xmlEvent, distinctContainerIDs);
				}
			}

			return null;
		}

		CommonContainer[] UpdateContainers(IXmlEventValueObject xmlEvent, List<ZString> containerIDs)
		{
			if (ContainersShouldBeUpdated)
			{
				var updatedContainers = containerIDs.Select(c => UpdateContainer(consol, xmlEvent, c)).WhereNotNull().ToArray();
				UpdateAirContainerTypes();
				return updatedContainers.Any() ? updatedContainers : null;
			}

			return null;
		}

		void UpdateAirContainerTypes()
		{
			if (consol.IsAir)
			{
				var containers = consol.Containers.Cast<CommonContainer>().ToArray();
				var containerWithType = containers.FirstOrDefault(c => !c.JC_RC.IsEmpty);
				if (containerWithType != null)
				{
					foreach (var container in containers.Where(c => c.JC_RC.IsEmpty))
					{
						container.JC_RC = containerWithType.JC_RC;
					}
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "log text")]
		bool ContainersShouldBeUpdated
		{
			get
			{
				if (FreightDataRegistry.Instance.AutomaticContainerCreation.Value.IsAlwaysCreate)
				{
					return true;
				}

				bool consolHasTransportLegWithValidATDorATA = consol.Transports.OfType<Transport>().Any(t => !t.JW_ATA.IsEmpty || !t.JW_ATD.IsEmpty);

				return !consolHasTransportLegWithValidATDorATA && !consol.Logs.GetAllLogs().OfType<StmALog>().Any(log => log.SL_SE_NKEvent == Events.MessageSentCode
						&& log.Parameters.TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, out var messageType)
						&& messageType == "Shipping Instruction"
						&& log.Parameters.TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, out var department)
						&& department == "Carrier"
				);
			}
		}

		CommonContainer UpdateContainer(CommonConsol parentConsol, IXmlEventValueObject eventAdded, ZString containerID)
		{
			var containerISOType = eventAdded.Context.ContainerISOCode;
			var containerResult = CommonUniversalFreightHelper.UpdateContainerNumberAndType(parentConsol, containerID, containerISOType, logger);

			if (containerResult != null)
			{
				CommonUniversalFreightHelper.AddCIDEventToContainer(containerResult, containerID, logger);
			}

			return containerResult;
		}
	}
}
