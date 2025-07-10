using CargoWise.Types;
using Enterprise.Customs.TW.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.DataTransfer.Testing;

[TestedType(typeof(CustomsSupportingInformationCollectionDataObjectReader))]
sealed class CustomsSupportingInformationCollectionDataObjectReaderTest : TestCaseWithFactoryAndMessagingHelpers
{
	public void TestCreateNewCustomsSupportingInformationDataObjectReaderType()
	{
		var helper = new Customs.DataTransfer.Universal.UniversalDataObjectReaderHelper(Factory, Core.Constants.CountryCodes.Taiwan, Core.Constants.CountryCodes.Taiwan);
		var reader = new CustomsSupportingInformationCollectionDataObjectReaderForTest(new TestErrorLogger(), helper);
		AssertType<CustomsSupportingInformationDataObjectReader>(reader.CreateNewCustomsSupportingInformationDataObjectReader_Exposed(new CustomsSupportingInformation(), ZGuid.Empty, null, null));
	}
}

class CustomsSupportingInformationCollectionDataObjectReaderForTest : CustomsSupportingInformationCollectionDataObjectReader
{
	public CustomsSupportingInformationCollectionDataObjectReaderForTest(IXmlImportLogger logger, Customs.DataTransfer.Universal.UniversalDataObjectReaderHelper helper, string dataContext = "") : base(logger, helper, dataContext)
	{
	}

	public Customs.DataTransfer.Universal.CustomsSupportingInformationDataObjectReader CreateNewCustomsSupportingInformationDataObjectReader_Exposed(CustomsSupportingInformation customsSupportingInformation, ZGuid parentPK, ZString parentTableCode, Customs.DataTransfer.Universal.CustomsSupportingInformationDataObjectReader.GetMatchingDataPredicate matchExisting)
	{
		return base.CreateNewCustomsSupportingInformationDataObjectReader(customsSupportingInformation, parentPK, parentTableCode, matchExisting);
	}
}
