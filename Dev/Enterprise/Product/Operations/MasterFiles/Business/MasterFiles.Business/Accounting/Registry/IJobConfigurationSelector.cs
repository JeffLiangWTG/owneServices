using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public interface IJobConfigurationSelector
	{
		ZString DirectionCode { get; set; }
		bool DirectionCode_ReadOnly { get; }
		ZPropertyInfo DirectionCodeInfo { get; }
		CodeDescriptionPairList DirectionList { get; }
		ZString JobType { get; set; }
		ZPropertyInfo JobTypeInfo { get; }
		CodeDescriptionPairList JobTypeList { get; }
		ZString Mode { get; set; }
		bool Mode_ReadOnly { get; }
		ZPropertyInfo ModeInfo { get; }
		CodeDescriptionPairList ModeList { get; }
		IJobConfigurationSelector[] ParentCollectionForValidation { get; }
		JobConfigurationSelectorLookups Lookups { get; }
		void ValidateJobType();
		void ValidateDirectionCode();
		void ValidateMode();
	}
}
