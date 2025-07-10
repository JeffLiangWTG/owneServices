using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(DocAddressPassportDetailForm))]
	sealed class DocAddressPassportDetailFormTest : ZFormBasherTest
	{
		#region Implementation

		protected override Form GetFormToBashCore()
		{
			JobDocAddress docAddress = Factory.New<JobDocAddress>();
			return new DocAddressPassportDetailForm(docAddress);
		}

		#endregion
	}
}
