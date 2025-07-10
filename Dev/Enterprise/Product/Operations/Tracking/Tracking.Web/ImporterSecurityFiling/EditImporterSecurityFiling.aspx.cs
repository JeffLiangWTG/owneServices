using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.ISF.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Registry.Business.Web;
using Enterprise.Tracking.Business;
using Enterprise.Tracking.Business.ImporterSecurityFiling;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.Modules;
using Enterprise.ZArchitecture.Web.Utilities.Exceptions;

namespace Enterprise.Tracking.Web.ImporterSecurityFiling
{
	public partial class EditImporterSecurityFiling : BasePageWithAuthorisation, IAccessControllablePage
	{
		#region DataSource

		protected ZGuid ISFHeaderPK
		{
			get { return GetGuidFromParameter(RefParameterName); }
		}

		protected override BusinessObject GetNewDataSource()
		{
			TrackingCusISFHeader result = ISFHeaderPK.IsValid
											? TrackingCusISFHeader.GetFromISFHeaderPK(Factory, ISFHeaderPK, SiteUser)
											: Factory.New<TrackingCusISFHeader>();

			if (result != null)
			{
				((IAccessControlled)result).IsInTextSuppressionMode = false;
			}

			return result;
		}

		protected TrackingCusISFHeader CurrentISF
		{
			get { return DataSource as TrackingCusISFHeader; }
		}

		protected override bool IsPersistDataSourceBetweenPostbacks
		{
			get { return true; }
		}

		#endregion

		#region Save

		[DefaultValue(false)]
		protected bool ShouldSendISF { get; set; }

		protected void SendISF_Click(object sender, EventArgs e)
		{
			ShouldSendISF = NotificationFlags.DisplayMessageErrors = true;
			SaveDataSourceFactory(true);
		}

		protected void SaveISF_Click(object sender, EventArgs e)
		{
			ShouldSendISF = NotificationFlags.DisplayMessageErrors = false;
			SaveDataSourceFactory(true);
		}

		protected override void OnDataSourceFactorySaved()
		{
			base.OnDataSourceFactorySaved();
			string errorMessage = null;
			if (ShouldSendISF)
			{
				if (SiteUser.CanSendISF)
				{
					var messageSender = ObjectFactory.Get<Integration.Customs.US.ISF.IUSISFWebMessageSender>();
					errorMessage = messageSender.SendUpsertMessage(CurrentISF.PK);

					if (string.IsNullOrEmpty(errorMessage))
					{
						ShouldSendISF = false;
					}
				}
				else
				{
					errorMessage = Res.GetString("4f9e237e-c9de-4bf5-9147-66c897e02fe6", "You are not authorized to send ISF.");
				}
			}

			BusinessObjectFactory tempFactory = new BusinessObjectFactory();
			TrackingCusISFHeader tempISF = tempFactory.Load<TrackingCusISFHeader>(DataSource.PK);
			if (tempISF == null)
			{
				try
				{
					((WebExceptionReporter)ExceptionReporter.Instance).ReportDeveloperException((NoResString)"ISF Not Saved", string.Format((NoResString)"ISF database record could not be found. ISF PK = {0} {1} {2}", DataSource.PK, System.Environment.NewLine, ((TrackingCusISFHeader)DataSource).XmlData), new Exception("Developer generated exception")); // Developer only
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
				}
				errorMessage = Res.GetString("b2225092-619a-4ae0-bff0-9058bf4e0515", "There was a problem while saving your changes.  Please try saving again.");
			}

			if (!string.IsNullOrEmpty(errorMessage))
			{
				Session[zPageCustomAlertMessageIndexer] = errorMessage;
			}
			else
			{
				HttpContext.Current.Response.Redirect(string.Format((NoResString)"{0}?Ref={1}", AppInstance.ISFDetailsPage, DataSource.PK)); // URL Redirection
			}
		}

		#endregion

		#region Implementation

		protected override string GetPageName()
		{
			return WebTracker.Pages.EditImporterSecurityFiling;
		}

