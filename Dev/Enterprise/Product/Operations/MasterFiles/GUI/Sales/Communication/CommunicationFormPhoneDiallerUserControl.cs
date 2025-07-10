using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterFiles.GUI
{
	public partial class CommunicationFormPhoneDiallerUserControl : MultiPhoneDiallerUserControl
	{
		public CommunicationFormPhoneDiallerUserControl()
		{
			InitializeComponent();
		}

		#region DataBinding

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			base.OnCurrentDataItemChanging(e);

			var communication = Communication;
			if (communication != null)
			{
				UnhookCommunicationEvents(communication);
			}
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);

			var communication = Communication;
			if (communication != null)
			{
				HookCommunicationEvents(communication);
			}
		}

		#endregion

		#region Communication

		protected OrgSalesCall Communication
		{
			get { return (OrgSalesCall)CurrentDataItem; }
		}

		void HookCommunicationEvents(OrgSalesCall communication)
		{
			communication.OQ_OHInfo.ValueChanged += Communication_ValueChanged;
			communication.OQ_OCInfo.ValueChanged += Communication_ValueChanged;
			communication.LinkedInquiryChanged += Communication_ValueChanged;
		}

		void UnhookCommunicationEvents(OrgSalesCall communication)
		{
			communication.OQ_OHInfo.ValueChanged -= Communication_ValueChanged;
			communication.OQ_OCInfo.ValueChanged -= Communication_ValueChanged;
			communication.LinkedInquiryChanged -= Communication_ValueChanged;
		}

		void Communication_ValueChanged(object sender, EventArgs e)
		{
			RefreshControls();
		}

		#endregion

		#region DialInfo

		protected override PhoneDialInfo GetDefaultDialInfo()
		{
			var communication = Communication;
			if (communication != null)
			{
				var linkedInquiry = communication.LinkedInquiry;
				if (linkedInquiry != null)
				{
					return new InquiryPhoneDialInfoBuilder().GetDefaultDialInfo(linkedInquiry);
				}
				else
				{
					return new ContactPhoneDialInfoBuilder().GetDefaultDialInfo(communication.Contact, communication.Header);
				}
			}

			return null;
		}

		protected override IEnumerable<PhoneDialInfo> AlternativePhoneDialInfoList
		{
			get
			{
				var communication = Communication;
				if (communication != null)
				{
					var linkedInquiry = communication.LinkedInquiry;
					if (linkedInquiry != null)
					{
						return new InquiryPhoneDialInfoBuilder().GetAlternativePhoneDialInfos(linkedInquiry);
					}
					else
					{
						return new ContactPhoneDialInfoBuilder().GetAlternativePhoneDialInfos(communication.Contact, communication.Header);
					}
				}

				return Enumerable.Empty<PhoneDialInfo>();
			}
		}

		#endregion

		#region Messages

		protected override string NoDefaultPhoneContactDialsCaption
		{
			get { return Res.GetString("7958ebd2-b386-4e6e-a743-3045f04bbdf3", "Contact does not have a Work or Office phone contact details."); }
		}

		protected override string NoPhoneContactDetailsCaption
		{
			get { return Res.GetString("0612b3cd-dd3c-48c0-882a-a7ce979a9c3d", "No phone contact details available."); }
		}

		#endregion

		public class InquiryPhoneDialInfoBuilder
		{
			public PhoneDialInfo GetDefaultDialInfo(SalesEnquiry inquiry)
			{
				var workPhone = inquiry.O1_Phone;
				if (!workPhone.IsEmpty)
				{
					return new PhoneDialInfo(workPhone, PhoneContactItemDescriptionList.Descriptions.Work);
				}

				return null;
			}

			public IEnumerable<PhoneDialInfo> GetAlternativePhoneDialInfos(SalesEnquiry inquiry)
			{
				if (!inquiry.O1_Phone.IsEmpty)
				{
					yield return new PhoneDialInfo(inquiry.O1_Phone, PhoneContactItemDescriptionList.Descriptions.Work);
				}

				if (!inquiry.O1_Mobile.IsEmpty)
				{
					yield return new PhoneDialInfo(inquiry.O1_Mobile, PhoneContactItemDescriptionList.Descriptions.Mobile);
				}
			}
		}
	}
}
