using System;
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class JobServiceWithExposedHashTable : JobService
	{
		public JobServiceWithExposedHashTable(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new Type GetTypeFromPrefix(string prefix)
		{
			Type type = base.GetTypeFromPrefix(prefix);
			PrefixToTypeHash["Z0"] = typeof(DummyWithServices);
			return type;
		}
	}
}
