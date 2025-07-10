using CargoWise.EntityFramework;
using Enterprise.Telematics.Business;
using Enterprise.Telematics.Module.Controllers;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Telematics.Module.Test.Controllers
{
	[TestedType(typeof(TelPreDriveChecklistTemplateController))]
	class TelPreDriveChecklistTemplateControllerBasherTest : ZControllerBasherTest
	{
		#region ZControllerBasherTest

		protected override ControllerID GetControllerID() => ControllerIDs.TelPreDriveChecklistTemplate;

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var header = Factory.New<TelPreDriveChecklistTemplateHeader>();
			header.TTH_Type = TelPreDriveChecklistHeaderTypes.Codes.PDR;
			header.TTH_Description = "Some Description";
			Factory.Save();
			return header;
		}
		#endregion
	}
}
