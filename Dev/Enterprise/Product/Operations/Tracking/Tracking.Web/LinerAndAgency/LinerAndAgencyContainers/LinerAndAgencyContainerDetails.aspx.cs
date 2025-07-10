using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CargoWise.EntityFramework;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Tracking.Business;

namespace Enterprise.Tracking.Web.LinerAndAgency
{
	/// <summary>
	/// Summary description for ShipmentDetails.
	/// </summary>
	public partial class LinerAndAgencyContainerDetails : BasePageWithAuthorisation
	{
		#region DataSource

		protected override bool IsPersistDataSourceBetweenPostbacks
		{
			get
			{
				return true;
			}
		}

		protected override BusinessObject GetNewDataSource()
		{
			return LinerAndAgencyContainer.FromPKFilteredBySiteUser(Factory, GetGuidFromParameter(RefParameterName), SiteUser);
		}

		protected LinerAndAgencyContainer CurrentContainer
		{
			get { return DataSource as LinerAndAgencyContainer; }
		}

		#endregion

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			bool isDataSourceValid = (CurrentContainer != null);

			AuthorisedContent.Visible = isDataSourceValid && CanAccessAuthorisedContent;
			NotFoundError.Visible = !isDataSourceValid;
			DocumentsGrid.Visible = SiteUser.CanViewDocuments;

			if (!isDataSourceValid)
			{
				NotFoundLabel.Text = Res.GetString("72865425-2BA8-4370-9C8D-499340644381", "Container was not found in the database or Container does not have any Pack Line or you don't have rights to view it.");
			}

			if (CurrentContainer != null && CurrentContainer.JC_Purpose == "REL")
			{
				DynamicCaption.Text = Res.GetString("77A9A442-5D54-4F69-94DD-7EA6063FB4F7", "Bill Of Lading Number");
			}
			else if (CurrentContainer != null && CurrentContainer.JC_Purpose == "BKD")
			{
				DynamicCaption.Text = Res.GetString("946A0982-6C3A-4B2B-86E9-8FBFE9B38A02", "Booking Ref");
			}

			if (SiteUser.IsShipmentQuickViewUser)
			{
				DynamicCaption.Visible = false;
				DynamicLabel.Visible = false;

				JobNumberCaption.Visible = false;
				JobNumberLabel.Visible = false;

				ContainerStatusCaption.Visible = false;
				ContainerStatusLabel.Visible = false;

				GrossWeighCaption.Visible = false;
				GrossWeightLabel.Visible = false;

				PickupCaption.Visible = false;
				PickupLabel.Visible = false;

				DeliverCaption.Visible = false;
				DeliverLabel.Visible = false;

				CommodityCaption.Visible = false;
				CommodityLabel.Visible = false;

				GoodValueCaption.Visible = false;
				GoodValueLabel.Visible = false;

				CurrencyCaption.Visible = false;
				CurrencyLabel.Visible = false;

				NetWeightCaption.Visible = false;
				NetWeightLabel.Visible = false;

				TareWeightCaption.Visible = false;
				TareWeightLabel.Visible = false;

				IsShipperCheckBox.Visible = false;
				ISEmptyCheckBox.Visible = false;
				IsDamagedCheckBox.Visible = false;

				PaymentTermCaption.Visible = false;
				PaymentTermLabel.Visible = false;

				ServiceLevelCaption.Visible = false;
				ServiceLevelLabel.Visible = false;

				ShipperRefCaption.Visible = false;
				ShipperRefLabel.Visible = false;

				OrderRefCaption.Visible = false;
				OrderRefLabel.Visible = false;

				GoodsDescriptionCaption.Visible = false;
				GoodsDescriptionLabel.Visible = false;

				RefrigrationCaption.Visible = false;

				IsControlledAtmosphereCheckBox.Visible = false;
				IsChillerCheckBox.Visible = false;
				IsFreezerCheckBox.Visible = false;

				TemperatureCaption.Visible = false;
				TemperatureLabel.Visible = false;

				HumidityCaption.Visible = false;
				HumidityLabel.Visible = false;

				TempRecordCaption.Visible = false;
				TempRecordLabel.Visible = false;

				AirVentCaption.Visible = false;
				AirVentLabel.Visible = false;

				ClipOnUnitNumberCaption.Visible = false;
				ClipOnUnitNumberLabel.Visible = false;

				EmptyPickupFromCaption.Visible = false;
				EmptyPickupFromLabel.Visible = false;

				EmptyReleaseNumberCaption.Visible = false;
				EmptyReleaseNumberLabel.Visible = false;

				EmptyReleaseFromCaption.Visible = false;
				EmptyReleaseFromLabel.Visible = false;

				EmptyReturnedToCaption.Visible = false;
				EmptyReturnedToLabel.Visible = false;

				VerifiedByCompanyCaption.Visible = false;
				VerifiedByCompanyLabel.Visible = false;

				VerifiedByPersonCaption.Visible = false;
				VerifiedByPersonLabel.Visible = false;

				VerifiedByPhoneCaption.Visible = false;
				VerifiedByPhoneLabel.Visible = false;

				VerifiedByEmailCaption.Visible = false;
				VerifiedByEmailLabel.Visible = false;

				DocumentsGrid.Visible = false;

				EditContainer.Visible = false;
			}
		}

		#region Page Setup

		protected override bool CanAccessAuthorisedContent
		{
			get { return SiteUser != null && SiteUser.CanViewLinerAndAgencyContainers; }
		}

		protected override void SetupAuthorisedContent(bool isAuthorised)
		{
			base.SetupAuthorisedContent(isAuthorised);
			EditButtonDiv.Visible = SiteUser != null && SiteUser.CanViewLinerAndAgencyContainers;
		}

		#endregion

		#region Setting up grids

		protected override string GetPageName()
		{
			return WebTracker.Pages.LinerAndAgencyContainerDetails;
		}

		protected override void OnPreBind()
		{
			base.OnPreBind();

			//these grids require DataSource to be loaded before grid's construction.
			//Data source have to be loaded after ViewState's construction

			SetupDocumentsGrid(DocumentsGrid);
		}

		#endregion

		#region Event Handlers

		[SuppressMessage("Microsoft.Globalization", "CA1305:Redirect URL.")]
		protected void EditContainer_Click(object sender, EventArgs e)
		{
			Response.Redirect(string.Format("{0}?{1}={2}", AppInstance.EditLinerAndAgencyContainerPage, RefParameterName, CurrentContainer.PK));
		}

		#endregion

		protected override string GetPageRelativePath()
		{
			return TrackingConstants.RelativePath.LinerAndAgencyContainerDetailsPage;
		}

		protected internal override IReadOnlyCollection<ILicenceCheckpoint> LicenceCheckPoints
		{
			get
			{
				var result = new List<ILicenceCheckpoint>();
				result.Add(Environment.Env.Licence.WebTrackerForwarding);
				return result.ToArray();
			}
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
			this.DataSourceAssemblyName = "Enterprise.Tracking.Business";
			this.DataSourceTypeName = "Enterprise.Tracking.Business.LinerAndAgencyContainer";
		}

		#endregion

		#endregion
	}
}
