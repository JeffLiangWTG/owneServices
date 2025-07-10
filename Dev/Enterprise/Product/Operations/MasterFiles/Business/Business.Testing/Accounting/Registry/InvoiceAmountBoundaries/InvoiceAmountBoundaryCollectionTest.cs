using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing;

[TestedType(typeof(InvoiceAmountBoundaryCollection))]
sealed class InvoiceAmountBoundaryCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<InvoiceAmountBoundaryCollection>
{
	public void TestAddNewWithParameters()
	{
		var collection = new InvoiceAmountBoundaryCollection();
		var boundary = collection.AddNew(new ZDateTime(2024, 1, 1), new ZDateTime(2024, 12, 31), 12000m);

		AssertEquals("StartDate", new ZDateTime(2024, 1, 1), boundary.StartDate);
		AssertEquals("EndDate", new ZDateTime(2024, 12, 31), boundary.EndDate);
		AssertEquals("Amount", 12000m, boundary.Amount);
	}

	public void TestAddDefaultValues()
	{
		var  collection = new InvoiceAmountBoundaryCollection();
		AssertEquals(0, collection.Count);

		collection.AddDefaultValues();
		AssertEquals(5, collection.Count);
		AssertEquals("StartDate", new ZDateTime(2024, 5, 5), collection[0].StartDate);
		AssertEquals("EndDate", new ZDateTime(2024, 12, 31), collection[0].EndDate);
		AssertEquals("Amount", 25000m, collection[0].Amount);
	}

	protected override bool RequiresFactory
	{
		get { return false; }
	}

	protected override bool RequiresFallbackLevel
	{
		get { return false; }
	}

	protected override InvoiceAmountBoundaryCollection GetCollectionToTest()
	{
		return new InvoiceAmountBoundaryCollection();
	}

	protected override BusinessObject GetNewElementToAddToTheCollection()
	{
		return new InvoiceAmountBoundary();
	}
}

