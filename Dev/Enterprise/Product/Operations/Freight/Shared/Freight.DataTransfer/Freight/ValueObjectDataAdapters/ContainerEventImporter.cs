using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.DataTransfer
{
	public class ContainerEventImporter
	{
		#region Import

		public void Import(ValueObjectImportContext importContext, Xsd.ContainerEvent containerEvent)
		{
			NotificationBuffer notifications = new NotificationBuffer(importContext);
			ValueObjectImportContext newImportContext = new ValueObjectImportContext(importContext.FactoryProvider, notifications);

			CommonContainer container = FindContainer(newImportContext, containerEvent);
			if (!notifications.HasErrors)
			{
				string message = "";
				string containerDescription = GetContainerDescription(containerEvent);
				if (container != null)
				{
					if (containerEvent.EventDate.IsValidSmallDateTime)
					{
						UpdateContainerDate(newImportContext, container, containerEvent);
						message = Res.GetString("3797e969-13c4-44c5-b3ed-9206f50a4f1e", "Container with {0}- {1} updated.", containerDescription, containerEvent.EventType);
					}
					else
					{
						message = Res.GetString("84835cd8-2ac6-40b5-afa6-38e1fc17475c", "Invalid date provided='") + containerEvent.EventDate + "'";
					}
				}
				else
				{
					message = Res.GetString("e0b4972c-bc55-4114-b4ff-6d95a8f559fd", "Could not find container with {0}", containerDescription);
				}
				notifications.Notify(new InfoNotification(message));
			}
		}

		#region UpdateContainerDate

		void UpdateContainerDate(ValueObjectImportContext context, CommonContainer container, Xsd.ContainerEvent containerEvent)
		{
			var eventDate = containerEvent.EventDate.ToSmallDateTime();
			switch (containerEvent.EventType)
			{
				case Xsd.ContainerEventEventType.ContainerYardEmptyPickupGateOut:
					container.JC_ContainerYardEmptyPickupGateOut = eventDate;
					break;
				case Xsd.ContainerEventEventType.DepartureCartageAdvised:
					container.JC_DepartureCartageAdvised = eventDate;
					break;
				case Xsd.ContainerEventEventType.DepartureCartageComplete:
					container.JC_DepartureCartageComplete = eventDate;
					break;
				case Xsd.ContainerEventEventType.DepartureCartageDemurrageTime:
					container.DepartureTruckWaitTime = eventDate;
					break;
				case Xsd.ContainerEventEventType.ArrivalEstimatedDelivery:
					container.JC_ArrivalEstimatedDelivery = eventDate;
					break;
				case Xsd.ContainerEventEventType.ArrivalCartageAdvised:
					container.JC_ArrivalCartageAdvised = eventDate;
					break;
				case Xsd.ContainerEventEventType.ArrivalCartageComplete:
					container.JC_ArrivalCartageComplete = eventDate;
					break;
				case Xsd.ContainerEventEventType.ArrivalCartageDemurrageTime:
					container.ArrivalTruckWaitTime = eventDate;
					break;
				case Xsd.ContainerEventEventType.FCLWharfGateIn:
					container.JC_FCLWharfGateIn = eventDate;
					break;
				case Xsd.ContainerEventEventType.FCLOnBoardVessel:
					container.JC_FCLOnBoardVessel = eventDate;
					break;
				case Xsd.ContainerEventEventType.FCLUnloadFromVessel:
					container.JC_FCLUnloadFromVessel = eventDate;
					break;
				case Xsd.ContainerEventEventType.FCLAvailable:
					container.JC_FCLAvailable = eventDate;
					break;
				case Xsd.ContainerEventEventType.FCLWharfGateOut:
					container.JC_FCLWharfGateOut = eventDate;
					break;
				case Xsd.ContainerEventEventType.FCLStorageCommences:
					container.JC_ArrivalCTOStorageStartDate = eventDate;
					break;
				case Xsd.ContainerEventEventType.LCLUnpack:
					container.JC_LCLUnpack = eventDate;
					break;
				case Xsd.ContainerEventEventType.LCLAvailable:
					container.JC_LCLAvailable = eventDate;
					break;
				case Xsd.ContainerEventEventType.LCLStorageCommences:
					container.JC_LCLStorageCommences = eventDate;
					break;
				case Xsd.ContainerEventEventType.EmptyRequired:
					container.JC_EmptyRequired = eventDate;
					break;
				case Xsd.ContainerEventEventType.EmptyReadyForReturn:
					container.JC_EmptyReadyForReturn = eventDate;
					break;
				case Xsd.ContainerEventEventType.EmptyReturnBy:
					container.JC_EmptyReturnedBy = eventDate;
					break;
				case Xsd.ContainerEventEventType.ContainerYardEmptyReturnGateIn:
					container.JC_ContainerYardEmptyReturnGateIn = eventDate;
					break;
				case Xsd.ContainerEventEventType.DepartureEstimatedPickup:
					container.JC_DepartureEstimatedPickup = eventDate;
					break;
				case Xsd.ContainerEventEventType.ArrivalSlotTimeBooked:
					container.JC_ArrivalSlotDateTime = eventDate;
					context.SetPropertyInfoValueIfValueNotEmpty(container.JC_ArrivalSlotReferenceInfo, containerEvent.EventReference);
					break;
				case Xsd.ContainerEventEventType.DepartureSlotTimeBooked:
					container.JC_DepartureSlotDateTime = eventDate;
					context.SetPropertyInfoValueIfValueNotEmpty(container.JC_DepartureSlotReferenceInfo, containerEvent.EventReference);
					break;
				default:
					context.AddError((NoResString)"Unknown event type = " + containerEvent.EventType);
					break;
			}
		}

		#endregion

		string GetContainerDescription(Xsd.ContainerEvent containerEvent)
		{
			string message = ZString.Empty;
			if (!containerEvent.EnterpriseJobNumber.IsEmpty)
			{
				message += Res.GetString("01ab56ea-e0ca-45ec-8016-92b8fddb0746", "Job='{0}'", containerEvent.EnterpriseJobNumber) + " ";
			}
			message += Res.GetString("218bdd10-8110-4c5c-a69f-e1a2346c1a22", "Container Number='{0}'", containerEvent.ContainerNumber) + " ";
			if (!containerEvent.Vessel.VesselName.IsEmpty)
			{
				message += Res.GetString("7e600e81-d884-480f-8ac1-90cbce166021", "Vessel Name='{0}'", containerEvent.Vessel.VesselName.Trim()) + " ";
			}
			if (!containerEvent.Vessel.Voyage.IsEmpty)
			{
				message += Res.GetString("3add496a-2739-4b1d-90f8-5165eb300455", "Voyage='{0}'", containerEvent.Vessel.Voyage) + " ";
			}
			if (!containerEvent.Vessel.Lloyds.IsEmpty)
			{
				message += Res.GetString("0401a3b4-de9b-4dd1-a94c-0f430b1f9a3a", "Lloyds='{0}'", containerEvent.Vessel.Lloyds) + " ";
			}
			if (!containerEvent.Vessel.ArrivalDate.IsEmpty)
			{
				message += Res.GetString("5dcb64aa-a643-4e4c-b9d9-a0c4cd628e3c", "Arrival Date='") + containerEvent.Vessel.ArrivalDate.ToShortDateString() + "' ";
			}
			return message;
		}

		#endregion

		#region FindContainer

		CommonContainer FindContainer(ValueObjectImportContext importContext, Xsd.ContainerEvent containerEvent)
		{
			CommonContainer result = null;
			if (!containerEvent.ContainerNumber.IsEmpty &&
				(!containerEvent.Vessel.VesselName.IsEmpty || !containerEvent.Vessel.Lloyds.IsEmpty) &&
				!containerEvent.Vessel.Voyage.IsEmpty)
			{
				result = FindContainer(importContext, containerEvent.ContainerNumber, containerEvent.Vessel.VesselName, containerEvent.Vessel.Lloyds, containerEvent.Vessel.Voyage);
			}
			else if (!containerEvent.EnterpriseJobNumber.IsEmpty && !containerEvent.ContainerNumber.IsEmpty)
			{
				result = FindContainer(importContext, containerEvent.EnterpriseJobNumber, containerEvent.ContainerNumber);
			}
			else if (!containerEvent.ContainerNumber.IsEmpty && containerEvent.Vessel.ArrivalDate.IsValid)
			{
				result = FindContainer(importContext, containerEvent.ContainerNumber, containerEvent.Vessel.ArrivalDate);
			}
			else
			{
				importContext.Notify(new ErrorNotification(ErrorType.XmlSchemaValidation, Res.GetString("b4e5673b-9103-406f-8a44-c82b66ca644a", "Key information was not specified (Container Number, Vessel Name, Voyage), ({0} Job Number, Container Number) or (Container Number, Arrival Date).", Core.Constants.ProductName)));
			}
			return result;
		}

		#region From Container/Vessel/Lloyds/Voyage

		CommonContainer FindContainer(ValueObjectImportContext importContext, ZString containerNumber, ZString vesselName, ZString lloyds, ZString voyage)
		{
			if (!lloyds.IsEmpty)
			{
				RefVessel[] possibleVessel = importContext.Factory.Load<RefVessel>(new ZQuery(RefVesselSchema.RV_LloydsNumber, lloyds));
				if (possibleVessel.Length == 1)
				{
					vesselName = possibleVessel[0].RV_Name;
				}
			}
			return FindContainer(importContext, containerNumber, vesselName, voyage);
		}

		CommonContainer FindContainer(ValueObjectImportContext importContext, ZString containerNumber, ZString vesselName, ZString voyage)
		{
			ZQuery query = new ZQuery();
			query.AddToFilter(JobContainerSchema.JC_ContainerNum, containerNumber);
			query.AddToFilter(JobContainerSchema.JC_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThan, ZDateTime.Now.AddMonths(-6));

			CommonContainer[] containers = importContext.Factory.Load<CommonContainer>(query);
			foreach (CommonContainer container in containers)
			{
				if (IsContainerMatching(container, vesselName, voyage))
				{
					return container;
				}
			}
			return null;
		}

		bool IsContainerMatching(CommonContainer container, ZString vesselName, ZString voyage)
		{
			bool result = false;
			result = result || IsContainerMatchingForConsol(container, vesselName, voyage);
			result = result || IsContainerMatchingForDeclaration(container, vesselName, voyage);
			return result;
		}

		bool IsContainerMatchingForConsol(CommonContainer container, ZString vesselName, ZString voyage)
		{
			return container.Consol != null && container.Consol.JK_JX_JV_NKVessel == vesselName && container.Consol.JK_JX_JV_VoyageFlight == voyage;
		}

		bool IsContainerMatchingForDeclaration(CommonContainer container, ZString vesselName, ZString voyage)
		{
			ZQuery query = new ZQuery(CusContainerSchema.CO_JC, container.PK);
			BusinessObject[] cusContainers = (BusinessObject[])container.Factory.Load<Enterprise.Integration.Customs.Shared.IBaseCusContainer>(query);
			foreach (BusinessObject cusContainer in cusContainers)
			{
				BusinessObject declaration = (BusinessObject)cusContainer["Declaration"];
				if ((ZString)declaration[JobDeclarationSchema.JE_VesselName] == vesselName &&
					(ZString)declaration[JobDeclarationSchema.JE_VoyageFlightNo] == voyage)
				{
					return true;
				}
			}
			return false;
		}

		#endregion

		#region From Job Number/Container

		CommonContainer FindContainer(ValueObjectImportContext importContext, ZString jobNumber, ZString containerNumber)
		{
			CommonContainer result = FindContainerFromConsol(importContext, jobNumber, containerNumber)
									 ?? FindContainerFromDeclaration(importContext, jobNumber, containerNumber);
			return result;
		}

		CommonContainer FindContainerFromConsol(ValueObjectImportContext importContext, ZString jobNumber, ZString containerNumber)
		{
			CommonConsol consol = importContext.Factory.LoadFromNaturalKey<CommonConsol>(JobConsolSchema.JK_UniqueConsignRef, jobNumber);
			if (consol != null)
			{
				foreach (CommonContainer container in consol.Containers)
				{
					if (container.JC_ContainerNum.ToUpper() == containerNumber.ToUpper())
					{
						return container;
					}
				}
			}
			return null;
		}

		CommonContainer FindContainerFromDeclaration(ValueObjectImportContext importContext, ZString jobNumber, ZString containerNumber)
		{
			ZQuery query = new ZQuery();
			query.AddToFilter(JobDeclarationSchema.JE_DeclarationReference, jobNumber);
			BusinessObject declaration = (BusinessObject)importContext.Factory.LoadTop1<Enterprise.Integration.Customs.IBaseJobDeclaration>(query);

			if (declaration != null)
			{
				foreach (BusinessObject customsContainer in (BusinessObjectCollection)declaration["CusContainers"])
				{
					if (containerNumber.ToUpper() == ((ZString)customsContainer[CusContainerSchema.CO_ContainerNumber]).ToUpper())
					{
						return importContext.Factory.Load<CommonContainer>((ZGuid)customsContainer[CusContainerSchema.CO_JC]);
					}
				}
			}
			return null;
		}

		#endregion

		#region From Container/Arrival

		CommonContainer FindContainer(ValueObjectImportContext importContext, ZString containerNumber, ZDateTime arrivalDate)
		{
			ZQuery query = new ZQuery();
			query.AddToFilter(JobContainerSchema.JC_ContainerNum, containerNumber);
			query.AddToFilter(JobContainerSchema.JC_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThan, ZDateTime.Now.AddMonths(-6));

			CommonContainer[] containers = importContext.Factory.Load<CommonContainer>(query);
			foreach (CommonContainer container in containers)
			{
				if (IsContainerMatching(container, arrivalDate))
				{
					return container;
				}
			}
			return null;
		}

		bool IsContainerMatching(CommonContainer container, ZDateTime arrivalDate)
		{
			return
				IsContainerMatchingForConsol(container, arrivalDate) ||
				IsContainerMatchingForDeclaration(container, arrivalDate);
		}

		bool IsContainerMatchingForConsol(CommonContainer container, ZDateTime arrivalDate)
		{
			return
				container.Consol != null &&
				container.Consol.JK_JX_JB_E_ARV.IsValid &&
				container.Consol.JK_JX_JB_E_ARV >= arrivalDate.AddDays(-7) &&
				container.Consol.JK_JX_JB_E_ARV <= arrivalDate.AddDays(7);
		}

		bool IsContainerMatchingForDeclaration(CommonContainer container, ZDateTime arrivalDate)
		{
			ZQuery query = new ZQuery(CusContainerSchema.CO_JC, container.PK);
			BusinessObject[] cusContainers = (BusinessObject[])container.Factory.Load<Enterprise.Integration.Customs.Shared.IBaseCusContainer>(query);
			foreach (BusinessObject cusContainer in cusContainers)
			{
				BusinessObject declaration = (BusinessObject)cusContainer["Declaration"];
				ZDateTime declarationArrivalDate = (ZDateTime)declaration[JobDeclarationSchema.JE_DateOfArrival];
				if (declarationArrivalDate.IsValid &&
					declarationArrivalDate >= arrivalDate.AddDays(-7) &&
					declarationArrivalDate <= arrivalDate.AddDays(7))
				{
					return true;
				}
			}
			return false;
		}

		#endregion

		#endregion
	}
}
