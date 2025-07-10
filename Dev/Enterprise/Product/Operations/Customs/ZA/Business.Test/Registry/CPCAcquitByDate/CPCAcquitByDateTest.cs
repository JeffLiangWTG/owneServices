using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.DataRegistry.Business.Testing
{
	[TestedType(typeof(CPCAcquitByDate))]
	sealed class CPCAcquitByDateTest : RegistryBusinessObjectTemplateTestCase<CPCAcquitByDate>
	{
		public void TestDefaultValue()
		{
			var data = new CPCAcquitByDate();
			AssertEquals("default Quantity should be 24", 24, data.Quantity);
			AssertEquals("default Quantity should be MONTH(S)", "MONTH(S)", data.Unit);
		}

		public void TestValidateQuantity()
		{
			var data = new CPCAcquitByDate();
			CombineAssertions(() =>
			{
				data.Quantity = -1;
				AssertHasError("Quantity could not be -1", data.QuantityInfo, CPCAcquitByDate.ValidQuantityRequired);
				data.Quantity = 0;
				AssertEquals("no error when Quantity = 0", false, data.QuantityInfo.HasMessageErrors());
				data.Quantity = 1;
				AssertEquals("no error when Quantity = 1", false, data.QuantityInfo.HasMessageErrors());
			});
		}

		public void TestValidateUnit()
		{
			var data = new CPCAcquitByDate();
			CombineAssertions(() =>
			{
				data.Unit = "";
				AssertHasError("Unit could not be empty", data.UnitInfo, MandatoryValidation.MustBeEnteredMessage("Unit"));
				data.Unit = "Year(s)";
				AssertHasError("Unit should be in the list", data.UnitInfo, CPCAcquitByDate.ValidUnitRequired);
				data.Unit = "DAY(S)";
				AssertEquals("no error when Unit = 'DAY(S)", false, data.UnitInfo.HasMessageErrors());
				data.Unit = "MONTH(S)";
				AssertEquals("no error when Unit = 'MONTH(S)", false, data.UnitInfo.HasMessageErrors());
			});
		}

		protected override bool RequiresFactory => true;

		protected override bool RequiresFallbackLevel => true;

		protected override BusinessObject GetNewBusinessObject()
		{
			return new CPCAcquitByDate();
		}

		protected override CPCAcquitByDate GetBusinessObjectToClone()
		{
			return (CPCAcquitByDate)GetNewBusinessObject();
		}

		protected override CPCAcquitByDate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}
	}
}
