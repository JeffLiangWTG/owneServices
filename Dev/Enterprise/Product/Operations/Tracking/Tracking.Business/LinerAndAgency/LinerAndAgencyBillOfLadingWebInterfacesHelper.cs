using Enterprise.DocumentEngine.GUI;
using Enterprise.Freight.Agency.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Tracking.Business
{
	public class LinerAndAgencyBillOfLadingWebInterfacesHelper : LinerAndAgencyBaseWebInterfacesHelper, IDocumentsMenuProvider
	{
		public LinerAndAgencyBillOfLadingWebInterfacesHelper(BillOfLading billOfLading)
			: base(billOfLading)
		{
		}

		public BillOfLading BillOfLading
		{
			get { return AgencyShipment as BillOfLading; }
		}

		#region IBizOChangesEmailNotification members

		public override ControllerID ControllerForEnterpriseUrl
		{
			get { return ControllerIDs.AgencyBillOfLading; }
		}

		public override GuidRegistryItem EmailGroupRegistryItem
		{
			get { return WebDataRegistry.Instance.LinerAndAgencyBillOfLadingNotificationEmailGroup; }
		}

		public override CodeDescriptionBoolRegistryItem StaffRolesToNotify
		{
			get { return WebDataRegistry.Instance.LinerAndAgencyBillOfLadingNotificationStaffRoles; }
		}

		public override CodePairRegistryItem NotificationSendingRule
		{
			get { return WebDataRegistry.Instance.LinerAndAgencyBillOfLadingNotificationOptions; }
		}

		#region Email Reporting

		public override void AddPropertiesForEmailReporting(DataState state)
		{
			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("078A8F6D-598E-44AF-85D7-6B73F876FF63", "Shipper's Ref#"), BillOfLading.JS_BookingReference);

			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("a0e42589-defd-4993-8e0f-e2efffe46add", "Origin"), BillOfLading.JS_RL_NKOrigin);
			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("a2c4636c-483b-4ad2-8c08-b6fbd520bb6f", "Destination"), AgencyShipment.JS_RL_NKDestination);

			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("023f0e30-28b1-4d87-a734-aedbf2448dbd", "ETD"), BillOfLading.JS_E_DEP);
			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("20262c4c-fc8e-470b-90fc-b1830ffa4aea", "ETA"), BillOfLading.JS_E_ARV);

			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("2ac0852c-b7f3-4870-8aeb-9b4483fa1d3a", "Payment Term"), BillOfLading.JS_PaymentTerm);
			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("a24b1408-a0ab-4899-b3ca-f57405abe2de", "Release Type"), BillOfLading.JS_ReleaseType);
			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("172b7122-d0f6-4bab-b0d4-e5606458c854", "Original Bills"), BillOfLading.JS_NoOriginalBills);
			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("ece45e7a-e31c-4a2f-adee-a08b260f3231", "Copy Bills"), BillOfLading.JS_NoCopyBills);

			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("33e7fc17-90a1-455e-91ca-352c212d1496", "Description"), BillOfLading.JS_GoodsDescription);
			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("be6c86fa-098f-43fc-bf1e-1fa1ee13434b", "Marks & Numbers"), BillOfLading.JS_MarksAndNumbers);

			AddAddressForEmailReporting(state, FreightDataRegistry.Instance.ConsignorShipperTerminology.Value, BillOfLading.ConsignorDocumentaryAddress);
			AddAddressForEmailReporting(state, ResString.GetMultilingualString("80bb9c8b-e98e-4b2d-ad8c-05a94c2110f4", "Consignee"), BillOfLading.ConsigneeDocumentaryAddress);
			AddAddressForEmailReporting(state, ResString.GetMultilingualString("c9225582-0666-4ad6-b6e7-d175fa883ce6", "Notify Party"), BillOfLading.NotifyPartyDocumentaryAddress);

			AddContainersForEmailReporting(state, BillOfLading.RealContainers);
			AddPackLinesForEmailReporting(state, BillOfLading.OuterPackLines);
			DocumentHelper.AddDocumentsForEmailReporting(state, PropertiesForEmailReporting);
			IMilestonesProvider milestoneProvider = BillOfLading as IMilestonesProvider;
			if (milestoneProvider != null)
			{
				milestoneProvider.Milestones.AddForEmailReporting(state, PropertiesForEmailReporting);
			}
		}

		void AddAddressForEmailReporting(DataState state, MultilingualString addressTitle, JobDocAddress address)
		{
			PropertiesForEmailReporting.Add(state, addressTitle, address.E2_CompanyNameTruncated);
			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("d1bed84a-cb62-44e5-8014-84c713b5a0d5", "{0} Address", addressTitle), address.AddressAsASingleLine);
			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("d5503881-b34d-4df8-83e6-61e9bbefe19a", "{0} Contact", addressTitle), address.E2_Contact);
		}

		#endregion

		#endregion

		public WebUserEditableNote DetailedGoodsDescriptionHelper
		{
			get { return detailedGoodsDescriptionHelper ?? (detailedGoodsDescriptionHelper = new WebUserEditableNote(this, PredefinedNoteTypes.Instance.DetailedGoodsDescription)); }
		}
		WebUserEditableNote detailedGoodsDescriptionHelper;

		public WebUserEditableNote MarksAndNumbersHelper
		{
			get { return marksAndNumbersHelper ?? (marksAndNumbersHelper = new WebUserEditableNote(this, PredefinedNoteTypes.Instance.MarksAndNumbers)); }
		}
		WebUserEditableNote marksAndNumbersHelper;

		#region IDocumentsMenuProvider Members

		public DocumentsMenuHelper DocumentsMenuHelper
		{
			get { return new LinerAndAgencyBillOfLadingDocMenuHelper(BillOfLading); }
		}

		#endregion
	}
}
