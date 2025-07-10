using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Registry.Business;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.Modules;

namespace Enterprise.Tracking.Web.GridColumn.Providers.LinerAndAgency
{
	public class CustomizedLinerAndAgencyContainerColumnProvider : GridColumnProvider
	{
		#region Constructor

		public CustomizedLinerAndAgencyContainerColumnProvider()
		{
		}

		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0004:Remove Unnecessary Cast", Justification = "ZBindToChecker requires a redundant cast")]
		protected override void CustomizeDictionaryCore()
		{
			base.CustomizeDictionaryCore();

			AddContainerNumberColumn();

			ZBindToChecker.CheckBindTo(((RefContainerCollection)(((LinerAndAgencyContainer)(null)).RefContainer_List)));
			ZBindToChecker.CheckBindTo(((ZGuid)(((LinerAndAgencyContainer)(null)).JC_RC)));
			AddToDictionaryAsDefault(new ZGuidDropDownListColumn(Res.GetString("a6a6e8d0-be4e-4a75-9d33-a86f1deac587", "Type"), LinerAndAgencyContainer.Schema.JC_RC)
			{
				ValueFieldName = "PK",
				TextFieldName = RefContainer.Schema.RC_Code,
				DisplayStyle = OComboBoxDropDownStyle.CodeOnly,
				AutoPostBack = true,
				ColumnKey = WebTracker.Grids.TrackingContainers.Type
			});

			ZBindToChecker.CheckBindTo(((ZDecimal)(((LinerAndAgencyContainer)(null)).JC_ContainerCount)));
			AddToDictionaryAsDefault(new ZCalcEditColumn(Res.GetString("3914c902-976b-41e5-97c7-9fd00c65bbd2", "Count"), LinerAndAgencyContainer.Schema.JC_ContainerCount) { Decimals = 0, ColumnKey = WebTracker.Grids.LinerAndAgencyContainers.ContainersCount });

			ZBindToChecker.CheckBindTo(((ZDecimal)(((LinerAndAgencyContainer)(null)).JC_Calc_NetWeight)));
			AddToDictionaryAsDefault(new ZCalcEditColumn(Res.GetString("7bd957f9-66d8-4f56-9af9-f13e2af3a8cf", "Net Wt."), LinerAndAgencyContainer.Schema.JC_Calc_NetWeight) { ColumnKey = WebTracker.Grids.LinerAndAgencyContainers.NetWeight });

			ZBindToChecker.CheckBindTo(((ZDecimal)(((LinerAndAgencyContainer)(null)).JC_TareWeight)));
			AddToDictionaryAsDefault(new ZCalcEditColumn(Res.GetString("25d66603-279b-4645-b7de-df0a33918ec9", "Tare Wt."), LinerAndAgencyContainer.Schema.JC_TareWeight) { ReadOnly = true, ColumnKey = WebTracker.Grids.LinerAndAgencyContainers.TareWeight });

			ZBindToChecker.CheckBindTo(((ZDecimal)(((LinerAndAgencyContainer)(null)).JC_GrossWeight)));
			AddToDictionaryAsDefault(new ZCalcEditColumn(Res.GetString("1fc8199e-a8f9-488d-ae69-1070597f821f", "Gross Wt."), LinerAndAgencyContainer.Schema.JC_GrossWeight) { ReadOnly = true, ColumnKey = WebTracker.Grids.LinerAndAgencyContainers.GrossWeight });

			ZBindToChecker.CheckBindTo(((ZString)(((LinerAndAgencyContainer)(null)).JC_GrossWeightUQ)));
			AddToDictionaryAsDefault(new ZDropDownListColumn(Res.GetString("1db1110a-575c-41a2-950c-d28adfbd6bb0", "WQ"), LinerAndAgencyContainer.Schema.JC_GrossWeightUQ) { DisplayStyle = OComboBoxDropDownStyle.CodeOnly, ColumnKey = WebTracker.Grids.LinerAndAgencyContainers.WQ });

			ZBindToChecker.CheckBindTo(((ZString)(((LinerAndAgencyContainer)(null)).JC_RH_NKContainerCommodityCode)));
			AddToDictionaryAsDefault(new ZCodeFindBoxColumn(Res.GetString("4dca421c-8d87-4591-9b78-77d36b373109", "Commodity"), LinerAndAgencyContainer.Schema.JC_RH_NKContainerCommodityCode) { ModuleID = WebModuleIDs.RefCommodityCode, ColumnKey = WebTracker.Grids.LinerAndAgencyContainers.Commodity });

			ZBindToChecker.CheckBindTo(((ZBool)(((LinerAndAgencyContainer)(null)).JC_IsShipperOwned)));
			AddToDictionaryAsDefault(new ZCheckBoxColumn(Res.GetString("73917685-eda0-4459-b66f-157b2123007d", "Is Shipper Owned"), LinerAndAgencyContainer.Schema.JC_IsShipperOwned) { ColumnKey = WebTracker.Grids.LinerAndAgencyContainers.IsShipperOwned });

			ZBindToChecker.CheckBindTo(((ZString)(((LinerAndAgencyContainer)(null)).JC_SealNum)));
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("9ffa81b9-600d-43d8-b8bd-74368dcae1dd", "Seal #"), LinerAndAgencyContainer.Schema.JC_SealNum) { ColumnKey = WebTracker.Grids.LinerAndAgencyContainers.SealNumber });

