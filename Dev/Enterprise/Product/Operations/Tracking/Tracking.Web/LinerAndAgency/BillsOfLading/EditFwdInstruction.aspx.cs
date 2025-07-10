using System;
using CargoWise.EntityFramework;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Web.Business;

namespace Enterprise.Tracking.Web.LinerAndAgency
{
	public partial class EditFwdInstruction : BillOfLadingBasePage
	{
		protected override string GetPageRelativePath()
		{
			return TrackingConstants.RelativePath.LinerAndAgencyEditForwardingInstructionPage;
		}

		protected override bool CanAccessAuthorisedContent
		{
			get { return SiteUser.CanEditLinerAndAgencyFwdInstructions; }
		}

		protected override string GetPageName()
		{
			return WebTracker.Pages.EditBillOfLading;
		}

		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);
			DisabledSubmitButtons.Add(SaveButton.ClientID);
		}

		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);

			SetupConsignorAddressControl();
			SetupConsigneeAddressControl();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (BillOfLading != null)
			{
				PaymentTerm.Enabled = !HasInvoices;
			}

			BindDetailedGoodsDescriptionNote(DetailedGoodsDescription);
			BindMarksAndNumbersNote(MarksAndNumbers);
			if (DataSource != null && !IsPostBack)
			{
				BindWebUserEditableNote(WebUserNote);
			}
		}

		#region DataSource

		protected override BusinessObject GetNewDataSource()
		{
			BillOfLading result = (BillOfLading)base.GetNewDataSource();

			if (result.JS_ShipmentStatus == ShipmentStatusList.Codes.Booked)
			{
				result.Confirm();
				result.JS_ShipmentStatus = ShipmentStatusList.Codes.WebFwdInstruction; // Convert confirmed Booking to Web Forwarding Instruction 
			}

			if (result.JS_ShipmentStatus != ShipmentStatusList.Codes.WebFwdInstruction)
			{
				result = null; // Do not allow to edit anything but Forwarding Instruction
			}

			return result;
		}

		protected override bool IsPersistDataSourceBetweenPostbacks
		{
			get { return true; }
		}

		#endregion

		#region Additional Binding

		protected void BindDetailedGoodsDescriptionNote(ZArchitecture.Web.GUI.WebControls.ZTextBox note)
		{
			if (note != null)
			{
				note.BindTo = "DetailedGoodsDescriptionHelper.EditableNoteText";
				note.Bind(WebInterfacesHelper);
				note.BindTo = string.Empty;
			}
		}

		protected void BindMarksAndNumbersNote(ZArchitecture.Web.GUI.WebControls.ZTextBox note)
		{
			if (note != null)
			{
				note.BindTo = "MarksAndNumbersHelper.EditableNoteText";
				note.Bind(WebInterfacesHelper);
				note.BindTo = string.Empty;
			}
		}

		#endregion

		#region Address Controls setup

		void SetupConsignorAddressControl()
		{
			ConsignorAddress.NewOrgRelationType = NewOrgRelationTypes.Supplier;
		}

		void SetupConsigneeAddressControl()
		{
			ConsigneeAddress.NewOrgRelationType = NewOrgRelationTypes.Buyer;
		}

		#endregion

		#region Event Handling

		protected override string ViewPageUrl
		{
			get { return AppInstance.LinerAndAgencyBillOfLadingDetailsPage; }
		}

		#endregion
	}
}
