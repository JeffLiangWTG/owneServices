using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

[TestedType(typeof(TranCircumstanceCollection))]
class TranCircumstanceCollectionTest : CusCodeDataCollectionTest<TranCircumstance>
{
	public void TestAsString()
	{
		var declaration = Factory.New<JobDeclaration>();
		var header = declaration.Invoices.AddNew();
		var collection = new TranCircumstanceCollection(header);
		AssertEquals(0, collection.Count);
		collection.AsString = "A1111,A2222,A3333";
		AssertEquals(3, collection.Count);
		collection.AddNew("B1111");
		AssertEquals("A1111,A2222,A3333,B1111", collection.AsString);
	}

	public void TestDefaultOfCY_Order()
	{
		var declaration = Factory.New<JobDeclaration>();
		var header = declaration.Invoices.AddNew();
		var collection = new TranCircumstanceCollection(header);
		AssertEquals((short)1, collection.AddNew().CY_Order);
		AssertEquals((short)2, collection.AddNew().CY_Order);
		AssertEquals((short)3, collection.AddNew().CY_Order);
	}

	protected override BusinessObject GetNewElementToAddToTheCollection()
	{
		var result = Factory.New<TranCircumstance>();
		result.CY_Order = 3;
		return result;
	}

	protected override CusCodeDataCollection<TranCircumstance> GetCusCodeDataCollection()
	{
		return Factory.New<JobDeclaration>().Invoices.AddNew().AdditionalTranCircumstanceCodes;
	}
}
