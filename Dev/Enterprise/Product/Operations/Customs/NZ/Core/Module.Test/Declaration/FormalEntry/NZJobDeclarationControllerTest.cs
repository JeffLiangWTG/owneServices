using CargoWise.EntityFramework;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Module.Declaration.FormalEntry.Testing
{
	[TestedType(typeof(JobDeclarationController))]
	sealed class NZJobDeclarationControllerTest : Customs.Module.Testing.JobDeclarationControllerTestCase
	{
		protected override bool CountryHasExWarehouse
		{
			get
			{
				return false;
			}
		}

		protected override string CountryCode
		{
			get
			{
				return "NZ";
			}
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			Factory.Save();
			return declaration;
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.Customs.JobDeclaration;
		}

		protected override Customs.Business.BaseJobDeclaration GetJobDeclaration()
		{
			return Factory.New<JobDeclaration>();
		}
	}
}
