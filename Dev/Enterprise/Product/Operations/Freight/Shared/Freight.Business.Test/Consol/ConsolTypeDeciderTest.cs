using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Integration;
using Enterprise.Integration.Freight;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business.Testing
{
	sealed class ConsolTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForLoad()
		{
			var forwardingConsol = (BusinessObject)Factory.New<Enterprise.Integration.Forwarding.IForwardingConsol>();
			var cfsConsol = (BusinessObject)Factory.New<CFS.ICFSLoadListConsol>();

			var notForwardingNotCFSConsol = (BusinessObject)Factory.New<ICommonConsol>();
			notForwardingNotCFSConsol[JobConsolSchema.JK_IsForwarding] = false;
			notForwardingNotCFSConsol[JobConsolSchema.JK_IsCFS] = false;

			Factory.Save();

			AssertTypeLoaded(Factory, ObjectFactory.GetType<Enterprise.Integration.Forwarding.IForwardingConsol>(), forwardingConsol.PK);
			AssertTypeLoaded(Factory, ObjectFactory.GetType<CFS.ICFSLoadListConsol>(), cfsConsol.PK);
			AssertTypeLoaded(Factory, ObjectFactory.GetType<ICommonConsol>(), notForwardingNotCFSConsol.PK);

			var newFactory = new BusinessObjectFactory();

			AssertTypeLoaded(newFactory, ObjectFactory.GetType<Enterprise.Integration.Forwarding.IForwardingConsol>(), forwardingConsol.PK);
			AssertTypeLoaded(newFactory, ObjectFactory.GetType<CFS.ICFSLoadListConsol>(), cfsConsol.PK);
			AssertTypeLoaded(newFactory, ObjectFactory.GetType<ICommonConsol>(), notForwardingNotCFSConsol.PK);
		}

		public void TestGetTypeForLoad_SimultaneousCFSAndForwarding_TryToUseFactoryCacheFirst()
		{
			var consol = (BusinessObject)Factory.New<ICommonConsol>();
			consol[JobConsolSchema.JK_IsForwarding] = true;
			consol[JobConsolSchema.JK_IsCFS] = true;

			Factory.Save();

			var anotherFactory = new BusinessObjectFactory();
			var explicitlyLoadedForwardingConsol = anotherFactory.Load<Enterprise.Integration.Forwarding.IForwardingConsol>(consol.PK);
			AssertTypeLoaded(anotherFactory, ObjectFactory.GetType<Enterprise.Integration.Forwarding.IForwardingConsol>(), consol.PK);

			var yetAnotherFactory = new BusinessObjectFactory();
			var explicitlyLoadedCFSConsol = yetAnotherFactory.Load<CFS.ICFSLoadListConsol>(consol.PK);
			AssertTypeLoaded(yetAnotherFactory, ObjectFactory.GetType<CFS.ICFSLoadListConsol>(), consol.PK);
		}

		public void TestGetTypeForLoad_SimultaneousCFSAndForwarding_CheckCFSContextService()
		{
			var consol = (BusinessObject)Factory.New<ICommonConsol>();
			consol[JobConsolSchema.JK_IsForwarding] = true;
			consol[JobConsolSchema.JK_IsCFS] = true;

			Factory.Save();

			var anotherFactory = new BusinessObjectFactory();
			AssertEquals("Prerequisite: factory should not be in CFS context by default", FreightDomainContext.Unspecified, anotherFactory.GetFreightDomainContext());
			AssertTypeLoaded(anotherFactory, ObjectFactory.GetType<Enterprise.Integration.Forwarding.IForwardingConsol>(), consol.PK);

			anotherFactory = new BusinessObjectFactory();
			anotherFactory.SetFreightDomainContext(FreightDomainContext.CFS);

			AssertEquals("Prerequisite: factory is in CFS context", FreightDomainContext.CFS, anotherFactory.GetFreightDomainContext());
			AssertTypeLoaded(anotherFactory, ObjectFactory.GetType<CFS.ICFSLoadListConsol>(), consol.PK);
		}

		public void TestGetTypeForLoad_WhenVoyageRelatedJobsAreCachedInFactory()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			var sailing = Factory.NewWithValidTestData<JobSailing>();
			consol.Transports[0].JW_JX = sailing.PK;

			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var reloadedSailing = factory2.Load<JobSailing>(sailing.PK);
			var relatedJob = reloadedSailing.Voyage.RelatedJobs[0];

			var cachedRelatedJob = factory2.GetBizOsForPK(relatedJob.PK.ToGuid())[0];
			AssertEquals("Precondition: relatedJob is cached in factory2", typeof(VoyageRelatedJob), cachedRelatedJob.GetType());
			AssertEquals("Precondition: relatedJob has same PK as consol", cachedRelatedJob.PK, consol.PK);
			AssertTypeLoaded(factory2, ObjectFactory.GetType<Enterprise.Integration.Forwarding.IForwardingConsol>(), cachedRelatedJob.PK);
		}

		void AssertTypeLoaded(BusinessObjectFactory factory, Type expectedType, ZGuid consolPK)
		{
			AssertEquals("TypeDecider called when loading by type", expectedType, factory.Load<CommonConsol>(consolPK).GetType());
			AssertEquals("TypeDecider called when loading by table prefix", expectedType, factory.Load(JobConsolSchema.Constants.Prefix, consolPK).GetType());
		}

		public void TestGetTypeForNew()
		{
			AssertEquals(ObjectFactory.GetType<ICommonConsol>(), new ConsolTypeDecider().GetTypeForNew());
		}

		public void TestGetTypeForBinding()
		{
			AssertEquals(ObjectFactory.GetType<ICommonConsol>(), new ConsolTypeDecider().GetTypeForBinding());
		}
	}
}
