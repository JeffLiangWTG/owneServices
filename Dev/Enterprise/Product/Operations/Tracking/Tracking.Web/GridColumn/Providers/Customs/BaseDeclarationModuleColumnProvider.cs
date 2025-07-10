using System.Collections.Generic;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
	public class BaseDeclarationModuleColumnProvider : GridColumnProvider
	{
		protected override void CustomizeDictionaryCore()
		{
			base.CustomizeDictionaryCore();
			ZHyperLinkColumn shipmentNumberColumn = new ZHyperLinkColumn(Res.GetString("0150feb2-431e-42c5-bae7-d953cd3b9663", "Job#"), ShipmentDeclarationSchema.Constants.Number) { ColumnKey = WebTracker.Grids.TrackingDeclarations.JobNumber };
			shipmentNumberColumn.DataNavigateUrlFormatString = this.UrlFormatWithAppRoot(TrackingConstants.RelativePath.ShipmentPage) + (NoResString)"?Ref={0}&Table={1}"; // Partial URL string
			shipmentNumberColumn.DataNavigateUrlFields = new string[2] { "PersistentBizOPK", "TableName" };
			AddToDictionaryAsRequired(shipmentNumberColumn);

			AddToDictionaryAsDefault(new ZFindBoxColumn(Res.GetString("1f9aa551-a300-43d7-9cb3-8097be416a7d", "Branch"), "Declaration+JE_GB", "Declaration.Lookups.BranchCollection") { ColumnKey = WebTracker.Grids.TrackingDeclarations.Branch });
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("bb22076d-5f5d-417a-a942-eedd9ab4e608", "Type"), "Declaration+JE_MessageType") { ColumnKey = WebTracker.Grids.TrackingDeclarations.Type });
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("aeacd621-7872-44db-8400-b716dec6dcdd", "Transport"), "Declaration+JE_TransportMode") { ColumnKey = WebTracker.Grids.TrackingDeclarations.Transport });
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("dbef47b7-c38c-4cb1-a26a-861de7b44768", "Job Number"), "Declaration+JE_DeclarationReference") { ColumnKey = WebTracker.Grids.TrackingDeclarations.DeclarationReference });
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("542f55c1-0f8b-46de-be44-e9522c78b266", "Vessel"), "Declaration+JE_VesselName") { ColumnKey = WebTracker.Grids.TrackingDeclarations.Vessel });
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("6b976d37-2c32-4fbe-830d-35242499c2a6", "Voyage/Flight"), "Declaration+JE_VoyageFlightNo") { ColumnKey = WebTracker.Grids.TrackingDeclarations.Voyage });
			AddToDictionaryAsDefault(new ZDateTimeColumn(Res.GetString("0eab9a22-6479-4172-b291-387bd8ae92e9", "Date Of Arrival"), "Declaration+JE_DateOfArrival") { ColumnKey = WebTracker.Grids.TrackingDeclarations.DateOfArrival });
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("a4955d0c-b823-4a01-9172-a8b400e49561", "Origin"), "Declaration+JE_RL_NKOrigin") { ColumnKey = WebTracker.Grids.TrackingDeclarations.Origin });
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("ccab21e0-d260-40d2-b06a-75c36b713161", "Final Dest."), "Declaration+JE_RL_NKFinalDestination") { ColumnKey = WebTracker.Grids.TrackingDeclarations.FinalDestination });
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("b7dcc9c9-9f84-4103-b06d-b45352a4f0ad", "House Bill"), "Declaration+JE_HouseBill") { ColumnKey = WebTracker.Grids.TrackingDeclarations.HouseBill });
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("57a12ee8-0db4-458d-aa86-89a366d2ddd4", "Supplier"), ShipmentDeclarationSchema.Constants.ConsignorName) { ColumnKey = WebTracker.Grids.TrackingDeclarations.Supplier });
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("e83b30d0-a9ae-4d7a-bf37-dc7f75e22d32", "Importer"), ShipmentDeclarationSchema.Constants.ConsigneeName) { ColumnKey = WebTracker.Grids.TrackingDeclarations.Importer });
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("6d269c20-9ad9-4415-9c56-45a7e23c80e9", "Country/Region"), "Declaration+Country+Description") { ColumnKey = WebTracker.Grids.TrackingDeclarations.Country });
			AddToDictionary(new ZFindBoxColumn(Res.GetString("16c0df1e-66cb-49c1-bb6b-ee7e20ebeb2c", "Importer Code"), "Declaration+JE_OH_Importer", "Declaration.Lookups.ImportersList") { ColumnKey = WebTracker.Grids.TrackingDeclarations.ImporterCode });
			AddToDictionary(new ZFindBoxColumn(Res.GetString("87c52b2f-6173-4330-b232-108035bc9d10", "Supplier Code"), "Declaration+JE_OH_Supplier", "Declaration.Lookups.SuppliersList") { ColumnKey = WebTracker.Grids.TrackingDeclarations.SupplierCode });
			AddToDictionary(new ZTextEditColumn(Res.GetString("93687393-c714-4b02-ac85-3ec3abee4de1", "Agents Ref"), "Declaration+JE_AgentsReference") { ColumnKey = WebTracker.Grids.TrackingDeclarations.AgentsReference });
			AddToDictionary(new ZTextEditColumn(Res.GetString("c2c1a797-3649-4ccd-8063-ab0a7fb8cb26", "Container Mode"), "Declaration+JE_ContainerMode") { ColumnKey = WebTracker.Grids.TrackingDeclarations.ContainerMode });
			AddToDictionary(new ZTextEditColumn(Res.GetString("e6976d1d-b327-40bc-a637-bc6f26a363f7", "Containers Count"), "Declaration+JE_ContainerCount") { ColumnKey = WebTracker.Grids.TrackingDeclarations.Containers });
			AddToDictionary(new ZDateTimeColumn(Res.GetString("c6bff09f-d495-4f49-8c09-c59512b4720c", "Date of First Arrival"), "Declaration+JE_DateOfFirstArrival") { ColumnKey = WebTracker.Grids.TrackingDeclarations.DateOfFirstArrival });
			AddToDictionary(new ZTextEditColumn(Res.GetString("06065d39-7786-467c-bf44-b8a1e01cf991", "EFT Mode"), "Declaration+JE_EFTMode") { ColumnKey = WebTracker.Grids.TrackingDeclarations.EFTMode });
			AddToDictionary(new ZDateTimeColumn(Res.GetString("9d96ee9d-e1ab-4e63-b1e9-58ac88a694ce", "Entry Auth. Date"), "Declaration+JE_EntryAuthorisationDate") { ColumnKey = WebTracker.Grids.TrackingDeclarations.EntryAuthorisationDate });
			AddToDictionary(new ZDateTimeColumn(Res.GetString("857fee92-01ce-45c7-a661-d34a57588929", "Export Date"), "Declaration+JE_ExportDate") { ColumnKey = WebTracker.Grids.TrackingDeclarations.ExportDate });
			AddToDictionary(new ZTextEditColumn(Res.GetString("e74f1b29-5d07-4786-9503-f5a495068876", "Export Goods Type"), "Declaration+JE_ExportGoodsType") { ColumnKey = WebTracker.Grids.TrackingDeclarations.ExportGoodsType });
			AddToDictionary(new ZTextEditColumn(Res.GetString("1deaa50d-beb4-4041-a2b6-05d062149f80", "Goods Description"), "Declaration+JE_GoodsDescription") { ColumnKey = WebTracker.Grids.TrackingDeclarations.GoodsDescription });
			AddToDictionary(new ZTextEditColumn(Res.GetString("8e017f71-d63c-40fe-ab2d-3ccdf6d8ed41", "Master Bill"), "Declaration+JE_MasterBill") { ColumnKey = WebTracker.Grids.TrackingDeclarations.MasterBill });
			AddToDictionary(new ZTextEditColumn(Res.GetString("94d7e951-52e5-43ca-be81-0d87fcb02651", "Sub Type"), "Declaration+JE_MessageSubType") { ColumnKey = WebTracker.Grids.TrackingDeclarations.SubType });
			AddToDictionary(new ZTextEditColumn(Res.GetString("A1589A57-A426-4912-8C74-7217FD4CE0CA", "Owner's Ref#"), "Declaration+JE_OwnerRef") { ColumnKey = WebTracker.Grids.TrackingDeclarations.OwnerRef });
			AddToDictionary(new ZTextEditColumn(Res.GetString("31d0f8b5-e940-4865-9315-3760839a9147", "Arrival"), "Declaration+JE_RL_NKPortOfArrival") { ColumnKey = WebTracker.Grids.TrackingDeclarations.Arrival });
			AddToDictionary(new ZTextEditColumn(Res.GetString("7cc1cbe4-a98b-4073-88ff-7a7d78e70ffa", "First Arrival"), "Declaration+JE_RL_NKPortOfFirstArrival") { ColumnKey = WebTracker.Grids.TrackingDeclarations.FirstArrival });
			AddToDictionary(new ZTextEditColumn(Res.GetString("50634ec0-d198-4430-b2c4-6035ec6e5c00", "Loading"), "Declaration+JE_RL_NKPortOfLoading") { ColumnKey = WebTracker.Grids.TrackingDeclarations.Loading });
			AddToDictionary(new ZTextEditColumn(Res.GetString("c13a73a5-e751-473f-9b1a-134c02a5e7ec", "Total Packs"), "Declaration+JE_TotalNoOfPacks") { ColumnKey = WebTracker.Grids.TrackingDeclarations.TotalPacks });
			AddToDictionary(new ZTextEditColumn(Res.GetString("ff6b9cd7-3cee-4740-a97b-f8c03dc509c4", "Pack Type"), "Declaration+JE_TotalNoOfPacksPackType") { ColumnKey = WebTracker.Grids.TrackingDeclarations.PackType });
			AddToDictionary(new ZTextEditColumn(Res.GetString("7efd9d86-0533-4191-be13-763cf3bc53ee", "Entry Number"), "Declaration+DeclarationNumber") { ColumnKey = WebTracker.Grids.TrackingDeclarations.EntryNumber });
			AddToDictionary(new ZDateTimeColumn(Res.GetString("1fcafc68-c744-41ce-b511-009a37a5bddd", "Earliest Customs Entry Issue Date"), "Declaration+EarliestCustomsEntryIssueDate") { ColumnKey = WebTracker.Grids.TrackingDeclarations.EarliestCustomsEntry });
			AddToDictionary(new ZTextEditColumn(Res.GetString("6D977935-136D-42D7-947B-27E8BEC08DF0", "Order Ref#"), ShipmentDeclarationSchema.Constants.OrderReference) { ColumnKey = WebTracker.Grids.TrackingDeclarations.OrderReferences });
			AddToDictionary(new ZTextEditColumn(Res.GetString("75325277-2a75-4562-ba0c-714ac2cbe735", "Volume"), "Declaration+JE_TotalVolume") { ColumnKey = WebTracker.Grids.TrackingDeclarations.Volume });
			AddToDictionary(new ZTextEditColumn(Res.GetString("fc441c2d-f0df-4658-8ab9-c9f02d2c95a5", "Volume UQ"), "Declaration+JE_TotalVolumeUnit") { ColumnKey = WebTracker.Grids.TrackingDeclarations.VolumeUnit });
			AddToDictionary(new ZTextEditColumn(Res.GetString("1eb27d96-7363-4a74-a010-757b3c268fb7", "Weight"), "Declaration+JE_TotalWeight") { ColumnKey = WebTracker.Grids.TrackingDeclarations.Weight });
			AddToDictionary(new ZTextEditColumn(Res.GetString("da30be5e-12f3-4841-a7a1-4dcd72a0f73e", "Weight UQ"), "Declaration+JE_TotalWeightUnit") { ColumnKey = WebTracker.Grids.TrackingDeclarations.WeightUnit });
			AddToDictionary(new ZTextEditColumn(Res.GetString("733a0859-59c2-44a7-822b-adac7a4cf73e", "Message Status"), "Declaration+JE_MessageStatus") { ColumnKey = WebTracker.Grids.TrackingDeclarations.MessageStatus });
			AddToDictionary(new ZDateTimeColumn(Res.GetString("88f8bb72-f9ba-4c00-bb9c-761e5fbae115", "Date Created"), "Declaration+JE_SystemCreateTimeUtc") { ColumnKey = WebTracker.Grids.TrackingDeclarations.DateCreated });
			AddToDictionary(new ZTextEditColumn(Res.GetString("85ecdf17-54ed-4288-ac31-dbaa2da5d586", "Broker"), "Declaration+JE_GS_NKCusAgent") { ColumnKey = WebTracker.Grids.TrackingDeclarations.Broker });
			AddToDictionary(new ZTextEditColumn(Res.GetString("D198A5C3-F7AA-4DF1-A036-4E7940031CEC", "Containers"), "Top3Containers") { ColumnKey = WebTracker.Grids.TrackingDeclarations.Top3Containers });
			AddToDictionary(new ZCalcEditColumn(Res.GetString("1d5b757a-6678-4a8d-b237-006c8b0e574f", "TEU"), ShipmentDeclarationSchema.Constants.TEUCount) { ColumnKey = WebTracker.Grids.TrackingDeclarations.TEUCount });
			AddCountrySpecificColumns();
			foreach (ZTemplateColumn column in ConfigurationHelper.GetMilestonesColumns((NoResString)"Milestones")) // Constant
			{
				AddToDictionary(column);
			}
		}

		protected virtual void AddCountrySpecificColumns()
		{
			AddToDictionary(new ZDateTimeColumn(Res.GetString("225a42a6-d102-4527-9b89-9f7e7a36f7f3", "Entry Submitted"), "Declaration+JE_EntrySubmittedDate") { ColumnKey = WebTracker.Grids.TrackingDeclarations.EntrySubmittedDate });
			AddToDictionary(new ZTextEditColumn(Res.GetString("35f724ec-feb5-4f77-a7e6-9bc6943cf388", "Entry Status"), "Declaration+JE_EntryStatus") { ColumnKey = WebTracker.Grids.TrackingDeclarations.EntryStatus });
			AddToDictionary(new ZTextEditColumn(Res.GetString("e361ddea-91ed-4e79-9bf9-d03aecab89c2", "Entry Status Desc."), "Declaration+JE_EntryStatusDescription") { ColumnKey = WebTracker.Grids.TrackingDeclarations.EntryStatusDescription });
			AddToDictionary(new ZTextEditColumn(Res.GetString("52494163-2e09-4516-ad60-5a2adc332d14", "Cargo Status"), "Declaration+JE_ConsolidatedCargoStatus") { ColumnKey = WebTracker.Grids.TrackingDeclarations.CargoStatus });
			AddToDictionary(new ZTextEditColumn(Res.GetString("9725ee15-bf65-4449-a1e4-39ff76303e88", "Cargo Status Description"), "Declaration+ConsolidatedCargoStatusDescription") { ColumnKey = WebTracker.Grids.TrackingDeclarations.CargoStatusDescription });
		}

		protected override List<int> GetOldColumnsOrder()
		{
			List<int> result = new List<int>();
			result.Add((int)WebTracker.Grids.TrackingDeclarations.JobNumber);
			result.Add((int)WebTracker.Grids.TrackingDeclarations.Branch);
			result.Add((int)WebTracker.Grids.TrackingDeclarations.Type);
			result.Add((int)WebTracker.Grids.TrackingDeclarations.Transport);
			result.Add((int)WebTracker.Grids.TrackingDeclarations.DeclarationReference);
			result.Add((int)WebTracker.Grids.TrackingDeclarations.Vessel);
			result.Add((int)WebTracker.Grids.TrackingDeclarations.Voyage);
			result.Add((int)WebTracker.Grids.TrackingDeclarations.DateOfArrival);
			result.Add((int)WebTracker.Grids.TrackingDeclarations.Origin);
			result.Add((int)WebTracker.Grids.TrackingDeclarations.FinalDestination);
			result.Add((int)WebTracker.Grids.TrackingDeclarations.HouseBill);
			result.Add((int)WebTracker.Grids.TrackingDeclarations.Supplier);
			result.Add((int)WebTracker.Grids.TrackingDeclarations.Importer);
			result.Add((int)WebTracker.Grids.TrackingDeclarations.ImporterCode);
			result.Add((int)WebTracker.Grids.TrackingDeclarations.SupplierCode);
			result.Add((int)WebTracker.Grids.TrackingDeclarations.AgentsReference);
			result.Add((int)WebTracker.Grids.TrackingDeclarations.ContainerMode);
			result.Add((int)WebTracker.Grids.TrackingDeclarations.Containers);
			result.Add((int)WebTracker.Grids.TrackingDeclarations.DateOfFirstArrival);
			result.Add((int)WebTracker.Grids.TrackingDeclarations.EFTMode);
			result.Add((int)WebTracker.Grids.TrackingDeclarations.EntryAuthorisationDate);
			result.Add((int)WebTracker.Grids.TrackingDeclarations.EntryStatus);
			result.Add((int)WebTracker.Grids.TrackingDeclarations.EntrySubmittedDate);
			result.Add((int)WebTracker.Grids.TrackingDeclarations.ExportDate);
			result.Add((int)WebTracker.Grids.TrackingDeclarations.ExportGoodsType);
			result.Add((int)WebTracker.Grids.TrackingDeclarations.GoodsDescription);
			result.Add((int)WebTracker.Grids.TrackingDeclarations.MasterBill);
			result.Add((int)WebTracker.Grids.TrackingDeclarations.SubType);
			result.Add((int)WebTracker.Grids.TrackingDeclarations.OwnerRef);
			result.Add((int)WebTracker.Grids.TrackingDeclarations.Arrival);
			result.Add((int)WebTracker.Grids.TrackingDeclarations.FirstArrival);
			result.Add((int)WebTracker.Grids.TrackingDeclarations.Loading);
			result.Add((int)WebTracker.Grids.TrackingDeclarations.TotalPacks);
			result.Add((int)WebTracker.Grids.TrackingDeclarations.PackType);
			result.Add((int)WebTracker.Grids.TrackingDeclarations.EntryNumber);
			result.Add((int)WebTracker.Grids.TrackingDeclarations.EarliestCustomsEntry);
			result.Add((int)WebTracker.Grids.TrackingDeclarations.EntryStatusDescription);
			result.Add((int)WebTracker.Grids.TrackingDeclarations.OrderReferences);
			result.Add((int)WebTracker.Grids.TrackingDeclarations.Volume);
			result.Add((int)WebTracker.Grids.TrackingDeclarations.VolumeUnit);
			result.Add((int)WebTracker.Grids.TrackingDeclarations.Weight);
			result.Add((int)WebTracker.Grids.TrackingDeclarations.WeightUnit);
			result.Add((int)WebTracker.Grids.TrackingDeclarations.MessageStatus);
			result.Add((int)WebTracker.Grids.TrackingDeclarations.CargoStatus);
			result.Add((int)WebTracker.Grids.TrackingDeclarations.CargoStatusDescription);
			result.Add((int)WebTracker.Grids.TrackingDeclarations.DateCreated);
			result.Add((int)WebTracker.Grids.TrackingDeclarations.Broker);
			result.Add((int)WebTracker.Grids.TrackingDeclarations.Top3Containers);
			foreach (ZTemplateColumn column in ConfigurationHelper.GetMilestonesColumns((NoResString)"Milestones")) // Constant
			{
				result.Add(((IUniqueKeyColumn)column).UniqueKey);
			}
			return result;
		}
	}
}
