using System.Windows.Forms;
using Enterprise.Customs.NZ.Business.MasterFiles;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.GUI.Testing
{
	[TestedType(typeof(CusClassificationForm))]
	sealed class CusClassificationFormBasherTest : ZArchitecture.GUI.Testing.ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var classification = Factory.New<CusClassification>();
			return new CusClassificationForm(classification);
		}
	}
}
