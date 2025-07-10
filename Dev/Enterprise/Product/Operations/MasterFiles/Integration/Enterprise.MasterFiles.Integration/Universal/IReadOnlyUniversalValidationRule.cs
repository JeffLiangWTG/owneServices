using CargoWise.Types;

namespace Enterprise.MasterFiles.Integration
{
	public interface IReadOnlyUniversalValidationRule
	{
		ZString BusinessRule { get; }
		ZBool IsActive { get; }
		ZString MessageLog { get; }
		ZShort Sequence { get; }
		ZString Status { get; }
		ZBool IsError { get; }
	}
}
