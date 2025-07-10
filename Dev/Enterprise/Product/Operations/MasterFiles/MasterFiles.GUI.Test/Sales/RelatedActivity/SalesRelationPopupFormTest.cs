using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(SalesRelationPopupForm))]
	sealed class SalesRelationPopupFormTest : ZFormBasherTest
	{
		#region Implementation

		protected override Form GetFormToBashCore()
		{
			var opportunity = Factory.New<OrgOpportunity>();
			return new SalesRelationPopupForm(new SalesRelationModel(opportunity));
		}

		#endregion
	}
}
