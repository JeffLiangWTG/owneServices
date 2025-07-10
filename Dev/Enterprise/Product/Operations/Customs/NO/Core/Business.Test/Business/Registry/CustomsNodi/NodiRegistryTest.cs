using CargoWise.EntityFramework;
using Enterprise.Customs.Business.Testing;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Registry.Testing;

[TestedType(typeof(NodiRegistry))]
sealed class NodiRegistryTest : RegistryBusinessObjectTemplateTestCase<NodiRegistry>
{
	public void TestSystemName_ReadOnly()
	{
		AssertEquals(true, nodiRegistry.SystemNameInfo.ReadOnly);
	}

	public void TestSystemName_Caption()
	{
		AssertEquals("System", DataBoundResourceStrings.GetDataForProperty(nodiRegistry.SystemNameInfo).Caption);
	}

	public void TestNodiNumberCaption()
	{
		AssertEquals("NODI Id", DataBoundResourceStrings.GetDataForProperty(nodiRegistry.NodiNumberInfo).Caption);
	}

	public void TestValidateNodiNumber()
	{
		ValidationTestHelper.AssertErrorIfNotEntered(nodiRegistry.NodiNumberInfo);
	}

	public void TestCurrentCustomsProductionNodiNumber()
	{
		AssertEquals("NO000101", NodiRegistry.CurrentCustomsProductionNodiNumber);
	}

	public void TestCurrentCustomsTestNodiNumber()
	{
		AssertEquals("NO000119", NodiRegistry.CurrentCustomsTestNodiNumber);
	}

	public void TestCurrentNctsProductionNodiNumber()
	{
		AssertEquals("NCTS.PROD", NodiRegistry.CurrentNctsProductionNodiNumber);
	}

	public void TestCurrentNctsTestNodiNumber()
	{
		AssertEquals("NCTS.TEST", NodiRegistry.CurrentNctsTestNodiNumber);
	}

	protected override void SetUp()
	{
		base.SetUp();
		nodiRegistry = new NodiRegistry();
	}
	NodiRegistry nodiRegistry;

	protected override bool RequiresFactory => true;

	protected override bool RequiresFallbackLevel => true;

	protected override NodiRegistry GetBusinessObjectToClone() => GetBusinessObjectToSerialise();

	protected override NodiRegistry GetBusinessObjectToSerialise()
	{
		BizObj.SystemName = NodiRegistry.CustomsProductionSystemName;
		BizObj.NodiNumber = NodiRegistry.CustomsProductionDefaultNodiNumber;
		return BizObj;
	}
}