			ZBindToChecker.CheckBindTo((ZDateTime)((LinerAndAgencyContainer)null).JC_GrossWeightVerificationDateTime);
			AddToDictionaryAsDefault(new ZDateTimeColumn(Res.GetString("5A00CE29-590F-4FAB-A6E5-94C1F5520444", "Verified Date"), LinerAndAgencyContainer.Schema.JC_GrossWeightVerificationDateTime, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.LinerAndAgencyContainers.VerifiedDate });

			ZBindToChecker.CheckBindTo((ZString)((LinerAndAgencyContainer)null).VerifiedMethod);
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("FA2831A3-3B6C-4DB2-97AA-BFD60FC4995A", "Verified Method"), LinerAndAgencyContainer.Schema.VerifiedMethod) { ColumnKey = WebTracker.Grids.LinerAndAgencyContainers.VerifiedMethod });

			ZBindToChecker.CheckBindTo((ZString)((LinerAndAgencyContainer)null).VerifiedByCompany);
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("7BDD443F-24EF-44D8-847D-80322A4D9A3B", "Verified Company"), LinerAndAgencyContainer.Schema.VerifiedByCompany) { ColumnKey = WebTracker.Grids.LinerAndAgencyContainers.VerifiedCompany });

			ZBindToChecker.CheckBindTo((ZString)((LinerAndAgencyContainer)null).VerifiedByPerson);
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("7D74D9C3-355C-4B38-88C8-68E402148DA5", "Verified Contact"), LinerAndAgencyContainer.Schema.VerifiedByPerson) { ColumnKey = WebTracker.Grids.LinerAndAgencyContainers.VerifiedContact });

			ZBindToChecker.CheckBindTo((ZString)((LinerAndAgencyContainer)null).VerifiedByPhone);
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("173D149B-B5A7-465A-BDC5-1644DC92BCCF", "Verified Phone"), LinerAndAgencyContainer.Schema.VerifiedByPhone) { ColumnKey = WebTracker.Grids.LinerAndAgencyContainers.VerifiedPhone });

			ZBindToChecker.CheckBindTo((ZString)((LinerAndAgencyContainer)null).VerifiedByEmail);
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("826FA30A-FB52-4265-85B5-D68EDB105B43", "Verified Email"), LinerAndAgencyContainer.Schema.VerifiedByEmail) { ColumnKey = WebTracker.Grids.LinerAndAgencyContainers.VerifiedEmail });

			ZBindToChecker.CheckBindTo(((ZString)(((LinerAndAgencyContainer)(null)).DepartureContainerYardAddress.AddressDescription)));
			AddToDictionary(new ZTextEditColumn(Res.GetString("ce865398-d553-4747-927d-afacc09ef47e", "Empty Pickup From"), "DepartureContainerYardAddress.AddressDescription") { ColumnKey = WebTracker.Grids.LinerAndAgencyContainers.EmptyPickupFrom });

			ZBindToChecker.CheckBindTo(((ZDateTime)(((LinerAndAgencyContainer)(null)).JC_ContainerYardEmptyPickupGateOut)));
			AddToDictionary(new ZDateTimeColumn(Res.GetString("8ca306e5-811e-4ca5-8648-91f60486ac48", "Empty Released"), LinerAndAgencyContainer.Schema.JC_ContainerYardEmptyPickupGateOut) { ColumnKey = WebTracker.Grids.LinerAndAgencyContainers.EmptyReleased });

			ZBindToChecker.CheckBindTo(((ZDateTime)(((LinerAndAgencyContainer)(null)).JC_FCLWharfGateIn)));
			AddToDictionary(new ZDateTimeColumn(Res.GetString("0c26c32d-6e0b-43c9-b390-67ee2ad1341c", "Wharf Gate In"), LinerAndAgencyContainer.Schema.JC_FCLWharfGateIn) { ColumnKey = WebTracker.Grids.LinerAndAgencyContainers.WharfGateIn });

			ZBindToChecker.CheckBindTo(((ZDateTime)(((LinerAndAgencyContainer)(null)).JC_FCLOnBoardVessel)));
			AddToDictionary(new ZDateTimeColumn(Res.GetString("2b2d6699-7d82-4e65-9778-4944bf1ad0c9", "Loaded"), LinerAndAgencyContainer.Schema.JC_FCLOnBoardVessel) { ColumnKey = WebTracker.Grids.LinerAndAgencyContainers.Loaded });

			ZBindToChecker.CheckBindTo(((ZString)(((LinerAndAgencyContainer)(null)).ArrivalContainerYardAddress.AddressDescription)));
			AddToDictionary(new ZTextEditColumn(Res.GetString("ea8d84dd-0884-4d29-bd67-ce2dba98236f", "Empty Return To"), "ArrivalContainerYardAddress.AddressDescription") { ColumnKey = WebTracker.Grids.LinerAndAgencyContainers.EmptyReturnTo });

			ZBindToChecker.CheckBindTo(((ZDateTime)(((LinerAndAgencyContainer)(null)).JC_EmptyReturnedBy)));
			AddToDictionary(new ZDateTimeColumn(Res.GetString("6700025e-91b6-4396-80e8-659389cb4fdb", "Empty Return By"), LinerAndAgencyContainer.Schema.JC_EmptyReturnedBy) { ColumnKey = WebTracker.Grids.LinerAndAgencyContainers.EmptyReturnBy });

			ZBindToChecker.CheckBindTo(((ZDateTime)(((LinerAndAgencyContainer)(null)).JC_FCLUnloadFromVessel)));
			AddToDictionary(new ZDateTimeColumn(Res.GetString("c2d03795-aa0f-4e0b-b3ee-9fffff62eaa1", "Unloaded"), LinerAndAgencyContainer.Schema.JC_FCLUnloadFromVessel) { ColumnKey = WebTracker.Grids.LinerAndAgencyContainers.Unloaded });

			ZBindToChecker.CheckBindTo(((ZDateTime)(((LinerAndAgencyContainer)(null)).JC_FCLWharfGateOut)));
			AddToDictionary(new ZDateTimeColumn(Res.GetString("e9cd00f5-6e4f-47e1-9930-6df55f5e6607", "Wharf Gate Out"), LinerAndAgencyContainer.Schema.JC_FCLWharfGateOut) { ColumnKey = WebTracker.Grids.LinerAndAgencyContainers.WharfGateOut });

			ZBindToChecker.CheckBindTo(((ZDateTime)(((LinerAndAgencyContainer)(null)).JC_ContainerYardEmptyReturnGateIn)));
			AddToDictionary(new ZDateTimeColumn(Res.GetString("af4b3e73-ee9e-4404-abe7-fcd77d9cd647", "Empty Returned"), LinerAndAgencyContainer.Schema.JC_ContainerYardEmptyReturnGateIn) { ColumnKey = WebTracker.Grids.LinerAndAgencyContainers.EmptyReturned });
		}

		void AddContainerNumberColumn()
		{
#pragma warning disable IDE0004 // Remove Unnecessary Cast Justification = "ZBindToChecker requires a redundant cast"
			ZBindToChecker.CheckBindTo((ZString)((LinerAndAgencyContainer)null).JC_ContainerNum);
#pragma warning restore IDE0004 // Remove Unnecessary Cast

			var siteUser = WebEnv.AppInstance?.SiteUser as TrackingSiteUser;
			var isShipmentQuickViewUser = siteUser?.IsShipmentQuickViewUser ?? false;
			var headerText = Res.GetString("64dfaffe-d618-4acf-a738-219424cc3f2e", "Container #");
			var columnKey = WebTracker.Grids.LinerAndAgencyContainers.ContainerNumber;
			var bindTo = LinerAndAgencyContainer.Schema.JC_ContainerNum;

			if (isShipmentQuickViewUser && !WebDataRegistry.Instance.WebTrackerContainerQuickView.Value)
			{
				AddToDictionaryAsDefault(new ZTextEditColumn(headerText, bindTo) { ColumnKey = columnKey });
			}
			else
			{
				var containerNumberColumn = new ZHyperLinkColumn(headerText, bindTo) { ColumnKey = columnKey };
				if (isShipmentQuickViewUser)
				{
					containerNumberColumn.DataNavigateUrlFormatString = this.UrlFormatWithAppRoot(TrackingConstants.RelativePath.LinerAndAgencyContainerDetailsPage) + (NoResString)"?Ref={0}&ContainerQuickViewNumber={1}"; // Part of URL string
					containerNumberColumn.DataNavigateUrlFields = new[] { "PK", bindTo };
				}
				else
				{
					containerNumberColumn.DataNavigateUrlFormatString = this.UrlFormatWithAppRoot(TrackingConstants.RelativePath.LinerAndAgencyContainerDetailsPage) + (NoResString)"?Ref={0}"; // Part of URL string
					containerNumberColumn.DataNavigateUrlFields = new[] { "PK" };
				}

				AddToDictionaryAsRequired(containerNumberColumn);
			}
		}
	}
}
