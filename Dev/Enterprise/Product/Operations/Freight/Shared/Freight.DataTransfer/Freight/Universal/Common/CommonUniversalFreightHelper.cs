using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Extensions;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Params = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;

namespace Enterprise.Freight.DataTransfer.Universal
{
	public static class CommonUniversalFreightHelper
	{
		public static T MatchedContainer<T>(Container containerDO, DataObjectList<Container> containers, CommonConsol consolBO)
			where T : CommonContainer
		{
			var containerNumber = containerDO.ContainerNumber.GetValueOrDefault();
			if (!containerNumber.IsEmpty)
			{
				return (T)consolBO.Containers.FindAnyByContainerNumber(containerNumber);
			}

			var containerType = containerDO.ContainerType.GetCodeAsUpperCase();
			var containerCount = containerDO.ContainerCount;

			var matchingContainersOnConsol = consolBO.Containers
				.Cast<T>()
				.Where(c => c.JC_ContainerNum.IsEmpty
						&& c.RefContainer != null
						&& c.RefContainer.RC_Code == containerType
						&& c.JC_ContainerCount == containerCount)
				.Take(2)
				.ToArray();

			var matchingContainersInXML = containers
				.Where(cXML => cXML.ContainerNumber.GetValueOrDefault().IsEmpty
							&& cXML.ContainerType.GetCodeAsUpperCase() == containerType
							&& cXML.ContainerCount == containerCount)
				.Take(2)
				.ToArray();

			if (matchingContainersOnConsol.Length == 1 && matchingContainersInXML.Length == 1)
			{
				return matchingContainersOnConsol[0];
			}

			return null;
		}

		public static void RemoveDuplicateTransportsOnShipments(CommonConsol consolBO)
		{
			foreach (ITransportParent shipment in consolBO.Shipments)
			{
				var shipmentTransports = shipment.Transports;
				var consolTransports = consolBO.Transports;

				for (var shipmentTransportIndex = 0; shipmentTransportIndex < shipmentTransports.Count; shipmentTransportIndex++)
				{
					var transport = shipmentTransports[shipmentTransportIndex];

					for (var consolTransportIndex = 0; consolTransportIndex < consolTransports.Count; consolTransportIndex++)
					{
						var transportToCheck = consolTransports[consolTransportIndex];
						if (transportToCheck.JW_RL_NKLoadPort == transport.JW_RL_NKLoadPort
							&& transportToCheck.JW_RL_NKDiscPort == transport.JW_RL_NKDiscPort)
						{
							if (transportToCheck.PK == transport.PK)
							{
								consolTransports.Add(transportToCheck.Clone());
							}

							shipmentTransports.RemoveAndDelete(transport);
							shipmentTransportIndex--;
							break;
						}
					}
				}
			}
		}

		public static bool IsUNDGDataObjectMatchedToUNDGBusinessObject(UNDG undgDO, UNDGDataItem undgBO)
		{
			if (undgDO == null || undgBO == null)
			{
				return false;
			}

			var undgCodeFromDO = undgDO.UNDGCode.GetValueOrDefault();
			var imoClassFromDO = undgDO.IMOClass.GetValueOrDefault();

			return (!undgCodeFromDO.IsEmpty && undgBO.Substance != null && undgBO.Substance.DG_Code == undgCodeFromDO)
					|| (undgBO.DI_DG.IsEmpty && undgCodeFromDO.IsEmpty && !imoClassFromDO.IsEmpty && imoClassFromDO == undgBO.DI_IMOClass);
		}

		#region Container matching/updating

		internal static void AddCIDEventToContainer(CommonContainer container, string containerNum, IXmlImportLogger logger)
		{
			var parameters = new[]
				{
					Params.New.AsKeyFor(containerNum),
					Params.Type.AsKeyFor(Constants.EventReferenceParameterTypes.ContainerID),
					Params.Reason.AsKeyFor(Constants.EventReferenceParameterReasons.ContainerNumberAdvised)
				};

			container.Logs.AddNew(AutoEvents.ChangeOfIdentifier, parameters);

			logger?.LogBoth(LogType.Information, Res.GetString("3591576e-eb0c-4a44-a82a-bdbf24f28907", "Container number advised: {0}.", containerNum));
		}

