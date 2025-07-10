using System;
using System.Linq;
using CargoWise.Common;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GLJournalExchangeRateTypeRegistryDataType))]
	sealed class GLJournalExchangeRateTypeRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<GLJournalExchangeRateTypeRegistryDataType>
	{
		protected override string ExpectedEditorName => "GLJournalExchangeRateTypeRegistryItemEditor";

		protected override GLJournalExchangeRateTypeRegistryDataType GetNewDataType() => new GLJournalExchangeRateTypeRegistryDataType();

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var itemsSystemLevel = AccountingMasterFilesRegistry.Instance.CurrencyExchangeRateTypes.Value;
			itemsSystemLevel.Cast<CodeDescriptionBool>()
				.ForEach(x =>
				{
					x.Bool = true;
				});
			AccountingMasterFilesRegistry.Instance.CurrencyExchangeRateTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, itemsSystemLevel);

			var gLJournalExchangeRateType1 = new GLJournalExchangeRateType();
			gLJournalExchangeRateType1.BalanceSheetAccountTypeExchangeRateType = "PER";
			gLJournalExchangeRateType1.ProfitAndLossAccountTypeExchangeRateType = "PER";
			var byteArray1 = DataType.Serialise(gLJournalExchangeRateType1);

			var gLJournalExchangeRateType2 = new GLJournalExchangeRateType();
			gLJournalExchangeRateType2.BalanceSheetAccountTypeExchangeRateType = "CUS";
			gLJournalExchangeRateType2.ProfitAndLossAccountTypeExchangeRateType = "CUS";
			var byteArray2 = DataType.Serialise(gLJournalExchangeRateType2);

			return new[]
			{
				new ValidSampleAndBinaryValueInDB(gLJournalExchangeRateType1, byteArray1)
				, new ValidSampleAndBinaryValueInDB(gLJournalExchangeRateType2, byteArray2)
			};
		}
	}
}
