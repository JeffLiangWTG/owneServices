using System;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Business.HelperClasses;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.QuotedBookings.Business.Test
{
	[TestedType(typeof(QuotedBooking))]
	public class QuotedBookingWorkflowProviderTest : WorkflowProviderTest<QuotedBooking, QuotedBookingProcessTaskCollection>
	{
		public void TestLogs_SpotQuote()
		{
			QuotedBooking quotedBooking = QuotedBooking.New(QuoteBookingType.SpotQuote, Factory);

			quotedBooking.Quote.Logs.AddNew(Events.Authorised);
			Factory.Save();

			BusinessObjectFactory factory = new BusinessObjectFactory();
			quotedBooking = QuotedBooking.New(quotedBooking.Quote.PK, ZGuid.Empty, factory);
			AssertContainsExactElementsInAnyOrder("should not contain quote logs", Array.Empty<StmALog>(), quotedBooking.Logs.GetAllLogs());

			quotedBooking.Logs.AddNew(Events.QuotationAccepted);
			factory.Save();

			factory = new BusinessObjectFactory();
			quotedBooking = QuotedBooking.New(quotedBooking.Quote.PK, ZGuid.Empty, factory);
			AssertContainsExactElementsInAnyOrder(quotedBooking.HumanReadableName + "logs",
					new[] { Events.QuotationAcceptedCode },
					quotedBooking.Logs.GetAllLogs().Cast<StmALog>().Select(log => log.SL_SE_NKEvent.ToString()));
		}

		public void TestLogs_Booking()
		{
			QuotedBooking quotedBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);

			quotedBooking.Booking.Logs.AddNew(Events.Authorised);
			Factory.Save();

			BusinessObjectFactory factory = new BusinessObjectFactory();
			quotedBooking = QuotedBooking.New(ZGuid.Empty, quotedBooking.Booking.PK, factory);
			AssertContainsExactElementsInAnyOrder("should not contain booking logs", Array.Empty<StmALog>(), quotedBooking.Logs.GetAllLogs());

			quotedBooking.Logs.AddNew(Events.Booked);
			factory.Save();

			factory = new BusinessObjectFactory();
			quotedBooking = QuotedBooking.New(ZGuid.Empty, quotedBooking.Booking.PK, factory);
			AssertContainsExactElementsInAnyOrder(quotedBooking.HumanReadableName + "logs",
					new[] { Events.BookedCode },
					quotedBooking.Logs.GetAllLogs().Cast<StmALog>().Select(log => log.SL_SE_NKEvent.ToString()));
		}

		public void TestLogs_QuotedBooking()
		{
			QuotedBooking quotedBooking = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);

			quotedBooking.Booking.Logs.AddNew(Events.Authorised);
			quotedBooking.Quote.Logs.AddNew(Events.Authorised);
			Factory.Save();

			BusinessObjectFactory factory = new BusinessObjectFactory();
			quotedBooking = QuotedBooking.New(quotedBooking.Quote.PK, quotedBooking.Booking.PK, factory);
			AssertContainsExactElementsInAnyOrder("should not contain quote and booking logs", Array.Empty<StmALog>(), quotedBooking.Logs.GetAllLogs());

			quotedBooking.Logs.AddNew(Events.Booked);
			factory.Save();

			factory = new BusinessObjectFactory();
			quotedBooking = QuotedBooking.New(quotedBooking.Quote.PK, quotedBooking.Booking.PK, factory);
			AssertContainsExactElementsInAnyOrder(quotedBooking.HumanReadableName + "logs",
					new[] { Events.BookedCode },
					quotedBooking.Logs.GetAllLogs().Cast<StmALog>().Select(log => log.SL_SE_NKEvent.ToString()));
		}

		public void TestDoNotApplyTemplatesMultipleTimes()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = "GB";
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = company.PK;
			branch.GB_RN_NKCountryCode = "GB";

			var consolTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			consolTemplate.P0_ProcessType = "CON";
			consolTemplate.GlobalTemplate = true;
			MakeMilestone(consolTemplate, "\"<CompanyCountryCode>\"==\"GB\"");
			MakeMilestone(consolTemplate);
			MakeTask(consolTemplate);
			MakeTask(consolTemplate, "\"<CompanyCountryCode>\"==\"GB\"");

			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "SHP";
			template.GlobalTemplate = true;
			MakeMilestone(template, "\"<CompanyCountryCode>\"==\"GB\"");
			MakeMilestone(template);
			MakeTask(template);
			MakeTask(template, "\"<CompanyCountryCode>\"==\"GB\"");

			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			consol.IsRoot = true;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_UniqueConsignRef = "S1010";

			var bookingShipment = Factory.Load<BookingShipment>(shipment.PK);
			AssertEquals(0, bookingShipment.WorkflowItems.Milestones.Count);

			Factory.Save();
			var getQuery = new Func<ZGuid, ZString, ZQuery>((pk, type) => new ZQuery(new ZQuery(ProcessTasksSchema.P9_ParentID, pk), new ZQuery(ProcessTasksSchema.P9_Type, type)));

			AssertEquals(1, Factory.Load<ProcessTask>(getQuery(consol.PK, "MIL")).Length);
			AssertEquals(1, Factory.Load<ProcessTask>(getQuery(shipment.PK, "MIL")).Length);
			AssertEquals(1, Factory.Load<ProcessTask>(getQuery(consol.PK, "UDF")).Length);
			AssertEquals(1, Factory.Load<ProcessTask>(getQuery(shipment.PK, "UDF")).Length);

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				Factory.Save();
				consol.ApplyWorkflowTemplates();
				shipment.ApplyWorkflowTemplates();
				bookingShipment.ApplyWorkflowTemplates();

				AssertEquals(2, Factory.Load<ProcessTask>(getQuery(consol.PK, "MIL")).Length);
				AssertEquals(2, Factory.Load<ProcessTask>(getQuery(shipment.PK, "MIL")).Length);
				AssertEquals(2, Factory.Load<ProcessTask>(getQuery(consol.PK, "UDF")).Length);
				AssertEquals(2, Factory.Load<ProcessTask>(getQuery(shipment.PK, "UDF")).Length);
			}
		}

		public class BookingShipment : ForwardingShipment
		{
			public BookingShipment(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override string GetTemplateCategory()
			{
				return MailTemplateCategoryList.Codes.Bookings;
			}
		}

		ProcessTask MakeMilestone(ProcessTaskTemplate template, string udf = null)
		{
			var milestone = template.WorkflowItems.Milestones.AddNew();
			milestone.P9_Description = "Nibby";
			milestone.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;
			if (udf != null)
			{
				milestone.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;
				milestone.TemplateConditions.TemplateCondition2Value = udf;
			}
			return milestone;
		}

		ProcessTask MakeTask(ProcessTaskTemplate template, string udf = null)
		{
			var task = template.WorkflowItems.Tasks.AddNew();
			task.P9_Type = "UDF";
			task.P9_Description = "Nibby";
			if (udf != null)
			{
				task.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;
				task.TemplateConditions.TemplateCondition2Value = udf;
			}
			return task;
		}

		public void TestTemplateTasksAreNotCreatedForWrappedShipmentAndQuote()
		{
			string shipmentWorkflowType = ((IWorkflowProvider)Factory.New<ForwardingShipment>()).WorkflowType;
			string quoteWorkflowType = ((IWorkflowProvider)Factory.New<Quote>()).WorkflowType;

			Action<string, string> createTemplateMilestone = (workflowType, eventCode) =>
			{
				ProcessTaskTemplate template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
				template.P0_ProcessType = workflowType;

				ProcessTask milestone = template.WorkflowItems.Milestones.AddNew();
				milestone.TriggerConditions.TriggerEventCode = eventCode;
			};

			createTemplateMilestone(shipmentWorkflowType, "AAA");
			createTemplateMilestone(quoteWorkflowType, "BBB");
			createTemplateMilestone(ExpectedWorkflowType, "CCC");

			Factory.Save();

			QuotedBooking quotedBooking = GetNewBusinessObject(Factory);
			Factory.Save();

			var quotedBookingMilestones = ((IWorkflowProvider)quotedBooking).WorkflowItems.Milestones;
			AssertEquals("Only one milestone added from templates", 1, quotedBookingMilestones.Count);
			AssertEquals("QuotedBooking milestone", "CCC", quotedBookingMilestones[0].P9_SE_NKMilestoneEvent);
		}

		public void TestTemplateSelectionCriteria()
		{
			var client = Factory.New<OrgHeader>();

			var quotedBooking = GetNewBusinessObject(Factory);
			quotedBooking.ClientPK = client.PK;
			quotedBooking.Mode = Core.Constants.ContainerModes.LCL;
			quotedBooking.Origin = "AUSYD";
			quotedBooking.Destination = "AUBNE";

			ColumnValueRanker ranker = (quotedBooking as IWorkflowProviderCore).GetTemplateSelectionCriteria() as ColumnValueRanker;

			AssertContainsExactElementsInAnyOrder(new ZGuid[] { quotedBooking.Job.JH_GB, ZGuid.Empty },
					ranker.GetValues(ProcessTaskTemplateSchema.P0_GB));

			AssertContainsExactElementsInAnyOrder(new ZGuid[] { quotedBooking.Job.JH_GE, ZGuid.Empty },
					ranker.GetValues(ProcessTaskTemplateSchema.P0_GE));

			AssertContainsExactElementsInAnyOrder(new ZString[] { "SEA", "OTH", ZString.Empty },
					ranker.GetValues(ProcessTaskTemplateSchema.P0_SubType1));

			AssertContainsExactElementsInAnyOrder(new ZString[] { QuotedBooking.QuickBookingCode, ZString.Empty },
					ranker.GetValues(ProcessTaskTemplateSchema.P0_SubType2));

			AssertContainsExactElementsInAnyOrder(new ZString[] { DirectionsContext.Domestic, ZString.Empty },
					ranker.GetValues(ProcessTaskTemplateSchema.P0_SubType3));

			AssertContainsExactElementsInAnyOrder(new ZString[] { "AUSYD", "AU", ZString.Empty },
					ranker.GetValues(ProcessTaskTemplateSchema.P0_LoadPortCountry));

			AssertContainsExactElementsInAnyOrder(new ZString[] { "AUBNE", "AU", ZString.Empty },
					ranker.GetValues(ProcessTaskTemplateSchema.P0_DischargePortCountry));

			AssertContainsExactElementsInAnyOrder(new ZGuid[] { client.PK, ZGuid.Empty },
					ranker.GetValues(ProcessTaskTemplateSchema.P0_OH_Client));
		}

		public void TestTemplateSelectionCriteria_Client()
		{
			var client = Factory.NewWithValidTestData<OrgHeader>();
			var controllingCustomer = Factory.NewWithValidTestData<OrgHeader>();

			var quotedBooking = GetNewBusinessObject(Factory);
			quotedBooking.ClientPK = client.PK;
			quotedBooking.ControllingCustomerDocumentaryAddress.OrganisationPK = controllingCustomer.PK;

			var ranker = (quotedBooking as IWorkflowProviderCore).GetTemplateSelectionCriteria() as ColumnValueRanker;
			AssertContainsExactElementsInAnyOrder(new ZGuid[] { client.PK, ZGuid.Empty },
				ranker.GetValues(ProcessTaskTemplateSchema.P0_OH_Client));

			var clientInTemplateSelectionCriteriaCollection = (ClientInTemplateSelectionCriteriaCollection)WorkflowDataRegistry.Instance.ClientInTemplateSelection.Value.Clone(null, null);
			var clientInTemplateSelectionCriteria = clientInTemplateSelectionCriteriaCollection.GetValueByCode("QBK");
			clientInTemplateSelectionCriteria.SelectedItems.Add(clientInTemplateSelectionCriteria.AvailableItems.GetValueByCode("CPY"));
			WorkflowDataRegistry.Instance.ClientInTemplateSelection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, clientInTemplateSelectionCriteriaCollection);
			Factory.Save();

			ranker = (quotedBooking as IWorkflowProviderCore).GetTemplateSelectionCriteria() as ColumnValueRanker;
			AssertContainsExactElementsInAnyOrder(new ZGuid[] { client.PK, controllingCustomer.PK, ZGuid.Empty },
				ranker.GetValues(ProcessTaskTemplateSchema.P0_OH_Client));
		}

		public void TestTemplateSelectionCriteriaContainerCode()
		{
			var quotedBooking = GetNewBusinessObject(Factory);

			Action<string> assertContainerCode = (expected) =>
			{
				ColumnValueRanker ranker = (quotedBooking as IWorkflowProviderCore).GetTemplateSelectionCriteria() as ColumnValueRanker;
				AssertEquals(expected, ranker.GetValues(ProcessTaskTemplateSchema.P0_SubType1)[1]);
			};

			quotedBooking.Mode = Core.Constants.ContainerModes.LCL;
			assertContainerCode(Core.Constants.ContainerModes.Other);

			quotedBooking.Mode = Core.Constants.ContainerModes.Loose;
			assertContainerCode(Core.Constants.ContainerModes.Other);

			quotedBooking.Mode = Core.Constants.ContainerModes.FTL;
			assertContainerCode(Core.Constants.ContainerModes.Other);

			quotedBooking.Mode = Core.Constants.ContainerModes.FCL;
			assertContainerCode(Core.Constants.ContainerModes.FCL);

			quotedBooking.TransportMode = Core.Constants.TransportModes.Road;
			quotedBooking.ContainerMode = Core.Constants.ContainerModes.LTL;
			assertContainerCode(Core.Constants.ContainerModes.Other);
		}

		public void TestUpdateProcessTaskTemplate()
		{
			ProcessTaskTemplate taskTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();

			AssertEquals("prerequisite", ZString.Empty, taskTemplate.P0_SubType1);
			AssertEquals("prerequisite", ZString.Empty, taskTemplate.P0_SubType2);
			AssertEquals("prerequisite", ZString.Empty, taskTemplate.P0_SubType3);

			QuotedBooking quotedBooking = GetNewBusinessObject(Factory);
			quotedBooking.TransportMode = Core.Constants.TransportModes.Air;
			quotedBooking.Origin = "AUSYD";
			quotedBooking.Destination = "NZAKL";

			((IProcessTaskTemplateUpdatable)quotedBooking).Update(taskTemplate);

			AssertEquals("AIR", taskTemplate.P0_SubType1);
			AssertEquals("QBN", taskTemplate.P0_SubType2);
			AssertEquals("EXP", taskTemplate.P0_SubType3);
		}

		#region Implementation

		protected override ZString ExpectedWorkflowType
		{
			get { return WorkflowDescriptors.QuotedBookingWorkflowDescriptorCode; }
		}

		protected override QuotedBooking GetNewBusinessObject(BusinessObjectFactory factory)
		{
			ForwardingShipment booking = QuotedBooking.CreateNewBooking(factory);
			JobHeader job = new JobHeader.Loader(booking).TryCreate();
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			QuotedBooking quotedBooking = QuotedBooking.New(ZGuid.Empty, booking.PK, factory);

			return quotedBooking;
		}

		protected override IWorkflowProvider ReloadWorkflowProvider(BusinessObjectFactory factory, QuotedBooking workFlowProvider)
		{
			return QuotedBooking.New(ZGuid.Empty, workFlowProvider.Booking.PK, factory);
		}

		#endregion
	}
}