		protected override string GetPageRelativePath()
		{
			return TrackingConstants.RelativePath.EditISFPage;
		}

		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);
			NotificationFlags.DisplayAll = false;
			NotificationFlags.DisplayErrors = true;
			SetupDocAddressControls();
			ImporterFindBox.ModuleID = WebModuleIDs.OrgConsigneeTracking;
			ImporterIDTypeDropDown.ShowEmptyItem = true;
			ConsigneeIDTypeDropDown.ShowEmptyItem = true;
			CountryOfIssueFindBox.ModuleID = WebModuleIDs.RefCountry;
			ConsigneeCountryOfIssueFindBox.ModuleID = WebModuleIDs.RefCountry;
			AsyncPostBackTrigger trigger = new AsyncPostBackTrigger();
			trigger.ControlID = "AddressesDataGrid";
			trigger.EventName = (NoResString)"Load"; // javascript event name
			UpdatePanelLines.Triggers.Add(trigger);
		}

		public WebModuleID ModuleIDForOrganisationsControls
		{
			get { return SiteUser.IsForwarder ? WebModuleIDs.OrgAgentTracking : WebModuleIDs.OrganisationTracking; }
		}

		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);
			DisabledSubmitButtons.Add(SendISF.ClientID);
			DisabledSubmitButtons.Add(SaveISF.ClientID);
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			bool isDataSourceValid = (CurrentISF != null);
			EditControls.Visible = isDataSourceValid;
			NotFoundError.Visible = !isDataSourceValid;
			if (!isDataSourceValid)
			{
				NotFoundLabel.Text = Res.GetString("ef610524-3b7c-4efa-b1ec-f26cf73c9ff8", "ISF was not found in the database or you don't have rights to view it.");
			}
			SetVisibleONEntryType();
			SetSendButtonAvailability();
			AddressesDataGrid.PostDataChanged += new EventHandler(LinesDataGrid.OnPostDataChanged);
			AddressesDataGrid.AfterDeleteCommand += new DataGridCommandEventHandler(LinesDataGrid.OnPostDataChanged);
		}

		protected void RefreshVisible(object sender, EventArgs e)
		{
			SetVisibleONEntryType();
		}

		protected override bool CanAccessAuthorisedContent
		{
			get { return SiteUser != null && SiteUser.CanEditISF; }
		}

		#endregion

		#region Visible

		static string SendISFEnabledToolTip
		{
			get { return Res.GetString("3ff8a217-5b5b-4990-81a2-d8b4f644b1d7", "Save and Send"); }
		}
		static string SendISFDisabledToolTip
		{
			get { return Res.GetString("01edbc99-b8da-47a1-819c-a900e3927b57", "Unable to send while awaiting response"); }
		}

		protected void SetSendButtonAvailability()
		{
			SendISF.Enabled = CurrentISF != null && !CurrentISF.IsWaitingForResponse;
			SendISF.ToolTip = SendISF.Enabled ? SendISFEnabledToolTip : SendISFDisabledToolTip;
		}

		protected void SetVisibleONEntryType()
		{
			bool isISF10 = CurrentISF != null && CurrentISF.IsISF10Entry;
			ISF5Details.Visible = !isISF10;
			ISF10Details.Visible = isISF10;

			ShipmentTypeDropDown.Width = Unit.Percentage(100);
			SellingPartyAddress.Visible = isISF10;
			BuyingPartyAddress.Visible = isISF10;
			StuffingLocation.Visible = isISF10;
			ConsolidatorAddress.Visible = isISF10;
			BookingPartyAddress.Visible = !isISF10;

			SellingPartyAddress.IsOptional = !isISF10;
			BuyingPartyAddress.IsOptional = !isISF10;
			StuffingLocation.IsOptional = !isISF10;
			ConsolidatorAddress.IsOptional = !isISF10;
			BookingPartyAddress.IsOptional = isISF10;
		}

		#endregion

		#region DocAddressControls

		protected virtual ZDocAddressControl GetNewZDocAddressControl()
		{
			return Page.LoadControl(DocAddressControlResource.FileName) as ZDocAddressControl;
		}

		protected ZDocAddressControl ShipToAddressToAddControl;
		protected ZDocAddressControl DocAddressToAddControl;

		protected virtual void SetupDocAddressControls()
		{
			SetUpDocAddressControl(BookingPartyAddress);
			SetUpDocAddressControl(ConsolidatorAddress);
			SetUpDocAddressControl(StuffingLocation);
			SetUpDocAddressControl(ShipToPartyAddress);
			SetUpDocAddressControl(BuyingPartyAddress);
			SetUpDocAddressControl(SellingPartyAddress);
		}

		void SetUpDocAddressControl(ZDocAddressWebControl addressControl)
		{
			addressControl.OrgModuleID = ModuleIDForOrganisationsControls;
			if (addressControl.IsConsignee)
			{
				addressControl.NewOrgRelationType = NewOrgRelationTypes.Buyer;
			}
			if (addressControl.IsConsignor)
			{
				addressControl.NewOrgRelationType = NewOrgRelationTypes.Supplier;
			}
		}

		#endregion

		#region Grids

		protected override void OnPreBind()
		{
			base.OnPreBind();
			//these grids require DataSource to be loaded before grid's construction.
			//Data source have to be loaded after ViewState's construction

			SetupContainersDataGrid(ContainersDataGrid);
			SetupLinesDataGrid(LinesDataGrid);
			SetupAddressesDataGrid(AddressesDataGrid, false);
			SetupAddressesDataGrid(ShipToAddressesDataGrid, true);
			SetupReferenceDataGrid(ReferenceGrid);
		}

		void SetupAddressesDataGrid(ZDataGrid addressesDataGrid, bool showTypeDecider)
		{
			addressesDataGrid.AutoGenerateColumns = false;
			if (showTypeDecider)
			{
				addressesDataGrid.Columns.Add(new ZDropDownListColumn(Res.GetString("13037fb6-dc23-4f34-85ee-17c4b2b8f2e7", "Address Type"), JobDocAddress.Schema.E2_AddressType, "Parent.SupportedDocAddressesList"));
			}
			addressesDataGrid.Columns.Add(new ZDocAddressColumn(true));
		}

		void SetupLinesDataGrid(ZDataGrid linesDataGrid)
		{
			linesDataGrid.AutoGenerateColumns = false;
			linesDataGrid.Columns.Add(new ZCodeFindBoxColumn(Res.GetString("c22463b6-d6f8-4b89-a578-2a78c3ed7ea1", "Origin"), CusISFLine.Schema.BL_RN_NKGoodsOrigin)
			{
				ColumnKey = WebTracker.Grids.TrackingImporterSecurityFilingLines.Origin,
				ValueFieldName = RefCountry.Schema.RN_Code,
				BindToList = "Lookups.GoodsOrigins",
				ModuleID = WebModuleIDs.RefCountry,
				AutoPostBack = true
			});

			linesDataGrid.Columns.Add(new ZTextEditColumn(Res.GetString("ef6a1ae9-1132-4ed0-8060-0147fe35c315", "Tariff"), CusISFLine.Schema.BL_HarmonisedNum)
			{
				ColumnKey = WebTracker.Grids.TrackingImporterSecurityFilingLines.Tariff,
				AutoPostBack = true,
				ID = "ISFLineTarrif"
			});

			linesDataGrid.Columns.Add(new ZGuidDropDownListColumn(Res.GetString("90745237-3163-4c3a-bf0f-04f4b96a897e", "Manufacturer"), CusISFLine.Schema.BL_ManufacturerDocAddressPK)
			{
				ColumnKey = WebTracker.Grids.TrackingImporterSecurityFilingLines.Manufacturer,
				BindToList = "Header.ManufacturerAddressesList",
				ValueFieldName = "PK",
				TextFieldName = JobDocAddress.Schema.E2_CompanyName,
				AutoPostBack = true,
				ID = "ISFLineManufacturer"
			});

			linesDataGrid.Columns.Add(new ZCodeFindBoxColumn(Res.GetString("09be6e10-d0c4-4b0b-bc62-b09061b96c9e", "Product"), CusISFLine.Schema.BL_TextProductCode)
			{
				ColumnKey = WebTracker.Grids.TrackingImporterSecurityFilingLines.Product,
				ModuleID = WebModuleIDs.OrgSupplierPartTracking,
				AutoPostBack = true
			});
		}

		public void LinesDataGrid_ItemCommand(object sender, EventArgs e)
		{
			LinesDataGrid.Bind(DataSource);
		}

		protected void SetupContainersDataGrid(ZDataGrid containersGrid)
		{
			containersGrid.AutoGenerateColumns = false;

			containersGrid.Columns.Add(new ZDropDownListColumn(Res.GetString("8585ed9b-7811-468d-9b7a-79a1f84b18f2", "Desc. Code"), CusISFEquip.Schema.BE_EquipCode) { DisplayStyle = OComboBoxDropDownStyle.CodeOnly });
			containersGrid.Columns.Add(new ZTextEditColumn(Res.GetString("a4e4271d-2bff-4880-a973-c0cf7c2ff867", "Container Number"), CusISFEquip.Schema.BE_ContainerNum));
			containersGrid.Columns.Add(new ZTextEditColumn(Res.GetString("ae95624c-83c8-406c-b0b3-cbe4fe0d486f", "ISO"), CusISFEquip.Schema.BE_ContainerISO));
		}

		protected void SetupReferenceDataGrid(ZDataGrid referenceGrid)
		{
			referenceGrid.AutoGenerateColumns = false;

			referenceGrid.Columns.Add(new ZDropDownListColumn(Res.GetString("6560373c-3b90-4507-9625-eed1313ff8d1", "Type"), CusISFBill.Schema.BB_BillType, "Lookups.ReferenceBillTypes") { DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly });
			referenceGrid.Columns.Add(new ZTextEditColumn(Res.GetString("b1818af1-53dc-4f21-9cb4-e6332eeed563", "Number"), CusISFBill.Schema.BB_BillNum) { TextTransform = TextTransformOptions.UpperCase });
		}

		#endregion

		#region IAccessControllablePage Members

		IAccessControlled IAccessControllablePage.dataSource
		{
			get { return CurrentISF; }
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
					result.Add(BuyingPartyAddress);
					break;
				case ISFAccessRules.Captions.Consignee_Details:
					result.Add(ConsigneeDetails);
					break;
				case ISFAccessRules.Captions.Consolidator:
					result.Add(ConsolidatorAddress);
					break;
				case ISFAccessRules.Captions.Containers:
					result.Add(Containers);
					break;
				case ISFAccessRules.Captions.Importer_Details:
					result.Add(ImporterDetails);
					break;
				case ISFAccessRules.Captions.ISF_Details:
					result.Add(Entry_Type);
					result.Add(upDetails);
					break;
				case ISFAccessRules.Captions.Lines:
					result.Add(Lines);
					break;
				case ISFAccessRules.Captions.Manufacturers:
					result.Add(Addresses);
					break;
				case ISFAccessRules.Captions.Selling_Party:
					result.Add(SellingPartyAddress);
					result.Add(ShipToAddresses);
					break;
				case ISFAccessRules.Captions.Ship_To_Parties:
					result.Add(ShipToPartyAddress);
					result.Add(ShipToAddresses);
					break;
				case ISFAccessRules.Captions.Stuffing_Location:
					result.Add(StuffingLocation);
					break;
				case ISFAccessRules.Captions.Documents:
					break;
				default:
					throw new NotImplementedException("Registry Access captions don't correspond edit page's: " + caption);
			}

			return result.ToArray();
		}

		#endregion
	}
}
