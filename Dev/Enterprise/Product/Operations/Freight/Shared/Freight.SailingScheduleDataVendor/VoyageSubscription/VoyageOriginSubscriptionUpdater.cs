using System;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.SailingScheduleDataVendor
{
	[Serializable]
	internal class VoyageOriginSubscriptionUpdater : VoyageSubscriptionUpdater
	{
		public override string Name
		{
			get { return "VoyOriginSubscriptionUpdater"; }
		}

		public override string FriendlyName
		{
			get { return (NoResString)"Voyage Origin Subscription Updater"; }
		}

		protected override Type GetBusinessObjectType()
		{
			return typeof(VoyageOrigin);
		}

		protected override JobVoyage GetParentVoyage(BusinessObject businessObject)
		{
			var voyageOrigin = businessObject as VoyageOrigin;

			if (voyageOrigin == null)
			{
				return null;
			}

			if (voyageOrigin.JA_RL_NKPortOfLoading.Length != 5)
			{
				DefaultLogger.Log(LogType.Error, "The origin port has a wrong UNLOCO: " + voyageOrigin.JA_RL_NKPortOfLoading);
				return null;
			}

			return voyageOrigin.Voyage;
		}

		protected override ITopLevelDataObjectWriter GetDataObjectWriter(IDataWritingManager manager)
		{
			return new VoyageOriginSubscriptionEventDataObjectWriter(manager);
		}
	}
}
