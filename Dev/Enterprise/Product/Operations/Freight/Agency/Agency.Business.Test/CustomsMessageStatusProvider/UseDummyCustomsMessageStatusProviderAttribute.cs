using System;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	public sealed class UseDummyCustomsMessageStatusProviderAttribute : TestSetupAttribute
	{
		public static bool UseDummy
		{
			get
			{
				return useDummy;
			}
		}

		[ThreadStatic]
		static bool useDummy;
		public override void SetUp(TestCase testCase)
		{
			DummyCustomsMessageStatusProvider.Reset();
			useDummy = true;
			CustomsMessageStatusProviderFactory.useCustomsMessageStatusProviderFactory_ForTesting = true;
		}

		public override void TearDown(TestCase testCase)
		{
			useDummy = false;
			CustomsMessageStatusProviderFactory.useCustomsMessageStatusProviderFactory_ForTesting = false;
			DummyCustomsMessageStatusProvider.Reset();
		}
	}
}
