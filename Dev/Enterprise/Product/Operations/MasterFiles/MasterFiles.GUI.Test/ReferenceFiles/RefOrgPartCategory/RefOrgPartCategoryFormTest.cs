using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(RefOrgPartCategoryForm))]
	sealed class RefOrgPartCategoryFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var category = Factory.New<OrgPartCategory>();
			return new RefOrgPartCategoryForm(category);
		}

		protected override bool AllowFormSizeFixed => true;
	}
}
