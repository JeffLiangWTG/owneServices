using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	[TestsSubclassesOf(typeof(ICommercialInvoiceFormLayoutProvider))]
	public abstract class CommercialInvoiceFormLayoutProviderAbstractTest<TLayoutProvider, JobComInvoiceHeader> : TestCaseWithFactory
		where TLayoutProvider : ICommercialInvoiceFormLayoutProvider, new()
		where JobComInvoiceHeader : BaseJobComInvoiceHeader
	{
		public void TestGetInvoiceHeaderDetailsLayout()
		{
			AssertEquals("GetInvoiceHeaderDetailsLayout", ExpectedCommercialInvoiceHeaderDetailsLayoutType, provider.GetInvoiceHeaderDetailsLayout(GetNewDeclaration())?.GetType());
		}
		protected virtual BaseJobComInvoiceHeader GetNewDeclaration() => Factory.New<BaseJobComInvoiceHeader>();

		protected ICommercialInvoiceFormLayoutProvider GetCommercialInvoiceFormLayoutProviderForTesting() => new TLayoutProvider();

		protected abstract Type ExpectedCommercialInvoiceHeaderDetailsLayoutType { get; }

		protected override void SetUp()
		{
			base.SetUp();
			provider = GetCommercialInvoiceFormLayoutProviderForTesting();
		}
		protected ICommercialInvoiceFormLayoutProvider provider;
	}
}
