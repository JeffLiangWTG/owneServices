using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.DataTransfer.Integration;
using Enterprise.Freight.DataTransfer;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.DataTransfer
{
	public class DeclarationUpdater : IDeclarationUpdater
	{
		#region IDeclarationUpdater Members

		void IDeclarationUpdater.Update(BusinessObject bizObj, Xsd.CartageJob xsdCartage, IValueObjectImportContext context)
		{
			var declaration = (BaseJobDeclaration)bizObj;

			if (declaration != null)
			{
				if (declaration.IsContainerised)
				{
					UpdateDeclarationContainersFromXsd(declaration, xsdCartage, context);
				}
				else
				{
					UpdateDeclarationLooseFromXsd(declaration, xsdCartage, context);
				}
			}
		}

		#region UpdateContainers

		void UpdateDeclarationContainersFromXsd(BaseJobDeclaration declaration, Xsd.CartageJob xsdCartage, IValueObjectImportContext context)
		{
			var isPickup = declaration.IsExport;
			var cartageCompleteDate = ZDateTime.Empty;
			var dictionary = GroupCartageLegsByContainers(xsdCartage.CartageLegs);

			foreach (ZString key in dictionary.Keys)
			{
				BaseCusContainer cusContainer = declaration.CusContainers.Find(key);
				if (cusContainer != null)
				{
					InternalContainerUpdater.UpdateContainerFromXsd(isPickup, xsdCartage, cusContainer.JobContainer, dictionary[key], context);
				}
			}

			var demurrageTotal = new TimeSpan();
			foreach (BaseCusContainer container in declaration.CusContainers)
			{
				var jobContainer = container.JobContainer;
				var complete = (isPickup) ? !jobContainer.JC_DepartureCartageComplete.IsEmpty : !jobContainer.JC_ArrivalCartageComplete.IsEmpty;

				if (!complete)
				{
					demurrageTotal = new TimeSpan();
					break;
				}

				var demurrage = (isPickup) ? jobContainer.DepartureTruckWaitTime : jobContainer.ArrivalTruckWaitTime;
				if (demurrage.IsValid && !demurrage.IsEmpty)
				{
					demurrageTotal += new TimeSpan(demurrage.DayOfYear - 1, demurrage.Hour, demurrage.Minute, 0);
				}
			}

			if (demurrageTotal.TotalMinutes > 0)
			{
				UpdateDate(declaration.JE_PickupOrDeliveryTruckWaitTimeInfo, ZeroTime + demurrageTotal, context);
			}
		}

		#endregion

		#region UpdateDeclarationLooseFromXsd

		void UpdateDeclarationLooseFromXsd(BaseJobDeclaration declaration, Xsd.CartageJob xsdCartage, IValueObjectImportContext context)
		{
			var jobLooseComplete = declaration.JE_CartageCompleted;
			var demurrageTotal = new TimeSpan();

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
				UpdateDate(declaration.JE_CartageCompletedInfo, jobLooseComplete, context);
			}

			if (demurrageTotal.TotalMinutes > 0)
			{
				UpdateDate(declaration.JE_PickupOrDeliveryTruckWaitTimeInfo, ZeroTime + demurrageTotal, context);
			}
		}

		#endregion

		#region GroupCartageLegsByContainers

		Dictionary<ZString, List<Xsd.CartageLeg>> GroupCartageLegsByContainers(Xsd.CartageLegCollection cartageLegCollection)
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

		#endregion

		#region GetDemurrage

		TimeSpan GetDemurrage(Xsd.CartageLegDates dates)
		{
			var pickupDemurrage = dates.PickupDemurrage;
			var deliveryDemurrage = dates.DeliveryDemurrage;

			var pickupTimeSpan = !pickupDemurrage.IsEmpty && pickupDemurrage.IsValid ? new TimeSpan(pickupDemurrage.DayOfYear - 1, pickupDemurrage.Hour, pickupDemurrage.Minute, 0) : new TimeSpan();
			var demurrageTimeSpan = !deliveryDemurrage.IsEmpty && deliveryDemurrage.IsValid ? new TimeSpan(deliveryDemurrage.DayOfYear - 1, deliveryDemurrage.Hour, deliveryDemurrage.Minute, 0) : new TimeSpan();

			return pickupTimeSpan + demurrageTimeSpan;
		}

		#endregion

		#region UpdateDate

		void UpdateDate(ZPropertyInfo info, ZDateTime date, IValueObjectImportContext context)
		{
			if (date.IsValid)
			{
				context.SetPropertyInfoValue(info, date.ToDateTime());
			}
		}

		#endregion

		#region ZeroTime

		ZDateTime ZeroTime
		{
			get { return new ZDateTime(ZDateTime.Today.Year, 1, 1); }
		}

		#endregion

		#endregion
	}
}
