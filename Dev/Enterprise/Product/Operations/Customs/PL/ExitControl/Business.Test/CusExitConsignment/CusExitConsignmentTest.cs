using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.PL.ExitControl.Business.Testing;

[TestedType(typeof(CusExitConsignment))]
sealed class CusExitConsignmentTest : EnterpriseBusinessObjectTestCase
{
	protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObject(Factory).consignment;

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject(factory).consignment;

	protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject(Factory).consignment;

	public static (CusExitConsignment consignment, CusExitHeader header) GetNewBusinessObject(BusinessObjectFactory factory)
	{
		var header = CusExitHeaderTest.GetNewBusinessObject(factory);
		var consignment = header.CusExitConsignments.AddNew();
		return (consignment, header);
	}
}