		internal static CommonContainer UpdateContainerNumberAndType(CommonConsol consol, ZString newNumber, ZString containerISOType, IXmlImportLogger logger)
		{
			if (consol.IsAir && consol.JK_ConsolMode == Constants.ContainerModes.Loose)
			{
				logger?.LogBoth(LogType.Information, Res.GetString("1e44ec8d-d928-4a68-bdb3-2aa2bc5bb0e2", "This Air Consol is of Loose (LSE) type. Containers are not required for loose packages and will not be created for this Consol."));
				return null;
			}

			if (newNumber.Length > JobContainerSchema.JC_ContainerNum.MaxLength)
			{
				logger?.LogBoth(LogType.Information, Res.GetString("33156d3f-2d03-44e6-9ffe-519f4ecebf53", "Container Number {0} exceeds the valid max length. Containers will not be created for this Consol.", newNumber));
				return null;
			}

			if (!CheckCanCreateNewContainerFromMatchConsolResultServiceOrRegistry(consol, newNumber, logger))
			{
				return null;
			}

			var potentialMatch = FindBestPotentialMatch(consol, containerISOType, logger);

			if (potentialMatch != null)
			{
				return UpdateContainerNumberAndTypeWithPotentialMatch(consol, potentialMatch, newNumber, containerISOType, logger);
			}

			var newContainer = CreateNewContainer(consol, newNumber, logger);

			if (!containerISOType.IsEmpty)
			{
				TryUpdateContainerTypeFromISOCode(newContainer, containerISOType, logger);
			}

			return newContainer;
		}

		static CommonContainer FindBestPotentialMatch(CommonConsol consol, ZString containerISOType, IXmlImportLogger logger)
		{
			CommonContainer result = null;
			var eligibleContainers = System.Array.Empty<CommonContainer>();
			var activeContainersWithoutNumber = consol.Containers.Cast<CommonContainer>()
				.Where(c => c.JC_ContainerNum.IsEmpty && (c.RefContainer?.RC_IsActive ?? false))
				.ToArray();

			if (containerISOType.IsEmpty || IsPotentialMatch(consol, containerISOType, logger))
			{
				eligibleContainers = activeContainersWithoutNumber.ToArray();
			}

			if (eligibleContainers.Length == 1 && containerISOType.IsEmpty)
			{
				result = eligibleContainers[0];
			}

			if (result == null && !containerISOType.IsEmpty)
			{
				result = GetBestMatchingContainer(activeContainersWithoutNumber, containerISOType);
			}

			return result;
		}

		static bool IsPotentialMatch(CommonConsol consol, ZString containerISOType, IXmlImportLogger logger)
		{
			return GetMatchingRefContainer(consol.Factory, containerISOType, logger) != null;
		}

		public static RefContainer GetMatchingRefContainer(BusinessObjectFactory factory, ZString containerISOType, IXmlImportLogger logger)
		{
			var refContainers = factory.Load<RefContainer>(new ZQuery(RefContainerSchema.RC_ISOType, containerISOType));
			switch (refContainers.Length)
			{
				case 0:
					logger?.LogBoth(LogType.Warning, Res.GetString("33be407e-3c10-40f9-a7d2-e8319ebc4903", "There are no Container Types with ISO Code: {0}", containerISOType));
					return null;

				case 1:
					if (!refContainers[0].RC_IsActive)
					{
						logger?.LogBoth(LogType.Warning, Res.GetString("0a975e80-333d-4c61-b180-28772d02664b", "There is 1 Container Type with ISO Code: {0}, but it is inactive", containerISOType));
					}

					return refContainers[0];

				default:
					var activeRefContainers = refContainers.Where(x => x.RC_IsActive).ToArray();

					if (activeRefContainers.Length == 1)
					{
						return activeRefContainers[0];
					}

					logger?.LogBoth(LogType.Warning,
						activeRefContainers.Length > 1
							? Res.GetString("9840a74b-bb4c-4669-bf6c-e52da0603df1",
								"2 or more active Container Types have ISO Code: {0}, so none of these will be used.", containerISOType)
							: Res.GetString("3fc2a129-721f-4bc7-8340-34009a4d2eaa",
								"2 or more inactive Container Types have ISO Code: {0}, so none of these will be used.", containerISOType));
					return null;
			}
		}

