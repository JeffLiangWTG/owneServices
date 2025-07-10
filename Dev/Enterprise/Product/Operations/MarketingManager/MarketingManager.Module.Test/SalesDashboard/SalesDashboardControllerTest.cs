using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Module.Testing
{
	[TestedType(typeof(SalesDashboardController))]
	public class SalesDashboardControllerTest : ZControllerBasherTest
	{
		#region TestViewForm

		public override void TestViewForm()
		{
			Assert("TestViewForm needs to be tested for each SalesDashboardActivity type instead of just one", true);
		}

		[SnailTest]
		public void TestViewForm_OpportunitySalesDashboardActivity()
		{
			AssertControllerNotNull();
			try
			{
				var form = Controller.ShowViewForm(GetOpportunityActivityThatIsInTheDatabase());
				AssertNotNull(form);
				AssertEquals(ControllerIDs.Opportunity, form.ControllerID);
				AssertEquals(ODisplayMode.ReadOnly, form.DisplayMode);
			}
			catch (ModuleFeatureNotSupportedException)
			{
			}
		}

		[SnailTest]
		public void TestViewForm_InquiryActivity()
		{
			AssertControllerNotNull();
			try
			{
				var form = Controller.ShowViewForm(GetInquiryActivityThatIsInTheDatabase());
				AssertNotNull(form);
				AssertEquals(ControllerIDs.SalesEnquiry, form.ControllerID);
				AssertEquals(ODisplayMode.ReadOnly, form.DisplayMode);
			}
			catch (ModuleFeatureNotSupportedException)
			{
			}
		}

		[SnailTest]
		public void TestViewForm_CommunicationActivity()
		{
			AssertControllerNotNull();
			try
			{
				var form = Controller.ShowViewForm(GetCommunicationActivityThatIsInTheDatabase());
				AssertNotNull(form);
				AssertEquals(ControllerIDs.Communication, form.ControllerID);
				AssertEquals(ODisplayMode.ReadOnly, form.DisplayMode);
			}
			catch (ModuleFeatureNotSupportedException)
			{
			}
		}

		[SnailTest]
		public void TestViewForm_CampaignActivity()
		{
			AssertControllerNotNull();
			try
			{
				var form = Controller.ShowViewForm(GetCampaignActivityThatIsInTheDatabase(false));
				AssertNotNull(form);
				AssertEquals(ControllerIDs.GlbCompanyCampaign, form.ControllerID);
				AssertEquals(ODisplayMode.ReadOnly, form.DisplayMode);
			}
			catch (ModuleFeatureNotSupportedException)
			{
			}
		}

		[SnailTest]
		public void TestViewForm_OneOffQuoteActivity()
		{
			AssertControllerNotNull();
			try
			{
				var form = Controller.ShowViewForm(GetOneOffQuoteActivityThatIsInTheDatabase());
				AssertNotNull(form);
				AssertEquals(ControllerIDs.OneOffQuotes, form.ControllerID);
				AssertEquals(ODisplayMode.ReadOnly, form.DisplayMode);
			}
			catch (ModuleFeatureNotSupportedException)
			{
			}
		}

		[SnailTest]
		public void TestViewForm_QuotationActivity()
		{
			AssertControllerNotNull();
			try
			{
				var form = Controller.ShowViewForm(GetQuotationActivityThatIsInTheDatabase());
				AssertNotNull(form);
				AssertEquals(ControllerIDs.Quotations, form.ControllerID);
				AssertEquals(ODisplayMode.ReadOnly, form.DisplayMode);
			}
			catch (ModuleFeatureNotSupportedException)
			{
			}
		}

		[SnailTest]
		public void TestViewForm_ProjectActivity()
		{
			AssertControllerNotNull();
			try
			{
				var form = Controller.ShowViewForm(GetProjectActivityThatIsInTheDatabase());
				AssertNotNull(form);
				AssertEquals(ControllerIDs.Project, form.ControllerID);
				AssertEquals(ODisplayMode.ReadOnly, form.DisplayMode);
			}
			catch (ModuleFeatureNotSupportedException)
			{
			}
		}

		public void TestViewForm_WhenOpportunityHasBeenDeleted_ShouldReturnNull()
		{
			Assert_WhenOpportunityHasBeenDeleted_ShouldReturnNull(Controller.ShowViewForm);
		}
		#endregion

		#region TestEditForm

		public override void TestEditForm()
		{
			Assert("TestEditForm needs to be tested for each SalesDashboardActivity type instead of just one", true);
		}

		[SnailTest]
		public void TestEditForm_OpportunitySalesDashboardActivity()
		{
			AssertControllerNotNull();
			IZForm testForm = null;
			try
			{
				testForm = Controller.ShowEditForm(GetOpportunityActivityThatIsInTheDatabase());
				AssertEquals(ControllerIDs.Opportunity, testForm.ControllerID);
				AssertEquals(ODisplayMode.Browse, testForm.DisplayMode);
			}
			catch (ModuleFeatureNotSupportedException)
			{
			}
			finally
			{
				if (testForm != null)
				{ ((ZForm)testForm).Close(); }
			}
		}

		[SnailTest]
		public void TestEditForm_InquiryActivity()
		{
			AssertControllerNotNull();
			IZForm testForm = null;
			try
			{
				testForm = Controller.ShowEditForm(GetInquiryActivityThatIsInTheDatabase());
				AssertEquals(ControllerIDs.SalesEnquiry, testForm.ControllerID);
				AssertEquals(ODisplayMode.Browse, testForm.DisplayMode);
			}
			catch (ModuleFeatureNotSupportedException)
			{
			}
			finally
			{
				if (testForm != null)
				{ ((ZForm)testForm).Close(); }
			}
		}

		[SnailTest]
		public void TestEditForm_CommunicationActivity()
		{
			AssertControllerNotNull();
			IZForm testForm = null;
			try
			{
				testForm = Controller.ShowEditForm(GetCommunicationActivityThatIsInTheDatabase());
				AssertEquals(ControllerIDs.Communication, testForm.ControllerID);
				AssertEquals(ODisplayMode.Browse, testForm.DisplayMode);
			}
			catch (ModuleFeatureNotSupportedException)
			{
			}
			finally
			{
				if (testForm != null)
				{ ((ZForm)testForm).Close(); }
			}
		}

		[SnailTest]
		public void TestEditForm_CampaignActivity()
		{
			AssertControllerNotNull();
			IZForm testForm = null;
			try
			{
				testForm = Controller.ShowEditForm(GetCampaignActivityThatIsInTheDatabase(false));
				AssertEquals(ControllerIDs.GlbCompanyCampaign, testForm.ControllerID);
				AssertEquals(ODisplayMode.Browse, testForm.DisplayMode);
			}
			catch (ModuleFeatureNotSupportedException)
			{
			}
			finally
			{
				((ZForm)testForm)?.Close();
			}
		}

		[SnailTest]
		public void TestEditForm_OneOffQuoteActivity()
		{
			AssertControllerNotNull();
			IZForm testForm = null;
			try
			{
				testForm = Controller.ShowEditForm(GetOneOffQuoteActivityThatIsInTheDatabase());
				AssertEquals(ControllerIDs.OneOffQuotes, testForm.ControllerID);
				AssertEquals(ODisplayMode.Browse, testForm.DisplayMode);
			}
			catch (ModuleFeatureNotSupportedException)
			{
			}
			finally
			{
				((ZForm)testForm)?.Close();
			}
		}

		[SnailTest]
		public void TestEditForm_QuotationActivity()
		{
			AssertControllerNotNull();
			IZForm testForm = null;
			try
			{
				testForm = Controller.ShowEditForm(GetQuotationActivityThatIsInTheDatabase());
				AssertEquals(ControllerIDs.Quotations, testForm.ControllerID);
				AssertEquals(ODisplayMode.Browse, testForm.DisplayMode);
			}
			catch (ModuleFeatureNotSupportedException)
			{
			}
			finally
			{
				((ZForm)testForm)?.Close();
			}
		}

		[SnailTest]
		public void TestEditForm_ProjectActivity()
		{
			AssertControllerNotNull();
			IZForm testForm = null;
			try
			{
				testForm = Controller.ShowEditForm(GetProjectActivityThatIsInTheDatabase());
				AssertEquals(ControllerIDs.Project, testForm.ControllerID);
				AssertEquals(ODisplayMode.Browse, testForm.DisplayMode);
			}
			catch (ModuleFeatureNotSupportedException)
			{
			}
			finally
			{
				((ZForm)testForm)?.Close();
			}
		}
		public void TestEditForm_WhenOpportunityHasBeenDeleted_ShouldReturnNull()
		{
			Assert_WhenOpportunityHasBeenDeleted_ShouldReturnNull(Controller.ShowEditForm);
		}

		#endregion

		public override void TestNewForm()
		{
			AssertControllerNotNull();
			try
			{
				AssertEquals("Should not show new form", null, Controller.ShowNewForm());
			}
			catch (ModuleFeatureNotSupportedException)
			{
			}
		}

		public override void TestDeleteForm()
		{
			AssertControllerNotNull();
			try
			{
				AssertEquals("Should not show delete form", null, Controller.ShowDeleteForm(null));
			}
			catch (ModuleFeatureNotSupportedException)
			{
			}
		}

		public override void TestTemplateCopyForm()
		{
			AssertControllerNotNull();
			try
			{
				AssertEquals("Should not show template copy form", null, Controller.ShowTemplateCopyForm(null));
			}
			catch (ModuleFeatureNotSupportedException)
			{
			}
		}

		#region Implementation

		protected override Type GetBusinessObjectType()
		{
			return typeof(SalesDashboardActivity);
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.SalesDashboard;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			throw new NotSupportedException("This controller can contain many different business objects. All tests that use this method should be overrided, and tested for each business object type");
		}

		OpportunitySalesDashboardActivity GetOpportunityActivityThatIsInTheDatabase()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var opportunity = org.SalesOpportunities.AddNew();
			var task = opportunity.WorkflowItems.AddNew();
			task.P9_Status = "WRK";
			Factory.Save();

			return Factory.LoadTop1<OpportunitySalesDashboardActivity>(new ZQuery());
		}

		InquirySalesDashboardActivity GetInquiryActivityThatIsInTheDatabase()
		{
			var inquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			var task = inquiry.WorkflowItems.AddNew();
			task.P9_Status = "WRK";
			Factory.Save();

			return Factory.LoadTop1<InquirySalesDashboardActivity>(new ZQuery());
		}

		CommunicationSalesDashboardActivity GetCommunicationActivityThatIsInTheDatabase()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.SalesCalls.AddNew();
			Factory.Save();

			return Factory.LoadTop1<CommunicationSalesDashboardActivity>(new ZQuery());
		}

		CampaignSalesDashboardActivity GetCampaignActivityThatIsInTheDatabase(bool orgColdCallRegister)
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var item = Factory.NewWithValidTestData<GlbCompanyCampaignItem>();
			if (orgColdCallRegister)
			{
				var register = Factory.NewWithValidTestData<OrgColdCallRegister>();
				register.O1_OC_LinkedContact = contact.PK;
				item.G8_RecipientID = register.PK;
				item.G8_RecipientTableCode = "O1";
			}
			else
			{
				item.G8_RecipientID = contact.PK;
				item.G8_RecipientTableCode = "OC";
			}

			Factory.Save();

			return Factory.LoadTop1<CampaignSalesDashboardActivity>(new ZQuery());
		}

		OneOffQuoteSalesDashboardActivity GetOneOffQuoteActivityThatIsInTheDatabase()
		{
			var quotedBookingBuilder = ObjectFactory.Get<Freight.Integration.QuotedBooking.IQuotedBookingBuilder>();
			var quotedBooking = quotedBookingBuilder.CreateNew(Freight.Integration.QuoteBookingType.BookingWithQuote, Factory);
			quotedBooking.TransportMode = Core.Constants.TransportModes.Sea;
			quotedBooking.ForwardingShipment[JobShipmentSchema.JS_RL_NKOrigin] = "AUSYD";
			quotedBooking.ForwardingShipment[JobShipmentSchema.JS_RL_NKDestination] = "NZAKL";

			Factory.Save();

			return Factory.LoadTop1<OneOffQuoteSalesDashboardActivity>(new ZQuery());
		}

		QuotationSalesDashboardActivity GetQuotationActivityThatIsInTheDatabase()
		{
			Factory.NewWithValidTestData<Quote>();

			Factory.Save();

			return Factory.LoadTop1<QuotationSalesDashboardActivity>(new ZQuery());
		}

		ProjectSalesDashboardActivity GetProjectActivityThatIsInTheDatabase()
		{
			Factory.NewWithValidTestData(ObjectFactory.GetType<ProcessManagement.Integration.IProject>());

			Factory.Save();

			return Factory.LoadTop1<ProjectSalesDashboardActivity>(new ZQuery());
		}

		void Assert_WhenOpportunityHasBeenDeleted_ShouldReturnNull(Func<BusinessObject, IZForm> showAction)
		{
			AssertControllerNotNull();

			var anotherFactory =
				new BusinessObjectFactory();

			var opportunityInAnotherFactory = anotherFactory.NewWithValidTestData<OrgOpportunity>();
			anotherFactory.Save();

			var salesActivity = Factory.LoadTop1<OpportunitySalesDashboardActivity>(
				new ZQuery(ViewSalesDashboardActivitySchema.VSA_ParentId, opportunityInAnotherFactory.PK));
			opportunityInAnotherFactory.Delete();
			anotherFactory.Save();

			var testForm = showAction(salesActivity);
			AssertNull(testForm);
			AssertEquals("The selected record has been deleted by another user. It cannot be displayed.", UnitTestUserNotification.Instance.LastMessage.Text);
		}
		#endregion
	}
}
