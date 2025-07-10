using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	/// <summary>
	/// Inherits from PackLineCollection and sets JL_FreightMode to "OUT"
	/// </summary>
	public class OuterPackLineCollection : PackLineCollection
	{
		public OuterPackLineCollection(CommonShipment master, BusinessObjectFactory factory)
			: base(master, factory)
		{
		}

		protected override TableColumn[] GetColumnsToFetchForEnumerate()
		{
			return new[]
			{
				new TableColumn(JobContainerPackPivotSchema.Constants.TableName, JobContainerPackPivotSchema.Constants.J6_JL),
				new TableColumn(JobTransportLegPackLineDivotSchema.Constants.TableName, JobTransportLegPackLineDivotSchema.Constants.J8_JL)
			};
		}

		#region BusinessObjectCollection overrides

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			base.OnAdded(bizOAdded);

			PackLine line = (PackLine)bizOAdded;

			if (line.JL_FreightMode != FreightConstants.OuterPackType)
			{
				Globals.Message.ShowDeveloperErrorOnce("PackLineNotAnOuterPack", "Packline added to OuterPackLineCollection is not an OuterPack"
					, "OuterPackLineCollection.OnAdded");
			}

			if (!IsLoading
				&& !IsUpdatingByDataRefreshBus
				&& automatedContainerPackingSuspensionLevel == 0
				&& CurrentConsol is CommonConsol consol
				&& consol.AutomaticallyUpdatePackLineContainers)
			{
				CurrentConsol.AllocatePackLine(line);
			}
		}

		int automatedContainerPackingSuspensionLevel;

		public IDisposable TemporarilyDisableAutomaticPackingIntoContainer()
		{
			automatedContainerPackingSuspensionLevel += 1;
			return new DisposableAction(() => automatedContainerPackingSuspensionLevel -= 1);
		}

		protected override void OnRemoved(BusinessObject bizO)
		{
			base.OnRemoved(bizO);
			PackLine line = bizO as PackLine;
			if (!IsLoading && line != null && Shipment != null)
			{
				foreach (CommonContainer container in Shipment.Containers)
				{
					if (container.PackLines.Contains(line))
					{
						container.PackLines.Remove(line);
					}
				}
			}
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			var packLine = (PackLine)child;
			packLine.JL_FreightMode = FreightConstants.OuterPackType;
			packLine.JL_UnitOfDimension = Env.Registry.OuterPacklinesMeasurementDefaultUnit;

			SetDefaultCommodity(packLine);
			packLine.CurrentConsol = CurrentConsol;

			//Maybe we should add a collection for all
			var shipment = packLine.Shipment;
			if (shipment == null && !IsNonCommittedCollectionElement(packLine) && !IsMasterRepresentingAllChildShipments)
			{
				ReportErrorForDefaultingPacklineWithoutShipment(packLine);
			}
		}

		protected override void SetCollectionRelationships(BusinessObject dependent)
		{
			base.SetCollectionRelationships(dependent);

			if (!IsLoading && !IsMasterRepresentingAllChildShipments)
			{
				var packLine = (PackLine)dependent;
				var shipment = packLine.Shipment;

				if (shipment != null)
				{
					AddPackLineToExistingConfirms(packLine, shipment.DeliveryConfirms);
					AddPackLineToExistingConfirms(packLine, shipment.PickupConfirms);
					AddPackLineToExistingConfirms(packLine, shipment.OriginCFSArrivals);
					AddPackLineToExistingConfirms(packLine, shipment.OriginCFSDepartures);
					AddPackLineToExistingConfirms(packLine, shipment.DestinationCFSArrivals);
					AddPackLineToExistingConfirms(packLine, shipment.DestinationCFSDepartures);
				}
			}
		}

		void AddPackLineToExistingConfirms(PackLine packLine, CommonPickupDeliveryConfirmCollection confirms)
		{
			foreach (CommonPickupDeliveryConfirm confirm in confirms)
			{
				var divotsQuery = new ZQuery(JobTransportLegPackLineDivotSchema.J8_EU_PickupDeliverConfirm, confirm.PK)
					.AddToFilter(JobTransportLegPackLineDivotSchema.J8_JL, packLine.PK);

				if (!Factory.Load<CommonConfirmDivot>(divotsQuery).Any())
				{
					confirm.DebugLog.AppendLine(FormattableString.Invariant($"Adding PackLine '{packLine.PK}' to existing Confirm: '{confirm.PK}'"));
					confirm.CreateDivot(packLine, false);
				}
				else
				{
					confirm.DebugLog.AppendLine(FormattableString.Invariant($"Skipped adding divot for PackLine: '{packLine.PK}' and Confirm: '{confirm.PK}' as Divot already exists"));
				}
			}
		}

		void ReportErrorForDefaultingPacklineWithoutShipment(PackLine packline)
		{
			var errorMessage = string.Format("Shipment should not be null when setting default for new outer pack line and adding pack line to existing confirms. JL_JS: {0}  Related issue number: {1}", packline.JL_JS, "00848807");
			ErrorReporter.ReportOnce("DefaultingPacklineWithoutShipment", errorMessage);
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			ZQuery result = base.CreateAdditionalFilter();
			result.AddToFilter(JoinCondition.And, JobPackLinesSchema.JL_FreightMode, SQLComparisonOperator.Equal, FreightConstants.OuterPackType);
			return result;
		}

		#endregion

		#region Update ParentShipment Totals

		protected override void OnPackLineJL_PackageCountChanged(object sender, EventArgs e)
		{
			if (Shipment != null)
			{
				Shipment.TotalOuterPacksInfo.RefreshBinding();
			}
		}

		protected override void OnPackLineJL_ActualWeightChanged(object sender, EventArgs e)
		{
			if (Shipment != null)
			{
				Shipment.TotalOuterPacksWeightInfo.RefreshBinding();
			}
		}

		protected override void OnPackLineJL_ActualVolumeChanged(object sender, EventArgs e)
		{
			if (Shipment != null)
			{
				Shipment.TotalOuterPacksVolumeInfo.RefreshBinding();
			}
		}

		protected override void RefreshPackLineTotalsOnShipmentCore()
		{
			if (Shipment != null)
			{
				Shipment.TotalOuterPacksInfo.RefreshBinding();
				Shipment.TotalOuterPacksWeightInfo.RefreshBinding();
				Shipment.TotalOuterPacksVolumeInfo.RefreshBinding();
			}
		}

		#endregion

		#region Default Commodity Code

		public delegate void PackLineDefaultCommoditySetter(PackLine line);
		public event PackLineDefaultCommoditySetter OnSettingDefaultCommodityForNewChild;

		protected void SetDefaultCommodity(PackLine line)
		{
			if (OnSettingDefaultCommodityForNewChild != null)
			{
				OnSettingDefaultCommodityForNewChild(line);
			}

			if (line.JL_RH_NKCommodityCode.IsEmpty)
			{
				line.JL_RH_NKCommodityCode = GetDefaultCommodityCode();
			}
		}

		public void SetDefaultCommodityForCollection()
		{
			var emptyPackLines = this.OfType<PackLine>().Where(x => x.JL_RH_NKCommodityCode.IsEmpty);
			if (emptyPackLines.Any())
			{
				var defaultCommodityCode = GetDefaultCommodityCode();
				emptyPackLines.ForEach(x => x.JL_RH_NKCommodityCode = defaultCommodityCode);
			}
		}

		ZString GetDefaultCommodityCode()
		{
			var defaultCommodityCode = ZString.Empty;

			if (Shipment is not null)
			{
				if (Shipment.IsImport()
					&& Shipment.Consignee != null
					&& Shipment.Consignee.MiscServ.OM_CMDoesImports
					&& !Shipment.Consignee.MiscServ.OM_RH_NKCMMainImportCmdty.IsEmpty
					&& Shipment.Consignee.MiscServ.IsSalesLead)
				{
					defaultCommodityCode = Shipment.Consignee.MiscServ.OM_RH_NKCMMainImportCmdty;
				}
				else if (Shipment.IsExport()
						 && Shipment.Consignor != null
						 && Shipment.Consignor.MiscServ.OM_CMDoesExports
						 && !Shipment.Consignor.MiscServ.OM_RH_NKCMMainExportCmdty.IsEmpty
						 && Shipment.Consignor.MiscServ.IsSalesLead)
				{
					defaultCommodityCode = Shipment.Consignor.MiscServ.OM_RH_NKCMMainExportCmdty;
				}
			}

			if (defaultCommodityCode.IsEmpty && Factory.Load<RefCommodityCode>(Env.Registry.CommodityCode) != null)
			{
				defaultCommodityCode = Factory.Load<RefCommodityCode>(Env.Registry.CommodityCode).RH_Code;
			}

			return defaultCommodityCode;
		}

		#endregion

		#region CurrentConsol

		/// <summary>
		/// The currently selected consol on the CommonShipment containers tab.
		/// </summary>
		public CommonConsol CurrentConsol
		{
			get
			{
				if (currentConsol == null && Shipment != null && Shipment.Consols.Count > 0)
				{
					CurrentConsol = Shipment.Consols[0];
				}

				return currentConsol;
			}
			set
			{
				if (currentConsol != value
					&& (value == null || Shipment?.Consols?.GetRelationshipBusinessObject(value) != null))
				{
					currentConsol = value;
					var currentPackLines = this.ToList();
					foreach (PackLine packLine in currentPackLines)
					{
						packLine.CurrentConsol = value;
					}
				}
			}
		}
		CommonConsol currentConsol;

		#endregion

		#region Totals

		public ZInt TotalInStock
		{
			get { return Convert.ToInt32(TotalCalculation.GetTotal(this, PackLine.Schema.JL_Calc_InStock)); }
		}

		public ZInt TotalOutturnedInStock
		{
			get { return Convert.ToInt32(TotalCalculation.GetTotal(this, PackLine.Schema.JL_Calc_OutturnedInStock)); }
		}

		public ZInt TotalDelivered
		{
			get { return Totals.TotalPackages; }
		}

		public ZInt TotalPackagesToDeliver
		{
			get { return Totals.TotalPackagesToDeliver; }
		}

		public ZDecimal TotalWeightToDeliver
		{
			get { return Totals.TotalWeightToDeliver; }
		}

		public ZString TotalWeightToDeliverUnit
		{
			get { return TotalWeightUnit; }
		}

		public ZDecimal TotalVolumeToDeliver
		{
			get { return Totals.TotalVolumeToDeliver; }
		}

		public ZString TotalVolumeToDeliverUnit
		{
			get { return TotalVolumeUnit; }
		}

		public ZInt TotalManifestedPacks
		{
			get { return Convert.ToInt32(TotalCalculation.GetTotal(this, PackLine.Schema.JL_PackageCount)); }
		}

		public ZInt TotalOutturned
		{
			get { return Convert.ToInt32(TotalCalculation.GetTotal(this, PackLine.Schema.JL_Outturn)); }
		}

		public ZInt TotalDamaged
		{
			get { return Convert.ToInt32(TotalCalculation.GetTotal(this, PackLine.Schema.JL_Damaged)); }
		}

		public ZInt TotalPillaged
		{
			get { return Convert.ToInt32(TotalCalculation.GetTotal(this, PackLine.Schema.JL_Pillaged)); }
		}

		#endregion

		#region Implementation

		protected override ZString ShipmentPackageUnit
		{
			get { return Shipment != null ? Shipment.ShipmentOuterPacksUnit : ZString.Empty; }
		}

		#endregion
	}
}
