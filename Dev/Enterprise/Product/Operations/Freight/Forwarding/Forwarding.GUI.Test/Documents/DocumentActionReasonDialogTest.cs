using System.Windows.Forms;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	[TestedType(typeof(DocumentActionReasonDialog))]
	public class DocumentActionReasonDialogTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var list = new CodeDescriptionPairList();
			list.AddPair("aaa", "aaa desc");
			list.AddPair("bbb", "bbb desc");
			list.AddPair("ccc", "ccc desc");

			return new DocumentActionReasonDialog(
				"message",
				"caption",
				list);
		}
	}
}
