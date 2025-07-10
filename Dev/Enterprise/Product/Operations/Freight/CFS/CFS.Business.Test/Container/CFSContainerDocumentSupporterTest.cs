using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Freight.CFS.Business.Testing
{
	[TestedType(typeof(CFSContainerDocumentSupporter))]
	class CFSContainerDocumentSupporterTest : DocumentSupporterTest
	{
		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			var container = Factory.New<CFSContainer>();
			container.Services.AddNew();

			return container;
		}

		public void TestSupportedDataContext()
		{
			CFSContainer container = Factory.New<CFSContainer>();
			AssertEquals("Core.Constants.DataContext.PackUnpackContainerRego is Supported", true, container.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.PackUnpackContainerRego)));
			AssertEquals("Core.Constants.DataContext.CartageAdvice is Supported", true, container.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.CartageAdvice)));
			AssertEquals("Core.Constants.DataContext.ERA is Supported", true, container.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.ERA)));
			AssertEquals("Core.Constants.DataContext.RequestForService is Supported", true, container.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.RequestForService)));
			AssertEquals("Core.Constants.DataContext.Service is Supported", true, container.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.Service)));
			AssertEquals("Core.Constants.DataContext.GenericFreightJobServices is Supported", true, container.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.GenericFreightJobServices)));
		}

		#region TestGetDocBusinessObjectForRequestForService

		public void TestGetDocBusinessObjectsForRequestForService()
		{
			var container = Factory.New<CFSContainer>();

			var servicesSelectionProvider = new Mock<IServicesSelectionProvider>();

			Factory.SetValue(() => servicesSelectionProvider.Object);

			servicesSelectionProvider
				.Setup(m => m.GetServicesToPrint(It.Is<IHaveServices>(p => (CFSContainer)p == container)))
				.Returns((JobService[])null);
			DocumentWrapper[] wrapper = container.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.RequestForService, null);
			AssertEquals("Wrapper should be null", null, wrapper);
			servicesSelectionProvider
				.Verify(m => m.GetServicesToPrint(It.Is<IHaveServices>(p => (CFSContainer)p == container)), Times.Once);

			JobService service1 = container.Services.AddNew();
			JobService service2 = container.Services.AddNew();

			servicesSelectionProvider
				.Setup(m => m.GetServicesToPrint(It.Is<IHaveServices>(p => (CFSContainer)p == container)))
				.Returns(new[] { service1, service2 });
			wrapper = container.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.RequestForService, null);
			AssertEquals("Service Wrappers should be created", 2, wrapper.Length);
			AssertEquals("Wrapper type should be DocService", "DocService", wrapper[0].GetType().Name);
			AssertEquals("Wrapper type should be DocService", "DocService", wrapper[1].GetType().Name);
			servicesSelectionProvider
				.Verify(m => m.GetServicesToPrint(It.Is<IHaveServices>(p => (CFSContainer)p == container)),
					Times.Exactly(2));
		}

		#endregion

		public void TestGetContactOrganisation()
		{
			var contactOrg = Container.DocumentSupporter.GetContactOrganisation("", ContactType.NoContactType, DocumentDirection.ANY);
			AssertEquals("ContactOrg", null, contactOrg);

			contactOrg = Container.DocumentSupporter.GetContactOrganisation("", ContactType.LocalTransport, DocumentDirection.ANY);
			AssertEquals("ContactOrg", null, contactOrg.OrgHeader);

			var factoryToSave = new BusinessObjectFactory();
			var cartageCo1 = factoryToSave.NewWithValidTestData<OrgHeader>();
			var cartageCo2 = factoryToSave.NewWithValidTestData<OrgHeader>();
			factoryToSave.Save();

			Container.JC_ArrivalTransportPK = cartageCo1.PK;
			contactOrg = Container.DocumentSupporter.GetContactOrganisation("", ContactType.LocalTransport, DocumentDirection.ANY);
			AssertEquals("ContactOrg", cartageCo1.PK, contactOrg.OrgHeader.PK);

			contactOrg = Container.DocumentSupporter.GetContactOrganisation("", ContactType.NoContactType, DocumentDirection.ANY);
			AssertEquals("ContactOrg", null, contactOrg);

			Container.JC_ArrivalTransportPK = ZGuid.Empty;
			contactOrg = Container.DocumentSupporter.GetContactOrganisation("", ContactType.LocalTransport, DocumentDirection.ANY);
			AssertEquals("ContactOrg", null, contactOrg.OrgHeader);

			LoadList.JK_OA_CartageCoAddress = cartageCo2.MainAddress.PK;
			contactOrg = Container.DocumentSupporter.GetContactOrganisation("", ContactType.LocalTransport, DocumentDirection.ANY);
			AssertEquals("ContactOrg", cartageCo2.PK, contactOrg.OrgHeader.PK);

			contactOrg = Container.DocumentSupporter.GetContactOrganisation("", ContactType.NoContactType, DocumentDirection.ANY);
			AssertEquals("ContactOrg", null, contactOrg);
		}

		public void TestGetContactOrganisationForClient()
		{
			var client = Factory.LoadTop1<OrgHeader>(new ZQuery());
			Container.JC_OH_CFSClient = client.PK;
			var contact = Container.DocumentSupporter.GetContactOrganisation("", ContactType.ImportFreightAgent, DocumentDirection.ANY);
			AssertEquals("Contact Organisation for FIS type", client.PK, contact.OrgHeader.PK);
		}

		#region Implementation

		protected CFSContainer Container;
		protected CFSLoadListConsol LoadList;

		protected override void SetUp()
		{
			base.SetUp();
			LoadList = Factory.New<CFSLoadListConsol>();
			Container = LoadList.Containers.AddNew();
		}

		#endregion
	}
}
