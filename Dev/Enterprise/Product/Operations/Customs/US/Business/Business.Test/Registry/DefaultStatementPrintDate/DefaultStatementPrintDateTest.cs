using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.DataRegistry.Business.Testing
{
	[TestedType(typeof(DefaultStatementPrintDate))]
	sealed class DefaultStatementPrintDateTest : RegistryBusinessObjectTemplateTestCase<DefaultStatementPrintDate>
	{
		public void TestNumberOfDaysAndValidation()
		{
			DefaultStatementPrintDate obj = new DefaultStatementPrintDate();
			obj.DoDefaultPrelimStatementPrintDate = true;
			obj.NumberOfDays = 3;
			AssertEquals("NumberOfDays setter OK", 3, obj.NumberOfDays);

			obj.NumberOfDays = -21;
			AssertHasError(obj.NumberOfDaysInfo, DefaultStatementPrintDate.NumberOfDaysCannotBeNegative);

			obj.NumberOfDays = 9;
			AssertNoError(obj.NumberOfDaysInfo, DefaultStatementPrintDate.NumberOfDaysCannotBeNegative);

			obj.NumberOfDays = 11;
			AssertHasError(obj.NumberOfDaysInfo, DefaultStatementPrintDate.NumberOfDaysCannotBeGreaterThan10);

			obj.NumberOfDays = 10;
			AssertNoError(obj.NumberOfDaysInfo, DefaultStatementPrintDate.NumberOfDaysCannotBeGreaterThan10);
		}

		#region Implementation

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override DefaultStatementPrintDate GetBusinessObjectToClone()
		{
			DefaultStatementPrintDate result = new DefaultStatementPrintDate();
			result.DoDefaultPrelimStatementPrintDate = false;
			result.NumberOfDays = 0;
			return result;
		}

		protected override DefaultStatementPrintDate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}
		#endregion
	}
}
