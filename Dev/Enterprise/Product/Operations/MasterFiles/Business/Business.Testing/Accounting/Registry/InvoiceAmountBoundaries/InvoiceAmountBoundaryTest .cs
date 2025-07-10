using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing;

[TestedType(typeof(InvoiceAmountBoundary))]
sealed class InvoiceAmountBoundaryTest : RegistryBusinessObjectTemplateTestCase<InvoiceAmountBoundary>
{
	public void TestValidateStartDate_Empty()
	{
		var collection = new InvoiceAmountBoundaryCollection();
		var b = collection.AddNew();

		b.StartDate = ZDateTime.Empty;
		b.EndDate = new ZDateTime(2024, 10, 15);
		AssertHasErrorContaining("Empty", b.StartDateInfo, MandatoryValidation.MustBeEntered);

		b.StartDate = new ZDateTime(2024, 1, 2);
		AssertNoErrorContaining("Not empty", b.StartDateInfo, MandatoryValidation.MustBeEntered);
	}

	public void TestValidateStartDate_Overlap()
	{
		var collection = new InvoiceAmountBoundaryCollection();
		var b1 = collection.AddNew();
		b1.StartDate = new ZDateTime(2024, 4, 10);
		b1.EndDate = new ZDateTime(2024, 10, 15);
		b1.Amount = 5000m;

		var b2 = collection.AddNew();
		b2.EndDate = new ZDateTime(2024, 12, 31);
		b2.StartDate = new ZDateTime(2024, 1, 1);

		const string errorMessage = $"Other amount's dates: '10-Apr-24' and '15-Oct-24'.";

		AssertHasErrorContaining(b2.StartDateInfo, "This amount's dates overlap another amount's dates.");
		AssertHasErrorContaining("Start date is before another period", b2.StartDateInfo, errorMessage);

		b2.StartDate = new ZDateTime(2024, 6, 1);
		AssertHasErrorContaining(b2.StartDateInfo, "This amount's dates overlap another amount's dates.");
		AssertHasErrorContaining("Start date is in the middle of another period", b2.StartDateInfo, errorMessage);

		b2.StartDate = new ZDateTime(2024, 11, 1);
		AssertNoErrorContaining(b2.StartDateInfo, "This amount's dates overlap another amount's dates.");
		AssertNoErrorContaining("No overlap", b1.StartDateInfo, errorMessage);
	}

	public void TestValidateEndDate_Empty()
	{
		var collection = new InvoiceAmountBoundaryCollection();
		var b = collection.AddNew();

		b.StartDate = new ZDateTime(2024, 6, 15);
		b.EndDate = ZDateTime.Empty;
		AssertHasErrorContaining("Empty", b.EndDateInfo, MandatoryValidation.MustBeEntered);

		b.EndDate = new ZDateTime(2025, 1, 2);
		AssertNoErrorContaining("Not empty", b.EndDateInfo, MandatoryValidation.MustBeEntered);
	}

	public void TestValidateEndDate_Overlap()
	{
		var collection = new InvoiceAmountBoundaryCollection();
		var b1 = collection.AddNew();
		b1.StartDate = new ZDateTime(2024, 4, 10);
		b1.EndDate = new ZDateTime(2024, 10, 15);

		var b2 = collection.AddNew();
		b2.StartDate = new ZDateTime(2024, 1, 1);
		b2.EndDate = new ZDateTime(2024, 12, 31);

		string errorMessage = $"Other amount's dates: '10-Apr-24' and '15-Oct-24'.";

		AssertHasErrorContaining(b2.EndDateInfo, "This amount's dates overlap another amount's dates.");
		AssertHasErrorContaining("End date is after another period ", b2.EndDateInfo, errorMessage);

		b2.EndDate = new ZDateTime(2024, 6, 1);
		AssertHasErrorContaining(b2.EndDateInfo, "This amount's dates overlap another amount's dates.");
		AssertHasErrorContaining("End date is in the middle of another period", b2.EndDateInfo, errorMessage);

		b2.EndDate = new ZDateTime(2024, 1, 1);
		AssertNoErrorContaining(b2.EndDateInfo, "This amount's dates overlap another amount's dates.");
		AssertNoErrorContaining("No overlap", b2.EndDateInfo, errorMessage);
	}

	public void TestValidateAmount()
	{
		var boundary = new InvoiceAmountBoundary { Amount = -5000m };
		AssertHasError(boundary.AmountInfo, "Amount must be greater or equal to 0.");

		boundary.Amount = 0m;
		AssertNoError(boundary.AmountInfo, "Amount must be greater or equal to 0.");
	}

	public void TestIsWithinDateAmount()
	{
		var boundary = new InvoiceAmountBoundary
		{
			StartDate = new ZDateTime(2025, 4, 1),
			EndDate = new ZDateTime(2025, 6, 30)
		};

		AssertEquals(false, boundary.IsWithinDateAmount(new ZDateTime(2024, 3, 31)));
		AssertEquals(true, boundary.IsWithinDateAmount(new ZDateTime(2025, 4, 1)));
		AssertEquals(true, boundary.IsWithinDateAmount(new ZDateTime(2025, 5, 1)));
		AssertEquals(true, boundary.IsWithinDateAmount(new ZDateTime(2025, 6, 30)));
		AssertEquals(false, boundary.IsWithinDateAmount(new ZDateTime(2025, 7, 1)));
	}

	protected override InvoiceAmountBoundary GetBusinessObjectToClone()
	{
		var boundary = new InvoiceAmountBoundary();
		boundary.StartDate = new ZDateTime(2024, 3, 18);
		boundary.EndDate = new ZDateTime(2024, 8, 20);
		boundary.Amount = 15000m;
		return boundary;
	}

	protected override InvoiceAmountBoundary GetBusinessObjectToSerialise()
	{
		return GetBusinessObjectToClone();
	}

	protected override bool RequiresFactory => false;

	protected override bool RequiresFallbackLevel => false;
}
