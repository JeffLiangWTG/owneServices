using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Manifest.Business.Testing;

static class PersistenceTestHelper
{
	public static void AssertValueIsPersistedInGenAddOnColumn<T>(BusinessObjectFactory factory, ZPropertyInfo propertyInfo, string columnName, T sampleValue)
		where T : IZType
	{
		var filterQuery = new ZQuery(GenAddOnColumnSchema.XA_Name, columnName);
		propertyInfo.Value = sampleValue;
		Assertion.AssertEquals($"{propertyInfo.Name} value", sampleValue, propertyInfo.Value);
		Assertion.AssertNotNull($"{propertyInfo.Name} is persisted in GenAddOnColumn", factory.LoadTop1<GenAddOnColumn>(filterQuery));
	}
}
