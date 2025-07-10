using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.DataRegistry.Business.Testing
{
	[TestedType(typeof(AutoSendStatementDateChangeRequestRegistryDataType))]
	sealed class AutoSendStatementDateChangeRequestRegistryDataTypeNonPersistentTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<AutoSendStatementDateChangeRequestRegistryDataType>
	{
		protected override string ExpectedEditorName => "AutoSendStatementDateChangeRequestRegistryItemEditor";

		protected override AutoSendStatementDateChangeRequestRegistryDataType GetNewDataType()
		{
			return new AutoSendStatementDateChangeRequestRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var overrideByAll = new AutoSendStatementDateChangeRequest();
			overrideByAll.OverrideAllOrByOrganisation = "ALL";

			var overrideByOrg = new AutoSendStatementDateChangeRequest();
			overrideByOrg.OverrideAllOrByOrganisation = "ORG";

			var byteArraryAll = new byte[]
			{
				60,0,63,0,120,0,109,0,108,0,32,0,118,0,101,0,114,0,115,0,105,0,111,0,110,0,61,0,34,0,49,0,46,0,48,0,34,0,32,0,101,0,110,0,99,0,111,0,100,0,105,0,110,0,103,0,61,0,34,0,117,0,116,0,102,
				0,45,0,49,0,54,0,34,0,63,0,62,0,60,0,65,0,117,0,116,0,111,0,83,0,101,0,110,0,100,0,83,0,116,0,97,0,116,0,101,0,109,0,101,0,110,0,116,0,68,0,97,0,116,0,101,0,67,0,104,0,97,0,110,0,103,
				0,101,0,82,0,101,0,113,0,117,0,101,0,115,0,116,0,62,0,60,0,79,0,118,0,101,0,114,0,114,0,105,0,100,0,101,0,65,0,108,0,108,0,79,0,114,0,66,0,121,0,79,0,114,0,103,0,97,0,110,0,105,0,115,0,97,
				0,116,0,105,0,111,0,110,0,62,0,65,0,76,0,76,0,60,0,47,0,79,0,118,0,101,0,114,0,114,0,105,0,100,0,101,0,65,0,108,0,108,0,79,0,114,0,66,0,121,0,79,0,114,0,103,0,97,0,110,0,105,0,115,0,97,
				0,116,0,105,0,111,0,110,0,62,0,60,0,47,0,65,0,117,0,116,0,111,0,83,0,101,0,110,0,100,0,83,0,116,0,97,0,116,0,101,0,109,0,101,0,110,0,116,0,68,0,97,0,116,0,101,0,67,0,104,0,97,0,110,0,103,
				0,101,0,82,0,101,0,113,0,117,0,101,0,115,0,116,0,62,0
			};
			var byteArrayOrg = new byte[]
			{
				60,0,63,0,120,0,109,0,108,0,32,0,118,0,101,0,114,0,115,0,105,0,111,0,110,0,61,0,34,0,49,0,46,0,48,0,34,0,32,0,101,0,110,0,99,0,111,0,100,0,105,0,110,0,103,0,61,0,34,0,117,0,116,0,102,
				0,45,0,49,0,54,0,34,0,63,0,62,0,60,0,65,0,117,0,116,0,111,0,83,0,101,0,110,0,100,0,83,0,116,0,97,0,116,0,101,0,109,0,101,0,110,0,116,0,68,0,97,0,116,0,101,0,67,0,104,0,97,0,110,0,103,
				0,101,0,82,0,101,0,113,0,117,0,101,0,115,0,116,0,62,0,60,0,79,0,118,0,101,0,114,0,114,0,105,0,100,0,101,0,65,0,108,0,108,0,79,0,114,0,66,0,121,0,79,0,114,0,103,0,97,0,110,0,105,0,115,0,97,
				0,116,0,105,0,111,0,110,0,62,0,79,0,82,0,71,0,60,0,47,0,79,0,118,0,101,0,114,0,114,0,105,0,100,0,101,0,65,0,108,0,108,0,79,0,114,0,66,0,121,0,79,0,114,0,103,0,97,0,110,0,105,0,115,0,97,
				0,116,0,105,0,111,0,110,0,62,0,60,0,47,0,65,0,117,0,116,0,111,0,83,0,101,0,110,0,100,0,83,0,116,0,97,0,116,0,101,0,109,0,101,0,110,0,116,0,68,0,97,0,116,0,101,0,67,0,104,0,97,0,110,0,103,
				0,101,0,82,0,101,0,113,0,117,0,101,0,115,0,116,0,62,0
			};

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(overrideByAll, byteArraryAll),
				new ValidSampleAndBinaryValueInDB(overrideByOrg, byteArrayOrg)
			};
		}
	}
}
