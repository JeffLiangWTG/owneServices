using CargoWise.Types;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.US.DataTransfer.Universal.Testing
{
	partial class InBondDataObjectWriterTest
	{
		protected void AssertCustomsReferenceContents(UniversalCustoms.CustomsReference customsReferenceData, ZString referenceTypeCode, ZString subTypeCode, ZString? referenceNumber)
		{
			AssertNotNull("Precondition: customsReferenceData", customsReferenceData);
			CombineAssertions(delegate
			{
				AssertNotNull("customsReferenceData.Type", customsReferenceData.Type);
				AssertEquals("customsReferenceData.Type.Code", referenceTypeCode, customsReferenceData.Type.Code);
				AssertNotNull("customsReferenceData.SubType", customsReferenceData.SubType);
				AssertEquals("customsReferenceData.SubType.Code", subTypeCode, customsReferenceData.SubType.Code);
				AssertEquals("customsReferenceData.Reference", referenceNumber, customsReferenceData.Reference);
			});
		}
	}
}
