using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	[TestedType(typeof(AllocateSelectionJobMawbForm))]
	public class AllocateSelectionJobMawbFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			AllocateSelectionJobMawb selectionBo = new AllocateSelectionJobMawb(new BusinessObjectFactory(), System.Array.Empty<BusinessObject>());
			return new AllocateSelectionJobMawbForm(selectionBo);
		}
	}
}
