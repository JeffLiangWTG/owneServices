using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.US.AMS.Messaging.Business.StowPlan;
using Enterprise.Freight.Agency.Business;

namespace Enterprise.Customs.US.AMS.Business
{
	public class StowPlanContainerData : AutoStowPlanContainerData, IStowPlanContainerData, IStowPlanNotificationProvider
	{
		public StowPlanContainerData(BillOfLadingContainer container)
			: base(container.Factory)
		{
			this.container = container;
			InitializeStockData();
		}
		readonly BillOfLadingContainer container;

		#region IStowPlanContainerStockData members

		public override ZString EquipmentNumber
		{
			get { return container.JC_ContainerNum; }
		}

		public override ZString StowPosition
		{
			get { return container.JC_StowagePosition; }
		}

		public override ZDecimal GrossWeightInKG
		{
			get
			{
				var result = ZDecimal.Zero;
				if (Core.Constants.Weight.ContainsCode(container.JC_GrossWeightUQ))
				{
					result = Core.Constants.Weight.Convert(container.JC_GrossWeight, container.JC_GrossWeightUQ, Core.Constants.Weight.Kilograms);
				}
				return result;
			}
		}

		public StowPlanContainerStockData StockData
		{
			get
			{
				InitializeStockData();
				return fStockData;
			}
		}
		StowPlanContainerStockData fStockData;

		void InitializeStockData()
		{
			var stock = container.Stock;
			if (fStockData != null && (stock == null || fStockData.stock.PK != stock.PK))
			{
				UnRegisterEditableChildObject(fStockData);
				fStockData = null;
			}
			if (stock != null && fStockData == null)
			{
				fStockData = new StowPlanContainerStockData(stock);
				RegisterEditableChildObject(fStockData);
			}
		}

		IStowPlanContainerStockData IStowPlanContainerData.Stock
		{
			get { return StockData; }
		}

		public IEnumerable<ZString> HazardCodes
		{
			get { return container.PackLines.Cast<BillOfLadingPackLine>().SelectMany(x => x.UNDGs.Where(s => s.Substance != null).Select(y => y.Substance.DG_Code)); }
		}

		public ZString ISOType => container.RefContainer.RC_ISOType;

		#endregion

		#region IStowPlanNotificationProvider members

		Guid IStowPlanNotificationProvider.TargetPK
		{
			get { return container.PK.ToGuid(); }
		}

		string IStowPlanNotificationProvider.TargetCode
		{
			get { return container.TablePrefix; }
		}

		string IStowPlanNotificationProvider.TargetSubject
		{
			get { return container.HumanReadableName; }
		}

		#endregion

		public bool IsWrapperOf(BillOfLadingContainer containerToCompare)
		{
			return this.container.PK == containerToCompare.PK;
		}
	}
}
