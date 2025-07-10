using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Test
{
	[TestedType(typeof(StmFeatureTestForm))]
	public class StmFeatureTestFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var feature = Factory.New<StmFeatureTest>();
			return new StmFeatureTestForm(feature)
			{
				Width = 840,
				Height = 760
			};
		}
	}
}
