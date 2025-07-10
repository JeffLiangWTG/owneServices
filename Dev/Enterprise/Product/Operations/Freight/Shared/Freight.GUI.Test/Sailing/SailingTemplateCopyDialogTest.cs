using System.Windows.Forms;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.GUI.Testing
{
	[TestedType(typeof(SailingTemplateCopyDialog))]
	sealed class SailingTemplateCopyDialogTest : ZFormBasherTest
	{
		#region Implementation

		protected override Form GetFormToBashCore()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			SailingTemplateCopyCriteria criteria = new SailingTemplateCopyCriteria(voyage);
			return new SailingTemplateCopyDialog(criteria);
		}

		#endregion
	}
}
