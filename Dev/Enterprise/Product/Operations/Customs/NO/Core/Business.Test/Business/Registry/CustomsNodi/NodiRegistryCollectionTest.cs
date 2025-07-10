using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Registry.Testing;

[TestedType(typeof(NodiRegistryCollection))]
sealed class NodiRegistryCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<NodiRegistryCollection>
{
	public void TestAllowNew()
	{
		AssertEquals(false, collection.AllowNew);
	}

	public void TestAllowRemove()
	{
		AssertEquals(false, collection.AllowRemove);
	}

	public void TestDefaultValue_CustomsProductionSystemName()
	{
		AssertEquals("NO000101", collection.DefaultCollection.Cast<NodiRegistry>().Single(x => x.SystemName == NodiRegistry.CustomsProductionSystemName).NodiNumber);
	}

	public void TestDefaultValue_CustomsTestSystemName()
	{
		AssertEquals("NO000119", collection.DefaultCollection.Cast<NodiRegistry>().Single(x => x.SystemName == NodiRegistry.CustomsTestSystemName).NodiNumber);
	}

	public void TestDefaultValue_NctsProductionSystemName()
	{
		AssertEquals("NCTS.PROD", collection.DefaultCollection.Cast<NodiRegistry>().Single(x => x.SystemName == NodiRegistry.NctsProductionSystemName).NodiNumber);
	}

	public void TestDefaultValue_NctsTestSystemName()
	{
		AssertEquals("NCTS.TEST", collection.DefaultCollection.Cast<NodiRegistry>().Single(x => x.SystemName == NodiRegistry.NctsTestSystemName).NodiNumber);
	}

	protected override void SetUp()
	{
		base.SetUp();
		collection = new NodiRegistryCollection();
	}
	NodiRegistryCollection collection;

	protected override bool RequiresFactory => true;

	protected override bool RequiresFallbackLevel => false;

	protected override NodiRegistryCollection GetCollectionToTest() => new NodiRegistryCollection();

	protected override BusinessObject GetNewElementToAddToTheCollection() => new NodiRegistry();
}
