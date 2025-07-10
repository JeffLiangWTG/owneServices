using System;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(SupplementaryCodeCollection))]
sealed class SupplementaryCodeCollectionTest : BaseSupplementaryCodeCollectionAbstractTest<SupplementaryCode>
{
	public void TestConstructor() => CombineAssertions(() =>
	{
		var line = Factory.NewWithValidTestData<JobComInvoiceLine>();
		AssertExceptionThrown<ArgumentNullException>("When info is null", () => new SupplementaryCodeCollection(null, new SupplementaryCodeProviderForTest()));
		AssertExceptionThrown<ArgumentNullException>("When provider is null", () => new SupplementaryCodeCollection(line.JI_AdditionalSupplementsInfo, null));
	});

	protected override CusCodeDataCollection<SupplementaryCode> GetCusCodeDataCollection()
	{
		var invoiceLine = Factory.New<JobDeclaration>().InvoiceLines.AddNew();
		return invoiceLine.AdditionalSupplementaryCodes;
	}

	protected override BusinessObject GetNewElementToAddToTheCollection()
	{
		var result = Factory.New<SupplementaryCode>();
		result.CY_Order = 3;
		return result;
	}

	sealed class SupplementaryCodeProviderForTest : BaseSupplementaryCodeProvider
	{
		public SupplementaryCodeProviderForTest() : base(Constants.CountryCodes.Norway)
		{
		}
	}
}