		static CommonContainer UpdateContainerNumberAndTypeWithPotentialMatch(CommonConsol consol, CommonContainer candidateContainer, string newNumber, ZString containerISOType, IXmlImportLogger logger)
		{
			CommonContainer resultContainer;

			var matchesISOType = candidateContainer.RefContainer != null && candidateContainer.RefContainer.RC_ISOType == containerISOType;

			if (candidateContainer.JC_ContainerCount == 1)
			{
				candidateContainer.JC_ContainerNum = newNumber;

				if (candidateContainer.RefContainer != null && !candidateContainer.RefContainer.RC_IsActive) //If the ISO type is not active, set type to empty
				{
					logger?.LogBoth(LogType.Warning, Res.GetString("c734a6b3-ee86-4a3a-9cf6-1f0aa730a584", "Since the Container Type with ISO Code: {0} is inactive, container {1}'s container type is unset. Please go to the consol and choose a container type.", candidateContainer.RefContainer.ISOType.ISOCode, candidateContainer.JC_ContainerNum));
					candidateContainer.JC_RC = ZGuid.Empty;
				}

				resultContainer = candidateContainer;
			}
			else
			{
				if (!CheckCanCreateNewContainerFromMatchConsolResultServiceOrRegistry(consol, newNumber, logger))
				{
					return null;
				}

				var newContainer = CreateNewContainer(consol, newNumber, logger);
				var containerTypeUpdated = !containerISOType.IsEmpty && TryUpdateContainerTypeFromISOCode(newContainer, containerISOType, logger);

				using (candidateContainer.SuppressDefaultingOfWeights())
				{
					if (matchesISOType)
					{
						candidateContainer.JC_ContainerCount--;
					}
					else if (!containerTypeUpdated || (candidateContainer.RefContainer != null && newContainer.RefContainer != null))
					{
						candidateContainer.JC_ContainerCount--;
						newContainer.JC_RC = candidateContainer.JC_RC;
					}
				}

				if (newContainer.RefContainer == null || !newContainer.RefContainer.RC_IsActive)
				{
					logger?.LogBoth(LogType.Warning, Res.GetString("9115526b-161d-42e1-86f6-8a017700e12d", "The new container {0}'s container type is unset. Please go to the consol and choose a container type.", newContainer.JC_ContainerNum));
					if (newContainer.RefContainer != null)
					{
						newContainer.JC_RC = ZGuid.Empty;
					}
				}

				if (candidateContainer.RefContainer != null && !candidateContainer.RefContainer.RC_IsActive)
				{
					logger?.LogBoth(LogType.Warning, Res.GetString("82cc27ce-7314-420a-84dc-1e00844eb095", "Since the Container Type with ISO Code: {0} is inactive, the existing consol container's container type is unset. Please go to the consol and choose a container type.", candidateContainer.RefContainer.ISOType.ISOCode));
					candidateContainer.JC_RC = ZGuid.Empty;
				}

				resultContainer = newContainer;
			}

			return resultContainer;
		}

		static CommonContainer CreateNewContainer(CommonConsol consol, string newNumber, IXmlImportLogger logger)
		{
			var newContainer = consol.Containers.AddNew();
			newContainer.JC_ContainerNum = newNumber;

			if (newContainer.IsMeasurementsOutOfRange)
			{
				newContainer.Delete();
				throw new DataObjectReadFailureException(Res.GetString("d29ca763-1586-4786-8d46-29b3dcafe89f",
					"Container allocated pack line weight and/or volume would exceed the database maximum value."));
			}

			logger?.LogBoth(LogType.Information, Res.GetString("745de056-f9f9-4f15-a1ab-7243b3a61f00", "Created new container {0}.", newNumber));

			return newContainer;
		}

