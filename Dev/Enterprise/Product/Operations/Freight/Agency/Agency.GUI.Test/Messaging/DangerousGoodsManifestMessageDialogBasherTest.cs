using System.Windows.Forms;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.GUI.Testing
{
	[TestedType(typeof(DangerousGoodsManifestMessageDialog))]
	internal class DangerousGoodsManifestMessageDialogBasherTest : ZFormBasherTest
	{
		#region Implementation

		protected override Form GetFormToBashCore()
		{
			var message = new DangerousGoodsManifestMessage(Factory.New<JobVoyage>());
			return new DangerousGoodsManifestMessageDialog(message);
		}

		#endregion
	}
}
