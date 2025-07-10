using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Tracking.Business;
using Enterprise.Tracking.Business.ImporterSecurityFiling;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
	public class ISFModuleColumnProvider : GridColumnProvider
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0004:Remove Unnecessary Cast", Justification = "ZBindToChecker requires a redundant cast")]
		protected override void CustomizeDictionaryCore()
		{
			base.CustomizeDictionaryCore();
			ZBindToChecker.CheckBindTo((ZString)((TrackingCusISFHeader)null).BF_JobReference);
			AddToDictionaryAsRequired(new ZHyperLinkColumn(Res.GetString("3aea801f-f6ec-4ba1-9a72-a23fdc61d0e7", "Job Ref."), TrackingCusISFHeader.Schema.BF_JobReference)
			{
				ColumnKey = WebTracker.Grids.TrackingImporterSecurityFilings.JobReference,
				DataNavigateUrlFormatString = (UrlFormatWithAppRoot(TrackingConstants.RelativePath.ISFDetailsPage) + (NoResString)"?Ref={0}"), // Partial URL String
				DataNavigateUrlFields = new[] { "PK" }
			});

			ZBindToChecker.CheckBindTo((ZString)((TrackingCusISFHeader)null).BF_CustomsReference);
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("5b861fce-d103-4179-b17f-cfbb6e5c27c7", "Customs Reference"), TrackingCusISFHeader.Schema.BF_CustomsReference) { ColumnKey = WebTracker.Grids.TrackingImporterSecurityFilings.CustomsReference });

			ZBindToChecker.CheckBindTo((ZString)((TrackingCusISFHeader)null).Importer.OH_FullName);
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("30dbc5a1-6c5a-404f-b126-892ce6c46ec8", "Importer"), "Importer+OH_FullName") { ColumnKey = WebTracker.Grids.TrackingImporterSecurityFilings.Importer });

			ZBindToChecker.CheckBindTo((ZString)((TrackingCusISFHeader)null).BF_HouseBill);
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("9daff5b5-3bd2-4ee7-823c-3a335f7146f8", "House Bill"), TrackingCusISFHeader.Schema.BF_HouseBill) { ColumnKey = WebTracker.Grids.TrackingImporterSecurityFilings.HouseBill });

			ZBindToChecker.CheckBindTo((ZString)((TrackingCusISFHeader)null).BF_CustomsStatusDescription);
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("36f2cace-1947-41ad-b43d-7c856b56afc8", "Status"), TrackingCusISFHeader.Schema.BF_CustomsStatusDescription) { ColumnKey = WebTracker.Grids.TrackingImporterSecurityFilings.Status });

			ZBindToChecker.CheckBindTo((ZString)((TrackingCusISFHeader)null).SellingParty.E2_CompanyName);
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("ada672c4-b2d5-4b7e-8bfe-9e7e4a030a56", "Selling Party"), "SellingParty+E2_CompanyName") { ColumnKey = WebTracker.Grids.TrackingImporterSecurityFilings.SellingParty });

			ZBindToChecker.CheckBindTo((ZString)((TrackingCusISFHeader)null).BuyingParty.E2_CompanyName);
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("58407cf5-6580-43b1-b615-54ef7c9e40a2", "Buying Party"), "BuyingParty+E2_CompanyName") { ColumnKey = WebTracker.Grids.TrackingImporterSecurityFilings.BuyingParty });

			ZBindToChecker.CheckBindTo((ZString)((TrackingCusISFHeader)null).MainShipToParty.E2_CompanyName);
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("ed62e523-4634-43a3-9799-76128db44775", "Main Ship To Party"), "MainShipToParty+E2_CompanyName") { ColumnKey = WebTracker.Grids.TrackingImporterSecurityFilings.MainShipToParty });

			AddToDictionary(new ZTextEditColumn(Res.GetString("435d53a7-6375-49db-8a6c-7c8d826cdf3b", "Carrier SCAC"), TrackingCusISFHeader.Schema.BF_SCAC) { ColumnKey = WebTracker.Grids.TrackingImporterSecurityFilings.CarrierSCAC });
			AddToDictionary(new ZTextEditColumn(Res.GetString("4a28a003-a6ad-42bc-9990-946a309201f7", "ID Type"), TrackingCusISFHeader.Schema.BF_ImporterCodeType) { ColumnKey = WebTracker.Grids.TrackingImporterSecurityFilings.IDType });

			AddToDictionary(new ZTextEditColumn(Res.GetString("4ececeed-2837-48ec-b8e0-d4c8f9a7d260", "Importer Id Type"), TrackingCusISFHeader.Schema.BF_ImporterCodeType) { ColumnKey = WebTracker.Grids.TrackingImporterSecurityFilings.ImporterIDType });
			AddToDictionary(new ZTextEditColumn(Res.GetString("b66b6bcb-331a-43a6-949f-ef77c4ed8ac0", "Importer Identification"), TrackingCusISFHeader.Schema.BF_ImporterCode) { ColumnKey = WebTracker.Grids.TrackingImporterSecurityFilings.ImporterIdentification });
			AddToDictionary(new ZDateTimeColumn(Res.GetString("6e6ad408-c758-4223-865c-be0018453523", "DOB"), TrackingCusISFHeader.Schema.BF_DateOfBirth, ZDateTimePickerFormat.Short) { ColumnKey = WebTracker.Grids.TrackingImporterSecurityFilings.DateOfBirth });
			AddToDictionary(new ZTextEditColumn(Res.GetString("352143cc-2ff8-4c1a-a037-41a8929ae486", "Issue Ctry/Rgn."), TrackingCusISFHeader.Schema.BF_CountryOfIssue) { ColumnKey = WebTracker.Grids.TrackingImporterSecurityFilings.IssueCountry });

			AddToDictionary(new ZTextEditColumn(Res.GetString("88471423-a274-4403-9b5f-d0200e842e8f", "Unload Port"), TrackingCusISFHeader.Schema.BF_RL_NKPortOfUnload) { ColumnKey = WebTracker.Grids.TrackingImporterSecurityFilings.UnloadPort });
			AddToDictionary(new ZTextEditColumn(Res.GetString("a7b32e41-dfc3-46f9-a77e-99c344a9d3d2", "Delivery Port"), TrackingCusISFHeader.Schema.BF_RL_NKPlaceOfDelivery) { ColumnKey = WebTracker.Grids.TrackingImporterSecurityFilings.DeliveryPort });
			AddToDictionary(new ZTextEditColumn(Res.GetString("04c28e16-d94a-4ee7-8ccb-cb353ce22279", "Bond Holder"), TrackingCusISFHeader.Schema.BF_BondNumberOrHolder) { ColumnKey = WebTracker.Grids.TrackingImporterSecurityFilings.BondHolder });
			AddToDictionary(new ZTextEditColumn(Res.GetString("883ade23-ed1c-49bc-a1b1-77a7107816ce", "Surety Code"), TrackingCusISFHeader.Schema.BF_SuretyCode) { ColumnKey = WebTracker.Grids.TrackingImporterSecurityFilings.SuretyCode });

			AddToDictionary(new ZTextEditColumn(Res.GetString("c7fed8b5-9b2a-4e4f-8e9e-817324f133a2", "Cnee. ID Type"), TrackingCusISFHeader.Schema.BF_ConsigneeCodeType) { ColumnKey = WebTracker.Grids.TrackingImporterSecurityFilings.ConsigneeIDType });
			AddToDictionary(new ZTextEditColumn(Res.GetString("655ea0bd-80cf-4817-878d-a955a8fa65de", "Cnee. ID"), TrackingCusISFHeader.Schema.BF_ConsigneeCode) { ColumnKey = WebTracker.Grids.TrackingImporterSecurityFilings.ConsigneeID });

			AddToDictionary(new ZTextEditColumn(Res.GetString("3dd3883c-9eaa-4d85-9b1a-9dc3dc81296a", "Ocean Bill"), TrackingCusISFHeader.Schema.BF_OceanBill) { ColumnKey = WebTracker.Grids.TrackingImporterSecurityFilings.OceanBill });
			AddToDictionary(new ZTextEditColumn(Res.GetString("31eba763-7229-4974-b962-2ca53d44709e", "Master Bill"), TrackingCusISFHeader.Schema.BF_MasterBill) { ColumnKey = WebTracker.Grids.TrackingImporterSecurityFilings.MasterBill });

			ZBindToChecker.CheckBindTo((ZString)((TrackingCusISFHeader)null).FirstUSTransport.JW_Vessel);
			AddToDictionary(new ZTextEditColumn(Res.GetString("336b2b34-c9a6-467e-9fac-cb6098cc03e6", "1st US Route Vessel"), "FirstUSTransport+JW_Vessel") { ColumnKey = WebTracker.Grids.TrackingImporterSecurityFilings.FirstUSRouteVessel });
			ZBindToChecker.CheckBindTo((ZString)((TrackingCusISFHeader)null).FirstUSTransport.JW_VoyageFlight);
			AddToDictionary(new ZTextEditColumn(Res.GetString("883fe88e-b4c2-4b9a-a2dd-4b6487d9bb67", "1st US Route Voyage/Flight"), "FirstUSTransport+JW_VoyageFlight") { ColumnKey = WebTracker.Grids.TrackingImporterSecurityFilings.FirstUSRouteVoyage });
			ZBindToChecker.CheckBindTo((ZString)((TrackingCusISFHeader)null).FirstUSTransport.JW_RL_NKLoadPort);
			AddToDictionary(new ZTextEditColumn(Res.GetString("287638fc-e326-4f91-9f45-f576cbf6480a", "1st US Route Load Port"), "FirstUSTransport+JW_RL_NKLoadPort") { ColumnKey = WebTracker.Grids.TrackingImporterSecurityFilings.FirstUSRouteLoadPort });
			ZBindToChecker.CheckBindTo((ZString)((TrackingCusISFHeader)null).FirstUSTransport.JW_RL_NKDiscPort);
			AddToDictionary(new ZTextEditColumn(Res.GetString("763ddb00-bd22-481b-9932-3b09042284c3", "1st US Route Discharge Port"), "FirstUSTransport+JW_RL_NKDiscPort") { ColumnKey = WebTracker.Grids.TrackingImporterSecurityFilings.FirstUSRouteDischargePort });
			ZBindToChecker.CheckBindTo((ZDateTime)((TrackingCusISFHeader)null).FirstUSTransport.JW_ETD);
			AddToDictionary(new ZDateTimeColumn(Res.GetString("fddb5acb-3c29-4ea0-b74a-10e44dc46c8e", "1st US Route ETD"), "FirstUSTransport+JW_ETD", ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.TrackingImporterSecurityFilings.FirstUSRouteETD });
			ZBindToChecker.CheckBindTo((ZDateTime)((TrackingCusISFHeader)null).FirstUSTransport.JW_ETA);
			AddToDictionary(new ZDateTimeColumn(Res.GetString("61d0bec3-8e87-47c3-aa89-baae9e610e54", "1st US Route ETA"), "FirstUSTransport+JW_ETA", ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.TrackingImporterSecurityFilings.FirstUSRouteETA });
			ZBindToChecker.CheckBindTo((ZDateTime)((TrackingCusISFHeader)null).FirstUSTransport.JW_ATD);
			AddToDictionary(new ZDateTimeColumn(Res.GetString("ef1ef623-a16c-47c2-9dc7-6bceb1094124", "1st US Route ATD"), "FirstUSTransport+JW_ATD", ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.TrackingImporterSecurityFilings.FirstUSRouteATD });
			ZBindToChecker.CheckBindTo((ZDateTime)((TrackingCusISFHeader)null).FirstUSTransport.JW_ATA);
			AddToDictionary(new ZDateTimeColumn(Res.GetString("9a3a97fa-3bc2-4c3d-b81b-2bab0b88267b", "1st US Route ATA"), "FirstUSTransport+JW_ATA", ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.TrackingImporterSecurityFilings.FirstUSRouteATA });

			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("a061f305-7739-4fca-bac6-82bcb4364133", "Action Reason Code"), TrackingCusISFHeader.Schema.ActionReasonCodeDescription) { ColumnKey = WebTracker.Grids.TrackingImporterSecurityFilings.ActionReasonCode });

			ZBindToChecker.CheckBindTo((ZDateTime)((TrackingCusISFHeader)null).BF_FirstAcceptedDate);
			AddToDictionary(new ZDateTimeColumn(Res.GetString("a68df440-f13b-45a7-bcda-10d5bbd5c7cf", "First Accepted Date"), TrackingCusISFHeader.Schema.BF_FirstAcceptedDate, ZDateTimePickerFormat.Short) { ColumnKey = WebTracker.Grids.TrackingImporterSecurityFilings.FirstAcceptedDate });
			ZBindToChecker.CheckBindTo((ZDateTime)((TrackingCusISFHeader)null).BF_LastAcceptedDate);
			AddToDictionary(new ZDateTimeColumn(Res.GetString("02552c8f-d149-4976-a49a-5aa8c723e165", "Last Accepted Date"), TrackingCusISFHeader.Schema.BF_LastAcceptedDate, ZDateTimePickerFormat.Short) { ColumnKey = WebTracker.Grids.TrackingImporterSecurityFilings.LastAcceptedDate });
			ZBindToChecker.CheckBindTo((ZDateTime)((TrackingCusISFHeader)null).BF_SystemCreateTimeUtc);
			AddToDictionary(new ZDateTimeColumn(Res.GetString("75a461da-2c6c-4ba7-8956-9618f359f5e6", "Created Time"), TrackingCusISFHeader.Schema.BF_SystemCreateTimeUtc, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.TrackingImporterSecurityFilings.CreatedTime });
		}

		protected override List<int> GetOldColumnsOrder()
		{
			List<int> result = new List<int>();
			result.Add((int)WebTracker.Grids.TrackingImporterSecurityFilings.JobReference);
			result.Add((int)WebTracker.Grids.TrackingImporterSecurityFilings.CustomsReference);
			result.Add((int)WebTracker.Grids.TrackingImporterSecurityFilings.Importer);
			result.Add((int)WebTracker.Grids.TrackingImporterSecurityFilings.HouseBill);
			result.Add((int)WebTracker.Grids.TrackingImporterSecurityFilings.Status);
			result.Add((int)WebTracker.Grids.TrackingImporterSecurityFilings.SellingParty);
			result.Add((int)WebTracker.Grids.TrackingImporterSecurityFilings.BuyingParty);
			result.Add((int)WebTracker.Grids.TrackingImporterSecurityFilings.MainShipToParty);
			result.Add((int)WebTracker.Grids.TrackingImporterSecurityFilings.CarrierSCAC);
			result.Add((int)WebTracker.Grids.TrackingImporterSecurityFilings.IDType);
			result.Add((int)WebTracker.Grids.TrackingImporterSecurityFilings.ImporterIDType);
			result.Add((int)WebTracker.Grids.TrackingImporterSecurityFilings.ImporterIdentification);
			result.Add((int)WebTracker.Grids.TrackingImporterSecurityFilings.DateOfBirth);
			result.Add((int)WebTracker.Grids.TrackingImporterSecurityFilings.IssueCountry);
			result.Add((int)WebTracker.Grids.TrackingImporterSecurityFilings.UnloadPort);
			result.Add((int)WebTracker.Grids.TrackingImporterSecurityFilings.DeliveryPort);
			result.Add((int)WebTracker.Grids.TrackingImporterSecurityFilings.BondHolder);
			result.Add((int)WebTracker.Grids.TrackingImporterSecurityFilings.SuretyCode);
			result.Add((int)WebTracker.Grids.TrackingImporterSecurityFilings.ConsigneeIDType);
			result.Add((int)WebTracker.Grids.TrackingImporterSecurityFilings.ConsigneeID);
			result.Add((int)WebTracker.Grids.TrackingImporterSecurityFilings.OceanBill);
			result.Add((int)WebTracker.Grids.TrackingImporterSecurityFilings.MasterBill);
			result.Add((int)WebTracker.Grids.TrackingImporterSecurityFilings.FirstUSRouteVessel);
			result.Add((int)WebTracker.Grids.TrackingImporterSecurityFilings.FirstUSRouteVoyage);
			result.Add((int)WebTracker.Grids.TrackingImporterSecurityFilings.FirstUSRouteLoadPort);
			result.Add((int)WebTracker.Grids.TrackingImporterSecurityFilings.FirstUSRouteDischargePort);
			result.Add((int)WebTracker.Grids.TrackingImporterSecurityFilings.FirstUSRouteETD);
			result.Add((int)WebTracker.Grids.TrackingImporterSecurityFilings.FirstUSRouteETA);
			result.Add((int)WebTracker.Grids.TrackingImporterSecurityFilings.FirstUSRouteATD);
			result.Add((int)WebTracker.Grids.TrackingImporterSecurityFilings.FirstUSRouteATA);
			result.Add((int)WebTracker.Grids.TrackingImporterSecurityFilings.ActionReasonCode);
			result.Add((int)WebTracker.Grids.TrackingImporterSecurityFilings.FirstAcceptedDate);
			result.Add((int)WebTracker.Grids.TrackingImporterSecurityFilings.LastAcceptedDate);
			result.Add((int)WebTracker.Grids.TrackingImporterSecurityFilings.CreatedTime);
			return result;
		}
	}
}
