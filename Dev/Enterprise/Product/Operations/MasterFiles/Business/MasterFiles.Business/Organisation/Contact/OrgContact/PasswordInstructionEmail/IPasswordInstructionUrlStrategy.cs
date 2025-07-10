using System;
using CargoWise.Application;

namespace Enterprise.MasterFiles.Business
{
	public enum PasswordInstructionUrlType
	{
		Default,
		Glow,
	}

	public interface IPasswordInstructionUrlStrategy
	{
		string GenerateUrl(IPasswordInstructionEmailSource source, PasswordInstructionType instructionType, PasswordResetInfo passwordResetInfo);
	}

	public static class PasswordInstructionUrlStrategyFactory
	{
		public static IPasswordInstructionUrlStrategy GetStrategy(PasswordInstructionUrlType type)
		{
			var strategyName = $"{nameof(IPasswordInstructionUrlStrategy)}.{type}";
			return ObjectFactory.Get<IPasswordInstructionUrlStrategy>(strategyName)
				?? throw new NotSupportedException($"Unsupported password instraction strategy type: {type}");
		}
	}
}
