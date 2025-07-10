using System.Windows.Forms;
using Enterprise.TransportConsignment.Business.Testing;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.TransportConsignment.GUI.Testing
{
	[TestedType(typeof(SignatureForm))]
	public class SignatureFormTest : ZFormBasherTest
	{
		#region TestDrawSignature

		public void TestDrawSignature()
		{
			var signature = Helper.GetSignature();
			AssertEquals("Signature contains Signature.", false, signature.IsEmpty);

			using (var form = new SignatureForm(signature))
			{
				AssertNull(form.SignaturePictureBox.Image);

				form.Show();
				AssertNotNull(form.SignaturePictureBox.Image);
				AssertEquals(200, form.SignaturePictureBox.Image.Width);
				AssertEquals(100, form.SignaturePictureBox.Image.Height);
			}
		}

		#endregion

		#region Overrides

		protected override Form GetFormToBashCore()
		{
			return new SignatureForm(Helper.GetSignature());
		}

		#endregion

		#region Implementation

		protected TransportBookingConsignmentTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingConsignmentTestHelper(Factory)); }
		}

		TransportBookingConsignmentTestHelper helper;

		#endregion
	}
}
