using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(SqlSystemConfigurationsCollection))]
	sealed class SqlSystemConfigurationsCollectionTest : NonPersistentBusinessObjectCollectionTestCase<SqlSystemConfigurationsCollection>
	{
		protected override SqlSystemConfigurationsCollection GetCollectionToTest()
		{
			return new SqlSystemConfigurationsCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new SqlSystemConfiguration();
		}
	}
}
