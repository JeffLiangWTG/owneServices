using Enterprise.ZArchitecture.Core.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class InvoiceTypeLayoutListTest : CodeDescriptionPairTest
	{
		class InvoiceTypeLayoutListForTest : InvoiceTypeLayoutList
		{
			static new InvoiceTypeLayoutListForTest New()
			{
				return new InvoiceTypeLayoutListForTest();
			}

			public static void RegisterThisSubTypeOverride()
			{
				OverridableNewDelegate.Value = new NewDelegate(New);
			}

			public static void UnregisterThisSubTypeOverride()
			{
				OverridableNewDelegate.ResetValue();
			}
		}

		public void TestNew()
		{
			InvoiceTypeLayoutListForTest.RegisterThisSubTypeOverride();
			InvoiceTypeLayoutList list = InvoiceTypeLayoutList.New();
			AssertEquals("Must have type TestInvoiceTypeLayoutList", typeof(InvoiceTypeLayoutListForTest), list.GetType());

			InvoiceTypeLayoutListForTest.UnregisterThisSubTypeOverride();
			list = InvoiceTypeLayoutList.New();
			AssertEquals("Must have type InvoiceTypeLayoutList", typeof(InvoiceTypeLayoutList), list.GetType());
		}
	}
}
