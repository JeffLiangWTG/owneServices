using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.DataRegistry.Business.Testing
{
	[TestedType(typeof(DefaultStatementPrintDateRegistryDataType))]
	sealed class DefaultStatementPrintDateRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<DefaultStatementPrintDateRegistryDataType>
	{
		protected override string ExpectedEditorName
		{
			get { return "DefaultStatementPrintDateRegistryItemEditor"; }
		}

		protected override DefaultStatementPrintDateRegistryDataType GetNewDataType()
		{
			return new DefaultStatementPrintDateRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var statementPrintDateData1 = new DefaultStatementPrintDate();
			statementPrintDateData1.NumberOfDays = 0;
			statementPrintDateData1.DoDefaultPrelimStatementPrintDate = true;

			var statementPrintDateData2 = new DefaultStatementPrintDate();
			statementPrintDateData2.NumberOfDays = 1;
			statementPrintDateData2.DoDefaultPrelimStatementPrintDate = false;

			return
			[
				new ValidSampleAndBinaryValueInDB(statementPrintDateData1, new DefaultStatementPrintDateRegistryDataType().Serialise(statementPrintDateData1)),
				new ValidSampleAndBinaryValueInDB(statementPrintDateData2, new DefaultStatementPrintDateRegistryDataType().Serialise(statementPrintDateData2))
			];
		}
	}
}
