using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web;
using System.Web.UI;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Registry.Business.Web;
using Enterprise.Tracking.Business;
using Enterprise.Tracking.Business.ImporterSecurityFiling;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web.ImporterSecurityFiling
{
	public partial class ImporterSecurityFilingDetails : BasePageWithAuthorisation, IAccessControllablePage
	{
		#region DataSource

		protected override bool IsPersistDataSourceBetweenPostbacks
		{
			get { return true; }
		}

		protected override BusinessObject GetNewDataSource()
		{
			TrackingCusISFHeader result = TrackingCusISFHeader.GetFromISFHeaderPK(Factory, GetGuidFromParameter("Ref"), SiteUser);
			if (result != null)
			{
				((IAccessControlled)result).IsInTextSuppressionMode = false;
				return result;
			}
			else
			{
				return null;
			}
		}

		protected TrackingCusISFHeader ISFHeader
		{
			get { return DataSource != null ? ((TrackingCusISFHeader)DataSource) : null; }
		}

		#endregion

		#region Visible
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			SetUpPage();
		}

		protected void DuplicateISF_Click(object sender, EventArgs e)
		{
			if (ISFHeader != null)
			{
				TrackingCusISFHeader newISFHeader = ISFHeader.Duplicate();
				if (newISFHeader != null)
				{
					((IAccessControlled)newISFHeader).IsInTextSuppressionMode = false;
					SaveDataSourceAndAddEmailNotifier(newISFHeader.PK, newISFHeader);
					Response.Redirect(string.Format((NoResString)"{0}?Ref={1}&{2}={3}", AppInstance.EditISFPage, newISFHeader.PK, ZPage.DataSourceInSessionParameterName, ZPage.DataSourceInSessionParameterValue)); // Redirection path
				}
			}
		}

		protected void SetUpPage()
		{
			if (ISFHeader != null)
			{
				bool isISF10 = ISFHeader.IsISF10Entry;

				ISF10Details.Visible = isISF10;
				ISF5Details.Visible = !isISF10;
				RoutingDetails.Visible = ISFHeader.HasTransportsInfoForWeb;

				ShipToGovRegNoArea.Visible = !ISFHeader.MainShipToParty.E2_GovRegNum.IsEmpty;
				SellingPartyGovRegNoArea.Visible = !ISFHeader.SellingParty.E2_GovRegNum.IsEmpty;
				BuyingPartyGovRegNoArea.Visible = !ISFHeader.BuyingParty.E2_GovRegNum.IsEmpty;
				StuffingLocationGovRegNoArea.Visible = !ISFHeader.StuffingLocation.E2_GovRegNum.IsEmpty;
				ConsolidatorGovRegNoArea.Visible = !ISFHeader.Consolidator.E2_GovRegNum.IsEmpty;
				BookingPartyGovRegNoArea.Visible = !ISFHeader.BookingParty.E2_GovRegNum.IsEmpty;

				SellingPartyAddressHolder.Visible = isISF10;
				BuyingPartyAddressHolder.Visible = isISF10;
				StuffingLocationHolder.Visible = isISF10;
				ConsolidatorAddressHolder.Visible = isISF10;
				BookingPartyAddressHolder.Visible = !isISF10;
				SetDeleteISFEnabledProperty();
			}

			DeleteISF.Visible = SiteUser != null && SiteUser.IsLoggedIn && SiteUser.CanDeleteISF;
			DocumentsGrid.Visible = SiteUser.CanViewDocuments;
		}

		void SetDeleteISFEnabledProperty()
		{
			DeleteISF.Enabled = ISFHeader.CanSendDelete && Env.Security.ImporterSecurityFilingMessaging.IsAllowed && !ISFHeader.IsWaitingForResponse;
		}

		#endregion

		#region Buttons

		protected void EditISF_Click(object sender, EventArgs e)
		{
			Response.Redirect(string.Format((NoResString)"{0}?Ref={1}", AppInstance.EditISFPage, ISFHeader.ISFHeaderPK)); // URL parameter
		}

		protected void DeleteISF_Click(object sender, EventArgs e)
		{
			var messageSender = ObjectFactory.Get<Integration.Customs.US.ISF.IUSISFWebMessageSender>();
			var errorMessage = messageSender.SendDeleteMessage(ISFHeader.PK);

			if (!string.IsNullOrEmpty(errorMessage))
			{
				Session[zPageCustomAlertMessageIndexer] = errorMessage;
			}

			HttpContext.Current.Response.Redirect(string.Format(CultureInfo.InvariantCulture, (NoResString)"{0}?Ref={1}", AppInstance.ISFDetailsPage, DataSource.PK)); // URL Redirection
		}

		#endregion

		protected override string GetPageName()
		{
			return WebTracker.Pages.ImporterSecurityFilingDetails;
		}

		protected override string GetPageRelativePath()
		{
			return TrackingConstants.RelativePath.ISFDetailsPage;
		}

		protected override bool CanAccessAuthorisedContent
		{
			get { return SiteUser.CanViewISF; }
		}

		#region Grids

		protected override void OnPreBind()
		{
			base.OnPreBind();
			//these grids require DataSource to be loaded before grid's construction.
			//Data source have to be loaded after ViewState's construction
			SetupDocumentsGrid(DocumentsGrid);
			SetupContainersDataGrid(ContainersDataGrid);
			SetupReferenceDataGrid();
			SetupLinesDataGrid();
			SetupAddressesDataGrid(AddressesDataGrid);
			SetupAddressesDataGrid(AdditionalShipToAddressesDataGrid);
		}

		void SetupAddressesDataGrid(ZGrid addressesGrid)
		{
			addressesGrid.ColumnProvider = new ISFAddressColumnProvider();
		}

		void SetupLinesDataGrid()
		{
			LinesDataGrid.ColumnProvider = new ISFLineColumnProvider();
		}

		void SetupReferenceDataGrid()
		{
			ReferenceDataGrid.ColumnProvider = new ISFReferenceColumnProvider();
		}

		protected void SetupContainersDataGrid(ZGrid containersGrid)
		{
			containersGrid.ColumnProvider = new ISFContainerColumnProvider();
		}

		#endregion

		#region IAccessControllablePage Members

		IAccessControlled IAccessControllablePage.dataSource
		{
			get { return ISFHeader; }
		}

		Control[] IAccessControllablePage.GetControls(string caption)
		{
			List<Control> result = new List<Control>();

			switch (caption)
			{
				case ISFAccessRules.Captions.Bond:
					result.Add(BondDetails);
					break;
				case ISFAccessRules.Captions.Buying_Party:
					result.Add(BuyingPartyAddressHolder);
					break;
				case ISFAccessRules.Captions.Consignee_Details:
					result.Add(CneeDetails);
					break;
				case ISFAccessRules.Captions.Consolidator:
					result.Add(ConsolidatorAddressHolder);
					break;
				case ISFAccessRules.Captions.Containers:
					result.Add(ContainersDataGrid);
					break;
				case ISFAccessRules.Captions.Importer_Details:
					result.Add(ImporterDetails);
					break;
				case ISFAccessRules.Captions.ISF_Details:
					result.Add(SharedDetails);
					result.Add(ReferenceDataGrid);
					result.Add(RoutingDetails);
					result.Add(ISF10Details);
					result.Add(CneeDetails);
					result.Add(BondDetails);
					break;
				case ISFAccessRules.Captions.Lines:
					result.Add(LinesDataGrid);
					break;
				case ISFAccessRules.Captions.Manufacturers:
					result.Add(AddressesDataGrid);
					break;
				case ISFAccessRules.Captions.Selling_Party:
					result.Add(SellingPartyAddressHolder);
					break;
				case ISFAccessRules.Captions.Ship_To_Parties:
					result.Add(ShipToPartyAddressHolder);
					result.Add(AdditionalShipToAddressesDataGrid);
					break;
				case ISFAccessRules.Captions.Stuffing_Location:
					result.Add(StuffingLocationHolder);
					break;
				case ISFAccessRules.Captions.Documents:
					result.Add(DocumentsGrid);
					break;
				default:
					throw new NotImplementedException("Registry Access captions don't correspond details page's: " + caption);
			}

			return result.ToArray();
		}

		#endregion
	}
}
