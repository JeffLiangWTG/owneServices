using System;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business
{
	[TestedType(typeof(SalesEnquiryCloseAction))]
	sealed class SalesEnquiryCloseActionTest : NonPersistentBusinessObjectTestCase
	{
		public void TestHasValidSelectedEnquiries()
		{
			var property = typeof(SalesEnquiryCloseAction).GetProperty("HasValidSelectedEnquiries", BindingFlags.Instance | BindingFlags.NonPublic);

			var deletedEnquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			var validEnquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			Factory.Save();

			var action = new SalesEnquiryCloseAction(Array.Empty<ZGuid>());
			AssertEquals("Unselected enquiry", false, property.GetMethod.Invoke(action, null));

			action = new SalesEnquiryCloseAction(new ZGuid[] { validEnquiry.PK });
			AssertEquals("Selected, valid enquiry", true, property.GetMethod.Invoke(action, null));

			deletedEnquiry.Delete();
			Factory.Save();

			action = new SalesEnquiryCloseAction(new ZGuid[] { deletedEnquiry.PK });
			AssertEquals("Selected, deleted enquiry", false, property.GetMethod.Invoke(action, null));
		}

		public void TestIsClosable()
		{
			var isCloseAllowed = Env.Security.InquiryManagerClose.IsAllowed;

			try
			{
				var deletedEnquiry = Factory.NewWithValidTestData<SalesEnquiry>();
				var validEnquiry = Factory.NewWithValidTestData<SalesEnquiry>();
				Factory.Save();

				var action = new SalesEnquiryCloseAction(Array.Empty<ZGuid>());
				Env.Security.InquiryManagerClose.IsAllowed = false;
				AssertEquals("Unselected enquiry. No security", false, action.IsClosable);
				Env.Security.InquiryManagerClose.IsAllowed = true;
				AssertEquals("Unselected enquiry. Has security", false, action.IsClosable);

				action = new SalesEnquiryCloseAction(new ZGuid[] { validEnquiry.PK });
				Env.Security.InquiryManagerClose.IsAllowed = false;
				AssertEquals("Selected, valid enquiry. No security", false, action.IsClosable);
				Env.Security.InquiryManagerClose.IsAllowed = true;
				AssertEquals("Selected, valid enquiry. Has security", true, action.IsClosable);

				deletedEnquiry.Delete();
				Factory.Save();

				action = new SalesEnquiryCloseAction(new ZGuid[] { deletedEnquiry.PK });
				Env.Security.InquiryManagerClose.IsAllowed = false;
				AssertEquals("Selected, deleted enquiry. No security", false, action.IsClosable);
				Env.Security.InquiryManagerClose.IsAllowed = true;
				AssertEquals("Selected, deleted enquiry. Has security", false, action.IsClosable);
			}
			finally
			{
				Env.Security.InquiryManagerClose.IsAllowed = isCloseAllowed;
			}
		}

		public void TestRunPreSaveValidationCore()
		{
			var action = new SalesEnquiryCloseAction(Array.Empty<ZGuid>());
			action.CloseReason = "";

			action.RunPreSaveValidation();

			AssertHasErrors("Reason is mandatory", action.CloseReasonInfo);
		}

		public void TestValidateCloseReason()
		{
			var enquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			Factory.Save();
			var action = new SalesEnquiryCloseAction(new ZGuid[] { enquiry.PK });
			action.CloseReason = "";
			action.Synchronise();
			AssertHasErrors("Reason is mandatory", action.CloseReasonInfo);

			SetSalesEnquiryFieldsMandatoryRegistry(OrgColdCallRegisterSchema.Constants.O1_CloseReason, false);
			action.CloseReason = "";
			action.Synchronise();
			AssertNoErrors("Reason is not mandatory", action.CloseReasonInfo);

			action.CloseReason = "NOP";
			action.Synchronise();
			AssertHasErrors("Reason is invalid", action.CloseReasonInfo);

			action.CloseReason = "WAT";
			action.Synchronise();
			AssertNoErrors("Reason is valid", action.CloseReasonInfo);

			action.CloseReason = "LOL";
			action.Synchronise();
			AssertHasErrors("Reason is inactive and thus invalid", action.CloseReasonInfo);
		}

		public void TestSynchronise()
		{
			var openEnquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			openEnquiry.O1_LeadStatus = SalesEnquiryStatusCodeList.Codes.Open;
			var convertedEnquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			convertedEnquiry.O1_LeadStatus = SalesEnquiryStatusCodeList.Codes.Converted;
			var closedEnquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			closedEnquiry.O1_LeadStatus = SalesEnquiryStatusCodeList.Codes.Closed;
			Factory.Save();

			var action = new SalesEnquiryCloseAction(Array.Empty<ZGuid>());
			action.Synchronise();
			AssertEquals(false, action.HasErrors);

			action = new SalesEnquiryCloseAction(new ZGuid[] { openEnquiry.PK, convertedEnquiry.PK, closedEnquiry.PK });
			action.Synchronise();
			AssertEquals(true, action.HasErrors);

			action.CloseReason = "WAT";
			action.Synchronise();
			AssertEquals(false, action.HasErrors);

			AssertEquals("Open Enquiry Status", SalesEnquiryStatusCodeList.Codes.Closed, openEnquiry.O1_LeadStatus);
			AssertEquals("Converted Enquiry Status", SalesEnquiryStatusCodeList.Codes.Converted, convertedEnquiry.O1_LeadStatus);
			AssertEquals("Closed Enquiry Status", SalesEnquiryStatusCodeList.Codes.Closed, closedEnquiry.O1_LeadStatus);

			AssertEquals("Open Enquiry Close Reason", action.CloseReason, openEnquiry.O1_CloseReason);
			AssertEquals("Converted Enquiry Close Reason", string.Empty, convertedEnquiry.O1_CloseReason);
			AssertEquals("Closed Enquiry Close Reason", action.CloseReason, closedEnquiry.O1_CloseReason);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new SalesEnquiryCloseAction(Array.Empty<ZGuid>());
		}

		protected override void SetUp()
		{
			base.SetUp();
			SetSalesEnquiryFieldsMandatoryRegistry(OrgColdCallRegisterSchema.Constants.O1_CloseReason, true);
			var collection = new CodeDescriptionBoolCollection(3);
			collection.Add("WAT", (NoResString)"Lacks information to be taken further", true);
			collection.Add("LOL", (NoResString)"Not a serious inquiry", false);
			OrganisationsDataRegistry.Instance.SalesEnquiryCloseReasonList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
		}

		void SetSalesEnquiryFieldsMandatoryRegistry(string field, bool value)
		{
			var mandatoryFieldsCollection = new CodeDescriptionBoolDisallowNewCollection();
			mandatoryFieldsCollection.Add(50, field, (NoResString)field, value);
			OrganisationsDataRegistry.Instance.SalesEnquiryFieldsMandatory.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, mandatoryFieldsCollection);
		}

		#endregion
	}
}
