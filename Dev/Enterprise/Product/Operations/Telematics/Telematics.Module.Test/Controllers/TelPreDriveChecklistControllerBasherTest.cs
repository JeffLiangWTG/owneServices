using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Telematics.Business;
using Enterprise.Telematics.Module.Controllers;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Telematics.Module.Test.Controllers
{
	[TestedType(typeof(TelPreDriveChecklistController))]
	class TelPreDriveChecklistControllerBasherTest : ZControllerBasherTest
	{
		#region ZControllerBasherTest

		protected override ControllerID GetControllerID() => ControllerIDs.TelPreDriveChecklist;

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var header = Factory.New<TelPreDriveChecklistHeader>();
			header.TPH_Type = TelPreDriveChecklistHeaderTypes.Codes.PDR;
			header.TPH_GS_NKDriver = "~BP";
			header.TPH_ChecklistCreateTimeUtc = ZDateTime.Now;
			Factory.Save();
			return header;
		}
		#endregion
	}
}
