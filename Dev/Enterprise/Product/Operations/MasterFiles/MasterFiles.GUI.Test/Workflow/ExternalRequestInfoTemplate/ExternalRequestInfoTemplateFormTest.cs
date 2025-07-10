using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Test
{
	[TestedType(typeof(ExternalRequestInfoTemplateForm))]
	internal class ExternalRequestInfoTemplateFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var factory = new BusinessObjectFactory();
			var bO = factory.NewWithValidTestData<ExternalRequestInfoTemplate>();
			bO.RIT_Code = "XXX";
			bO.RIT_Description = "XXX Desc";
			bO.RIT_IsActive = true;
			bO.RIT_JobType = ExternalRequestTypeJobTypes.Codes.SPL;

			factory.Save();

			var result = new ExternalRequestInfoTemplateForm(bO);
			result.ControllerID = ControllerIDs.ExternalRequestInfoTemplate;
			return result;
		}
	}
}
