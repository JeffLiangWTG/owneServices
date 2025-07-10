using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal.Testing;
using Enterprise.Customs.US.ISF.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Customs.US.ISF.DataTransfer.Universal.Reader.Testing
{
	sealed class ISFContainerDataObjectReaderTest : DataObjectReaderTest
	{
		public void TestImportingISFContainerData()
		{
			var headerBO = Factory.New<CusISFHeader>();
			var containerDataObject = SetupContainer("TURE654321", "20FR", "20");
			var reader = new ISFContainerDataObjectReader(containerDataObject, logger, Factory, headerBO);
			var containerBO = reader.ReadIntoBusinessObject();
			AssertNotNull(containerBO);
			CombineAssertions(delegate
			{
				AssertEquals("containerBO.BE_EquipCode", "20", containerBO.BE_EquipCode);
				AssertEquals("containerBO.BE_ContainerNum", "TURE654321", containerBO.BE_ContainerNum);
				AssertEquals("containerBO.BE_ContainerISO", "20FR", containerBO.BE_ContainerISO);
			});
		}

		Container SetupContainer(ZString containerNum, ZString isoCode, ZString equipCode)
		{
			var result = new Container(DefaultDataObjectWriterStrategy.TestInstance)
			{
				ContainerNumber = containerNum,
				AddInfoCollection = new List<AddInfo>()
				{
					new AddInfo()
					{
						Key = ISFConstants.ContainerConstants.USContainerType, Value = equipCode }, new AddInfo()
						{
							Key = ISFConstants.ContainerConstants.ISOSizeTypeCode, Value = isoCode
						}
				}
			};
			return result;
		}
	}
}
