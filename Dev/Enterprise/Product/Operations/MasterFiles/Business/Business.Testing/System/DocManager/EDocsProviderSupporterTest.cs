using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Moq;

namespace Enterprise.MasterFiles.Business.Testing
{
	class EDocsProviderSupporterTest : TestCaseWithFactory
	{
		Mock<DocumentSupporter> mockDocumentSupporter;
		Mock<IEDocsProvider> mockEDocsProvider;
		EDocsProviderSupporter supporter;

		Mock<DocumentSupporter> MockDocumentSupporter
		{
			get
			{
				if (mockDocumentSupporter == null)
				{
					mockDocumentSupporter = new Mock<DocumentSupporter>(Factory.New<DummyBusinessObject>());
				}
				return mockDocumentSupporter;
			}
		}

		Mock<IEDocsProvider> MockEDocsProvider
		{
			get
			{
				if (mockEDocsProvider == null)
				{
					mockEDocsProvider = new Mock<IEDocsProvider>();
					mockEDocsProvider.Setup(m => m.DocumentSupporter).Returns(MockDocumentSupporter.Object);
					MockDocumentSupporter.Setup(m => m.BusinessContext).Returns(BusinessContext.AgencyBooking);
				}
				return mockEDocsProvider;
			}
		}

		protected EDocsProviderSupporter Supporter
		{
			get
			{
				if (supporter == null)
				{
					supporter = GetNewSupporter(MockEDocsProvider.Object);
				}
				return supporter;
			}
		}

		protected virtual EDocsProviderSupporter GetNewSupporter(IEDocsProvider eDocsProvider)
		{
			return new EDocsProviderSupporter(eDocsProvider);
		}

		public void TestBusinessContext()
		{
			AssertEquals("BusinessContext", BusinessContext.AgencyBooking, GetNewSupporter(MockEDocsProvider.Object).BusinessContext);
			MockDocumentSupporter.Reset();
			MockDocumentSupporter.Setup(m => m.BusinessContext).Returns(BusinessContext.AgencyDocumentation);
			AssertEquals("BusinessContext", BusinessContext.AgencyDocumentation, GetNewSupporter(MockEDocsProvider.Object).BusinessContext);
			MockDocumentSupporter.VerifyAll();
		}

		public void TestAddAndGetConsumers()
		{
			int originalLength = Supporter.GetConsumers().Length;

			MenuItemIdentifier consumer1 = new MenuItemIdentifier(BusinessContext.AgencyBooking, "ID1");
			MenuItemIdentifier consumer2 = new MenuItemIdentifier(BusinessContext.APTransaction, "ID2");

			Supporter.AddConsumer(consumer1);
			Supporter.AddConsumer(consumer2);

			MenuItemIdentifier[] consumers = Supporter.GetConsumers();
			AssertEquals("GetConsumers().Length", 2 + originalLength, consumers.Length);
			AssertCollectionContains("GetConsumers() should contain consumer1.", consumer1, consumers);
			AssertCollectionContains("GetConsumers() should contain consumer2.", consumer2, consumers);
		}

