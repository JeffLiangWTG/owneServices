using System.Reflection;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Warehouse.Transactions.GUI.Testing
{
	public class AssignLinesToUserAttacherTest : ZRecordAttacherTest
	{
		#region TestAttachCore

		public void TestAttachCore()
		{
			var activeUser = Helper.CreateGlbStaff("T1", "T1");
			var inActiveUser = Helper.CreateGlbStaff("T2", "T2", false);

			var assignableLine = new DummyLineAssigner { IsLineAssignable = true };
			var unAssignableLine = new DummyLineAssigner { IsLineAssignable = false };

			using (var form = new ZForm())
			{
				var attacher = new AssignLinesToUserAttacher<DummyLineAssigner>(new DummyLineAssigner[] { assignableLine, unAssignableLine }, Factory);
				attacher.Show(form);

				var staffCollectionPropertyInfo = typeof(ZRecordAttacher).GetField("originalFindBoxList", BindingFlags.NonPublic | BindingFlags.Instance);
				var staffCollection = (GlbStaffCollection)staffCollectionPropertyInfo.GetValue(attacher);

				AssertCollectionContains("Collection should contain active staff.", activeUser, staffCollection);
				AssertCollectionNotContains("Collection should not contain inactive staff.", inActiveUser, staffCollection);

				attacher.LastShownAttachPopupForTesting.SelectStaffForEmbeddedModuleSelection(activeUser);
				AssertEquals(activeUser, assignableLine.AssignedStaff);
				AssertNull(unAssignableLine.AssignedStaff);
			}
		}

		#endregion

		protected WhsTestHelperFunctions Helper
		{
			get { return helper ?? (helper = new WhsTestHelperFunctions(Factory)); }
		}
		WhsTestHelperFunctions helper;
	}
}
