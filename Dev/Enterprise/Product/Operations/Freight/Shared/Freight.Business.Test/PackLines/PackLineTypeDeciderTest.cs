using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business.Testing
{
	sealed class PackLineTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForLoad()
		{
			var forwardingShipment = (BusinessObject)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			var cfsShipment = (BusinessObject)Factory.New<Integration.CFS.ICFSShipment>();
			var agencyShipment = (BusinessObject)Factory.New<Integration.Agency.IAgencyShipment>();

			var forwardingPackline = (BusinessObject)Factory.New<Integration.Forwarding.IForwardingPackLine>();
			forwardingPackline[JobPackLinesSchema.JL_JS] = forwardingShipment.PK;

			var cfsPackline = (BusinessObject)Factory.New<Integration.CFS.ICFSPackLine>();
			cfsPackline[JobPackLinesSchema.JL_JS] = cfsShipment.PK;

			var agencyPackline = (BusinessObject)Factory.New<Integration.Agency.IAgencyPackLine>();
			agencyPackline[JobPackLinesSchema.JL_JS] = agencyShipment.PK;

			Factory.Save();

			AssertTypeLoaded(Factory, ObjectFactory.GetType<Integration.Forwarding.IForwardingPackLine>(), forwardingPackline.PK);
			AssertTypeLoaded(Factory, ObjectFactory.GetType<Integration.CFS.ICFSPackLine>(), cfsPackline.PK);
			AssertTypeLoaded(Factory, ObjectFactory.GetType<Integration.Agency.IAgencyPackLine>(), agencyPackline.PK);

			var newFactory = new BusinessObjectFactory();

			AssertTypeLoaded(newFactory, ObjectFactory.GetType<Integration.Forwarding.IForwardingPackLine>(), forwardingPackline.PK);
			AssertTypeLoaded(newFactory, ObjectFactory.GetType<Integration.CFS.ICFSPackLine>(), cfsPackline.PK);
			AssertTypeLoaded(newFactory, ObjectFactory.GetType<Integration.Agency.IAgencyPackLine>(), agencyPackline.PK);
		}

		void AssertTypeLoaded(BusinessObjectFactory factory, Type expectedType, ZGuid packlinePK)
		{
			AssertEquals("TypeDecider called when loading by type", expectedType, factory.Load<PackLine>(packlinePK).GetType());
			AssertEquals("TypeDecider called when loading by table prefix", expectedType, factory.Load(JobPackLinesSchema.Constants.Prefix, packlinePK).GetType());
		}

		public void TestGetTypeForNew()
		{
			AssertEquals(ObjectFactory.GetType<Integration.IPackLine>(), new PackLineTypeDecider().GetTypeForNew());
		}

		public void TestGetTypeForBinding()
		{
			AssertEquals(ObjectFactory.GetType<Integration.IPackLine>(), new PackLineTypeDecider().GetTypeForBinding());
		}
	}
}