		public void TestGetAndCreateProviderPlaceholder()
		{
			StmMenuItem menuItem1 = Factory.New<StmMenuItem>();
			StmMenuItem menuItem2 = Factory.New<StmMenuItem>();

			menuItem1.SU_BusinessContext = nameof(BusinessContext.Cartage);
			menuItem2.SU_BusinessContext = nameof(BusinessContext.ARInvoice);

			menuItem1.SU_MenuName = "Potato Salad";
			menuItem2.SU_MenuName = "Chicken Salad";

			AssertNull("GetProviderPlaceholder(menuItem1)", Supporter.GetProviderPlaceholder<StmMenuItem>(menuItem1));
			AssertNull("GetProviderPlaceholder(menuItem2)", Supporter.GetProviderPlaceholder<StmMenuItem>(menuItem2));

			MockStmMenuItem providerPlaceholder1 = Supporter.CreateProviderPlaceholder<MockStmMenuItem>(menuItem1);
			MockStmMenuItem providerPlaceholder2 = Supporter.CreateProviderPlaceholder<MockStmMenuItem>(menuItem2);
			MockStmMenuItem providerPlaceholder3 = Factory.New<MockStmMenuItem>();
			MockStmMenuItem providerPlaceholder4 = Factory.New<MockStmMenuItem>();

			providerPlaceholder3.CopyPersistentValuesFrom(providerPlaceholder1);
			providerPlaceholder3.SU_IsSystemDefined = false;

			providerPlaceholder4.CopyPersistentValuesFrom(providerPlaceholder2);
			providerPlaceholder4.SU_BusinessContext = nameof(BusinessContext.CFSLoadList);

			AssertEquals("GetProviderPlaceholder(menuItem1)", providerPlaceholder1, Supporter.GetProviderPlaceholder<MockStmMenuItem>(menuItem1));
			AssertEquals("GetProviderPlaceholder(menuItem1)", providerPlaceholder2, Supporter.GetProviderPlaceholder<MockStmMenuItem>(menuItem2));

			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();

			StmMenuItem anotherMenuItem1 = newFactory.New<StmMenuItem>();
			StmMenuItem anotherMenuItem2 = newFactory.New<StmMenuItem>();

			anotherMenuItem1.CopyPersistentValuesFrom(menuItem1);
			anotherMenuItem2.CopyPersistentValuesFrom(menuItem2);

			MockStmMenuItem anotherProviderPlaceholder1 = Supporter.GetProviderPlaceholder<MockStmMenuItem>(anotherMenuItem1);
			MockStmMenuItem anotherProviderPlaceholder2 = Supporter.GetProviderPlaceholder<MockStmMenuItem>(anotherMenuItem2);

			Assert("Precondition: anotherProviderPlaceholder1 should be different from providerPlaceholder1.", anotherProviderPlaceholder1 != providerPlaceholder1);
			Assert("Precondition: anotherProviderPlaceholder2 should be different from providerPlaceholder2.", anotherProviderPlaceholder2 != providerPlaceholder2);

			AssertEquals("anotherProviderPlaceholder1.PK", providerPlaceholder1.PK, anotherProviderPlaceholder1.PK);
			AssertEquals("anotherProviderPlaceholder1.SU_BusinessContext", nameof(BusinessContext.AgencyBooking), anotherProviderPlaceholder1.SU_BusinessContext);
			AssertEquals("anotherProviderPlaceholder1.SU_FilterList", "\"<EDocsProviderPlaceholderFor>\"==\"" + nameof(BusinessContext.Cartage) + "\\Potato Salad\"", anotherProviderPlaceholder1.SU_FilterList);
			AssertEquals("anotherProviderPlaceholder1.SU_IsSystemDefined", true, anotherProviderPlaceholder1.SU_IsSystemDefined);
			AssertEquals("anotherProviderPlaceholder1.SU_MenuName", "Potato Salad (Read-only placeholder for adding eDocs attachments)", anotherProviderPlaceholder1.SU_MenuName);
			AssertEquals("anotherProviderPlaceholder1.SU_GS_NKStaffCode", ZString.Empty, anotherProviderPlaceholder1.SU_GS_NKStaffCode);

			AssertEquals("anotherProviderPlaceholder2.PK", providerPlaceholder2.PK, anotherProviderPlaceholder2.PK);
			AssertEquals("anotherProviderPlaceholder2.SU_BusinessContext", nameof(BusinessContext.AgencyBooking), anotherProviderPlaceholder2.SU_BusinessContext);
			AssertEquals("anotherProviderPlaceholder2.SU_FilterList", "\"<EDocsProviderPlaceholderFor>\"==\"" + nameof(BusinessContext.ARInvoice) + "\\Chicken Salad\"", anotherProviderPlaceholder2.SU_FilterList);
			AssertEquals("anotherProviderPlaceholder2.SU_IsSystemDefined", true, anotherProviderPlaceholder1.SU_IsSystemDefined);
			AssertEquals("anotherProviderPlaceholder2.SU_MenuName", "Chicken Salad (Read-only placeholder for adding eDocs attachments)", anotherProviderPlaceholder2.SU_MenuName);
			AssertEquals("anotherProviderPlaceholder2.SU_GS_NKStaffCode", ZString.Empty, anotherProviderPlaceholder2.SU_GS_NKStaffCode);

			anotherProviderPlaceholder1.Delete();
			anotherProviderPlaceholder2.Delete();

			AssertNull("GetProviderPlaceholder(anotherMenuItem1)", Supporter.GetProviderPlaceholder<StmMenuItem>(anotherMenuItem1));
			AssertNull("GetProviderPlaceholder(anotherMenuItem2)", Supporter.GetProviderPlaceholder<StmMenuItem>(anotherMenuItem2));
		}
	}
}
