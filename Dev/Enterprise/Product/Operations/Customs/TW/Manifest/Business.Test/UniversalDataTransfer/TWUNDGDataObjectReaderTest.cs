using System;
using Enterprise.Customs.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.eServices;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Customs.TW.Manifest.Business.UniversalDataTransfer.Testing
{
	sealed class TWUNDGDataObjectReaderTest : DataObjectReaderTest
	{
		public void TestImportUNDG()
		{
			using (eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var subs = Factory.BOFactory.NewWithValidTestData<UNDGSubstance>();
				subs.DG_Code = "0001A";

				Factory.SaveForTesting();

				var undgDataObject = new UNDG
				{
					UNDGCode = subs.DG_Code
				};

				var undgBO = new TWUNDGDataObjectReader(undgDataObject, logger, Factory).ReadIntoBusinessObject();
				CombineAssertions("UNDG read correctly", () =>
				{
					AssertEquals("UNDG Code", "0001A", undgBO.SubstanceCode);
					AssertEquals("DG_NKSubs", "0001A", undgBO.DI_DG_NKSubs);
				});
			}
		}
	}
}
