using System;
using CargoWise.Types;
using Enterprise.Customs.US.AMS.Messaging.Business.StowPlan;
using Enterprise.Freight.Agency.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.AMS.Business
{
	public class StowPlanContainerStockData : AutoStowPlanContainerStockData, IStowPlanContainerStockData, IStowPlanNotificationProvider
	{
		public StowPlanContainerStockData(RefContainerStock stock)
			: base(stock.Factory)
		{
			this.stock = stock;
		}
		public readonly RefContainerStock stock;

		#region IStowPlanContainerStockData members

		public override ZString ContainerOperator => stock.Owner.USLocalCustomsCarrierCode();

		public override ZString EquipmentSizeType
		{
			get { return stock.R6_RC_ISOType; }
		}

		#endregion

		#region IStowPlanNotificationProvider members

		Guid IStowPlanNotificationProvider.TargetPK
		{
			get { return stock.PK.ToGuid(); }
		}

		string IStowPlanNotificationProvider.TargetCode
		{
			get { return stock.TablePrefix; }
		}

		string IStowPlanNotificationProvider.TargetSubject
		{
			get { return stock.HumanReadableName; }
		}

		#endregion
	}
}