		static bool CheckCanCreateNewContainerFromMatchConsolResultServiceOrRegistry(CommonConsol consol, string containerNumber, IXmlImportLogger logger)
		{
			if (FreightDataRegistry.Instance.AutomaticContainerCreation.Value.IsNeverCreate)
			{
				logger?.LogBoth(LogType.Warning,
					Res.GetString("eaedd4ca-4337-4f54-a9d0-faf23899ad16",
						@"Container Automation has received information on container {0} for Consol {1}.
Container cannot be created on the Consol as ""Registry > Freight > Global Tracking > Automatic Container Creation"" is ""Never Create"".",
						containerNumber,
						consol.HumanReadableShortcutName));
				return false;
			}

			var service = MatchConsolResultService.GetInstance(consol.Factory);
			if (service != null && service.HasConsolsInYearRange)
			{
				string message = string.Empty;
				bool isCoLoadConsol = consol.JK_AgentType == Constants.AgentType.CoLoad;
				switch (service.MatchType)
				{
					case MatchType.BookingReference:
						message = Res.GetString("72c03476-0cde-4263-b7f3-14bf4f2ef9e0",
@"System cannot create new Container {0} on Consol {1} as there are multiple consols with the same Booking Number {2}.
Consolidations with the same Booking Number:
{3}", containerNumber, consol.HumanReadableShortcutName, isCoLoadConsol ? consol.JK_CoLoadBookingReference : consol.JK_BookingReference, string.Join(",", service.ConsolNames));
						break;

					case MatchType.MasterBill:
						message = Res.GetString("8170aced-701d-3794-4eca-f07cc97569c4",
@"System cannot create new Container {0} on Consol {1} as there are multiple consols with the same Master Bill Number {2}.
Consolidations with the same Master Bill Number:
{3}", containerNumber, consol.HumanReadableShortcutName, isCoLoadConsol ? consol.JK_CoLoadMasterBill : consol.JK_MasterBillNum, string.Join(",", service.ConsolNames));
						break;

					case MatchType.All:
						message = Res.GetString("133c1015-704d-4f81-4b59-ec84654ec0f3",
@"System cannot create new Container {0} on Consol {1} as there are multiple consols with the same Booking Number {2} and Master Bill Number {3}.
Matched Consolidations:
{4}", containerNumber, consol.HumanReadableShortcutName, isCoLoadConsol ? consol.JK_CoLoadBookingReference : consol.JK_BookingReference,
isCoLoadConsol ? consol.JK_CoLoadMasterBill : consol.JK_MasterBillNum, string.Join(",", service.ConsolNames));
						break;
				}

				logger?.LogBoth(LogType.Error, message);

				return false;
			}

			return true;
		}

		internal static void UpdateContainerType(CommonContainer container, ZString containerISOType, IXmlImportLogger logger)
		{
			Argument.NotNull(container, "container");

			if (!containerISOType.IsEmpty)
			{
				TryUpdateContainerTypeFromISOCode(container, containerISOType, logger);
			}
		}

		static bool TryUpdateContainerTypeFromISOCode(CommonContainer container, ZString containerISOType, IXmlImportLogger logger)
		{
			var newContainerType = CalculateContainerTypeFromISO(containerISOType, container.Factory, ZString.Empty);
			var currentContainerTypeISOGroup = container.Container?.ISOType.GroupCode ?? ZString.Empty;
			var currentContainerTypeSize = container.Container?.RC_ISOType.SubstringSafe(0, 2) ?? ZString.Empty;

			var shouldUpdateContainerType = newContainerType != null &&
					  (currentContainerTypeISOGroup != newContainerType.ISOType.GroupCode ||
					   (currentContainerTypeISOGroup == newContainerType.ISOType.GroupCode && currentContainerTypeSize != newContainerType.RC_ISOType.SubstringSafe(0, 2)));

			if (shouldUpdateContainerType)
			{
				UpdateContainerType(container, newContainerType.PK, logger);
				return true;
			}
			return false;
		}

		static void UpdateContainerType(CommonContainer container, ZGuid containerTypePK, IXmlImportLogger logger)
		{
			var previousType = container.RefContainer != null ? container.RefContainer.RC_Code.ToString() : Res.GetString("cf8b2dc0-0603-42c6-912f-f3f850d3657a", "(empty)");
			container.JC_RC = containerTypePK;

			if (logger != null && container.IsInDatabase)
			{
				logger.LogBoth(LogType.Information, Res.GetString("106441ea-25c8-4463-8ac9-be9e31f16ecc", "{0}: Updated container type from {1} to {2}.", container.ContainerCode, previousType, container.RefContainer.RC_Code));
			}
		}

		static RefContainer CalculateContainerTypeFromISO(ZString containerISOType, BusinessObjectFactory factory, ZString rcCodeToMatch)
		{
			var refContainers = new RefContainer.Loader(factory).LoadFromISOTypeWithGroupFallback(containerISOType).ToArray();

			if (refContainers.Length == 1)
			{
				return refContainers[0];
			}
			else if (refContainers.Length > 1 && !rcCodeToMatch.IsEmpty)
			{
				return refContainers.FirstOrDefault(r => r.RC_Code == rcCodeToMatch);
			}

			return null;
		}

		#endregion

		public static bool HasSCAC(this OrgHeader org, ZString scac)
		{
			return org?.CustomsCodes.Cast<OrgCusCode>()
				.Any(c => c.OK_CodeType == OrgCusCode.CodeTypes.CarrierCode
					&& c.OK_RN_NKCodeCountry == Core.Constants.CountryCodes.UnitedStates
					&& c.OK_CustomsRegNo.ToUpper() == scac.ToUpper()) ?? false;
		}

		static CommonContainer GetBestMatchingContainer(CommonContainer[] containers, ZString containerISOType)
		{
			var maxScoreGroup = containers.GroupBy(c => GetContainerScore(c, containerISOType))
				.MaxBySafe(g => g.Key);

			if (maxScoreGroup != null && maxScoreGroup.Key > 0)
			{
				if (maxScoreGroup.Count() == 1)
				{
					return maxScoreGroup.First();
				}
				else
				{
					var commodityAndReleaseNumGroups = maxScoreGroup.GroupBy(c => new { c.JC_RH_NKContainerCommodityCode, c.JC_ReleaseNum });
					if (commodityAndReleaseNumGroups.Count() == 1)
					{
						return commodityAndReleaseNumGroups
							.First()
							.GroupBy(c => c.JC_ContainerCount)
							.MinBySafe(c => c.Key)
							.First();
					}
				}
			}

			return null;
		}

		static int GetContainerScore(CommonContainer container, ZString containerISOType)
		{
			Argument.NotNull(container, nameof(container));

			var score = 0;

			var currentContainerType = container.RefContainer;
			if (currentContainerType == null)
			{
				return score;
			}

			var newContainerType = CalculateContainerTypeFromISO(containerISOType, container.Factory, currentContainerType.RC_Code);
			if (newContainerType == null)
			{
				var currentContainerISOType = currentContainerType.RC_ISOType;
				if (currentContainerISOType.Length >= 3 && containerISOType.Length >= 3)
				{
					if (currentContainerISOType.Substring(0, 3) == containerISOType.Substring(0, 3))
					{
						return 2;
					}
					if (containerISOType[0] == currentContainerISOType[0] && containerISOType[2] == currentContainerISOType[2])
					{
						return 1;
					}
				}
				return 0;
			}

			if (currentContainerType.RC_ISOType == containerISOType)
			{
				score += 16;
			}

			var firstCurrentGroupCode = currentContainerType.ISOType.GroupCode.Left(1);
			var firstNewGroupCode = newContainerType.ISOType.GroupCode.Left(1);

			if (newContainerType.ISOType.Length == currentContainerType.ISOType.Length
				&& firstNewGroupCode == firstCurrentGroupCode)
			{
				score += newContainerType.ISOType.Width == currentContainerType.ISOType.Width
					|| newContainerType.ISOType.Height == currentContainerType.ISOType.Height
					? 8 : 4;
			}

			return score;
		}
	}
}
