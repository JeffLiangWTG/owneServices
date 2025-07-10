using System.Linq;
using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(RefDocOrgCusCodeForm))]
	sealed class RefDocOrgCusCodeFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore() => new RefDocOrgCusCodeForm(DocOrgCusCode);

		protected override bool AllowFormSizeFixed => true;

		public void TestDirectionExists()
		{
			using (var form = GetFormToBashCore())
			{
				var zPanel = form.Controls.OfType<ZPanel>().FirstOrDefault();
				var directionDropEdit = zPanel?.Controls.OfType<ZDropEdit>().FirstOrDefault(x => x.Name == "DirectionDropEdit");
				AssertNotNull(directionDropEdit);
			}
		}

		RefDocOrgCusCode DocOrgCusCode
		{
			get
			{
				if (docOrgCusCode != null)
				{
					return docOrgCusCode;
				}

				var code = Factory.NewWithValidTestData<RefDocOrgCusCode>();
				code.DOC_DocumentType = "HAW";
				Factory.Save();

				return docOrgCusCode = code;
			}
		}
		RefDocOrgCusCode docOrgCusCode;
	}
}
