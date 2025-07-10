using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	public class InnerPackLineCollection : PackLineCollection
	{
		public InnerPackLineCollection(CommonShipment master, BusinessObjectFactory factory)
			: base(master, factory)
		{
		}

		#region BusinessObjectCollection overrides

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			PackLine packLine = (PackLine)child;
			packLine.JL_FreightMode = FreightConstants.InnerPackType;
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			ZQuery result = base.CreateAdditionalFilter();
			result.AddToFilter(JoinCondition.And, JobPackLinesSchema.JL_FreightMode, SQLComparisonOperator.Equal, FreightConstants.InnerPackType);
			return result;
		}

		protected override void EnsurePackLineExists()
		{
			if (Shipment != null && Shipment.JS_TotalPackageCount > 0)
			{
				base.EnsurePackLineExists();
			}
		}

		#endregion

		#region Update ParentShipment Totals

		protected override void OnPackLineJL_PackageCountChanged(object sender, EventArgs e)
		{
			if (Shipment != null)
			{
				Shipment.TotalInnerPackLinePackagesInfo.RefreshBinding();
			}
		}

		protected override void OnPackLineJL_ActualWeightChanged(object sender, EventArgs e)
		{
			if (Shipment != null)
			{
				Shipment.TotalInnerPackLineWeightInfo.RefreshBinding();
			}
		}

		protected override void OnPackLineJL_ActualVolumeChanged(object sender, EventArgs e)
		{
			if (Shipment != null)
			{
				Shipment.TotalInnerPackLineVolumeInfo.RefreshBinding();
			}
		}

		protected override void RefreshPackLineTotalsOnShipmentCore()
		{
			if (Shipment != null)
			{
				Shipment.TotalInnerPackLinePackagesInfo.RefreshBinding();
				Shipment.TotalInnerPackLineWeightInfo.RefreshBinding();
				Shipment.TotalInnerPackLineVolumeInfo.RefreshBinding();
				Shipment.TotalInnerPackLineLoadingMetersInfo.RefreshBinding();
			}
		}

		#endregion
	}
}
