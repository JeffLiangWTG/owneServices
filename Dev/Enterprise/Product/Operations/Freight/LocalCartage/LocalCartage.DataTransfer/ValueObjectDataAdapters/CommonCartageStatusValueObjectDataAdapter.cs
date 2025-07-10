using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.DataTransfer;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.LocalCartage.DataTransfer
{
	public class CommonCartageStatusValueObjectDataAdapter : CommonCartageValueObjectDataAdapter
	{
		protected override void ImportFromValueObjectCore(CommonCartage bizObj, Xsd.CartageJob value, IValueObjectImportContext context)
		{
		}

		public override Xsd.InterchangeInfoTargetType TargetType
		{
			get { return Xsd.InterchangeInfoTargetType.LocalCartageStatus; }
		}

		protected override ZString BookingAction
		{
			get { return FreightConstants.CartageStatusExport; }
		}

		protected override ZString GetJobReference(CommonCartage cartage)
		{
			return cartage.JJ_OrderReferenceNumber;
		}

		public override CommonCartage CreateOrUpdateFromValueObject(Xsd.CartageJob value, IValueObjectImportContext context)
		{
			return ImportParentInformationFromValueObjectOnly(value, context);
		}

		CommonCartage ImportParentInformationFromValueObjectOnly(Xsd.CartageJob value, IValueObjectImportContext context)
		{
			CommonCartage result = null;
			Xsd.CartageJob xsdCartage = value;
			ZString jobReference = xsdCartage.ClientJobReference;

			CommonShipment parentShipment;
			BusinessObject parentDeclaration;
			CommonConsol parentLoadList;
			BusinessObject parentWhsOrder;

			if ((parentShipment = FindShipment(jobReference, context)) != null)
			{
				UpdateShipment(parentShipment, xsdCartage, context);
			}
			else if ((parentDeclaration = FindDeclaration(jobReference, context)) != null)
			{
				UpdateDeclaration(parentDeclaration, xsdCartage, context);
			}
			else if ((parentLoadList = FindLoadList(jobReference, context)) != null)
			{
				UpdateLoadList(parentLoadList, xsdCartage, context);
			}
			else if ((parentWhsOrder = FindWhsOrder(jobReference, context)) != null)
			{
				UpdateWhsOrder(parentWhsOrder, xsdCartage, context);
			}

			var builder = new ZStringBuilder();
			builder.AppendIfNotEmpty(GetReceivedNotificationMessage(xsdCartage, ZDateTime.Now, context));
			builder.AppendIfNotEmpty(GetCommentsMessage(xsdCartage));
			context.Notify(new InfoNotification(builder.ToStringWithNewLineBetweenAppends()));

			context.Notify(new InfoNotification(Res.GetString("6e4db46b-000d-45ae-a635-1a829ebd88c1", "Job {0} updated.", jobReference)));

			return result;
		}

		void UpdateShipment(CommonShipment shipment, Xsd.CartageJob xsdCartage, IValueObjectImportContext context)
		{
			UpdateShipmentJobFromXsd(shipment, xsdCartage);

			if (shipment.IsContainerised)
			{
				UpdateShipmentContainersFromXsd(shipment, xsdCartage, context);
			}
			else
			{
				UpdateShipmentLooseFromXsd(shipment, xsdCartage, context);
			}

			ImportBookingStatus(shipment.Logs, xsdCartage, context);
			ImportComments(shipment.Notes, xsdCartage, context);
		}

		void UpdateShipmentJobFromXsd(CommonShipment shipment, Xsd.CartageJob xsdCartage)
		{
			if (shipment.IsExport())
			{
				shipment.DocsAndCartage.JP_FCLPickupEquipmentNeeded = xsdCartage.DropMode;
			}
			else
			{
				shipment.DocsAndCartage.JP_FCLDeliveryEquipmentNeeded = xsdCartage.DropMode;
			}
		}

		void UpdateShipmentContainersFromXsd(CommonShipment shipment, Xsd.CartageJob xsdCartage, IValueObjectImportContext context)
		{
			var legsByContainer = GroupLegsByContainers(xsdCartage.CartageLegs);
			foreach (ZString key in legsByContainer.Keys)
			{
				var container = shipment.Containers.Cast<CommonContainer>().FirstOrDefault(c => c.JC_ContainerNum == key);
				if (container != null)
				{
					InternalContainerUpdater.UpdateContainerFromXsd(shipment.IsExport(), xsdCartage, container, legsByContainer[key], context);
				}
			}

			var demurrageTotal = new TimeSpan();
			foreach (CommonContainer container in shipment.Containers)
			{
				var complete = shipment.IsExport() ? !container.JC_DepartureCartageComplete.IsEmpty : !container.JC_ArrivalCartageComplete.IsEmpty;

				if (!complete)
				{
					demurrageTotal = new TimeSpan();
					break;
				}

				var demurrage = shipment.IsExport() ? container.DepartureTruckWaitTime : container.ArrivalTruckWaitTime;
				if (demurrage.IsValid && !demurrage.IsEmpty)
				{
					demurrageTotal += new TimeSpan(demurrage.DayOfYear - 1, demurrage.Hour, demurrage.Minute, 0);
				}
			}

			if (demurrageTotal.TotalMinutes > 0)
			{
				var jobDeumrrageInfo = shipment.IsExport() ? shipment.DocsAndCartage.JP_PickupTruckWaitTimeInfo : shipment.DocsAndCartage.JP_DeliveryTruckWaitTimeInfo;
				UpdateDate(jobDeumrrageInfo, ZeroTime + demurrageTotal, context);
			}
		}

		void UpdateShipmentLooseFromXsd(CommonShipment shipment, Xsd.CartageJob xsdCartage, IValueObjectImportContext context)
		{
			ZPropertyInfo jobCompleteInfo = shipment.IsExport() ? shipment.DocsAndCartage.JP_PickupCartageCompletedInfo : shipment.DocsAndCartage.JP_DeliveryCartageCompletedInfo;
			var demurrageTotal = new TimeSpan();

			var jobLooseComplete = (ZDateTime)jobCompleteInfo.Value;
			foreach (Xsd.CartageLeg xsdCartageLeg in xsdCartage.CartageLegs)
			{
				var xsdContainerLeg = xsdCartageLeg.Item as Xsd.CartageLegContainer;

				if (xsdContainerLeg == null)
				{
					demurrageTotal += GetDemurrage(xsdCartageLeg.CartageLegDates);

					var isPickingUpFromConsignor = xsdCartageLeg.Pickup.DocAddress.AddressType == Xsd.DocAddressAddressType.LCE;
					var isDeliveringToConsignee = xsdCartageLeg.Delivery.DocAddress.AddressType == Xsd.DocAddressAddressType.LCI;

					var legComplete = ZDateTime.Empty;
					if (isPickingUpFromConsignor)
					{
						legComplete = !xsdCartageLeg.CartageLegDates.PickupTimeInDate.IsEmpty ? xsdCartageLeg.CartageLegDates.PickupTimeInDate : xsdCartageLeg.CartageLegDates.PickupTimeOutDate;
					}
					else if (isDeliveringToConsignee)
					{
						legComplete = !xsdCartageLeg.CartageLegDates.DeliverTimeInDate.IsEmpty ? xsdCartageLeg.CartageLegDates.DeliverTimeInDate : xsdCartageLeg.CartageLegDates.DeliverTimeOutDate;
					}

					if (legComplete.IsValid && !legComplete.IsEmpty && (jobLooseComplete.IsEmpty || legComplete > jobLooseComplete))
					{
						jobLooseComplete = legComplete;
					}
				}
			}

			if (jobLooseComplete.IsValid && !jobLooseComplete.IsEmpty)
			{
				UpdateDate(jobCompleteInfo, jobLooseComplete, context);
			}

			if (demurrageTotal.TotalMinutes > 0)
			{
				var jobDeumrrageInfo = shipment.IsExport() ? shipment.DocsAndCartage.JP_PickupTruckWaitTimeInfo : shipment.DocsAndCartage.JP_DeliveryTruckWaitTimeInfo;
				UpdateDate(jobDeumrrageInfo, ZeroTime + demurrageTotal, context);
			}
		}

		CommonShipment FindShipment(ZString reference, IValueObjectImportContext context)
		{
			return context.Factory.LoadTop1<CommonShipment>(new ZQuery(JobShipmentSchema.JS_UniqueConsignRef, reference));
		}

		void UpdateDeclaration(BusinessObject declaration, Xsd.CartageJob xsdCartage, IValueObjectImportContext context)
		{
			var updater = (IDeclarationUpdater)Activator.CreateInstance(ObjectFactory.GetType<IDeclarationUpdater>(), Array.Empty<object>());
			updater.Update(declaration, xsdCartage, context);
			ImportBookingStatus(declaration.GetLogs(), xsdCartage, context);
			ImportComments(declaration.GetNotes(), xsdCartage, context);
		}

		BusinessObject FindDeclaration(ZString reference, IValueObjectImportContext context)
		{
			var filter = new ZQuery(JobDeclarationSchema.JE_DeclarationReference, reference);
			return (BusinessObject)context.Factory.LoadTop1<Enterprise.Integration.Customs.IBaseJobDeclaration>(filter);
		}

		void UpdateLoadList(CommonConsol loadList, Xsd.CartageJob xsdCartage, IValueObjectImportContext context)
		{
			UpdateLoadListFromXsd(loadList, xsdCartage, context);
			ImportBookingStatus(loadList.Logs, xsdCartage, context);
			ImportComments(loadList.Notes, xsdCartage, context);
		}

		void UpdateLoadListFromXsd(CommonConsol loadList, Xsd.CartageJob xsdCartage, IValueObjectImportContext context)
		{
			var legsByContainer = GroupLegsByContainers(xsdCartage.CartageLegs);
			foreach (ZString key in legsByContainer.Keys)
			{
				var container = loadList.Containers.Cast<CommonContainer>().FirstOrDefault(c => c.JC_ContainerNum == key);
				if (container != null)
				{
					InternalContainerUpdater.UpdateContainerFromXsd(loadList.IsExport(), xsdCartage, container, legsByContainer[key], context);
				}
			}
		}

		CommonConsol FindLoadList(ZString reference, IValueObjectImportContext context)
		{
			return context.Factory.LoadTop1<CommonConsol>(new ZQuery(JobConsolSchema.JK_UniqueConsignRef, reference));
		}

		BusinessObject FindWhsOrder(ZString jobReference, IValueObjectImportContext context)
		{
			return (BusinessObject)context.Factory.LoadTop1<IWhsOrder>(new ZQuery(WhsDocketSchema.WD_DocketID, jobReference));
		}

		void UpdateWhsOrder(BusinessObject whsOrder, Xsd.CartageJob xsdCartage, IValueObjectImportContext context)
		{
			ImportBookingStatus(whsOrder.GetLogs(), xsdCartage, context);
			ImportComments(whsOrder.GetNotes(), xsdCartage, context);
		}

		Dictionary<ZString, List<Xsd.CartageLeg>> GroupLegsByContainers(Xsd.CartageLegCollection cartageLegCollection)
		{
			var dictionary = new Dictionary<ZString, List<Xsd.CartageLeg>>();
			foreach (Xsd.CartageLeg cartageLeg in cartageLegCollection)
			{
				var containerLeg = cartageLeg.Item as Xsd.CartageLegContainer;
				if (containerLeg != null)
				{
					List<Xsd.CartageLeg> list;
					if (!dictionary.TryGetValue(containerLeg.ContainerNumber, out list))
					{
						list = new List<Xsd.CartageLeg>();
						dictionary.Add(containerLeg.ContainerNumber, list);
					}
					list.Add(cartageLeg);
				}
			}
			return dictionary;
		}

		TimeSpan GetDemurrage(Xsd.CartageLegDates dates)
		{
			var pickupDemurrage = dates.PickupDemurrage;
			var deliveryDemurrage = dates.DeliveryDemurrage;

			var pickupTimeSpan = !pickupDemurrage.IsEmpty && pickupDemurrage.IsValid ? new TimeSpan(pickupDemurrage.DayOfYear - 1, pickupDemurrage.Hour, pickupDemurrage.Minute, 0) : new TimeSpan();
			var demurrageTimeSpan = !deliveryDemurrage.IsEmpty && deliveryDemurrage.IsValid ? new TimeSpan(deliveryDemurrage.DayOfYear - 1, deliveryDemurrage.Hour, deliveryDemurrage.Minute, 0) : new TimeSpan();

			return pickupTimeSpan + demurrageTimeSpan;
		}

		ZDateTime ZeroTime
		{
			get { return new ZDateTime(ZDateTime.Today.Year, 1, 1); }
		}

		void UpdateDate(ZPropertyInfo info, ZDateTime date, IValueObjectImportContext context)
		{
			if (info != null && date.IsValid)
			{
				context.SetPropertyInfoValue(info, date.ToDateTime());
			}
		}
	}
}
