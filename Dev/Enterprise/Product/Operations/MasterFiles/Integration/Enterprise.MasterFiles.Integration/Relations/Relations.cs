using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Integration
{
	public struct RelationValidationResult
	{
		public RelationValidationResult(ZBool isValid)
			: this(isValid, (NoResString)string.Empty)
		{
		}

		public RelationValidationResult(ZBool isValid, MultilingualString reason)
		{
			IsValid = isValid;
			Reason = reason;
		}

		public readonly ZBool IsValid;
		public readonly MultilingualString Reason;
	}

	public struct RelationUpdateResult
	{
		public RelationUpdateResult(ZBool success)
			: this(success, (NoResString)string.Empty)
		{
		}

		public RelationUpdateResult(ZBool success, MultilingualString reason)
		{
			Success = success;
			Reason = reason;
		}

		public readonly ZBool Success;
		public readonly MultilingualString Reason;
	}
}
