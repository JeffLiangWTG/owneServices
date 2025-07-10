using System.Reflection;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

[TestedType(typeof(NctsGuarantee))]
sealed class NctsGuaranteeTest : EnterpriseBusinessObjectTestCase
{
	public void TestCopyCustomsOfficeFromGuaranteeHeader()
	{
		var propertyInfo = typeof(NctsGuarantee).GetProperty("CopyCustomsOfficeFromGuaranteeHeader", BindingFlags.NonPublic | BindingFlags.Instance);
		AssertEquals(false, propertyInfo.GetValue(Factory.New<NctsGuarantee>()));
	}

	public void TestCopyPW_BondFiledPort()
	{
		var guarantee = Factory.New<NctsGuarantee>();
		guarantee.PW_BondFiledPort = "filled";
		var guaranteeCopy = Factory.New<NctsGuarantee>();
		guaranteeCopy.CopyPersistentValuesFrom(guarantee);
		AssertEquals("copied", string.Empty, guaranteeCopy.PW_BondFiledPort);
		guaranteeCopy.PW_BondFiledPort = "filled";
		AssertEquals("setter", "filled", guaranteeCopy.PW_BondFiledPort);
	}

	protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);
	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
	{
		var nctsHeader = factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		return nctsHeader.MovementHeader.Guarantees.AddNew();
	}
}
