using System;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.SailingScheduleDataVendor
{
	[Serializable]
	internal class VoyageDestinationSubscriptionUpdater : VoyageSubscriptionUpdater
	{
		public override string Name
		{
			get { return "VoyDestinationSubscriptionUpdater"; }
		}

		public override string FriendlyName
		{
			get { return (NoResString)"Voyage Destination Subscription Updater"; }
		}

		protected override Type GetBusinessObjectType()
		{
			return typeof(VoyageDestination);
		}

		protected override JobVoyage GetParentVoyage(BusinessObject businessObject)
		{
			var voyageDestination = businessObject as VoyageDestination;

			if (voyageDestination == null)
			{
				return null;
			}

			if (voyageDestination.JB_RL_NKPortOfDischarge.Length != 5)
			{
				DefaultLogger.Log(LogType.Error, "The destination port has a wrong UNLOCO: " + voyageDestination.JB_RL_NKPortOfDischarge);
				return null;
			}

			return voyageDestination.Voyage;
		}

		protected override ITopLevelDataObjectWriter GetDataObjectWriter(IDataWritingManager manager)
		{
			return new VoyageDestinationSubscriptionEventDataObjectWriter(manager);
		}
	}
}
