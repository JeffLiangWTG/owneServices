using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(DefaultOSMGDataType))]
	sealed class DefaultOSMGDataTypeNonPersistentTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<DefaultOSMGDataType>
	{
		protected override string ExpectedEditorName => "DefaultOSMGRegistryItemEditor";

		protected override DefaultOSMGDataType GetNewDataType()
		{
			return new DefaultOSMGDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var result = new DefaultOSMG();
			result.OrgSecurityGroup = new CargoWise.Types.ZGuid("BD758B3D-0AE2-475B-985A-DE0067EA442A");

			var result2 = new DefaultOSMG();
			result2.OrgSecurityGroup = new CargoWise.Types.ZGuid("D92568D9-9C2D-42EE-A0DC-7C0209B27B36");

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(result, new DefaultOSMGDataType().Serialise(result)),
				new ValidSampleAndBinaryValueInDB(result2, new DefaultOSMGDataType().Serialise(result2))
			};
		}
	}
}
