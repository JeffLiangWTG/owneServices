using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.US.DataRegistry.Business.Testing
{
	[TestedType(typeof(DefaultStatementPrintDateRegistryItem))]
	sealed class DefaultStatementPrintDateRegistryItemTest : StronglyTypedRegistryItemTestCaseWithFactory<DefaultStatementPrintDate>
	{
		public void TestRegistryOptions()
		{
			AssertEquals("Options", RegistryOptions.CannotCallParameterlessValueGetter, new DefaultStatementPrintDateRegistryItem("DUMMY", (NoResString)"DUMMY", (NoResString)"DUMMY", (NoResString)"DUMMY").Options);
		}

		protected override StronglyTypedRegistryItem<DefaultStatementPrintDate, DefaultStatementPrintDate> GetNewRegistryItem()
		{
			return new DefaultStatementPrintDateRegistryItem("", null, null, null);
		}

		protected override DefaultStatementPrintDate ValidValue
		{
			get
			{
				DefaultStatementPrintDate statementPrintDateData = new DefaultStatementPrintDate();
				statementPrintDateData.NumberOfDays = 0;
				statementPrintDateData.DoDefaultPrelimStatementPrintDate = true;
				return statementPrintDateData;
			}
		}
	}
}
