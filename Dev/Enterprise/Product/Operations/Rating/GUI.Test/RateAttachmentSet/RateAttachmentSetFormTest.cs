using System.Windows.Forms;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Rating.GUI.Testing
{
	[TestedType(typeof(RateAttachmentSetForm))]
	internal sealed class RateAttachmentSetFormTest : ZFormBasherTest
	{
		#region Implementation

		protected override Form GetFormToBashCore()
		{
			var attachmentSet = Factory.New<RateAttachmentSet>();
			return new RateAttachmentSetForm(attachmentSet);
		}

		#endregion
	}
}
