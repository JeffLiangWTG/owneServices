using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class StaffAssignmentsControlTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestColumnsForCompanySpecific()
		{
			OrgHeader org = OrgHeader.New(Factory);
			using (ZForm form = new ZForm(org))
			using (StaffAssignmentsControl control = new StaffAssignmentsControl())
			{
				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(org, "");
				Application.DoEvents();

				AssertNotNull("Column exists by default", control.StaffAssignmentsGrid.Columns[OrgStaffAssignmentsSchema.O8_GC.Name]);
				AssertNotNull("Column Style exists by default", control.StaffAssignmentsGrid.TableStyles[0].GridColumnStyles[OrgStaffAssignmentsSchema.O8_GC.Name]);

				Assert(control.Org.StaffAssignments.CompanySpecific);
				control.SetCompanySpecific(false);
				Assert(!control.Org.StaffAssignments.CompanySpecific);
				control.SetCompanySpecific(true);
				Assert(control.Org.StaffAssignments.CompanySpecific);
			}
		}
	}
}
