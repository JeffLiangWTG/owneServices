using System.Windows.Forms;
using Enterprise.Freight.Agency.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.GUI.Testing
{
	[TestedType(typeof(SundryChargesForm))]
	internal class SundryChargesFormBasherTest : ZFormBasherTest
	{
		#region Implementation
		protected override Form GetFormToBashCore()
		{
			SundryCharges sundry = Factory.New<SundryCharges>();
			return new SundryChargesForm(sundry);
		}
		#endregion
	}
}
