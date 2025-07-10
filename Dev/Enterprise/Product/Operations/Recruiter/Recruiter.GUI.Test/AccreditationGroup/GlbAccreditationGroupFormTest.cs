using System.Windows.Forms;
using Enterprise.Recruiter.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.GUI.Testing
{
	[TestedType(typeof(GlbAccreditationGroupForm))]
	public class GlbAccreditationGroupFormTest : ZFormBasherTest
	{
		public void TestPreReqGrid_DoubleClickShouldOpenAccreditationForm()
		{
			var accreditation = Factory.NewWithValidTestData<GlbAccreditation>();
			Factory.Save();
			var accreditationGroup = Factory.NewWithValidTestData<GlbAccreditationGroup>();
			accreditationGroup.Accreditations.Add(accreditation);
			using (var form = new GlbAccreditationGroupFormForTest(accreditationGroup))
			{
				form.Show();
				form.PreReqGrid_DoubleClickExposed();
				AssertNull(ZFormModaliser.LastFormShownDialogForTest);
				form.PreReqGridExposed.InnerGrid.SelectSingleElement(accreditation);
				form.PreReqGrid_DoubleClickExposed();
				AssertNotNull(ZFormModaliser.LastFormShownDialogForTest);
			}
		}

		#region Implementation
		protected override Form GetFormToBashCore()
		{
			return new GlbAccreditationGroupForm(Factory.New<GlbAccreditationGroup>());
		}

		class GlbAccreditationGroupFormForTest : GlbAccreditationGroupForm
		{
			public GlbAccreditationGroupFormForTest(GlbAccreditationGroup accreditation) : base(accreditation)
			{
			}

			public GlbAccreditationGroupingModuleButtonGrid PreReqGridExposed => PreReqGrid;
			public void PreReqGrid_DoubleClickExposed()
			{
				PreReqGrid_DoubleClick(null, null);
			}
		}
		#endregion
	}
}
