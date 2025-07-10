using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business.Testing
{
	sealed class ConsolProcessTaskLoadStrategyTest : TestCaseWithFactory
	{
		public void TestGetTypeForLoad()
		{
			var forwardingType = ObjectFactory.GetType<Enterprise.Integration.Forwarding.IForwardingConsolProcessTask>();
			var cfsType = ObjectFactory.GetType<Integration.CFS.ICFSLoadListConsolProcessTask>();

			AssertStrategyType(forwardingType, ZGuid.Empty);
			AssertStrategyType(forwardingType, ZGuid.Invalid);

			var loadList = (CommonConsol)Factory.New<Integration.CFS.ICFSLoadListConsol>();
			AssertStrategyType(cfsType, loadList.PK);

			var forwardingConsol = (CommonConsol)Factory.New<Enterprise.Integration.Forwarding.IForwardingConsol>();
			AssertStrategyType(forwardingType, forwardingConsol.PK);
		}

		void AssertStrategyType(Type expectedType, ZGuid parentID)
		{
			AssertEquals(expectedType, Strategy.GetTypeForLoad(JobConsolSchema.Constants.Prefix, parentID, Factory));
			Factory.Save();
			AssertEquals(expectedType, Strategy.GetTypeForLoad(JobConsolSchema.Constants.Prefix, parentID, new BusinessObjectFactory()));
		}

		ConsolProcessTaskLoadStrategy Strategy
		{
			get { return strategy ?? (strategy = new ConsolProcessTaskLoadStrategy()); }
		}

		ConsolProcessTaskLoadStrategy strategy;
	}
}
