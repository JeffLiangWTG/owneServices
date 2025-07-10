using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.EConversation.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(GroupedEConversation))]
	sealed class GroupedEConversationTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => new GroupedEConversation(Factory.NewWithValidTestData<HRJobApplication>(), "Candidate created");
		public void TestDefaultMessageInRootConversation()
		{
			var borris = AutomatedRejectionTestHelper.CreateApplication(Factory, "Borris Johnson");
			Factory.Save();

			AssertEquals("Candidate created", borris.EConversation.GetTimeOrderedMessages().LastOrDefault().Body);
		}

		[TestDate(2017, 11, 10, 12, 0, 0)]
		public void TestDefaultMessageInRootConversationCreatedOnlyOnce()
		{
			var otherFactory = new BusinessObjectFactory();
			var otherApplication = AutomatedRejectionTestHelper.CreateApplication(otherFactory, "John Smith");
			_ = otherApplication.EConversation; //Simulate control binding

			TestDateAttribute.Date = new DateTime(2017, 11, 10, 12, 1, 0);
			var otherMessage = otherApplication.EConversation.RootConversation.AddMessageFromCurrentUser("Root Message1", true, false);
			otherFactory.Save();

			TestDateAttribute.Date = new DateTime(2017, 11, 10, 12, 2, 0);

			_ = otherApplication.EConversation; //Simulate control binding
			var convo = AutomatedRejectionTestHelper.CreateConversation(otherApplication, "borris@gmail.com", "recruiter1@wisetech.com");
			var message = convo.AddMessageFromCurrentUser("Normal Message1", true, false);
			Factory.Save();

			AssertEquals(1, otherApplication.EConversation.GetTimeOrderedMessages().Count(c => c.Body.Contains("Candidate created")));
		}

		[TestDate(2017, 11, 10, 11, 0, 0)]
		public void TestCreateRootConversationWitePreexistingConversation()
		{
			var application = AutomatedRejectionTestHelper.CreateApplication(Factory, "Borris Johnson");
			Factory.Save();

			var testConversationDate = new ZDateTime(2017, 11, 10, 12, 0, 0);
			var rootConversationDate = new ZDateTime(2017, 11, 10, 12, 3, 0);

			TestDateAttribute.Date = testConversationDate.ToDateTime();
			var convo = AutomatedRejectionTestHelper.CreateConversation(application, "borris@gmail.com", "recruiter1@wisetech.com");
			convo.AddMessageFromCurrentUser("message1", true, false);
			AssertEquals(1, GetApplicationConversations(application).Length);

			TestDateAttribute.Date = rootConversationDate.ToDateTime();
			_ = application.EConversation; //Simulate control binding
			AssertEquals(2, GetApplicationConversations(application).Length);
			AssertEquals(2, application.EConversation.GetTimeOrderedMessages().Count);

			var rootMessage = application.EConversation.GetTimeOrderedMessages().First();
			var testMessage = application.EConversation.GetTimeOrderedMessages().Last();
			AssertEquals("Candidate created", rootMessage.Body);
			AssertEquals(rootConversationDate, rootMessage.SendLocalDateTime);

			AssertEquals("message1", testMessage.Body);
			AssertEquals(testConversationDate, testMessage.SendLocalDateTime);
		}

		public void TestRootConversation()
		{
			var candidate = AutomatedRejectionTestHelper.CreateApplication(Factory, "Borris Johnson");
			Factory.Save();

			var rootConversation = candidate.EConversation.RootConversation;
			rootConversation.AddMessageFromCurrentUser("TestMessage", true, false);
			Assert(rootConversation.GetTimeOrderedMessages().Any(r => r.Body == "Candidate created"));
			AssertEquals(2, rootConversation.Messages.Count);
		}

		public void TestAddConverations()
		{
			var candidate = AutomatedRejectionTestHelper.CreateApplication(Factory, "Borris Johnson");
			Factory.Save();

			var groupedEConvo = candidate.EConversation;
			var rootConversation = groupedEConvo.RootConversation;

			rootConversation.AddMessageFromCurrentUser("TestMessage", true, false);

			AssertEquals(1, groupedEConvo.Conversations.Count);
			AssertEquals(2, rootConversation.Messages.Count);

			var convo1 = AutomatedRejectionTestHelper.CreateConversation(candidate, "borris@gmail.com", "recruiter1@wisetech.com");
			convo1.AddMessageFromCurrentUser("message1", true, false);

			AssertEquals(2, groupedEConvo.Conversations.Count);
			AssertEquals(1, convo1.Messages.Count);
		}

		[TestDate(2017, 11, 10, 12, 0, 0)]
		public void TestGetTimeOrderedMessages()
		{
			var candidate = AutomatedRejectionTestHelper.CreateApplication(Factory, "Borris Johnson");
			_ = candidate.EConversation; //Simulate Control binding
			Factory.Save();

			var convo1 = AutomatedRejectionTestHelper.CreateConversation(candidate, "borris@gmail.com", "recruiter1@wisetech.com");
			TestDateAttribute.Date = new DateTime(2017, 11, 10, 12, 1, 0);
			var message1 = convo1.AddMessageFromCurrentUser("message1", true, false);
			TestDateAttribute.Date = new DateTime(2017, 11, 10, 12, 3, 0);
			var message2 = convo1.AddMessageFromCurrentUser("message2", true, false);
			TestDateAttribute.Date = new DateTime(2017, 11, 10, 12, 5, 0);
			var message3 = convo1.AddMessageFromCurrentUser("message3", true, false);

			var convo2 = AutomatedRejectionTestHelper.CreateConversation(candidate, "borris@gmail.com", "recruiter2@wisetech.com");
			TestDateAttribute.Date = new DateTime(2017, 11, 10, 12, 2, 0);
			var message4 = convo2.AddMessageFromCurrentUser("message4", true, false);
			TestDateAttribute.Date = new DateTime(2017, 11, 10, 12, 4, 0);
			var message5 = convo2.AddMessageFromCurrentUser("message5", true, false);
			TestDateAttribute.Date = new DateTime(2017, 11, 10, 12, 6, 0);
			var message6 = convo2.AddMessageFromCurrentUser("message6", true, false);

			var messages = candidate.EConversation.GetTimeOrderedMessages();
			AssertEquals(0, messages.IndexOf(message6));
			AssertEquals(1, messages.IndexOf(message3));
			AssertEquals(2, messages.IndexOf(message5));
			AssertEquals(3, messages.IndexOf(message2));
			AssertEquals(4, messages.IndexOf(message4));
			AssertEquals(5, messages.IndexOf(message1));
			AssertEquals("Candidate created", messages[6].Body);
		}

		JobConversation[] GetApplicationConversations(HRJobApplication application) => Factory.Load<JobConversation>(new ZQuery(JobConversationSchema.JCC_ParentID, application.PK));
	}
}
