using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	[TestedType(typeof(DummyHostFormForBashing))]
	sealed class CusPermitRuleUserControlTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var form = new DummyHostFormForBashing(Factory.New<BaseCusPermitHeader>());
			form.CaptionRenderingEnabled = true;
			form.Text = "ZTemplateForm@#$_Basher_Test";
			return form;
		}
	}

	sealed class DummyHostFormForBashing : ZTemplateForm
	{
		public DummyHostFormForBashing(BusinessObject bizo)
			: base(bizo)
		{
		}
	}
}
