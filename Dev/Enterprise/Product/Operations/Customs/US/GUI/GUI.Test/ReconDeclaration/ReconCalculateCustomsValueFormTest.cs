using System.Windows.Forms;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.GUI.Testing
{
	[TestedType(typeof(ReconCalculateCustomsValueForm))]
	sealed class ReconCalculateCustomsValueFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Recon;
			dec.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			var reconDeclaration = new ReconDeclaration(dec);
			return new ReconCalculateCustomsValueForm(new ReconCustomsValueCalculationManager(reconDeclaration));
		}
	}
}
