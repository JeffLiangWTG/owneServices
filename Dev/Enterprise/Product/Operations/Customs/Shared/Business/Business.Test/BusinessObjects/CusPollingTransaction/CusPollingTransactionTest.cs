using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusPollingTransaction))]
	sealed class CusPollingTransactionTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var result = factory.NewWithValidTestData<CusPollingTransaction>();
			result.CPT_ApplicationCode = "TRC";
			result.CPT_Type = "TRO";
			result.CPT_Status = "OPN";
			result.CPT_NumberOfAttempts = 1;
			return result;
		}

		public void TestRegisteredTypeLoad()
		{
			var transaction = Factory.New<CusPollingTransaction>();
			transaction.CPT_ApplicationCode = "TRC";
			transaction.CPT_Type = "TRO";
			transaction.CPT_Status = "OPN";
			transaction.CPT_NumberOfAttempts = 1;
			transaction.CPT_TransactionID = "id";

			var bo = Factory.NewWithValidTestData(ObjectFactory.GetType<Integration.Customs.ICusEntryHeader>());
			transaction.CPT_ParentTableCode = bo.TablePrefix;
			transaction.CPT_ParentID = bo.PK;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedTransaction = newFactory.Load<CusPollingTransaction>(transaction.PK);

			var loadedBO = loadedTransaction.ParentObject;

			AssertEquals(bo.PK, loadedBO.PK);
		}

		public void TestCPT_ParentAsCusEntryHeader()
		{
			var transaction = Factory.New<CusPollingTransaction>();
			transaction.CPT_ApplicationCode = "TRC";
			transaction.CPT_Type = "TRO";
			transaction.CPT_Status = "OPN";
			transaction.CPT_NumberOfAttempts = 1;
			transaction.CPT_TransactionID = "id";
			var bo = Factory.NewWithValidTestData(ObjectFactory.GetType<Integration.Customs.ICusEntryHeader>());
			transaction.CPT_ParentTableCode = bo.TablePrefix;
			transaction.CPT_ParentID = bo.PK;

			CombineAssertions(() =>
			{
				AssertEquals("Parent", bo, transaction.ParentObject);
				AssertNoExceptionThrown("No exception when saving", () => Factory.Save());
			});
		}

		public void TestCPT_ParentAsCusExitReport()
		{
			var transaction = Factory.New<CusPollingTransaction>();
			transaction.CPT_ApplicationCode = "TRC";
			transaction.CPT_Type = "TRO";
			transaction.CPT_Status = "OPN";
			transaction.CPT_NumberOfAttempts = 1;
			transaction.CPT_TransactionID = "id";
			var bo = Factory.NewWithValidTestData(ObjectFactory.GetType<Integration.Customs.EUExitControl.ICusExitReport>());
			transaction.CPT_ParentTableCode = bo.TablePrefix;
			transaction.CPT_ParentID = bo.PK;

			CombineAssertions(() =>
			{
				AssertEquals("Parent", bo, transaction.ParentObject);
				AssertNoExceptionThrown("No exception when saving", () => Factory.Save());
			});
		}

		public void TestCPT_ParentAsEDIMessage()
		{
			var transaction = Factory.New<CusPollingTransaction>();
			transaction.CPT_ApplicationCode = "TRC";
			transaction.CPT_Type = "TRO";
			transaction.CPT_Status = "OPN";
			transaction.CPT_NumberOfAttempts = 1;
			transaction.CPT_TransactionID = "id";
			var bo = Factory.NewWithValidTestData(ObjectFactory.GetType<Messaging.Integration.IEDIMessage>());
			transaction.CPT_ParentTableCode = bo.TablePrefix;
			transaction.CPT_ParentID = bo.PK;

			CombineAssertions(() =>
			{
				AssertEquals("Parent", bo, transaction.ParentObject);
				AssertNoExceptionThrown("No exception when saving", () => Factory.Save());
			});
		}

		public void TestCPT_ParentAsGlbCompany()
		{
			var transaction = Factory.New<CusPollingTransaction>();
			transaction.CPT_ApplicationCode = "TRC";
			transaction.CPT_Type = "TRO";
			transaction.CPT_Status = "OPN";
			transaction.CPT_NumberOfAttempts = 1;
			transaction.CPT_TransactionID = "id";
			var bo = Factory.NewWithValidTestData(ObjectFactory.GetType<MasterFiles.Integration.IGlbCompany>());
			transaction.CPT_ParentTableCode = bo.TablePrefix;
			transaction.CPT_ParentID = bo.PK;

			CombineAssertions(() =>
			{
				AssertEquals("Parent", bo, transaction.ParentObject);
				AssertNoExceptionThrown("No exception when saving", () => Factory.Save());
			});
		}

		public void TestCPT_ParentAsCusInBondHeader()
		{
			var transaction = Factory.New<CusPollingTransaction>();
			transaction.CPT_ApplicationCode = "TRC";
			transaction.CPT_Type = "TRO";
			transaction.CPT_Status = "OPN";
			transaction.CPT_NumberOfAttempts = 1;
			transaction.CPT_TransactionID = "id";
			var bo = Factory.NewWithValidTestData(ObjectFactory.GetType<Integration.Customs.EU.NCTS.ICusInBondHeader>());
			transaction.CPT_ParentTableCode = bo.TablePrefix;
			transaction.CPT_ParentID = bo.PK;

			CombineAssertions(() =>
			{
				AssertEquals("Parent", bo, transaction.ParentObject);
				AssertNoExceptionThrown("No exception when saving", () => Factory.Save());
			});
		}

		public void TestCPT_ParentAsEDIInterchange()
		{
			var transaction = Factory.New<CusPollingTransaction>();
			transaction.CPT_ApplicationCode = "ITH";
			transaction.CPT_Type = "IVI";
			transaction.CPT_Status = "OPN";
			transaction.CPT_NumberOfAttempts = 1;
			transaction.CPT_TransactionID = "id";
			var bo = Factory.NewWithValidTestData(ObjectFactory.GetType<Messaging.Integration.IEDIInterchange>());
			transaction.CPT_ParentTableCode = bo.TablePrefix;
			transaction.CPT_ParentID = bo.PK;

			CombineAssertions(() =>
			{
				AssertEquals("Parent", bo, transaction.ParentObject);
				AssertNoExceptionThrown("No exception when saving", () => Factory.Save());
			});
		}

		public void TestRegisteredTypeMappings()
		{
			var transaction = Factory.New<CusPollingTransactionForTest>();

			var expectedMappings = new Dictionary<string, Type>()
			{
				{ CusEntryHeaderSchema.Constants.Prefix, ObjectFactory.GetType<Integration.Customs.ICusEntryHeader>() },
				{ CusExitReportSchema.Constants.Prefix, ObjectFactory.GetType<Integration.Customs.EUExitControl.ICusExitReport>() },
				{ EDIMessageSchema.Constants.Prefix, ObjectFactory.GetType<Messaging.Integration.IEDIMessage>() },
				{ GlbCompanySchema.Constants.Prefix, ObjectFactory.GetType<MasterFiles.Integration.IGlbCompany>() },
				{ CusInBondHeaderSchema.Constants.Prefix, ObjectFactory.GetType<Integration.Customs.EU.NCTS.ICusInBondHeader>() },
				{ EDIInterchangeSchema.Constants.Prefix, ObjectFactory.GetType<Messaging.Integration.IEDIInterchange>() },
			};

			var registered = transaction.RegisteredLinkedObjectTypesExposed;

			CombineAssertions(() =>
			{
				AssertEquals(expectedMappings.Count, registered.Count);

				foreach (var mapping in expectedMappings)
				{
					Assert($"Mapping for {mapping.Key} not found", registered.ContainsKey(mapping.Key));
					var bizoType = registered[mapping.Key].Invoke();
					AssertEquals($"Mapping type for {mapping.Key}", mapping.Value, bizoType);
					AssertEquals($"Table from type {bizoType.Name} does not match {mapping.Key} according to BusinessObjectFactory", mapping.Key, BusinessObjectFactory.GetTableCodeFromType(bizoType));
				}
			});
		}

		class CusPollingTransactionForTest : CusPollingTransaction
		{
			public CusPollingTransactionForTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public Dictionary<string, Func<Type>> RegisteredLinkedObjectTypesExposed => base.RegisteredLinkedObjectTypes;
		}
	}
}
