using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public interface IRevenueRecognition : IJobConfigurationSelector
	{
		ZString BrokerCode { get; set; }
		bool BrokerCode_ReadOnly { get; }
		ZPropertyInfo BrokerCodeInfo { get; }
		CodeDescriptionPairList BrokerList { get; }
		ZInt Offset { get; set; }
		bool Offset_ReadOnly { get; }
		ZPropertyInfo OffsetInfo { get; }
		ZString OffsetType { get; set; }
		bool OffsetType_ReadOnly { get; }
		ZPropertyInfo OffsetTypeInfo { get; }
		CodeDescriptionPairList OffsetTypeList { get; }
		ZString RecognitionDateOptionCode { get; set; }
		ZPropertyInfo RecognitionDateOptionCodeInfo { get; }
		CodeDescriptionPairList RecognitionDateOptionList { get; }
		RevenueRecognitionLookups RevenueRecognitionLookups { get; }
		void ValidateRecognitionDateOptionCode();
		void ValidateOffset();
		void ValidateOffsetType();
		void ValidateBrokerCode();
	}
}
