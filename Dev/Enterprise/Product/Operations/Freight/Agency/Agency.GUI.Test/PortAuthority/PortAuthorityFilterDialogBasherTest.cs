using System.Windows.Forms;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.GUI.Testing
{
	[TestedType(typeof(PortAuthorityFilterDialog))]
	internal class PortAuthorityFilterDialogBasherTest : ZFormBasherTest
	{
		#region Implementation
		protected override Form GetFormToBashCore()
		{
			PortAuthority filter = new PortAuthority(Factory.New<JobVoyage>());
			using (filter.SuspendSettingHasChanges())
			{
				filter.DeliverTo3rdParty = true;
			}

			return new PortAuthorityFilterDialog(filter);
		}
		#endregion
	}
}
