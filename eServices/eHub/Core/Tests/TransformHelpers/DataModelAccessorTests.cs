using System;
using System.Data;
using System.Linq;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.DataModel.Common;
using CargoWise.eHub.DataModel.eHubTransactions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Core.Tests
{
	[TestClass]
	public class DataModelAccessorTests
	{
		[TestMethod]
		public void DataModelAccessor_GetClientRegistrationCode()
		{
			var mockContext = MockRepository.GenerateMock<eHubTransactionsContext>();
			var mockContextFactory = MockRepository.GenerateMock<ContextFactory<eHubTransactionsContext>>();
			mockContextFactory.Stub(x => x.CreateContext()).Return(mockContext);
			var testClientRegistrations = new TestDbSet<eHubClientRegistration>();
			mockContext.Stub(x => x.eHubClientRegistrations).Return(testClientRegistrations);

			testClientRegistrations.Add(new eHubClientRegistration
			{
				eHubClient = new eHubClient { CC_ID = "CLIENT0001" },
				eHubRegistrationType = new eHubRegistrationType { RT_ID = "REG001" },
				CX_Code = "AAAA"
			});

			var dataModelAccessor = new DataModelAccessor(mockContextFactory);

			string result = dataModelAccessor.GetClientRegistrationCode("CLIENT0001", "", "REG001");

			Assert.AreEqual("AAAA", result);
		}

		[TestMethod]
		public void DataModelAccessor_GeteHubIDByQualifier()
		{
			var mockContext = MockRepository.GenerateMock<eHubTransactionsContext>();
			var mockContextFactory = MockRepository.GenerateMock<ContextFactory<eHubTransactionsContext>>();
			mockContextFactory.Stub(x => x.CreateContext()).Return(mockContext);
			var testClientRegistrations = new TestDbSet<eHubClientRegistration>();
			mockContext.Stub(x => x.eHubClientRegistrations).Return(testClientRegistrations);

			testClientRegistrations.Add(new eHubClientRegistration
			{
				eHubClient = new eHubClient { CC_ID = "CargoWiseAAA" },
				eHubRegistrationType = new eHubRegistrationType { RT_ID = "CARGOWISE" },
				CX_Qualifier = "C1GS",

			});

			var dataModelAccessor = new DataModelAccessor(mockContextFactory);

			string result = dataModelAccessor.GeteHubIDByQualifier("C1GS", "CARGOWISE");

			Assert.AreEqual("CargoWiseAAA", result);
		}

		[TestMethod]
		public void DataModelAccessor_GeteHubIDByCode()
		{
			var mockContext = MockRepository.GenerateMock<eHubTransactionsContext>();
			var mockContextFactory = MockRepository.GenerateMock<ContextFactory<eHubTransactionsContext>>();
			mockContextFactory.Stub(x => x.CreateContext()).Return(mockContext);
			var testClientRegistrations = new TestDbSet<eHubClientRegistration>();
			mockContext.Stub(x => x.eHubClientRegistrations).Return(testClientRegistrations);

			var dataModelAccessor = new DataModelAccessor(mockContextFactory);

			string result = dataModelAccessor.GeteHubIDByCode("C1GS", "CARGOWISE");
			Assert.AreEqual(null, result);

			testClientRegistrations.Add(new eHubClientRegistration
			{
				eHubClient = new eHubClient { CC_ID = "CargoWiseAAA" },
				eHubRegistrationType = new eHubRegistrationType { RT_ID = "CARGOWISE" },
				CX_Code = "C1GS",

			});

			result = dataModelAccessor.GeteHubIDByCode("C1GS", "CARGOWISE");
			Assert.AreEqual("CargoWiseAAA", result);
		}

		[TestMethod]
		public void DataModelAccessor_GeteHubIDByCode_Exception()
		{
			var mockContext = MockRepository.GenerateMock<eHubTransactionsContext>();
			var mockContextFactory = MockRepository.GenerateMock<ContextFactory<eHubTransactionsContext>>();
			mockContextFactory.Stub(x => x.CreateContext()).Return(mockContext);
			var testClientRegistrations = new TestDbSet<eHubClientRegistration>();
			mockContext.Stub(x => x.eHubClientRegistrations).Return(testClientRegistrations);

			var dataModelAccessor = new DataModelAccessor(mockContextFactory);

			testClientRegistrations.Add(new eHubClientRegistration
			{
				eHubClient = new eHubClient { CC_ID = "CargoWiseAAA" },
				eHubRegistrationType = new eHubRegistrationType { RT_ID = "CARGOWISE" },
				CX_Code = "C1GS"
			});

			var exceptionThrown = false;

			try
			{
				dataModelAccessor.GeteHubIDByCode("", "CARGOWISE", false);
			}
			catch (Exception)
			{
				exceptionThrown = true;
			}

			Assert.IsFalse(exceptionThrown);

			exceptionThrown = false;

			try
			{
				dataModelAccessor.GeteHubIDByCode("", "CARGOWISE");
			}
			catch (Exception)
			{
				exceptionThrown = true;
			}

			Assert.IsTrue(exceptionThrown);
		}

		[TestMethod]
		public void DataModelAccessor_GetClientRegistrationFlag1()
		{
			var mockContext = MockRepository.GenerateMock<eHubTransactionsContext>();
			var mockContextFactory = MockRepository.GenerateMock<ContextFactory<eHubTransactionsContext>>();
			mockContextFactory.Stub(x => x.CreateContext()).Return(mockContext);
			var testClientRegistrations = new TestDbSet<eHubClientRegistration>();
			mockContext.Stub(x => x.eHubClientRegistrations).Return(testClientRegistrations);

			testClientRegistrations.Add(new eHubClientRegistration
			{
				eHubClient = new eHubClient { CC_ID = "CLIENT0001" },
				eHubRegistrationType = new eHubRegistrationType { RT_ID = "REG001" },
				CX_Qualifier = "Q1",
				CX_Flag1 = 1
			});

			testClientRegistrations.Add(new eHubClientRegistration
			{
				eHubClient = new eHubClient { CC_ID = "CLIENT0001" },
				eHubRegistrationType = new eHubRegistrationType { RT_ID = "REG001" },
				CX_Qualifier = "Q2",
				CX_Flag1 = 0
			});

			testClientRegistrations.Add(new eHubClientRegistration
			{
				eHubClient = new eHubClient { CC_ID = "CLIENT0001" },
				eHubRegistrationType = new eHubRegistrationType { RT_ID = "REG001" },
				CX_Qualifier = "Q3",
				CX_Flag1 = null
			});

			var dataModelAccessor = new DataModelAccessor(mockContextFactory);

			string result1 = dataModelAccessor.GetClientRegistrationFlag1AsString("CLIENT0001", "Q1", "REG001");
			string result2 = dataModelAccessor.GetClientRegistrationFlag1AsString("CLIENT0001", "Q2", "REG001");
			string result3 = dataModelAccessor.GetClientRegistrationFlag1AsString("CLIENT0001", "Q3", "REG001");

			Assert.AreEqual("1", result1);
			Assert.AreEqual("0", result2);
			Assert.AreEqual("", result3);
		}

        [TestMethod]
        public void DataModelAccessor_GetClientRegistrationAttri1AsString()
        {
            var mockContext = MockRepository.GenerateMock<eHubTransactionsContext>();
            var mockContextFactory = MockRepository.GenerateMock<ContextFactory<eHubTransactionsContext>>();
            mockContextFactory.Stub(x => x.CreateContext()).Return(mockContext);
            var testClientRegistrations = new TestDbSet<eHubClientRegistration>();
            mockContext.Stub(x => x.eHubClientRegistrations).Return(testClientRegistrations);

            testClientRegistrations.Add(new eHubClientRegistration
            {
                eHubClient = new eHubClient { CC_ID = "CLIENT0001" },
                eHubRegistrationType = new eHubRegistrationType { RT_ID = "REG001" },
                CX_Qualifier = "Q1",
                CX_Attr1 = "A1",
                CX_Flag1 = 1
            });

            testClientRegistrations.Add(new eHubClientRegistration
            {
                eHubClient = new eHubClient { CC_ID = "CLIENT0001" },
                eHubRegistrationType = new eHubRegistrationType { RT_ID = "REG001" },
                CX_Qualifier = "Q2",
                CX_Attr1 = "A2",
                CX_Flag1 = 0
            });

            testClientRegistrations.Add(new eHubClientRegistration
            {
                eHubClient = new eHubClient { CC_ID = "CLIENT0001" },
                eHubRegistrationType = new eHubRegistrationType { RT_ID = "REG001" },
                CX_Qualifier = "Q3",
                CX_Attr1 = "A3",
                CX_Flag1 = null
            });

            var dataModelAccessor = new DataModelAccessor(mockContextFactory);

            string result1 = dataModelAccessor.GetClientRegistrationAttri1AsString("CLIENT0001", "Q1", "REG001");
            string result2 = dataModelAccessor.GetClientRegistrationAttri1AsString("CLIENT0001", "Q2", "REG001");
            string result3 = dataModelAccessor.GetClientRegistrationAttri1AsString("CLIENT0001", "Q3", "REG001");
            string result4 = dataModelAccessor.GetClientRegistrationAttri1AsString("NONEXIST", "NONEXIST", "NONEXIST");

            Assert.AreEqual("A1", result1);
            Assert.AreEqual("A2", result2);
            Assert.AreEqual("A3", result3);
            Assert.AreEqual("", result4);
        }

        [TestMethod]
		public void DataModelAccessor_GetClientRegistration()
		{
			var mockContext = MockRepository.GenerateMock<eHubTransactionsContext>();
			var mockContextFactory = MockRepository.GenerateMock<ContextFactory<eHubTransactionsContext>>();
			mockContextFactory.Stub(x => x.CreateContext()).Return(mockContext);
			var testClientRegistrations = new TestDbSet<eHubClientRegistration>();
			mockContext.Stub(x => x.eHubClientRegistrations).Return(testClientRegistrations);

			testClientRegistrations.Add(new eHubClientRegistration
			{
				eHubClient = new eHubClient { CC_ID = "CLIENT0001" },
				eHubRegistrationType = new eHubRegistrationType { RT_ID = "REG001" },
				CX_Code = "AAA",
				CX_Password1 = "password1"
			});
			var dataModelAccessor = new DataModelAccessor(mockContextFactory);

			var result1 = dataModelAccessor.GetClientRegistration("CLIENT0001", "REG001");

			Assert.AreEqual("password1", result1.CX_Password1);
			Assert.AreEqual("CLIENT0001", result1.eHubClient.CC_ID);
		}

		[TestMethod]
		public void DataModelAccessor_GetClientRegistrationCode_Qualifier()
		{
			var mockContext = MockRepository.GenerateMock<eHubTransactionsContext>();
			var mockContextFactory = MockRepository.GenerateMock<ContextFactory<eHubTransactionsContext>>();
			mockContextFactory.Stub(x => x.CreateContext()).Return(mockContext);
			var testClientRegistrations = new TestDbSet<eHubClientRegistration>();
			mockContext.Stub(x => x.eHubClientRegistrations).Return(testClientRegistrations);

			testClientRegistrations.Add(new eHubClientRegistration
			{
				eHubClient = new eHubClient { CC_ID = "CLIENT0001" },
				eHubRegistrationType = new eHubRegistrationType { RT_ID = "REG001" },
				CX_Code = "AAAA",
				CX_Qualifier = "QUALIFIER1"
			});
			testClientRegistrations.Add(new eHubClientRegistration
			{
				eHubClient = new eHubClient { CC_ID = "CLIENT0001" },
				eHubRegistrationType = new eHubRegistrationType { RT_ID = "REG001" },
				CX_Code = "BBBB",
			});

			var dataModelAccessor = new DataModelAccessor(mockContextFactory);

			var result1 = dataModelAccessor.GetClientRegistrationCode("CLIENT0001", "QUALIFIER1", "REG001");	// Matching Qualifier
			var result2 = dataModelAccessor.GetClientRegistrationCode("CLIENT0001", string.Empty, "REG001");    // Empty Qualifier
			var result3 = dataModelAccessor.GetClientRegistrationCode("CLIENT0001", "QUALIFIER2", "REG001");	// Non-matching Qualifier
			var result4 = dataModelAccessor.GetClientRegistrationCode("CLIENT0001", "QUALIFIER1", "REG999");	// Unknown Rego Type
			var result5 = dataModelAccessor.GetClientRegistrationCode("CLIENT9999", "QUALIFIER1", "REG001");	// Unknown Client

			Assert.AreEqual("AAAA", result1);
			Assert.AreEqual("BBBB", result2);
			Assert.AreEqual("BBBB", result3);
			Assert.AreEqual(string.Empty, result4);
			Assert.AreEqual(string.Empty, result5);
		}

		[TestMethod]
		public void DataModelAccessor_InsertSubscriptionValue()
		{
			var mockContext = MockRepository.GenerateMock<eHubTransactionsContext>();
			var mockContextFactory = MockRepository.GenerateMock<ContextFactory<eHubTransactionsContext>>();
			var mockTransaction = MockRepository.GenerateMock<IDbTransaction>();
			mockContextFactory.Stub(x => x.CreateContext()).Return(mockContext);
			mockContext.Stub(x => x.BeginTransaction()).Return(mockTransaction);
			var testClients = new TestDbSet<eHubClient>();
			var testSubscriptionTypes = new TestDbSet<eHubSubscriptionType>();
			var testSubscriptionValues = new TestDbSet<eHubSubscriptionValue>();
			mockContext.Stub(x => x.eHubClients).Return(testClients);
			mockContext.Stub(x => x.eHubSubscriptionTypes).Return(testSubscriptionTypes);
			mockContext.Stub(x => x.eHubSubscriptionValues).Return(testSubscriptionValues);

			Guid stPK = new Guid("9c43ec3f-9d0b-4b78-b5e0-c816d0b812a8");
			Guid providerPK = new Guid("02cf3550-8505-446b-8ec0-3ec9aebff5d5");
			Guid subscriberPK = new Guid("ce7e53cd-3a89-4e60-b387-861155e1c145");
			testSubscriptionTypes.Add(new eHubSubscriptionType { ST_PK = stPK, ST_ID = "INTID" });
			testClients.Add(new eHubClient { CC_PK = providerPK, CC_ID = "INTTRA" });
			testClients.Add(new eHubClient { CC_PK = subscriberPK, CC_ID = "TESTSENDER__1" });

			var dataModelAccessor = new DataModelAccessor(mockContextFactory);

			dataModelAccessor.InsertSubscriptionValue("INTID", "INTTRA", "TESTSENDER__1", "AAAA");

			CollectionAssert.AreEquivalent(
				new[] { new Tuple<Guid, Guid, Guid, string>(stPK, providerPK, subscriberPK, "AAAA") },
				mockContext.eHubSubscriptionValues.Local.Select(v => new Tuple<Guid, Guid, Guid, string>(v.SV_ST, v.SV_CC_Sender, v.SV_CC_Recipient, v.SV_Value)).ToArray()
			);
			mockContext.AssertWasCalled(x => x.SaveChanges());
		}

		[TestMethod]
		public void DataModelAccessor_InsertSubscriptionValue_Exists()
		{
			var mockContext = MockRepository.GenerateMock<eHubTransactionsContext>();
			var mockContextFactory = MockRepository.GenerateMock<ContextFactory<eHubTransactionsContext>>();
			var mockTransaction = MockRepository.GenerateMock<IDbTransaction>();
			mockContextFactory.Stub(x => x.CreateContext()).Return(mockContext);
			mockContext.Stub(x => x.BeginTransaction()).Return(mockTransaction);
			var testClients = new TestDbSet<eHubClient>();
			var testSubscriptionTypes = new TestDbSet<eHubSubscriptionType>();
			var testSubscriptionValues = new TestDbSet<eHubSubscriptionValue>();
			mockContext.Stub(x => x.eHubClients).Return(testClients);
			mockContext.Stub(x => x.eHubSubscriptionTypes).Return(testSubscriptionTypes);
			mockContext.Stub(x => x.eHubSubscriptionValues).Return(testSubscriptionValues);

			Guid stPK = new Guid("9c43ec3f-9d0b-4b78-b5e0-c816d0b812a8");
			Guid providerPK = new Guid("02cf3550-8505-446b-8ec0-3ec9aebff5d5");
			Guid subscriberPK = new Guid("ce7e53cd-3a89-4e60-b387-861155e1c145");
			testSubscriptionTypes.Add(new eHubSubscriptionType { ST_PK = stPK, ST_ID = "INTID" });
			testClients.Add(new eHubClient { CC_PK = providerPK, CC_ID = "INTTRA" });
			testClients.Add(new eHubClient { CC_PK = subscriberPK, CC_ID = "TESTSENDER__1" });
			testSubscriptionValues.Add(new eHubSubscriptionValue { SV_ST = stPK, SV_CC_Sender = providerPK, SV_CC_Recipient = subscriberPK, SV_Value = "AAAA" });

			var dataModelAccessor = new DataModelAccessor(mockContextFactory);

			dataModelAccessor.InsertSubscriptionValue("INTID", "INTTRA", "TESTSENDER__1", "AAAA");

			mockContext.AssertWasNotCalled(x => x.SaveChanges());
			mockTransaction.AssertWasNotCalled(x => x.Commit());
		}

		[TestMethod]
		public void DataModelAccessor_InsertSubscriptionValue_ReferenceType()
		{
			var mockContext = MockRepository.GenerateMock<eHubTransactionsContext>();
			var mockContextFactory = MockRepository.GenerateMock<ContextFactory<eHubTransactionsContext>>();
			var mockTransaction = MockRepository.GenerateMock<IDbTransaction>();
			mockContextFactory.Stub(x => x.CreateContext()).Return(mockContext);
			mockContext.Stub(x => x.BeginTransaction()).Return(mockTransaction);
			var testClients = new TestDbSet<eHubClient>();
			var testSubscriptionTypes = new TestDbSet<eHubSubscriptionType>();
			var testSubscriptionValues = new TestDbSet<eHubSubscriptionValue>();
			mockContext.Stub(x => x.eHubClients).Return(testClients);
			mockContext.Stub(x => x.eHubSubscriptionTypes).Return(testSubscriptionTypes);
			mockContext.Stub(x => x.eHubSubscriptionValues).Return(testSubscriptionValues);

			Guid stPK = new Guid("9c43ec3f-9d0b-4b78-b5e0-c816d0b812a8");
			Guid providerPK = new Guid("02cf3550-8505-446b-8ec0-3ec9aebff5d5");
			Guid subscriberPK = new Guid("ce7e53cd-3a89-4e60-b387-861155e1c145");
			testSubscriptionTypes.Add(new eHubSubscriptionType { ST_PK = stPK, ST_ID = "INTID" });
			testClients.Add(new eHubClient { CC_PK = providerPK, CC_ID = "INTTRA" });
			testClients.Add(new eHubClient { CC_PK = subscriberPK, CC_ID = "TESTSENDER__1" });
			testSubscriptionValues.Add(new eHubSubscriptionValue { SV_ST = stPK, SV_CC_Sender = providerPK, SV_CC_Recipient = subscriberPK, SV_Value = "AAAA", SV_Reference = "ABC123" });

			var dataModelAccessor = new DataModelAccessor(mockContextFactory);

			// Null ReferenceType - match, should NOT insert
			dataModelAccessor.InsertSubscriptionValue("INTID", "INTTRA", "TESTSENDER__1", "AAAA", "ABC123");
			mockContext.AssertWasNotCalled(x => x.SaveChanges());
			mockTransaction.AssertWasNotCalled(x => x.Commit());

			// Non-null ReferenceType - no match, should insert
			dataModelAccessor.InsertSubscriptionValue("INTID", "INTTRA", "TESTSENDER__1", "AAAA", "ABC123", "TYPE_Z");
			mockContext.AssertWasCalled(x => x.SaveChanges());
			mockTransaction.AssertWasCalled(x => x.Commit());
		}

		[TestMethod]
		public void DataModelAccessor_InsertSubscriptionValue_ReferenceType_Exists()
		{
			var mockContext = MockRepository.GenerateMock<eHubTransactionsContext>();
			var mockContextFactory = MockRepository.GenerateMock<ContextFactory<eHubTransactionsContext>>();
			var mockTransaction = MockRepository.GenerateMock<IDbTransaction>();
			mockContextFactory.Stub(x => x.CreateContext()).Return(mockContext);
			mockContext.Stub(x => x.BeginTransaction()).Return(mockTransaction);
			var testClients = new TestDbSet<eHubClient>();
			var testSubscriptionTypes = new TestDbSet<eHubSubscriptionType>();
			var testSubscriptionValues = new TestDbSet<eHubSubscriptionValue>();
			mockContext.Stub(x => x.eHubClients).Return(testClients);
			mockContext.Stub(x => x.eHubSubscriptionTypes).Return(testSubscriptionTypes);
			mockContext.Stub(x => x.eHubSubscriptionValues).Return(testSubscriptionValues);

			Guid stPK = new Guid("9c43ec3f-9d0b-4b78-b5e0-c816d0b812a8");
			Guid providerPK = new Guid("02cf3550-8505-446b-8ec0-3ec9aebff5d5");
			Guid subscriberPK = new Guid("ce7e53cd-3a89-4e60-b387-861155e1c145");
			testSubscriptionTypes.Add(new eHubSubscriptionType { ST_PK = stPK, ST_ID = "INTID" });
			testClients.Add(new eHubClient { CC_PK = providerPK, CC_ID = "INTTRA" });
			testClients.Add(new eHubClient { CC_PK = subscriberPK, CC_ID = "TESTSENDER__1" });
			testSubscriptionValues.Add(new eHubSubscriptionValue { SV_ST = stPK, SV_CC_Sender = providerPK, SV_CC_Recipient = subscriberPK, SV_Value = "AAAA", SV_Reference = "ABC123", SV_ReferenceType = "TYPE_X" });

			var dataModelAccessor = new DataModelAccessor(mockContextFactory);

			// Same ReferenceType - match, should NOT insert
			dataModelAccessor.InsertSubscriptionValue("INTID", "INTTRA", "TESTSENDER__1", "AAAA", "ABC123", "TYPE_X");
			mockContext.AssertWasNotCalled(x => x.SaveChanges());
			mockTransaction.AssertWasNotCalled(x => x.Commit());

			// Null ReferenceType - no match, should insert
			dataModelAccessor.InsertSubscriptionValue("INTID", "INTTRA", "TESTSENDER__1", "AAAA", "ABC123");
			mockContext.AssertWasCalled(x => x.SaveChanges());
			mockTransaction.AssertWasCalled(x => x.Commit());
		}

		//[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")] -- (BL) not working on TFS build
		[HostType("Moles")]
		[Ignore]
		public void DataModelAccessor_InsertSubscriptionValue_RefExists()
		{
			var mockContext = MockRepository.GenerateMock<eHubTransactionsContext>();
			var mockContextFactory = MockRepository.GenerateMock<ContextFactory<eHubTransactionsContext>>();
			var mockTransaction = MockRepository.GenerateMock<IDbTransaction>();
			mockContextFactory.Stub(x => x.CreateContext()).Return(mockContext);
			mockContext.Stub(x => x.BeginTransaction()).Return(mockTransaction);
			var testClients = new TestDbSet<eHubClient>();
			var testSubscriptionTypes = new TestDbSet<eHubSubscriptionType>();
			var testSubscriptionValues = new TestDbSet<eHubSubscriptionValue>();
			mockContext.Stub(x => x.eHubClients).Return(testClients);
			mockContext.Stub(x => x.eHubSubscriptionTypes).Return(testSubscriptionTypes);
			mockContext.Stub(x => x.eHubSubscriptionValues).Return(testSubscriptionValues);

			// TODO: WI00206827 find alternative for System.Moles
			// System.Moles.MDateTime.UtcNowGet = () => new DateTime(2015, 2, 3, 4, 5, 6);

			Guid stPK = new Guid("9c43ec3f-9d0b-4b78-b5e0-c816d0b812a8");
			Guid providerPK = new Guid("02cf3550-8505-446b-8ec0-3ec9aebff5d5");
			Guid subscriberPK = new Guid("ce7e53cd-3a89-4e60-b387-861155e1c145");
			testSubscriptionTypes.Add(new eHubSubscriptionType { ST_PK = stPK, ST_ID = "INTID", ST_ExpiryDays = 30 });
			testClients.Add(new eHubClient { CC_PK = providerPK, CC_ID = "INTTRA" });
			testClients.Add(new eHubClient { CC_PK = subscriberPK, CC_ID = "TESTSENDER__1" });
			var testValue = testSubscriptionValues.Add(new eHubSubscriptionValue
			{
				SV_ST = stPK,
				SV_CC_Sender = providerPK,
				SV_CC_Recipient = subscriberPK,
				SV_Value = "AAAA",
				SV_Reference = "BBBB",
				SV_ExpiryUTC = new DateTime(2015, 1, 2, 3, 4, 5)
			});

			var dataModelAccessor = new DataModelAccessor(mockContextFactory);

			dataModelAccessor.InsertSubscriptionValue("INTID", "INTTRA", "TESTSENDER__1", "AAAA", "CCCC");

			Assert.AreEqual(new DateTime(2015, 3, 5, 4, 5, 6), testValue.SV_ExpiryUTC);
			Assert.AreEqual("CCCC", testValue.SV_Reference);
			mockContext.AssertWasCalled(x => x.SaveChanges());
		}

		[TestMethod]
		public void GetClientSystemRegistration_SystemLevel()
		{
			var mockContext = MockRepository.GenerateMock<eHubTransactionsContext>();
			var mockContextFactory = MockRepository.GenerateMock<ContextFactory<eHubTransactionsContext>>();
			mockContextFactory.Stub(x => x.CreateContext()).Return(mockContext);

			var testClientSystem = new eHubClientSystem { EH_ID = "TSTSEN" };

			var testClientSystems = new TestDbSet<eHubClientSystem>();
			mockContext.Stub(x => x.eHubClientSystems).Return(testClientSystems);

			testClientSystems.Add(testClientSystem);

			var testClientSystemRegistrations = new TestDbSet<eHubClientSystemRegistration>();
			mockContext.Stub(x => x.eHubClientSystemRegistrations).Return(testClientSystemRegistrations);

			var registration = new eHubClientSystemRegistration
			{
				eHubRegistrationType = new eHubRegistrationType { RT_ID = "JPCustomsAccount" },
				CD_Code = "TSTUsername",
				CD_Attr1 = "TSTPassword",
				CD_EH = testClientSystem.EH_PK,
				eHubClientSystem = testClientSystem
			};
			testClientSystemRegistrations.Add(registration);

			var dataModelAccessor = new DataModelAccessor(mockContextFactory);

			var result = dataModelAccessor.GetClientSystemRegistration("TSTSEN", "EDI", "JPCustomsAccount");

			Assert.AreEqual(registration, result);
		}

		[TestMethod]
		public void GetClientSystemRegistration_QualifierLevel()
		{
			var mockContext = MockRepository.GenerateMock<eHubTransactionsContext>();
			var mockContextFactory = MockRepository.GenerateMock<ContextFactory<eHubTransactionsContext>>();
			mockContextFactory.Stub(x => x.CreateContext()).Return(mockContext);

			var testClientSystem = new eHubClientSystem { EH_ID = "TSTSEN" };

			var testClientSystems = new TestDbSet<eHubClientSystem>();
			mockContext.Stub(x => x.eHubClientSystems).Return(testClientSystems);

			testClientSystems.Add(testClientSystem);

			var testClientSystemRegistrations = new TestDbSet<eHubClientSystemRegistration>();
			mockContext.Stub(x => x.eHubClientSystemRegistrations).Return(testClientSystemRegistrations);

			var registration = new eHubClientSystemRegistration
			{
				eHubRegistrationType = new eHubRegistrationType { RT_ID = "JPCustomsAccount" },
				CD_Code = "TSTUsername",
				CD_Attr1 = "TSTPassword",
				CD_EH = testClientSystem.EH_PK,
				eHubClientSystem = testClientSystem
			};
			testClientSystemRegistrations.Add(registration);
			var registrationQualifier = new eHubClientSystemRegistration
			{
				eHubRegistrationType = new eHubRegistrationType { RT_ID = "JPCustomsAccount" },
				CD_Code = "TSTUsername",
				CD_Attr1 = "TSTPassword",
				CD_EH = testClientSystem.EH_PK,
				CD_Qualifier = "DAU",
				eHubClientSystem = testClientSystem
			};
			testClientSystemRegistrations.Add(registrationQualifier);

			var dataModelAccessor = new DataModelAccessor(mockContextFactory);

			var result = dataModelAccessor.GetClientSystemRegistration("TSTSEN", "DAU", "JPCustomsAccount");

			Assert.AreEqual(registrationQualifier, result);
		}
	}
}
