using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Test
{
	[TestedType(typeof(ExternalRequestTypeForm))]
	internal class ExternalRequestTypeFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var factory = new BusinessObjectFactory();
			var bO = factory.NewWithValidTestData<ExternalRequestType>();
			bO.RQT_Code = "XXX";
			bO.RQT_Description = "XXX Desc";
			bO.RQT_IsActive = true;
			bO.RQT_JobType = ExternalRequestTypeJobTypes.Codes.SPL;

			factory.Save();

			var result = new ExternalRequestTypeForm(bO);
			result.ControllerID = ControllerIDs.ExternalRequestTypes;
			return result;
		}
	}
}
