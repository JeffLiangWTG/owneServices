using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	[SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly")]
	public class InBondMenuStub : ZMenuItem, IEDIMenu
	{
		public InBondMenuStub(string name, Security.SecurityCheckpoint securityCheckpoint)
		{
			this.Text = name;
			securityCheckpointDeniedMenuItem = new ZMenuItem(string.Format(Culture.Invariant, (NoResString)"The menu is disabled because you don't have the appropriate security rights: {0}", securityCheckpoint?.DisplayTextPathToSecurityRight ?? string.Empty));
			MenuItems.Add(securityCheckpointDeniedMenuItem);
		}

		public BaseJobDeclaration Declaration { get { return fDeclaration; } set { fDeclaration = value; } }

		#region implementation

		readonly MenuItem securityCheckpointDeniedMenuItem;
		BaseJobDeclaration fDeclaration;

		#endregion
	}
}
