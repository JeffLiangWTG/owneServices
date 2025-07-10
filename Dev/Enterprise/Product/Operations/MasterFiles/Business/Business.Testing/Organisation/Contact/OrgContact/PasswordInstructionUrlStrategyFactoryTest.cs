using System;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	class PasswordInstructionUrlStrategyFactoryTest : TestCaseWithDummy
	{
		public void TestGetStrategy()
		{
			ObjectFactory.Substitute("IPasswordInstructionUrlStrategy.Default", new DummyStrategy());
			ObjectFactory.Substitute("IPasswordInstructionUrlStrategy.Glow", new DummyStrategyGlow());

			var testCases = new (PasswordInstructionUrlType UrlType, Type ExpectedType)[]
			{
				(PasswordInstructionUrlType.Default, typeof(DummyStrategy)),
				(PasswordInstructionUrlType.Glow, typeof(DummyStrategyGlow)),
			};

			Array.ForEach(
				testCases,
				test =>
				{
					var strategy = PasswordInstructionUrlStrategyFactory.GetStrategy(test.UrlType);
					AssertEquals(test.ExpectedType, strategy.GetType());
				});
		}

		class DummyStrategy : IPasswordInstructionUrlStrategy
		{
			public string GenerateUrl(IPasswordInstructionEmailSource source, PasswordInstructionType instructionType, PasswordResetInfo passwordResetInfo)
				=> throw new NotImplementedException();
		}

		class DummyStrategyGlow : IPasswordInstructionUrlStrategy
		{
			public string GenerateUrl(IPasswordInstructionEmailSource source, PasswordInstructionType instructionType, PasswordResetInfo passwordResetInfo)
				=> throw new NotImplementedException();
		}
	}
}
