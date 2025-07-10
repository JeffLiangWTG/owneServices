using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(MaximumCreditLimitDataType))]
	public class MaximumCreditLimitDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<MaximumCreditLimitDataType>
	{
		protected override string ExpectedEditorName => "MaximumCreditLimitRegistryItemEditor";

		protected override MaximumCreditLimitDataType GetNewDataType()
		{
			return new MaximumCreditLimitDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var factory = new BusinessObjectFactory();
			var currency1 = factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "CNY");
			var currency2 = factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "USD");
			factory.Save();

			var collection1 = new MaximumCreditLimitCollection();
			var maximumCreditLimitItem1 = collection1.AddNew();
			maximumCreditLimitItem1.CurrencyPK = currency1.PK;
			maximumCreditLimitItem1.CreditLimit = "100";
			maximumCreditLimitItem1.ComprehensiveReportEnabled = true;
			maximumCreditLimitItem1.CommercialBureauEnquiryEnabled = false;
			maximumCreditLimitItem1.LatePaymentRiskEnabled = false;
			maximumCreditLimitItem1.FailureRiskEnabled = false;

			var collection2 = new MaximumCreditLimitCollection();
			var maximumCreditLimitItem2 = collection2.AddNew();
			maximumCreditLimitItem2.CurrencyPK = currency2.PK;
			maximumCreditLimitItem2.CreditLimit = "200";
			maximumCreditLimitItem2.ComprehensiveReportEnabled = true;
			maximumCreditLimitItem2.CommercialBureauEnquiryEnabled = true;
			maximumCreditLimitItem2.LatePaymentRiskEnabled = true;
			maximumCreditLimitItem2.FailureRiskEnabled = true;

			return new[]
			{
				new ValidSampleAndBinaryValueInDB(collection1, new MaximumCreditLimitDataType().Serialise(collection1)),
				new ValidSampleAndBinaryValueInDB(collection2, new MaximumCreditLimitDataType().Serialise(collection2))
			};
		}
	}
}
