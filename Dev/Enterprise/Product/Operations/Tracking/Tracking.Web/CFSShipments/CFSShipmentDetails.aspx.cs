using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CargoWise.EntityFramework;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Registry.Business;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
	/// <summary>
	/// Summary description for CFSShipmentDetails.
	/// </summary>
	public partial class CFSShipmentDetails : BasePageWithAuthorisation
	{
		#region DataSource

		protected override BusinessObject GetNewDataSource()
		{
			return TrackingCFSShipment.FromPKFilteredBySiteUser(Factory, GetGuidFromParameter("Ref"), SiteUser);
		}

		public TrackingCFSShipment Shipment
		{
			get { return DataSource as TrackingCFSShipment; }
		}

		#endregion

		protected override bool CanAccessAuthorisedContent
		{
			get { return SiteUser.CanViewCFSShipments; }
		}

		protected override bool IsPersistDataSourceBetweenPostbacks
		{
			get { return true; }
		}

		protected override void SetupAuthorisedContent(bool isAuthorised)
		{
			base.SetupAuthorisedContent(isAuthorised);

			AuthorisedContent.Visible = isAuthorised;
			NotFoundError.Visible = !isAuthorised;

			if (!isAuthorised || Shipment == null)
			{
				ShipmentNotFoundLabel.Text = Res.GetString("B88E3EB8-9BBF-40A2-9BFA-66CEC3DD9C2F", "CFS Shipment was not found in the database or you don't have rights to view it.");
			}
			else
			{
				if (Shipment.JS_IsCancelled)
				{
					AuthorisedContent.Controls.AddAt(0, new ZTextLabel
					{
						CssClass = "SectionTitle",
						Text = Res.GetString("ca5505ed-c18b-4586-87fa-6afcf975ee06", "This shipment has been canceled"),
						ForeColor = System.Drawing.Color.Red
					});
				}

				if (SiteUser.IsShipmentQuickViewUser)
				{
					ForAuthentifiedUserOnly.Visible = false;
					ForAuthentifiedUserOnly2.Visible = false;
					ForAuthentifiedUserOnly3.Visible = false;
					ForAuthentifiedUserOnly4.Visible = false;

					AdditionalTermsRow.Visible = false;
					PayTermsRow.Visible = false;

					TransportGrid.ShouldShowControl = false;
					DeliveryInformationGrid.ShouldShowControl = false;
					PackLinesGrid.ShouldShowControl = false;

					ClientRefLabel.Visible = false;
					ClientReftextlabel.Visible = false;

					InterimReceiptLabel.Visible = false;
					InterimReceipttextlabel.Visible = false;

					EntryNoLabel.Visible = false;
					EntryNotextlabel.Visible = false;

					WhsLocationLabel.Visible = false;
					WhsLocationZcodefindboxlabel.Visible = false;

					MasterBillLabel.Visible = false;
					MasterBilltextLabel.Visible = false;
				}
				else
				{
					PayTermsRow.Visible = true;
					PayTermLabel.Text = Shipment.IsDomesticFreight ? Res.GetString("8eae27fd-5dc9-480f-b168-6a23fab540df", "Payment Term:") : Res.GetString("1366472d-8f32-b392-4457-e8d11fbd0893", "Incoterm:");
					AdditionalTermsRow.Visible = !Shipment.IsDomesticFreight;
				}

				SetupPage();
			}

			NotificationFlags.DisplayAll = false;
		}

		#region Setting up grids

		void SetupPage()
		{
			CodeDescriptionBoolCollection elements = (CodeDescriptionBoolCollection)WebDataRegistry.Instance.CFSShipmentPageCustomisation.Value;
			ETARow.Visible = !elements.GetBoolFromCode(WebDataRegistry.CFSShipmentDetailsPageElements.EtdAndEta);
			ETDRow.Visible = !elements.GetBoolFromCode(WebDataRegistry.CFSShipmentDetailsPageElements.EtdAndEta);
			StorageCommencesRow.Visible = !elements.GetBoolFromCode(WebDataRegistry.CFSShipmentDetailsPageElements.StorageCommencesDate);
			StorageCommencesParallelRow.Visible = StorageCommencesRow.Visible;
			CartageAdvisedRow.Visible = !elements.GetBoolFromCode(WebDataRegistry.CFSShipmentDetailsPageElements.CartageAdvisedDate);
			PickupCartageAdvisedRow.Visible = !elements.GetBoolFromCode(WebDataRegistry.CFSShipmentDetailsPageElements.CartageAdvisedDate);
			TransportGrid.Visible = !elements.GetBoolFromCode(WebDataRegistry.CFSShipmentDetailsPageElements.TransportGrid);
			DeliveryInformationGrid.Visible = !elements.GetBoolFromCode(WebDataRegistry.CFSShipmentDetailsPageElements.DeliveryGrid);
			PackLinesGrid.Visible = !elements.GetBoolFromCode(WebDataRegistry.CFSShipmentDetailsPageElements.GoodsPacksGrid);
			DocumentsGrid.Visible = SiteUser.CanViewDocuments;
		}

		protected override void SetupGrids()
		{
			TransportGrid.ColumnProvider = new TrackingTransportDetailsGridColumnProvider();
			DeliveryInformationGrid.ColumnProvider = new CommonPickupDeliveryConfirmDetailsGridColumnProvider();
		}

		protected override string GetPageName()
		{
			return WebTracker.Pages.CFSShipmentDetails;
		}

		#region Application path and formatting of Urls

		[SuppressMessage("Microsoft.Design", "CA1055:UriReturnValuesShouldNotBeStrings")]
		protected string UrlFormatWithAppRoot(string page)
		{
			return AppPath + page;
		}

		protected string AppPath
		{
			get
			{
#if DEBUG
				if (Enterprise.ZArchitecture.Environment.Globals.IsTest)
				{
					return "/";
				}
				else
#endif
				{
					string result = System.Web.HttpContext.Current.Request.ApplicationPath;
					if (result != "/")
					{
						result += "/";
					}

					return result;
				}
			}
		}

		#endregion

		protected override void OnPreBind()
		{
			base.OnPreBind();
			if (Shipment == null)
			{
				ShipmentNotFoundLabel.Text = Res.GetString("a9d64813-cdbc-44f6-9bf2-53f25ea9a477", "Shipment was not found in the database or you don't have rights to view it.");
			}
			else
			{
				OriginRow.Visible = !Shipment.IsDomesticFreight;
				DestinationRow.Visible = !Shipment.IsDomesticFreight;
			}
			//these grids require DataSource to be loaded before grid's construction.
			//Data source have to be loaded after ViewState's construction
			SetupDocumentsGrid(DocumentsGrid);
			SetupPackLinesGrid(PackLinesGrid, false);
		}

		public new TrackingSiteUser SiteUser
		{
			get { return WebEnv.AppInstance.SiteUser as TrackingSiteUser; }
		}

		#endregion

		protected internal override IReadOnlyCollection<ILicenceCheckpoint> LicenceCheckPoints
		{
			get
			{
				List<ILicenceCheckpoint> result = new List<ILicenceCheckpoint>();
				result.Add(Environment.Env.Licence.WebTrackerForwarding);
				return result.ToArray();
			}
		}

		protected override string GetPageRelativePath()
		{
			return TrackingConstants.RelativePath.CFSShipmentDetailsPage;
		}

		#region Autogenerated

		#region Web Form Designer generated code
		override protected void OnInit(EventArgs e)
		{
			//
			// CODEGEN: This call is required by the ASP.NET Web Form Designer.
			//
			InitializeComponent();
			base.OnInit(e);
		}

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			ShipmentDetailsLabel.BindTo = null;
			Ztextlabel5.BindTo = "JS_UniqueConsignRef";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((string)(((TrackingCFSShipment)(null)).JS_UniqueConsignRef)));
			HouseBill.BindTo = "JS_HouseBill";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((string)(((TrackingCFSShipment)(null)).JS_HouseBill)));
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((string)(((TrackingCFSShipment)(null)).JS_UniqueConsignRef)));
			ClientShipperRefData.BindTo = "JS_BookingReference";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((string)(((TrackingCFSShipment)(null)).JS_BookingReference)));
			GoodsDescription.BindTo = "JS_GoodsDescription";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((string)(((TrackingCFSShipment)(null)).JS_GoodsDescription)));
			ServiceLevelDescription.BindTo = "JS_RS_NKServiceLevel";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((TrackingCFSShipment)(null)).JS_RS_NKServiceLevel)));
			PlannedVolumeLabel.BindTo = "VolumeWithUnits";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((string)(((TrackingCFSShipment)(null)).VolumeWithUnits)));
			PlannedWeightLabel.BindTo = "WeightWithUnits";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((string)(((TrackingCFSShipment)(null)).WeightWithUnits)));
			OuterPackCount.BindTo = "JS_OuterPacks";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.INumericZType)(((TrackingCFSShipment)(null)).JS_OuterPacks)));
			PackType.BindTo = "JS_F3_NKPackType";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((string)(((TrackingCFSShipment)(null)).JS_F3_NKPackType)));
			Origin.BindTo = "JS_RL_NKOrigin";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((TrackingCFSShipment)(null)).JS_RL_NKOrigin)));
			Destination.BindTo = "JS_RL_NKDestination";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((TrackingCFSShipment)(null)).JS_RL_NKDestination)));
			ETD.BindTo = "ETDWithSuppression";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZDateTime)(((TrackingCFSShipment)(null)).ETDWithSuppression)));
			ETA.BindTo = "ETAWithSuppression";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZDateTime)(((TrackingCFSShipment)(null)).ETAWithSuppression)));
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZDateTime)(((TrackingCFSShipment)(null)).JS_E_DEP)));
			Ztextlabel3.BindTo = "ConsignorPickupAddress.AddressAsASingleLine";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((string)(((TrackingCFSShipment)(null)).ConsignorPickupAddress.AddressAsASingleLine)));
			Ztextlabel4.BindTo = "ConsigneeDeliveryAddress.AddressAsASingleLine";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((string)(((TrackingCFSShipment)(null)).ConsigneeDeliveryAddress.AddressAsASingleLine)));
			Zdatetimelabel4.BindTo = "AvailableDate";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZDateTime)(((TrackingCFSShipment)(null)).AvailableDate)));
			Zdatetimelabel5.BindTo = "StorageDate";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZDateTime)(((TrackingCFSShipment)(null)).StorageDate)));
			Zdatetimelabel1.BindTo = "DocsAndCartage.JP_EstimatedPickup";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZDateTime)(((TrackingCFSShipment)(null)).DocsAndCartage.JP_EstimatedPickup)));
			Zdatetimelabel6.BindTo = "DocsAndCartage.JP_EstimatedDelivery";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZDateTime)(((TrackingCFSShipment)(null)).DocsAndCartage.JP_EstimatedDelivery)));
			Zdatetimelabel2.BindTo = "DocsAndCartage.JP_PickupRequiredBy";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZDateTime)(((TrackingCFSShipment)(null)).DocsAndCartage.JP_PickupRequiredBy)));
			Zdatetimelabel7.BindTo = "DocsAndCartage.JP_DeliveryRequiredBy";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZDateTime)(((TrackingCFSShipment)(null)).DocsAndCartage.JP_DeliveryRequiredBy)));
			Zdatetimelabel9.BindTo = "DocsAndCartage.JP_PickupCartageAdvised";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZDateTime)(((TrackingCFSShipment)(null)).DocsAndCartage.JP_PickupCartageAdvised)));
			Zdatetimelabel8.BindTo = "DocsAndCartage.JP_DeliveryCartageAdvised";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZDateTime)(((TrackingCFSShipment)(null)).DocsAndCartage.JP_DeliveryCartageAdvised)));
			Zdatetimelabel3.BindTo = "DocsAndCartage.JP_PickupCartageCompleted";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZDateTime)(((TrackingCFSShipment)(null)).DocsAndCartage.JP_PickupCartageCompleted)));
			ReceivedDate.BindTo = "DocsAndCartage.JP_DeliveryCartageCompleted";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZDateTime)(((TrackingCFSShipment)(null)).DocsAndCartage.JP_DeliveryCartageCompleted)));
			ShipmentNotFoundLabel.BindTo = null;
			this.DataSourceAssemblyName = "Enterprise.Tracking.Business";
			this.DataSourceTypeName = "Enterprise.Tracking.Business.CFSShipment";
		}
		#endregion

		#endregion
	}
}
