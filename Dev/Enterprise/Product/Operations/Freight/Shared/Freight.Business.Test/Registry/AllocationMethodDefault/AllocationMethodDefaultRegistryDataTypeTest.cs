using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(AllocationMethodDefaultRegistryDataType))]
	sealed class AllocationMethodDefaultRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<AllocationMethodDefaultRegistryDataType>
	{
		#region Implementation

		protected override AllocationMethodDefaultRegistryDataType GetNewDataType()
		{
			return new AllocationMethodDefaultRegistryDataType(new AllocationMethodDefaultHeader());
		}

		protected override string ExpectedEditorName
		{
			get { return "AllocationMethodDefaultRegistryItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();

			AllocationMethodDefaultHeader header = new AllocationMethodDefaultHeader(factory);
			header.DefaultAllocationMethod = AllocationMethodList.Codes.Ignore;

			AllocationMethodDefaultRule rule = header.Rules.AddNew();
			rule.CountryCode = "AU";
			rule.AllocationMethod = AllocationMethodList.Codes.Origin;

			byte[] byteArrayValue = new byte[]
			{
				255,254,60,0,63,0,120,0,109,0,108,0,32,0,118,0,101,0,114,0,115,0,105,0,111,0,110,0,61,0,34,0,49,0,46,0,48,0,34,0,32,0,101,0,110,0,99,0,111,0,100,0,105,0,110,0,103,0,61,0,34,0,117,0,116,
				0,102,0,45,0,49,0,54,0,34,0,63,0,62,0,60,0,65,0,108,0,108,0,111,0,99,0,97,0,116,0,105,0,111,0,110,0,77,0,101,0,116,0,104,0,111,0,100,0,68,0,101,0,102,0,97,0,117,0,108,0,116,0,72,0,101,
				0,97,0,100,0,101,0,114,0,62,0,60,0,68,0,101,0,102,0,97,0,117,0,108,0,116,0,65,0,108,0,108,0,111,0,99,0,97,0,116,0,105,0,111,0,110,0,77,0,101,0,116,0,104,0,111,0,100,0,62,0,73,0,71,0,82,
				0,60,0,47,0,68,0,101,0,102,0,97,0,117,0,108,0,116,0,65,0,108,0,108,0,111,0,99,0,97,0,116,0,105,0,111,0,110,0,77,0,101,0,116,0,104,0,111,0,100,0,62,0,60,0,82,0,117,0,108,0,101,0,115,0,62,
				0,60,0,82,0,117,0,108,0,101,0,62,0,60,0,67,0,111,0,117,0,110,0,116,0,114,0,121,0,62,0,65,0,85,0,60,0,47,0,67,0,111,0,117,0,110,0,116,0,114,0,121,0,62,0,60,0,65,0,108,0,108,0,111,0,99,
				0,97,0,116,0,105,0,111,0,110,0,77,0,101,0,116,0,104,0,111,0,100,0,62,0,79,0,82,0,73,0,60,0,47,0,65,0,108,0,108,0,111,0,99,0,97,0,116,0,105,0,111,0,110,0,77,0,101,0,116,0,104,0,111,0,100,
				0,62,0,60,0,47,0,82,0,117,0,108,0,101,0,62,0,60,0,47,0,82,0,117,0,108,0,101,0,115,0,62,0,60,0,47,0,65,0,108,0,108,0,111,0,99,0,97,0,116,0,105,0,111,0,110,0,77,0,101,0,116,0,104,0,111,
				0,100,0,68,0,101,0,102,0,97,0,117,0,108,0,116,0,72,0,101,0,97,0,100,0,101,0,114,0,62,0
			};

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(header, byteArrayValue)
			};
		}

		#endregion
	}
}
