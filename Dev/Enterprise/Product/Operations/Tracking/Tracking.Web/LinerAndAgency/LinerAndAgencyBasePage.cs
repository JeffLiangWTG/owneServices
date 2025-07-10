using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Tracking.Business;
using Enterprise.Tracking.Web.GridColumn.Providers.LinerAndAgency;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.Modules;

namespace Enterprise.Tracking.Web.LinerAndAgency
{
	public abstract class LinerAndAgencyBasePage : BasePageWithAuthorisation
	{
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			NotificationFlags.DisplayAll = false;

			if (DataSource == null)
			{
				NotFoundLabel.Text = NotFoundLabelText;
				DataContent.Visible = false;
			}
			else
			{
				NotFoundError.Visible = false;
				DataContent.Visible = true;

				BindDocoments(DocumentsGrid);
				NotesPanel.Visible = WebInterfacesHelper.NotesHelper.VisibleNotes.Count > 0;
			}
		}

		protected abstract string NotFoundLabelText { get; }

		protected override void OnPreBind()
		{
			base.OnPreBind();
			SetupPackLinesGrid();
		}

		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);
			if (ContainersGrid != null)
			{
				ContainersGrid.Visible = IsFCL;
			}

			if (PacksGrid != null)
			{
				PacksGrid.Caption = IsRollOnRollOff ? Res.GetString("f592c564-ae8a-4be9-b610-57ab9e0a60e7", "Vehicles") : Res.GetString("d5126aa3-fa4a-4076-8459-fcfbe02c49c5", "Packs");
			}
		}

		protected bool HasInvoices
		{
			get
			{
				if (!hasInvoices.HasValue)
				{
					hasInvoices = false;

					Job shipmentJob = new Job.Loader(Shipment).Load();

					if (shipmentJob != null)
					{
						ZQuery invoiceQuery = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, ZArchitecture.Core.TransactionTypes.Invoice);
						invoiceQuery.AddToFilter(AccTransactionHeaderSchema.AH_JH, shipmentJob.PK);

						hasInvoices = Shipment.Factory.LoadTop1<AccTransactionHeader>(invoiceQuery) != null;
					}
				}

				return hasInvoices.Value;
			}
		}
		bool? hasInvoices;

		#region WebInterfacesHelper

		protected LinerAndAgencyBaseWebInterfacesHelper WebInterfacesHelper
		{
			get { return webInterfacesHelper ?? (webInterfacesHelper = GetNewWebInterfacesHelper()); }
		}
		LinerAndAgencyBaseWebInterfacesHelper webInterfacesHelper;

		protected abstract LinerAndAgencyBaseWebInterfacesHelper GetNewWebInterfacesHelper();

		#endregion

		#region BusinessObjectChangesEmailNotifier

		[SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults")]
		protected override void AddBusinessObjectChangesEmailNotifier(BusinessObject bizO)
		{
			if (bizO != null && bizO == Shipment && WebInterfacesHelper != null)
			{
				new BusinessObjectChangesEmailNotifier(WebInterfacesHelper);
			}
			if (bizO is IEventReferenceProvider)
			{
				new BusinessObjectModifiedEventHelper(bizO);
			}
		}

		#endregion

		#region Properties

		protected AgencyShipment Shipment
		{
			get { return DataSource as AgencyShipment; }
		}

		protected bool IsFCL
		{
			get { return Shipment != null && Shipment.JS_PackingMode == Core.Constants.ContainerModes.FCL; }
		}

		protected bool IsRollOnRollOff
		{
			get { return Shipment != null && Shipment.JS_PackingMode == Core.Constants.ContainerModes.RollOnRollOff; }
		}

		#endregion

		#region SetupGrids

		protected override void SetupGrids()
		{
			base.SetupGrids();

			SetupBookedContainersGrid();
			SetupRealContainersGrid();
			SetupDocumentsGrid(DocumentsGrid);
		}

		protected void SetupBookedContainersGrid()
		{
			if (BookedContainersGrid != null)
			{
				SetupContainersGrid(BookedContainersGrid);
			}
		}

		protected void SetupRealContainersGrid()
		{
			if (ContainersGrid != null)
			{
				SetupContainersGrid(ContainersGrid);
			}
		}

		void SetupContainersGrid(ZDataGrid containersGrid)
		{
			var grid = containersGrid as ZGrid;
			if (grid != null)
			{
				grid.ColumnProvider = new CustomizedLinerAndAgencyContainerColumnProvider();
			}
			else
			{
				PopulateGrid(containersGrid, GetContainersColumns());
			}
		}

		[SuppressMessage("Style", "IDE0004:Remove Unnecessary Cast", Justification = "ZBindToChecker requires a redundant cast")]
		DataGridColumn[] GetContainersColumns()
		{
			var containersGridColumns = new List<DataGridColumn>();

			var numColumn = new ZTextEditColumn(Res.GetString("ecda6d7c-e788-4ccd-8887-b39b5518556e", "Number"), AgencyBookingContainer.Schema.JC_ContainerNum);
			if (SiteUser != null && !SiteUser.CanEditLinerAndAgencyContainerNumbers)
			{
				numColumn.EditItemTemplate = numColumn.GetNewItemTemplate();
			}
			containersGridColumns.Add(numColumn);

			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((RefContainerCollection)(((AgencyShipmentContainer)(null)).RefContainer_List)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZGuid)(((AgencyShipmentContainer)(null)).JC_RC)));
			containersGridColumns.Add(new ZGuidDropDownListColumn(Res.GetString("a6a6e8d0-be4e-4a75-9d33-a86f1deac587", "Type"), AgencyShipmentContainer.Schema.JC_RC)
			{
				ValueFieldName = "PK",
				TextFieldName = RefContainer.Schema.RC_Code,
				DisplayStyle = OComboBoxDropDownStyle.CodeOnly,
				AutoPostBack = false
			});

			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZDecimal)(((AgencyShipmentContainer)(null)).JC_ContainerCount)));
			containersGridColumns.Add(new ZCalcEditColumn(Res.GetString("3914c902-976b-41e5-97c7-9fd00c65bbd2", "Count"), AgencyShipmentContainer.Schema.JC_ContainerCount) { Decimals = 0 });

			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZDecimal)(((AgencyShipmentContainer)(null)).JC_Calc_NetWeight)));
			containersGridColumns.Add(new ZCalcEditColumn(Res.GetString("7bd957f9-66d8-4f56-9af9-f13e2af3a8cf", "Net Wt."), AgencyBookingContainer.Schema.JC_Calc_NetWeight));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZDecimal)(((AgencyShipmentContainer)(null)).JC_TareWeight)));
			containersGridColumns.Add(new ZCalcEditColumn(Res.GetString("25d66603-279b-4645-b7de-df0a33918ec9", "Tare Wt."), AgencyBookingContainer.Schema.JC_TareWeight) { ReadOnly = true });
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZDecimal)(((AgencyShipmentContainer)(null)).JC_GrossWeight)));
			containersGridColumns.Add(new ZCalcEditColumn(Res.GetString("1fc8199e-a8f9-488d-ae69-1070597f821f", "Gross Wt."), AgencyBookingContainer.Schema.JC_GrossWeight) { ReadOnly = true });
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZString)(((AgencyShipmentContainer)(null)).JC_GrossWeightUQ)));
			containersGridColumns.Add(new ZDropDownListColumn(Res.GetString("65b738ce-8a7b-4836-83a5-09b77ad816ca", "WQ"), AgencyShipmentContainer.Schema.JC_GrossWeightUQ) { DisplayStyle = OComboBoxDropDownStyle.CodeOnly });

			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZString)(((AgencyShipmentContainer)(null)).JC_RH_NKContainerCommodityCode)));
			containersGridColumns.Add(new ZCodeFindBoxColumn(Res.GetString("4dca421c-8d87-4591-9b78-77d36b373109", "Commodity"), AgencyBookingContainer.Schema.JC_RH_NKContainerCommodityCode) { ModuleID = WebModuleIDs.RefCommodityCode });

			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZBool)(((AgencyShipmentContainer)(null)).JC_IsShipperOwned)));
			containersGridColumns.Add(new ZCheckBoxColumn(Res.GetString("73917685-eda0-4459-b66f-157b2123007d", "Is Shipper Owned"), AgencyBookingContainer.Schema.JC_IsShipperOwned));

			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZString)(((AgencyShipmentContainer)(null)).JC_SealNum)));
			containersGridColumns.Add(new ZTextEditColumn(Res.GetString("9ffa81b9-600d-43d8-b8bd-74368dcae1dd", "Seal #"), AgencyBookingContainer.Schema.JC_SealNum));

			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((((CommonContainerLookups)(null)).GrossWeightVerificationTypeList)));
			containersGridColumns.Add(new ZDropDownListColumn(Res.GetString("F65B8A77-480F-4C07-829D-E4E4EA460A4F", "Verified Method"), AgencyShipmentContainer.Schema.JC_GrossWeightVerificationType)
			{
				DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly,
				AutoPostBack = true
			});

			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZDateTime)(((AgencyShipmentContainer)(null)).JC_GrossWeightVerificationDateTime)));
			containersGridColumns.Add(new ZDateTimeColumn(Res.GetString("0B4ACD6D-268F-4A44-90DF-19675F27BB95", "Verified Date"), AgencyShipmentContainer.Schema.JC_GrossWeightVerificationDateTime));

			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZGuid)(((AgencyShipmentContainer)(null)).GrossWeightVerifiedByPK)));
			containersGridColumns.Add(new ZGuidFindBoxColumn(Res.GetString("B0C087AE-44B4-487C-8CCA-5138433A6CED", "Verified By"), AgencyBookingContainer.Schema.GrossWeightVerifiedByPK, "JobContainer.Lookups.GrossWeightVerifiedByList")
			{
				ModuleID = WebModuleIDs.OrganisationTracking
			});

			return containersGridColumns.ToArray();
		}

		protected void SetupPackLinesGrid()
		{
			PopulateGrid(PacksGrid, GetPacksColumns());
			VolumeCalculatorGridAddOn.Grid = PacksGrid;
		}

		[SuppressMessage("Style", "IDE0004:Remove Unnecessary Cast", Justification = "ZBindToChecker requires a redundant cast")]
		DataGridColumn[] GetPacksColumns()
		{
			var packsGridColumns = new List<DataGridColumn>();
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZGuid)(((AgencyShipmentPackLine)(null)).JL_JC)));
			containerColumn = new ZGuidDropDownListColumn(Res.GetString("e07012be-e794-4703-b97f-a338c8141251", "Container"), AgencyShipmentPackLine.Schema.JL_JC)
			{
				ValueFieldName = "PK",
				TextFieldName = AgencyBookingContainer.Schema.JC_ContainerCode,
				DisplayStyle = OComboBoxDropDownStyle.CodeOnly
			};
			if (IsFCL)
			{
				packsGridColumns.Add(containerColumn);
			}

			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZString)(((AgencyShipmentPackLine)(null)).JL_RefNumber)));
			ZTextEditColumn vinSerialColumn = new ZTextEditColumn(Res.GetString("12358c1a-39f0-4bc7-83f3-04f56e39c94c", "VIN/Serial"), AgencyShipmentPackLine.Schema.JL_RefNumber);
			if (IsRollOnRollOff)
			{
				packsGridColumns.Add(vinSerialColumn);
			}

			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZDecimal)(((AgencyShipmentPackLine)(null)).JL_PackageCount)));
			packsGridColumns.Add(new ZCalcEditColumn(IsRollOnRollOff ? Res.GetString("3914c902-976b-41e5-97c7-9fd00c65bbd2", "Count") : Res.GetString("d5126aa3-fa4a-4076-8459-fcfbe02c49c5", "Packs"), AgencyShipmentPackLine.Schema.JL_PackageCount) { Decimals = 0 });
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZString)(((AgencyShipmentPackLine)(null)).JL_F3_NKPackType)));
			ZDropDownListColumn packTypeColumn = new ZDropDownListColumn(Res.GetString("b09aeac0-bb5b-4ec4-af23-d687cfb56452", "Pack Type"), AgencyShipmentPackLine.Schema.JL_F3_NKPackType) { ValueFieldName = RefPackTypeSchema.F3_Code.Name, TextFieldName = RefPackTypeSchema.F3_Description.Name + (NoResString)"Multilingual" };// property name constant
			if (!IsRollOnRollOff)
			{
				packsGridColumns.Add(packTypeColumn);
			}

			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZString)(((AgencyShipmentPackLine)(null)).JL_DetailedDescription)));
			ZTextEditColumn descriptionColumn = new ZTextEditColumn(Res.GetString("6722b7a6-b384-4169-b554-c0630b4ca41a", "Description"), AgencyShipmentPackLine.Schema.JL_DetailedDescription);
			if (IsRollOnRollOff)
			{
				packsGridColumns.Add(descriptionColumn);
			}

			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZString)(((AgencyShipmentPackLine)(null)).JL_MarksAndNumbers)));
			ZTextEditColumn marksAndNumbersColumn = new ZTextEditColumn(Res.GetString("7f2ec15d-cf5a-427b-a941-6505a6a90579", "Marks And Numbers"), AgencyShipmentPackLine.Schema.JL_MarksAndNumbers);
			if (IsRollOnRollOff)
			{
				packsGridColumns.Add(marksAndNumbersColumn);
			}

			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZDecimal)(((AgencyShipmentPackLine)(null)).JL_ActualWeight)));
			packsGridColumns.Add(new ZCalcEditColumn(IsRollOnRollOff ? Res.GetString("1fc8199e-a8f9-488d-ae69-1070597f821f", "Gross Wt.") : Res.GetString("f5908b1c-a7e8-4682-826c-30db9e9a9fbf", "Weight"), AgencyShipmentPackLine.Schema.JL_ActualWeight));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZString)(((AgencyShipmentPackLine)(null)).JL_ActualWeightUQ)));
			packsGridColumns.Add(new ZDropDownListColumn(Res.GetString("90f480ce-dcb5-4826-97d8-4e7f7c873630", "UW"), AgencyShipmentPackLine.Schema.JL_ActualWeightUQ) { DisplayStyle = OComboBoxDropDownStyle.CodeOnly });

			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZDecimal)(((AgencyShipmentPackLine)(null)).JL_Length)));
			ZCalcEditColumn lengthColumn = new ZCalcEditColumn(Res.GetString("6a9a1bc0-388b-4d58-86c1-b095821979c1", "Length"), AgencyShipmentPackLine.Schema.JL_Length);
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZDecimal)(((AgencyShipmentPackLine)(null)).JL_Width)));
			ZCalcEditColumn widthColumn = new ZCalcEditColumn(Res.GetString("17dbdebe-d126-4c7f-a58b-40f141a2a98d", "Width"), AgencyShipmentPackLine.Schema.JL_Width);
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZDecimal)(((AgencyShipmentPackLine)(null)).JL_Height)));
			ZCalcEditColumn heightColumn = new ZCalcEditColumn(Res.GetString("ee6836b6-2984-476c-a9f6-767840ec2fed", "Height"), AgencyShipmentPackLine.Schema.JL_Height);
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZString)(((AgencyShipmentPackLine)(null)).JL_UnitOfDimension)));
			ZDropDownListColumn dimensionUnitColumn = new ZDropDownListColumn(Res.GetString("62953ae5-fbb7-40a5-b32c-c404888fbb29", "UD"), AgencyShipmentPackLine.Schema.JL_UnitOfDimension) { DisplayStyle = OComboBoxDropDownStyle.CodeOnly };
			if (!IsRollOnRollOff)
			{
				packsGridColumns.Add(lengthColumn);
				packsGridColumns.Add(widthColumn);
				packsGridColumns.Add(heightColumn);
				packsGridColumns.Add(dimensionUnitColumn);
			}

			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZDecimal)(((AgencyShipmentPackLine)(null)).JL_ActualVolume)));
			packsGridColumns.Add(new ZCalcEditColumn(Res.GetString("986e1e50-0fcf-464d-8f3e-8f568506795f", "Volume"), AgencyShipmentPackLine.Schema.JL_ActualVolume));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZString)(((AgencyShipmentPackLine)(null)).JL_ActualVolumeUQ)));
			packsGridColumns.Add(new ZDropDownListColumn(Res.GetString("f52acd04-dfe0-4813-b2d9-fe21874f2486", "UV"), AgencyShipmentPackLine.Schema.JL_ActualVolumeUQ) { DisplayStyle = OComboBoxDropDownStyle.CodeOnly });

			if (!IsRollOnRollOff)
			{
				packsGridColumns.Add(descriptionColumn);
			}

			if (SiteUser != null && !SiteUser.IsShipmentQuickViewUser)
			{
				CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZString)(((AgencyShipmentPackLine)(null)).JL_RH_NKCommodityCode)));
				packsGridColumns.Add(new ZCodeFindBoxColumn(Res.GetString("4dca421c-8d87-4591-9b78-77d36b373109", "Commodity"), AgencyShipmentPackLine.Schema.JL_RH_NKCommodityCode) { ModuleID = WebModuleIDs.RefCommodityCode });

				CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((UNDGSubstanceCollection)(((AgencyShipmentPackLine)(null)).UNDGs.UNDGSubstances)));
				CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZGuid)(((AgencyShipmentPackLine)(null)).FirstUNDGPK)));
				packsGridColumns.Add(new ZFindBoxColumn(Res.GetString("df14c671-f67c-4530-93ce-5951b3f7101d", "Dangerous Goods"), "FirstUNDGPK") { ModuleID = WebModuleIDs.DangerousGoods, BindToList = "UNDGs.UNDGSubstances", TextFieldName = UNDGSubstanceSchema.DG_Code.Name });

				CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZString)(((AgencyShipmentPackLine)(null)).JL_HarmonisedCode)));
				packsGridColumns.Add(new ZTextEditColumn(Res.GetString("fcf1ff8e-71c9-434f-99dc-9d8c156520a7", "Harmonized Code"), AgencyShipmentPackLine.Schema.JL_HarmonisedCode));
			}

			return packsGridColumns.ToArray();
		}

		#endregion

		#region Additional Binding

		protected void BindWebUserEditableNote(ZTextBox note)
		{
			if (note != null)
			{
				note.BindTo = "UserEditableNoteHelper.EditableNoteText";
				note.Bind(WebInterfacesHelper);
				note.BindTo = string.Empty;
			}
		}

		protected void BindDocoments(ZDataGrid documents)
		{
			if (documents != null)
			{
				documents.BindTo = "DocumentHelper.Documents";
				documents.Bind(WebInterfacesHelper);
				documents.BindTo = string.Empty;
			}
			if (eDocsAddNewLink != null)
			{
				eDocsAddNewLink.SetDataSource(WebInterfacesHelper);
			}
		}

		#endregion

		#region Event Handlers

		[SuppressMessage("Microsoft.Globalization", "CA1305:Redirect URL.")]
		protected void SaveButton_Click(object sender, EventArgs e)
		{
			NotificationFlags.DisplayErrors = true;
			DataSource.RunPreSaveValidation();
			if (!DataSource.HasErrors)
			{
				if (WebUserNote != null)
				{
					WebInterfacesHelper.UserEditableNoteHelper.EditableNoteText = WebUserNote.Text;
				}
				if (SaveDataSourceFactory() && !string.IsNullOrEmpty(ViewPageUrl))
				{
					Response.Redirect(string.Format((NoResString)"{0}?Ref={1}", ViewPageUrl, DataSource.PK)); // partial URL
				}
			}
		}

		[SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "Passing opaque value around. This is not the place to parse and/or validate.")]
		protected virtual string ViewPageUrl
		{
			get { return string.Empty; }
		}

		#endregion

		protected HtmlGenericControl NotFoundError;
		protected ZTextLabel NotFoundLabel;
		protected HtmlGenericControl DataContent;
		protected HtmlGenericControl ContainersPanel;
		protected ZDataGrid BookedContainersGrid;
		protected ZDataGrid ContainersGrid;
		protected ZDataGrid PacksGrid;
		protected ZTextLabel PacksLabel;
		ZGuidDropDownListColumn containerColumn;
		protected HtmlGenericControl DocumentsPanel;
		protected ZDataGrid DocumentsGrid;
		protected HtmlGenericControl NotesPanel;
		protected ZNotesControl Notes;
		protected ZTextBox WebUserNote;
	}
}
