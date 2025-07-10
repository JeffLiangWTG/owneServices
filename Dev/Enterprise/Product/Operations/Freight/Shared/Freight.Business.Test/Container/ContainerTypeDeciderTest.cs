using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Integration;
using Enterprise.Integration.Freight;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;
using static Enterprise.Integration.Customs.Shared;

namespace Enterprise.Freight.Business.Testing
{
	sealed class ContainerTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForLoad()
		{
			var forwardingConsol = (BusinessObject)Factory.New<Enterprise.Integration.Forwarding.IForwardingConsol>();
			var cfsConsol = (BusinessObject)Factory.New<CFS.ICFSLoadListConsol>();

			var forwardingShipment = (BusinessObject)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			var agencyBooking = (BusinessObject)Factory.New<Integration.Agency.IAgencyBooking>();
			var billOfLading = (BusinessObject)Factory.New<Integration.Agency.IBillOfLading>();

			var forwardingContainer = (BusinessObject)Factory.New<Enterprise.Integration.Forwarding.IForwardingContainer>();
			forwardingContainer[JobContainerSchema.JC_JK] = forwardingConsol.PK;

			var cfsContainer = (BusinessObject)Factory.New<CFS.ICFSContainer>();
			cfsContainer[JobContainerSchema.JC_JK] = cfsConsol.PK;

			var quotedBookingContainer = (BusinessObject)Factory.New<Enterprise.Integration.Forwarding.IForwardingContainer>();
			quotedBookingContainer[JobContainerSchema.JC_JS_FCLBookingOnlyLink] = forwardingShipment.PK;

			var agencyBookingContainer = (BusinessObject)Factory.New<Integration.Agency.IAgencyBookingContainer>();
			agencyBookingContainer[JobContainerSchema.JC_JS_FCLBookingOnlyLink] = agencyBooking.PK;

			var billOfLadingContainer = (BusinessObject)Factory.New<Integration.Agency.IBillOfLadingContainer>();
			billOfLadingContainer[JobContainerSchema.JC_JS_FCLBookingOnlyLink] = billOfLading.PK;

			var invalidContainerWithoutParent = (BusinessObject)Factory.New<ICommonContainer>();

			var declarationContainer = (BusinessObject)Factory.New<Enterprise.Integration.Forwarding.IForwardingContainer>();
			var declaration = Factory.New<IBaseJobDeclaration>();
			var cusContainer = (BusinessObject)Factory.New<IBaseCusContainer>();
			cusContainer[CusContainerSchema.CO_JC] = declarationContainer.PK;
			cusContainer[CusContainerSchema.CO_JE] = declaration.PK;

			Factory.Save();

			AssertTypeLoaded(Factory, ObjectFactory.GetType<Enterprise.Integration.Forwarding.IForwardingContainer>(), forwardingContainer.PK);
			AssertTypeLoaded(Factory, ObjectFactory.GetType<CFS.ICFSContainer>(), cfsContainer.PK);
			AssertTypeLoaded(Factory, ObjectFactory.GetType<Enterprise.Integration.Forwarding.IForwardingContainer>(), quotedBookingContainer.PK);
			AssertTypeLoaded(Factory, ObjectFactory.GetType<Integration.Agency.IAgencyBookingContainer>(), agencyBookingContainer.PK);
			AssertTypeLoaded(Factory, ObjectFactory.GetType<Integration.Agency.IBillOfLadingContainer>(), billOfLadingContainer.PK);
			AssertTypeLoaded(Factory, ObjectFactory.GetType<ICommonContainer>(), invalidContainerWithoutParent.PK);
			AssertTypeLoaded(Factory, ObjectFactory.GetType<Enterprise.Integration.Forwarding.IForwardingContainer>(), declarationContainer.PK);

			var newFactory = new BusinessObjectFactory();

			AssertTypeLoaded(newFactory, ObjectFactory.GetType<Enterprise.Integration.Forwarding.IForwardingContainer>(), forwardingContainer.PK);
			AssertTypeLoaded(newFactory, ObjectFactory.GetType<CFS.ICFSContainer>(), cfsContainer.PK);
			AssertTypeLoaded(newFactory, ObjectFactory.GetType<Enterprise.Integration.Forwarding.IForwardingContainer>(), quotedBookingContainer.PK);
			AssertTypeLoaded(newFactory, ObjectFactory.GetType<Integration.Agency.IAgencyBookingContainer>(), agencyBookingContainer.PK);
			AssertTypeLoaded(newFactory, ObjectFactory.GetType<Integration.Agency.IBillOfLadingContainer>(), billOfLadingContainer.PK);
			AssertTypeLoaded(newFactory, ObjectFactory.GetType<ICommonContainer>(), invalidContainerWithoutParent.PK);
			AssertTypeLoaded(Factory, ObjectFactory.GetType<Enterprise.Integration.Forwarding.IForwardingContainer>(), declarationContainer.PK);
		}

		void AssertTypeLoaded(BusinessObjectFactory factory, Type expectedType, ZGuid containerPK)
		{
			AssertEquals("TypeDecider called when loading by type", expectedType, factory.Load<CommonContainer>(containerPK).GetType());
			AssertEquals("TypeDecider called when loading by table prefix", expectedType, factory.Load(JobContainerSchema.Constants.Prefix, containerPK).GetType());
		}

		public void TestGetTypeForNew()
		{
			AssertEquals(ObjectFactory.GetType<ICommonContainer>(), new ContainerTypeDecider().GetTypeForNew());
		}

		public void TestGetTypeForBinding()
		{
			AssertEquals(ObjectFactory.GetType<ICommonContainer>(), new ContainerTypeDecider().GetTypeForBinding());
		}

		public void TestGetTypeForLoad_RowDeleted()
		{
			var factory = new BusinessObjectFactory();
			var forwardingConsol = factory.New<Enterprise.Integration.Forwarding.IForwardingConsol>();
			var forwardingContainer = forwardingConsol.Containers.AddNew();
			factory.Save();

			factory.ChangeRowStatus = (row) => { row.Delete(); };

			var loadResut = factory.Load(typeof(CommonContainer), new ZQuery(JobContainerSchema.PK, forwardingContainer.PK));
			AssertEquals("container has been deleted during load", 0, loadResut.Length);
		}
	}
}
