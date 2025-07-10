using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.DataTransfer.Universal
{
	public class ConsolContainerCollectionReader<TContainer, TConsol> : ContainerCollectionReader<TContainer>
		where TContainer : CommonContainer
		where TConsol : CommonConsol
	{
		public ConsolContainerCollectionReader(DataObjectList<Container> containers, IXmlImportLogger logger, UniversalObjectFactory factory, TConsol consol, IContainerLinkManager<TConsol> linkManager)
			: base(containers, logger, factory, consol.Containers)
		{
			this.consol = Argument.NotNull(consol, "consol");
			this.linkManager = Argument.NotNull(linkManager, "linkManager");
		}

		readonly TConsol consol;
		readonly IContainerLinkManager<TConsol> linkManager;
		readonly Dictionary<Container, TContainer> containerDoToContainerBoMap = new Dictionary<Container, TContainer>();

		DataObjectList<Container> ContainerDataObjects
		{
			get
			{
				return containerDataObjects ?? (containerDataObjects = new DataObjectList<Container>(DataObjects));
			}
		}
		DataObjectList<Container> containerDataObjects;

		protected override TContainer FindMatchingBusinessObject(Container dataObject)
		{
			if (containerDoToContainerBoMap.ContainsKey(dataObject))
			{
				return containerDoToContainerBoMap[dataObject];
			}

			return null;
		}

		protected override TContainer ReadIntoBusinessObject(Container dataObject, TContainer container)
		{
			var reader = new ContainerWithPackLinesDataObjectReader<TContainer, TConsol>(dataObject, logger, factory, linkManager, data => container, data => (TContainer)consol.Containers.AddNew());

			return reader.ReadIntoBusinessObject();
		}

		protected override bool SkipEntity(Container dataObject)
		{
			var matchedContainerBO = CommonUniversalFreightHelper.MatchedContainer<TContainer>(dataObject, ContainerDataObjects, consol);

			if (matchedContainerBO != null)
			{
				containerDoToContainerBoMap.Add(dataObject, matchedContainerBO);
			}

			return ContentType == CollectionContent.Partial && matchedContainerBO == null;
		}

		protected override void ProcessSkippedEntities(IEnumerable<Container> skippedEntities, IEnumerable<TContainer> unmatchedBusinessObjects)
		{
			if (skippedEntities != null && skippedEntities.Any())
			{
				var containersWithNumber = skippedEntities.Where(c => !c.ContainerNumber.GetValueOrDefault().IsEmpty).ToArray();
				var containerDOsNeedCreatNewContainerBO = skippedEntities.Except(containersWithNumber).ToList();

				foreach (var containerDO in containersWithNumber)
				{
					var containerType = GetContainerType(containerDO);
					var bestMatchContainer = FindBestPotentialMatch(unmatchedBusinessObjects, containerType);

					if (bestMatchContainer != null)
					{
						ReadIntoExistingContainer(bestMatchContainer, containerDO);
					}
					else
					{
						containerDOsNeedCreatNewContainerBO.Add(containerDO);
					}
				}

				foreach (var containerDO in containerDOsNeedCreatNewContainerBO)
				{
					var reader = new ContainerWithPackLinesDataObjectReader<TContainer, TConsol>(containerDO, logger, factory, linkManager, null, dataObject => (TContainer)consol.Containers.AddNew());
					reader.ReadIntoBusinessObject();
				}
			}
		}

		void ReadIntoExistingContainer(TContainer matchedContainerBO, Container containerDO)
		{
			var containerNeedToPopulate = matchedContainerBO;
			var containerType = GetContainerType(containerDO);
			var matchContainerIsMulti = false;

			if (matchedContainerBO.JC_ContainerCount > 1)
			{
				var newContainer = (TContainer)consol.Containers.AddNew();
				containerNeedToPopulate = newContainer;
				matchedContainerBO.JC_ContainerCount--;
				matchContainerIsMulti = true;
			}

			var containerTypeSetter = GetContainerTypeSetter(containerType, matchedContainerBO.RefContainer);
			var reader = new ContainerWithPackLinesDataObjectReader<TContainer, TConsol>(containerDO, logger, factory, linkManager, data => containerNeedToPopulate, null, containerTypeSetter);

			reader.ReadIntoBusinessObject();

			if (matchContainerIsMulti && !matchedContainerBO.RefContainer.RC_IsActive)
			{
				logger.Log(LogType.Warning, Res.GetString("aa5d15cc-e763-4bc6-82ab-7dbcad958e07"
					, "Matched container's Type has also been unset since it was inactive. Please go to the consol and choose a container type."
					, matchedContainerBO.RefContainer.RC_Code));
				matchedContainerBO.JC_RC = ZGuid.Empty;
			}

			CommonUniversalFreightHelper.AddCIDEventToContainer(containerNeedToPopulate, containerDO.ContainerNumber.GetValueOrDefault(), logger);
		}

		TContainer FindBestPotentialMatch(IEnumerable<TContainer> containers, ZString containerType)
		{
			TContainer result = null;
			var containersWithoutNumber = containers.Where(c => c.JC_ContainerNum.IsEmpty).ToArray();

			if (containersWithoutNumber.Length == 1)
			{
				var refContainer = factory.LoadFromUniqueKey<RefContainer>(RefContainerSchema.RC_Code, containerType);
				var isMatchedContainerType = containersWithoutNumber[0].RefContainer != null && containersWithoutNumber[0].RefContainer.RC_Code == containerType;

				if (isMatchedContainerType)
				{
					result = containersWithoutNumber[0];
				}
				else if (BusinessObjects.Length == 1 && ContainerDataObjects.Count == 1)
				{
					if ((containersWithoutNumber[0].JC_ContainerCount > 1 && refContainer == null)
						|| containersWithoutNumber[0].JC_ContainerCount == 1)
					{
						result = containersWithoutNumber[0];
					}
				}
			}
			else if (containersWithoutNumber.Length > 1)
			{
				var matchedContainers = containersWithoutNumber.Where(c => c.RefContainer != null && c.RefContainer.RC_Code == containerType).ToArray();

				if (matchedContainers.Length >= 1)
				{
					result = matchedContainers[0];
				}
			}

			return result;
		}

		Action<TContainer> GetContainerTypeSetter(ZString containerType, RefContainer fallbackRefContainer)
		{
			var refContainer = factory.LoadFromUniqueKey<RefContainer>(RefContainerSchema.RC_Code, containerType);

			return container =>
			{
				var refContainerRCCode = fallbackRefContainer == null ? (ZString)"null" : fallbackRefContainer.RC_Code;
				if (refContainer == null)
				{
					if (fallbackRefContainer != null && fallbackRefContainer.RC_IsActive)
					{
						container.JC_RC = fallbackRefContainer.PK;
						logger.Log(LogType.Warning, Res.GetString("e7efea95-4ff9-49c4-9ea2-544d1a4386d7"
							, "Container Type '{0}' is invalid, using the type from the matched container."
							, containerType));
					}
					else
					{
						container.JC_RC = ZGuid.Empty;
						logger.Log(LogType.Warning, Res.GetString("78fba7c9-6854-4abf-af0b-234bfd0b5362"
							, "Container Type '{0}' is invalid and matched container's type {1} is inactive so it will not be used. Please go to the consol and choose a container type."
							, containerType, refContainerRCCode));
					}
				}
				else
				{
					if (refContainer.RC_IsActive)
					{
						container.JC_RC = refContainer.PK;
						logger.Log(LogType.Information,
							Res.GetString("dd30536b-9907-4ce6-b796-97976fb82e34", "Successfully loaded matching Container Type."));
					}
					else if (fallbackRefContainer != null && fallbackRefContainer.RC_IsActive)
					{
						container.JC_RC = fallbackRefContainer.PK;
						logger.Log(LogType.Warning, Res.GetString("79b8105b-8cd1-4c83-a48c-6a945cbb7092"
							, "Container Type '{0}' is inactive so it will not be used, using the type from the matched container."
							, containerType));
					}
					else
					{
						container.JC_RC = ZGuid.Empty;
						logger.Log(LogType.Warning, Res.GetString("ba346630-79e2-43de-aa02-11864d9c0c70"
							, "Container Type '{0}' is inactive so it will not be used, matched container's type '{1}' is also inactive so it will not be used. Please go to the consol and choose a container type."
							, containerType, refContainerRCCode));
					}
				}
			};
		}

		ZString GetContainerType(Container containerDO)
		{
			return containerDO.ContainerType != null && containerDO.ContainerType.Code.HasValue
											? containerDO.ContainerType.Code.Value
											: ZString.Empty;
		}
	}
}
