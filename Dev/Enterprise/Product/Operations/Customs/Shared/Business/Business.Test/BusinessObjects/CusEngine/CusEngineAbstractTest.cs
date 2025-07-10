using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing;

[TestsSubclassesOf(typeof(CusEngine))]
public abstract class CusEngineAbstractTest : EnterpriseBusinessObjectTestCase
{
	public void TestLookupsType()
	{
		var engine = (CusEngine)GetNewBusinessObject();
		AssertType(ExpectedLookupsType, engine.Lookups);
	}

	public void TestValidationType()
	{
		var engine = (CusEngine)GetNewBusinessObject();
		AssertType(ExpectedValidationType, engine.Validation);
	}

	protected abstract Type ExpectedLookupsType { get; }
	protected abstract Type ExpectedValidationType { get; }
	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject(factory);
	protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject(Factory);
	protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObject(Factory);

	protected virtual BusinessObject GetEngineParent(BusinessObjectFactory factory)
	{
		var vehicle = factory.New<BaseJobDeclaration>().Invoices.AddNew().InvoiceLines.AddNew().Vehicles.AddNew();
		vehicle.CVH_VehicleIdentificationNumber = "VIN1234";
		return vehicle;
	}

	CusEngine GetNewBusinessObject(BusinessObjectFactory factory)
	{
		var parent = GetEngineParent(factory);
		var engine = (CusEngine)factory.NewWithValidTestData(ExpectedBusinessObjectType);

		engine.CEG_ParentID = parent.PK;
		engine.CEG_ParentTableCode = parent.TablePrefix;

		return engine;
	}
}
