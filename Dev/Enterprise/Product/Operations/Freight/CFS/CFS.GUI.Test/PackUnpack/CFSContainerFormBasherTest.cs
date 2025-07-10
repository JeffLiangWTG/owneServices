using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Freight.CFS.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.CFS.GUI.Testing
{
	[TestedType(typeof(CFSContainerForm))]
	sealed class CFSContainerFormBasherTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			CFSContainer container = factory.New<CFSContainer>();
			return new CFSContainerForm(container);
		}
	}
}
