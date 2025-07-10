using System.Reflection;
using System.Windows.Forms;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class CertificatesUserControlTest : TestCase
	{
		public void TestNormalCaseForDescriptionAndRefNumber()
		{
			using (var control = new CertificatesUserControl())
			{
				AssertEquals(CharacterCasing.Normal, ((ZGridColumnInfo)control.GetType().GetField("zTextBoxColumnStyleInfo1", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(control)).CharacterCasing);
				AssertEquals(CharacterCasing.Normal, ((ZGridColumnInfo)control.GetType().GetField("zTextBoxColumnStyleInfo2", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(control)).CharacterCasing);
				AssertEquals(CharacterCasing.Normal, ((ZTextBox)control.Controls.Find("DescriptionTextBox", true)[0]).CharacterCasing);
				AssertEquals(CharacterCasing.Normal, ((ZTextBox)control.Controls.Find("CertificateNumberTextBox", true)[0]).CharacterCasing);
			}
		}
	}
}
