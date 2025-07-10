using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(StmTemplateController))]
	sealed class StmTemplateControllerTest : ZControllerBasherTest
	{
		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var bizO = Factory.New(typeof(StmTemplate));
			Factory.Save();
			return bizO;
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.DocumentTemplate;
		}
	}
}
