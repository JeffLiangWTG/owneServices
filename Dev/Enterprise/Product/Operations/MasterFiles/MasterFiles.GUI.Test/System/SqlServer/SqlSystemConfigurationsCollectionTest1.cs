using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(SqlSystemConfiguration))]
	sealed class SqlSystemConfigurationsCollectionTest1 : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new SqlSystemConfiguration();
		}
	}
}
