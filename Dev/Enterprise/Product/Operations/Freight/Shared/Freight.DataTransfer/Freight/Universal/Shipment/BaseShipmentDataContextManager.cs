using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using ContextTypes = Enterprise.UniversalDataBuss.DataObjects.Universal.Event.ContextTypes;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.DataTransfer.Universal
{
	public abstract class BaseShipmentDataContextManager<T> : ShipmentDataContextManager<T>
		where T : CommonShipment
	{
		public override ZString DataContextKey
		{
			get { return ParentBO.JS_UniqueConsignRef; }
		}

		protected override ZQuery GetDataContextKeyMatchingQuery(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory, IXmlImportLogger logger)
		{
#if DEBUG
			if (matchingValues != null && matchingValues.DataObject is UniversalShipment && ((UniversalShipment)matchingValues.DataObject).GoodsDescription.GetValueOrDefault() == "ChangeMyUniqueIndexForZQuery")
			{
				return new ZQuery(JobShipmentSchema.JS_BookingReference, matchingValues.Key);
			}
#endif

			var result = new ZQuery(JobShipmentSchema.JS_UniqueConsignRef, matchingValues.Key);
			result.AddToFilter(JobShipmentSchema.JS_IsCancelled, false);

			return result;
		}

		protected override IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetEventContextValues()
		{
			var result = new List<KeyValuePair<TypeWithDescription, IZType>>();

			if (ParentBO != null)
			{
				var shipmentEventContextReader = new CommonShipmentEventContextReader(ParentBO, Helper);
				shipmentEventContextReader.AddShipmentContextValues(result);
			}

			return result.Count == 0 ? null : result;
		}

		protected override bool TryGetMatchingDataTarget(UniversalShipment universalShipment, out IDataTargetDataObject dataTarget)
		{
			dataTarget = universalShipment.GetMatchingDataTarget(DataContextType);
			if (dataTarget != null && universalShipment.WayBillType.GetCodeAsUpperCase() == WayBillTypeList.Codes.Master)
			{
				dataTarget = DataContextFactory.NewDataTarget();
				dataTarget.Type = DataContextType.ToString();
			}

			return dataTarget != null;
		}

		public override bool ManagesShipments
		{
			get { return true; }
		}

		#region OnUniversalEventAdded

		protected override void OnUniversalEventAddedCore(IXmlSessionTracker logger, UniversalEvent eventAdded)
		{
			base.OnUniversalEventAddedCore(logger, eventAdded);

			switch (eventAdded.EventType.GetValueOrDefault(""))
			{
				case Events.DeliveryCartageCompleteFinalisedCode:
					var contextCollection = eventAdded.ContextCollection;
					var receivedFrom = (contextCollection != null) ? contextCollection.FirstOrDefault(c => c.Type == nameof(ContextTypes.ReceivedFromName)) : null;
					if (receivedFrom != null && !receivedFrom.Value.GetValueOrDefault().IsEmpty)
					{
						ConfirmTimesSyncHelper.SetConfirmSignedBy(ParentBO, ConfirmTimesSyncHelper.ConfirmType.Delivery, receivedFrom.Value.Value, logger);
					}
					else
					{
						logger.Log(LogType.Information, Res.GetString("ForwardingShipmentDataContextManager|GoodsSignedByNotUpdated", "'Goods Signed By' has not been updated, because there was no {0} {1} specified or was empty.", "ContextType", "ReceivedFromName"));
					}
					break;
				case Events.ChangeOfIdentifierCode:
					UpdateContainerFromChangeOfIdentifierEvent(eventAdded, logger);
					break;
				case Events.StatusUpdatedCode:
					UpdateShipmentPacklinesImportRefNumber(eventAdded);
					break;
			}
		}

		void UpdateContainerFromChangeOfIdentifierEvent(UniversalEvent eventAdded, IXmlImportLogger logger)
		{
			var departureConsol = ParentBO.DepartureConsol;
			var eventReferenceType = EventParameters.GetEventParameter(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, eventAdded.EventParameters, eventAdded.EventReference).GetValueOrDefault();

			if (departureConsol != null && eventReferenceType == Constants.EventReferenceParameterTypes.ContainerID)
			{
				var oldNumber = EventParameters.GetEventParameter(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Old, eventAdded.EventParameters, eventAdded.EventReference).GetValueOrDefault();
				var newNumber = EventParameters.GetEventParameter(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.New, eventAdded.EventParameters, eventAdded.EventReference).GetValueOrDefault();
				var releaseNumber = EventParameters.GetEventParameter(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.ReferenceNumber, eventAdded.EventParameters, eventAdded.EventReference).GetValueOrDefault();

				var oldNumberContainer = departureConsol.Containers.Cast<CommonContainer>().FirstOrDefault(x => x.JC_ContainerNum == oldNumber);
				var newNumberContainer = departureConsol.Containers.Cast<CommonContainer>().FirstOrDefault(x => x.JC_ContainerNum == newNumber);
				var releaseNumberContainer = departureConsol.Containers.Cast<CommonContainer>().FirstOrDefault(x => x.JC_ReleaseNum == releaseNumber);

				void UpdateExistingContainer(CommonContainer container)
				{
					if (container != null && newNumberContainer != null && container.JC_ContainerNum == newNumberContainer.JC_ContainerNum)
					{
						return;
					}

					if (newNumberContainer != null)
					{
						foreach (PackLine packLine in ParentBO.OuterPackLines)
						{
							if (container == packLine.GetContainer(departureConsol))
							{
								packLine.SetContainer(departureConsol, newNumberContainer);
							}
						}
						var factory = container.Factory;
						var queryForCusContainer = new ZQuery(CusContainerSchema.CO_JC, container.PK);
						queryForCusContainer.FetchOnlyFromLocalCache = !container.IsInDatabase;
						var cusContainers = factory.Load<Enterprise.Integration.Customs.Shared.IBaseCusContainer>(queryForCusContainer);
						if (cusContainers.Length > 0)
						{
							var queryForCusContainerNew = new ZQuery(CusContainerSchema.CO_JC, newNumberContainer.PK);
							queryForCusContainerNew.FetchOnlyFromLocalCache = !newNumberContainer.IsInDatabase;
							var cusContainersNew = factory.Load<Enterprise.Integration.Customs.Shared.IBaseCusContainer>(queryForCusContainerNew);

							foreach (var cusContainer in cusContainers)
							{
								var cusContainerBO = (BusinessObject)cusContainer;
								if (!cusContainerBO.IsDeleted)
								{
									if (cusContainersNew.Any(x => x.CO_JE == cusContainer.CO_JE))
									{
										cusContainerBO.Delete();
									}
									else
									{
										cusContainer.CO_JC = newNumberContainer.PK;
									}
								}
							}
						}

						newNumberContainer.JC_ContainerNum = container.JC_ContainerNum;

						departureConsol.Containers.RemoveAndDelete(container);
					}
					else
					{
						container.JC_ContainerNum = newNumber;
					}
				}

				if (!oldNumber.IsEmpty && oldNumberContainer != null)
				{
					UpdateExistingContainer(oldNumberContainer);
				}
				else if (!releaseNumber.IsEmpty && releaseNumberContainer != null)
				{
					UpdateExistingContainer(releaseNumberContainer);
				}
				else if ((oldNumber.IsEmpty || oldNumberContainer == null) && (releaseNumber.IsEmpty || releaseNumberContainer == null) && newNumberContainer == null)
				{
					CommonUniversalFreightHelper.UpdateContainerNumberAndType(departureConsol, newNumber, ((IXmlEventValueObject)eventAdded).Context.ContainerISOCode, logger);
				}
			}
		}

		void UpdateShipmentPacklinesImportRefNumber(UniversalEvent eventAdded)
		{
			var eventParameters = eventAdded.EventParameters;

			if (eventParameters != null
				&& eventParameters.Type.GetValueOrDefault() == Constants.EventReferenceParameterTypes.LPDNotification
				&& eventParameters.CustomsReferenceNumber.HasValue
				&& eventParameters.EquipmentReferenceNumber.HasValue)
			{
				foreach (var packline in ParentBO.OuterPackLines.OfType<PackLine>())
				{
					if (packline
							.Containers
							.OfType<CommonContainer>()
							.Any(c => c.JC_ContainerNum == eventParameters.EquipmentReferenceNumber.Value)
						)
					{
						packline.JL_ImportRefNumber = eventParameters.CustomsReferenceNumber.Value;
					}
				}
			}
		}

		#endregion

		protected IUniversalFreightHelper Helper
		{
			get { return helper ?? (helper = GetNewHelper()); }
		}

		IUniversalFreightHelper helper;

		protected abstract IUniversalFreightHelper GetNewHelper();
	}
}
