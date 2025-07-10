using CargoWise.EntityFramework;
using Enterprise.Customs.NZ.Business.MasterFiles;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Module.Testing
{
	[TestedType(typeof(CusClassificationController))]
	sealed class CusClassificationControllerTest : ZControllerBasherTest
	{
		protected override string CountryCode
		{
			get
			{
				return "NZ";
			}
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.Customs.SingleTariffClassification;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			CusClassification testClass = Factory.New<CusClassification>();
			testClass.CC_Description = "TestDescription";
			testClass.CC_TariffNum = "0000.00.00";
			testClass.CC_LookupCode = "TestTest";
			Factory.Save();
			return testClass;
		}
	}
}
