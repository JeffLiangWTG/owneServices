using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(DefineGLSectionsForm))]
	sealed class DefineGLSectionsFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			AccGLHeaderBulkUpdater bizObj = new AccGLHeaderBulkUpdater(Factory);
			return new DefineGLSectionsForm(bizObj);
		}
	}
}
