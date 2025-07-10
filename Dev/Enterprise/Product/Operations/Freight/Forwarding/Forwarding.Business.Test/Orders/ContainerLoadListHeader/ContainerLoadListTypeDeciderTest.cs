using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration.Freight;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Freight.Integration.Forwarding;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	sealed class ContainerLoadListTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForLoad()
		{
			var forwardingContainerLoadList = (BusinessObject)Factory.New<ICYContainerLoadList>();
			forwardingContainerLoadList.FillWithValidTestData();

			var forwardingCFSContainerLoadList = (BusinessObject)Factory.New<ICFSContainerLoadList>();
			forwardingCFSContainerLoadList.FillWithValidTestData();

			var commonContainerLoadList = (BusinessObject)Factory.New<ICommonContainerLoadList>();
			commonContainerLoadList[ContainerLoadListHeaderSchema.CLH_PlannedTransportMode] = "SEA";
			commonContainerLoadList[ContainerLoadListHeaderSchema.CLH_LoadListId] = "JOB01";
			commonContainerLoadList[ContainerLoadListHeaderSchema.CLH_LoadMode] = "CFS";

			Factory.Save();

			AssertTypeLoaded(Factory, ObjectFactory.GetType<ICYContainerLoadList>(), forwardingContainerLoadList.PK);
			AssertTypeLoaded(Factory, ObjectFactory.GetType<ICFSContainerLoadList>(), forwardingCFSContainerLoadList.PK);
			AssertTypeLoaded(Factory, ObjectFactory.GetType<ICommonContainerLoadList>(), commonContainerLoadList.PK);

			var newFactory = new BusinessObjectFactory();

			AssertTypeLoaded(newFactory, ObjectFactory.GetType<ICYContainerLoadList>(), forwardingContainerLoadList.PK);
			AssertTypeLoaded(newFactory, ObjectFactory.GetType<ICFSContainerLoadList>(), forwardingCFSContainerLoadList.PK);
		}

		void AssertTypeLoaded(BusinessObjectFactory factory, Type expectedType, ZGuid clhPK)
		{
			AssertEquals("TypeDecider called when loading by type", expectedType, factory.Load<CommonContainerLoadList>(clhPK).GetType());
			AssertEquals("TypeDecider called when loading by table prefix", expectedType, factory.Load(ContainerLoadListHeaderSchema.Constants.Prefix, clhPK).GetType());
		}

		public void TestGetTypeForNew()
		{
			AssertEquals(ObjectFactory.GetType<ICommonContainerLoadList>(), new ContainerLoadListTypeDecider().GetTypeForNew());
		}

		public void TestGetTypeForBinding()
		{
			AssertEquals(ObjectFactory.GetType<ICommonContainerLoadList>(), new ContainerLoadListTypeDecider().GetTypeForBinding());
		}
	}
}
