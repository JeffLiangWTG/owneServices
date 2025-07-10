using System;
using System.Windows.Forms;
using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(SalesEnquiryModule))]
	sealed class SalesEnquiryModuleTest : ZModuleBasherTest
	{
		public void TestBusinessContexts()
		{
			using (SalesEnquiryModule module = new SalesEnquiryModule())
			{
				AssertEquals("Only one business context should be returned", 1, module.BusinessContexts.Length);
				AssertEquals("Organisation business context should be returned", BusinessContext.SalesEnquiry, module.BusinessContexts[0]);
				Assert("Business context array should be returned", module.BusinessContexts is BusinessContext[]);
			}
		}

		public void TestOnClose()
		{
			var collection = new CodeDescriptionBoolCollection(3);
			collection.Add("WAT", (NoResString)"Lacks information to be taken further", true);
			OrganisationsDataRegistry.Instance.SalesEnquiryCloseReasonList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			var enquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			Factory.Save();

			var isCloseAllowed = Env.Security.InquiryManagerClose.IsAllowed;

			try
			{
				using (var module = new SalesEnquiryModuleForTesting())
				{
					Env.Security.InquiryManagerClose.IsAllowed = false;
					module.Close();
					AssertNotNull(UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals(Env.Security.InquiryManagerClose.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessages();

					Env.Security.InquiryManagerClose.IsAllowed = true;
					module.SetGridSelectedElements(new ZGuid[] { enquiry.PK });
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.None;
					module.Close();
					AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessages();

					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					module.Close();
					AssertNotNull(UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("Selected open Inquiries are closed.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
			finally
			{
				Env.Security.InquiryManagerClose.IsAllowed = isCloseAllowed;
			}
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.SalesEnquiry;
		}

		class SalesEnquiryModuleForTesting : SalesEnquiryModule
		{
			public void Close()
			{
				base.OnClose(null, EventArgs.Empty);
			}

			public void SetGridSelectedElements(ZGuid[] elements)
			{
				Elements = elements;
			}
			ZGuid[] Elements;

			protected override ZGuid[] GridSelectedElements
			{
				get { return Elements; }
			}
		}
	}
}
