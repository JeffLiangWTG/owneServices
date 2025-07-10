using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(SalesEnquiryCloseForm))]
	sealed class SalesEnquiryCloseFormBasherTest : ZFormBasherTest
	{
		[RequiresSTA]
		public void TestFormVerb()
		{
			using (var form = GetFormToBashCore() as ZForm)
			{
				AssertEquals(string.Empty, form.FormVerb);
			}
		}

		public void TestDefaultButtons()
		{
			using (var form = GetFormToBashCore() as ZForm)
			{
				AssertEquals("CloseButton", ((ZButton)form.AcceptButton).Name);
				AssertEquals("CancelButtonX", ((ZButton)form.CancelButton).Name);
			}
		}

		public void TestCloseButton()
		{
			var enquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			Factory.Save();
			var action = new SalesEnquiryCloseAction(new ZGuid[] { enquiry.PK });
			using (var form = new SalesEnquiryCloseForm(action))
			{
				form.Show();

				SetSalesEnquiryFieldsMandatoryRegistry(OrgColdCallRegisterSchema.Constants.O1_CloseReason, true);
				AssertEquals("Precondition", true, action.IsClosable);
				form.AcceptButton.PerformClick();
				AssertNotNull(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(DialogResult.None, form.DialogResult);

				UnitTestUserNotification.Instance.ClearMessages();

				SetSalesEnquiryFieldsMandatoryRegistry(OrgColdCallRegisterSchema.Constants.O1_CloseReason, false);
				AssertEquals("Precondition", true, action.IsClosable);
				form.AcceptButton.PerformClick();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(DialogResult.OK, form.DialogResult);
			}
		}

		public void TestCancelButton()
		{
			using (var form = GetFormToBashCore() as ZForm)
			{
				form.Show();
				form.CancelButton.PerformClick();
				AssertEquals(DialogResult.Cancel, form.DialogResult);
			}
		}

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			var action = new SalesEnquiryCloseAction(Array.Empty<ZGuid>());
			return new SalesEnquiryCloseForm(action);
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
