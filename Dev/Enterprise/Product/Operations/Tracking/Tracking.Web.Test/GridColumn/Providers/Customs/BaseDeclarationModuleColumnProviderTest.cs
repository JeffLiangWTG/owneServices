using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Web;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using NUnit.Framework;

namespace Enterprise.Tracking.Web.Testing
{
	[TestedType(typeof(BaseDeclarationModuleColumnProvider))]
	class BaseDeclarationModuleColumnProviderTest : GridColumnProviderTest
	{
		public override void TestFixOldLayout()
		{
			string cachedRegistryValue = WebDataRegistry.Instance.MilestoneVisibility.Value;
			base.TestFixOldLayout();
			WebDataRegistry.Instance.MilestoneVisibility.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, cachedRegistryValue);
		}

		protected override void BeforeLayoutsWithFewDynamicColumns()
		{
			base.BeforeLayoutsWithFewDynamicColumns();
			WebDataRegistry.Instance.MilestoneVisibility.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, MilestoneVisibilityList.Codes.LastCompletedMilestoneOnly);
		}

		protected override void BeforeLayoutsWithAllDynamicColumns()
		{
			base.BeforeLayoutsWithAllDynamicColumns();
			WebDataRegistry.Instance.MilestoneVisibility.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, MilestoneVisibilityList.Codes.All);
		}

		protected override DataGridColumn[] GetColumnsForLayoutFixNoDynamicColumns()
		{
			return new DataGridColumn[]
			{
				TestProvider[WebTracker.Grids.TrackingDeclarations.Broker],
				TestProvider[WebTracker.Grids.TrackingDeclarations.JobNumber],
				TestProvider[WebTracker.Grids.TrackingDeclarations.MasterBill],
				TestProvider[WebTracker.Grids.TrackingDeclarations.OrderReferences]
			};
		}

		protected override DataGridColumn[] GetColumnsForLayoutFixAllDynamicColumns()
		{
			return new DataGridColumn[]
			{
				TestProvider[WebTracker.Grids.TrackingDeclarations.OrderReferences],
				TestProvider[WebTracker.Grids.Milestones.LastMilestoneDate],
				TestProvider[WebTracker.Grids.TrackingDeclarations.Broker],
				TestProvider[WebTracker.Grids.Milestones.NextMilestoneDate],
				TestProvider[WebTracker.Grids.TrackingDeclarations.JobNumber],
				TestProvider[WebTracker.Grids.Milestones.LastMilestoneDescription],
				TestProvider[WebTracker.Grids.TrackingDeclarations.OrderReferences],
				TestProvider[WebTracker.Grids.Milestones.NextMilestoneDescription]
			};
		}

		protected override DataGridColumn[] GetColumnsForLayoutFixFewDynamicColumns()
		{
			return new DataGridColumn[]
			{
				TestProvider[WebTracker.Grids.TrackingDeclarations.Broker],
				TestProvider[WebTracker.Grids.TrackingDeclarations.JobNumber],
				TestProvider[WebTracker.Grids.Milestones.LastMilestoneDate],
				TestProvider[WebTracker.Grids.TrackingDeclarations.MasterBill],
				TestProvider[WebTracker.Grids.Milestones.LastMilestoneDescription],
				TestProvider[WebTracker.Grids.TrackingDeclarations.MasterBill]
			};
		}

		protected override void SetupColumnsCore()
		{
			base.SetupColumnsCore();
			AddRequiredColumn(new ZHyperLinkColumn("Job#", ShipmentDeclarationSchema.Constants.Number)
			{
				ColumnKey = WebTracker.Grids.TrackingDeclarations.JobNumber,
				DataNavigateUrlFormatString = TrackingConstants.RelativePath.ShipmentPage + "?Ref={0}&Table={1}", // Partial URL string
				DataNavigateUrlFields = new string[2] { "PersistentBizOPK", "TableName" }
			});

			AddDefaultsColumn(new ZFindBoxColumn("Branch", "Declaration+JE_GB", "Declaration.Lookups.BranchCollection") { ColumnKey = WebTracker.Grids.TrackingDeclarations.Branch });
			AddDefaultsColumn(new ZTextEditColumn("Type", "Declaration+JE_MessageType") { ColumnKey = WebTracker.Grids.TrackingDeclarations.Type });
			AddDefaultsColumn(new ZTextEditColumn("Transport", "Declaration+JE_TransportMode") { ColumnKey = WebTracker.Grids.TrackingDeclarations.Transport });
			AddDefaultsColumn(new ZTextEditColumn("Job Number", "Declaration+JE_DeclarationReference") { ColumnKey = WebTracker.Grids.TrackingDeclarations.DeclarationReference });
			AddDefaultsColumn(new ZTextEditColumn("Vessel", "Declaration+JE_VesselName") { ColumnKey = WebTracker.Grids.TrackingDeclarations.Vessel });
			AddDefaultsColumn(new ZTextEditColumn("Voyage/Flight", "Declaration+JE_VoyageFlightNo") { ColumnKey = WebTracker.Grids.TrackingDeclarations.Voyage });
			AddDefaultsColumn(new ZDateTimeColumn("Date Of Arrival", "Declaration+JE_DateOfArrival") { ColumnKey = WebTracker.Grids.TrackingDeclarations.DateOfArrival });
			AddDefaultsColumn(new ZTextEditColumn("Origin", "Declaration+JE_RL_NKOrigin") { ColumnKey = WebTracker.Grids.TrackingDeclarations.Origin });
			AddDefaultsColumn(new ZTextEditColumn("Final Dest.", "Declaration+JE_RL_NKFinalDestination") { ColumnKey = WebTracker.Grids.TrackingDeclarations.FinalDestination });
			AddDefaultsColumn(new ZTextEditColumn("House Bill", "Declaration+JE_HouseBill") { ColumnKey = WebTracker.Grids.TrackingDeclarations.HouseBill });
			AddDefaultsColumn(new ZTextEditColumn("Supplier", ShipmentDeclarationSchema.Constants.ConsignorName) { ColumnKey = WebTracker.Grids.TrackingDeclarations.Supplier });
			AddDefaultsColumn(new ZTextEditColumn("Importer", ShipmentDeclarationSchema.Constants.ConsigneeName) { ColumnKey = WebTracker.Grids.TrackingDeclarations.Importer });
			AddDefaultsColumn(new ZTextEditColumn("Country/Region", "Declaration+Country+Description") { ColumnKey = WebTracker.Grids.TrackingDeclarations.Country });
			AddColumn(new ZFindBoxColumn("Importer Code", "Declaration+JE_OH_Importer", "Declaration.Lookups.ImportersList") { ColumnKey = WebTracker.Grids.TrackingDeclarations.ImporterCode });
			AddColumn(new ZFindBoxColumn("Supplier Code", "Declaration+JE_OH_Supplier", "Declaration.Lookups.SuppliersList") { ColumnKey = WebTracker.Grids.TrackingDeclarations.SupplierCode });
			AddColumn(new ZTextEditColumn("Agents Ref", "Declaration+JE_AgentsReference") { ColumnKey = WebTracker.Grids.TrackingDeclarations.AgentsReference });
			AddColumn(new ZTextEditColumn("Container Mode", "Declaration+JE_ContainerMode") { ColumnKey = WebTracker.Grids.TrackingDeclarations.ContainerMode });
			AddColumn(new ZTextEditColumn("Containers Count", "Declaration+JE_ContainerCount") { ColumnKey = WebTracker.Grids.TrackingDeclarations.Containers });
			AddColumn(new ZDateTimeColumn("Date of First Arrival", "Declaration+JE_DateOfFirstArrival") { ColumnKey = WebTracker.Grids.TrackingDeclarations.DateOfFirstArrival });
			AddColumn(new ZTextEditColumn("EFT Mode", "Declaration+JE_EFTMode") { ColumnKey = WebTracker.Grids.TrackingDeclarations.EFTMode });
			AddColumn(new ZDateTimeColumn("Entry Auth. Date", "Declaration+JE_EntryAuthorisationDate") { ColumnKey = WebTracker.Grids.TrackingDeclarations.EntryAuthorisationDate });
			AddColumn(new ZDateTimeColumn("Export Date", "Declaration+JE_ExportDate") { ColumnKey = WebTracker.Grids.TrackingDeclarations.ExportDate });
			AddColumn(new ZTextEditColumn("Export Goods Type", "Declaration+JE_ExportGoodsType") { ColumnKey = WebTracker.Grids.TrackingDeclarations.ExportGoodsType });
			AddColumn(new ZTextEditColumn("Goods Description", "Declaration+JE_GoodsDescription") { ColumnKey = WebTracker.Grids.TrackingDeclarations.GoodsDescription });
			AddColumn(new ZTextEditColumn("Master Bill", "Declaration+JE_MasterBill") { ColumnKey = WebTracker.Grids.TrackingDeclarations.MasterBill });
			AddColumn(new ZTextEditColumn("Sub Type", "Declaration+JE_MessageSubType") { ColumnKey = WebTracker.Grids.TrackingDeclarations.SubType });
			AddColumn(new ZTextEditColumn("Owner's Ref#", "Declaration+JE_OwnerRef") { ColumnKey = WebTracker.Grids.TrackingDeclarations.OwnerRef });
			AddColumn(new ZTextEditColumn("Arrival", "Declaration+JE_RL_NKPortOfArrival") { ColumnKey = WebTracker.Grids.TrackingDeclarations.Arrival });
			AddColumn(new ZTextEditColumn("First Arrival", "Declaration+JE_RL_NKPortOfFirstArrival") { ColumnKey = WebTracker.Grids.TrackingDeclarations.FirstArrival });
			AddColumn(new ZTextEditColumn("Loading", "Declaration+JE_RL_NKPortOfLoading") { ColumnKey = WebTracker.Grids.TrackingDeclarations.Loading });
			AddColumn(new ZTextEditColumn("Total Packs", "Declaration+JE_TotalNoOfPacks") { ColumnKey = WebTracker.Grids.TrackingDeclarations.TotalPacks });
			AddColumn(new ZTextEditColumn("Pack Type", "Declaration+JE_TotalNoOfPacksPackType") { ColumnKey = WebTracker.Grids.TrackingDeclarations.PackType });
			AddColumn(new ZTextEditColumn("Entry Number", "Declaration+DeclarationNumber") { ColumnKey = WebTracker.Grids.TrackingDeclarations.EntryNumber });
			AddColumn(new ZDateTimeColumn("Earliest Customs Entry Issue Date", "Declaration+EarliestCustomsEntryIssueDate") { ColumnKey = WebTracker.Grids.TrackingDeclarations.EarliestCustomsEntry });
			AddColumn(new ZTextEditColumn("Order Ref#", ShipmentDeclarationSchema.Constants.OrderReference) { ColumnKey = WebTracker.Grids.TrackingDeclarations.OrderReferences });
			AddColumn(new ZTextEditColumn("Volume", "Declaration+JE_TotalVolume") { ColumnKey = WebTracker.Grids.TrackingDeclarations.Volume });
			AddColumn(new ZTextEditColumn("Volume UQ", "Declaration+JE_TotalVolumeUnit") { ColumnKey = WebTracker.Grids.TrackingDeclarations.VolumeUnit });
			AddColumn(new ZTextEditColumn("Weight", "Declaration+JE_TotalWeight") { ColumnKey = WebTracker.Grids.TrackingDeclarations.Weight });
			AddColumn(new ZTextEditColumn("Weight UQ", "Declaration+JE_TotalWeightUnit") { ColumnKey = WebTracker.Grids.TrackingDeclarations.WeightUnit });
			AddColumn(new ZTextEditColumn("Message Status", "Declaration+JE_MessageStatus") { ColumnKey = WebTracker.Grids.TrackingDeclarations.MessageStatus });
			AddColumn(new ZDateTimeColumn("Date Created", "Declaration+JE_SystemCreateTimeUtc") { ColumnKey = WebTracker.Grids.TrackingDeclarations.DateCreated });
			AddColumn(new ZTextEditColumn("Broker", "Declaration+JE_GS_NKCusAgent") { ColumnKey = WebTracker.Grids.TrackingDeclarations.Broker });
			AddColumn(new ZTextEditColumn("Containers", "Top3Containers") { ColumnKey = WebTracker.Grids.TrackingDeclarations.Top3Containers });
			AddColumn(new ZCalcEditColumn("TEU", ShipmentDeclarationSchema.Constants.TEUCount) { ColumnKey = WebTracker.Grids.TrackingDeclarations.TEUCount });
			SetupCountrySpecificColumns();
			foreach (ZTemplateColumn column in ConfigurationHelper.GetMilestonesColumns("Milestones"))
			{
				AddColumn(column);
			}
		}

		protected virtual void SetupCountrySpecificColumns()
		{
			AddColumn(new ZDateTimeColumn("Entry Submitted", "Declaration+JE_EntrySubmittedDate") { ColumnKey = WebTracker.Grids.TrackingDeclarations.EntrySubmittedDate });
			AddColumn(new ZTextEditColumn("Entry Status", "Declaration+JE_EntryStatus") { ColumnKey = WebTracker.Grids.TrackingDeclarations.EntryStatus });
			AddColumn(new ZTextEditColumn("Entry Status Desc.", "Declaration+JE_EntryStatusDescription") { ColumnKey = WebTracker.Grids.TrackingDeclarations.EntryStatusDescription });
			AddColumn(new ZTextEditColumn("Cargo Status", "Declaration+JE_ConsolidatedCargoStatus") { ColumnKey = WebTracker.Grids.TrackingDeclarations.CargoStatus });
			AddColumn(new ZTextEditColumn("Cargo Status Description", "Declaration+ConsolidatedCargoStatusDescription") { ColumnKey = WebTracker.Grids.TrackingDeclarations.CargoStatusDescription });
		}

		protected override List<object> GetUnsortableColumnKeys() => new List<object>
		{
			WebTracker.Grids.TrackingDeclarations.Branch,
			WebTracker.Grids.TrackingDeclarations.ImporterCode,
			WebTracker.Grids.TrackingDeclarations.SupplierCode
		};

		protected override GridColumnProvider GetNewTestProvider()
		{
			return new BaseDeclarationModuleColumnProvider();
		}
	}
}
