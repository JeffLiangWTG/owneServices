using System;
using System.Text;
using Enterprise.Registry.Business.Testing;
using Enterprise.TransportCommon.Registry.Business.MobilityDocumentTypes;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.TransportCommon.Registry.Testing
{
	[TestedType(typeof(MobilityDocumentTypesDataType))]
	internal class MobilityDocumentTypesDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<MobilityDocumentTypesDataType>
	{
		protected override MobilityDocumentTypesDataType GetNewDataType()
		{
			return new MobilityDocumentTypesDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			MobilityDocumentTypeCollection sample1 = new MobilityDocumentTypeCollection();
			MobilityDocumentTypeCollection sample2 = new MobilityDocumentTypeCollection();

			var docType1 = new MobilityDocumentType();
			docType1.RT_PK = new Guid("c7124f15-84e7-461d-84b3-47be3f77fb45"); // Doc Type CAD
			var docType2 = new MobilityDocumentType();
			docType2.RT_PK = new Guid("88d7dda5-8513-4e5a-87cb-08f30f6301b2"); // Doc Type CAR

			sample1.Add(docType1);
			sample2.Add(docType2);

			byte[] byteArrayValue1 = Encoding.Unicode.GetBytes("<?xml version=\"1.0\" encoding=\"utf-16\"?><MobilityDocumentTypeCollection xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"><MobilityDocumentType><RT_PK>c7124f15-84e7-461d-84b3-47be3f77fb45</RT_PK><RT_DocType>CAD</RT_DocType></MobilityDocumentType></MobilityDocumentTypeCollection>");
			byte[] byteArrayValue2 = Encoding.Unicode.GetBytes("<?xml version=\"1.0\" encoding=\"utf-16\"?><MobilityDocumentTypeCollection xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"><MobilityDocumentType><RT_PK>88d7dda5-8513-4e5a-87cb-08f30f6301b2</RT_PK><RT_DocType>CAR</RT_DocType></MobilityDocumentType></MobilityDocumentTypeCollection>");

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(sample1, byteArrayValue1),
				new ValidSampleAndBinaryValueInDB(sample2, byteArrayValue2)
			};
		}
		protected override string ExpectedEditorName
		{
			get { return "MobilityDocumentTypesRegistryEditor"; }
		}
	}
}
