using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class SalesEnquiryCloseAction : NonPersistentBusinessObject, IObsoleteValidation
	{
		public SalesEnquiryCloseAction(ZGuid[] gridSelectedElements)
			: base(new BusinessObjectFactory())
		{
			Enquiries = Factory.Load<SalesEnquiry>(new ZQuery(OrgColdCallRegisterSchema.PK, gridSelectedElements));
		}
		readonly SalesEnquiry[] Enquiries;

		#region Synchronisation

		public void Synchronise()
		{
			if (!IsClosable)
			{
				return;
			}

			RunPreSaveValidation();
			if (!HasErrors)
			{
				foreach (SalesEnquiry enquiry in Enquiries)
				{
					if (enquiry.O1_LeadStatus == SalesEnquiryStatusCodeList.Codes.Open)
					{
						enquiry.DoClose();
					}

					if (enquiry.O1_LeadStatus == SalesEnquiryStatusCodeList.Codes.Closed)
					{
						enquiry.O1_CloseReason = CloseReason;
					}
				}

				try
				{
					Factory.Save();
				}
				catch (ZSaveException ex)
				{
					ZExceptionReporting.HandleSaveException(ex);
				}
			}
			MessageInfo.RefreshBinding();
		}

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateCloseReason();
		}

		void ValidateCloseReason()
		{
			CloseReasonInfo.ClearAllNotifications();

			if (IsCloseReasonMandatory)
			{
				MandatoryValidation.CheckEntered(CloseReasonInfo);
			}

			ListValidation.ErrorIfInvalidCode(CloseReasonInfo);
		}

		bool IsCloseReasonMandatory
		{
			get { return OrganisationsDataRegistry.Instance.SalesEnquiryFieldsMandatory.Value.GetBoolFromCode(OrgColdCallRegisterSchema.Constants.O1_CloseReason); }
		}

		#endregion

		#region Properties

		public override bool ReadOnly
		{
			get { return !IsClosable; }
			set { base.ReadOnly = value; }
		}

		public ZBool IsClosable
		{
			get { return Env.Security.InquiryManagerClose.IsAllowed && HasValidSelectedEnquiries; }
		}

		bool HasValidSelectedEnquiries
		{
			get { return Enquiries.Length > 0; }
		}

		public ZString Message
		{
			get
			{
				if (!Env.Security.InquiryManagerClose.IsAllowed)
				{
					return Env.Security.InquiryManagerClose.ErrorMessageForNotAllowed;
				}
				else if (HasValidSelectedEnquiries)
				{
					string suffix = IsCloseReasonMandatory ? Res.GetString("bcd15afd-9f74-4dd9-9991-f99e01e0198d", "This is mandatory.") : Res.GetString("832cd06a-bbf7-466a-afbe-184bb71ff804", "This is optional.");
					return Res.GetString("fb97c44c-a315-4628-a700-fbaf17659aac", "Select a Close Reason for the selected Inquiries. {0}", suffix);
				}
				else
				{
					return Res.GetString("58607bc3-8ea0-4293-bd9a-4a1131ad2ee6", "You have not selected any Inquiries or the Inquiries you have selected have already been deleted.");
				}
			}
		}

		public ZPropertyInfo MessageInfo
		{
			get { return GetZPropertyInfo(nameof(Message)); }
		}

		[List("Lookups.CloseReason_ActiveList")]
		public ZString CloseReason
		{
			get { return closeReason; }
			set
			{
				if (closeReason != value)
				{
					CheckMaximumLength(CloseReasonInfo, value);
					closeReason = value;
					if (!IsValidationSuspended)
					{
						ValidateCloseReason();
					}
					CloseReasonInfo.RefreshBinding();
				}
			}
		}
		ZString closeReason;

		public ZPropertyInfo CloseReasonInfo
		{
			get { return GetZPropertyInfo(nameof(CloseReason)); }
		}

		public int CloseReason_MaxLength
		{
			get { return 3; }
		}

		public SalesEnquiryLookups Lookups
		{
			get
			{
				if (lookups == null)
				{
					lookups = new SalesEnquiryLookups(null);
				}
				return lookups;
			}
		}

		SalesEnquiryLookups lookups;

		#endregion
	}
}
