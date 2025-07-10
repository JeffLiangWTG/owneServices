using System.Text;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.TransportCommon.Registry.Testing
{
	[TestedType(typeof(TransportReferenceNumberTypesDataType))]
	class TransportReferenceNumbersDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<TransportReferenceNumberTypesDataType>
	{
		#region Implementation

		protected override TransportReferenceNumberTypesDataType GetNewDataType()
		{
			return new TransportReferenceNumberTypesDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "TransportReferenceNumberTypesRegistryItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			TransportReferenceNumberTypeCollection sample = new TransportReferenceNumberTypeCollection();
			sample.Add("AA1", (NoResString)"DESC1");
			sample.Add("AA2", (NoResString)"DESC2");

			byte[] byteArrayValue = Encoding.Unicode.GetBytes("<?xml version=\"1.0\" encoding=\"utf-16\"?><TransportReferenceNumberTypes xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"><TransportReferenceNumberType><Code>AA1</Code><Description>DESC1</Description><IsUnique>Y</IsUnique></TransportReferenceNumberType><TransportReferenceNumberType><Code>AA2</Code><Description>DESC2</Description><IsUnique>Y</IsUnique></TransportReferenceNumberType></TransportReferenceNumberTypes>");

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(sample, byteArrayValue)
			};
		}

		#endregion
	}
}
