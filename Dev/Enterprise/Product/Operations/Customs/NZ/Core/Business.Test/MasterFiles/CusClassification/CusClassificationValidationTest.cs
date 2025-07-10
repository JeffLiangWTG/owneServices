
using System;
using System.Linq;
using CargoWise.ComponentModel;
using Enterprise.Customs.NZ.Business.TariffValidation;
using Enterprise.Customs.NZ.Business.Testing;
using Enterprise.Customs.NZ.Registry;

namespace Enterprise.Customs.NZ.Business.MasterFiles.Testing
{
	public class CusClassificationValidationTest : Customs.Business.Testing.CusClassificationValidationTest
	{
		public void TestCheckCC_TariffNumAndMakeSureItDoesntCallBase()
		{
			Classification.CC_TariffNum = "";
			AssertHasError(Classification.CC_TariffNumInfo, TariffValidator.MessageErrorTariffCodeMissing);
			AssertEquals("Classification.CC_TariffNumInfo.GetErrors().Length", 1, Classification.CC_TariffNumInfo.GetErrors().Take(2).Count());
		}

		public void TestCheckConcessionCode_UseRefDb()
		{
			using (NZCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				UniversalTariffHelperTest.SetupTariffData(Factory);
				var classification = Factory.NewWithValidTestData<CusClassification>();
				classification.CC_TariffNum = "123456789";

				classification.CC_ConcessionCode = "INVALID";
				AssertHasWarningContaining(classification.CC_ConcessionCodeInfo, "Concession Code [INVALID] not recognized");
				classification.CC_ConcessionCode = "100001A";
				AssertNoWarnings(classification.CC_ConcessionCodeInfo);
			}
		}

		#region Classification
		CusClassification Classification
		{
			get
			{
				if (fClassification == null)
				{
					fClassification = Factory.New<CusClassification>();
				}
				return fClassification;
			}
		}
		CusClassification fClassification;
		#endregion
	}
}
