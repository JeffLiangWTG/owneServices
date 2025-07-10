using CargoWise.EntityFramework;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Module.Declaration.ECIWriteOff.Testing
{
	[TestedType(typeof(NZCUSCARController))]
	sealed class NZCUSCARControllerTest : ZControllerBasherTest
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
			return ControllerIDs.Customs.NZ.CUSCAR;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageSubType = NZ.Business.JobMessageSubTypeList.Codes.WriteOff;
			Factory.Save();
			return declaration;
		}
	}
}
