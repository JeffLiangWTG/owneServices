using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transit.Business.Common;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Warehouse.Transit.Business.Testing
{
	public class TransitWarehouseNoteHelperTest : TestCaseWithFactory
	{
		#region TestGetNoteTypes

		public void TestGetNoteTypes()
		{
			AssertContainsExactElementsInAnyOrder(new PredefinedNoteType[] {
				PredefinedNoteTypes.Instance.AutoRatingAuditLog,
				PredefinedNoteTypes.Instance.UnrecognisedAdditionalReferenceTypes,
				PredefinedNoteTypes.Instance.UnmatchedOrgDetails },
				TransitWarehouseNoteHelper.GetNoteTypes());
		}

		#endregion

		#region TestFindOrCreateCIN750StmNote

		public void TestFindOrCreateCIN750StmNote()
		{
			var warehouse = Helper.CreateTRWWarehouse();

			var shipmentJobPK = ZGuid.NewZGuid();
			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, jobID: "EXTREF1", parentPK: shipmentJobPK, parentCode: "JS");
			Factory.Save();

			var staff = Helper.CreateGlbStaff("AAA", "AAA");
			var company = Factory.New<GlbCompany>();
			company.GC_Code = "ABC";

			var branch = Factory.New<GlbBranch>();
			branch.GB_Code = "DEF";
			branch.GB_GC = company.PK;

			var departmentNew = Factory.NewWithValidTestData<GlbDepartment>();
			departmentNew.GE_Code = "GHI";
			Factory.Save();

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), branch.PK.ToGuid(), departmentNew.PK.ToGuid()))
			{
				var note = rcn.FindOrCreateCIN750StmNote();
				note.ST_NoteText = "Note text";
				Factory.Save();
			}

			var note2 = rcn.FindOrCreateCIN750StmNote();
			AssertEquals("Note text", note2.ST_NoteText);
		}

		#endregion

		#region TestFindOrCreateCRESAStmNote

		public void TestFindOrCreateCRESAStmNote()
		{
			var warehouse = Helper.CreateTRWWarehouse();

			var shipmentJobPK = ZGuid.NewZGuid();
			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, jobID: "EXTREF1", parentPK: shipmentJobPK, parentCode: "JS");
			Factory.Save();

			var staff = Helper.CreateGlbStaff("AAA", "AAA");
			var company = Factory.New<GlbCompany>();
			company.GC_Code = "ABC";

			var branch = Factory.New<GlbBranch>();
			branch.GB_Code = "DEF";
			branch.GB_GC = company.PK;

			var departmentNew = Factory.NewWithValidTestData<GlbDepartment>();
			departmentNew.GE_Code = "GHI";
			Factory.Save();

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), branch.PK.ToGuid(), departmentNew.PK.ToGuid()))
			{
				var note = rcn.FindOrCreateCRESAStmNote();
				note.ST_NoteText = "Note text";
				Factory.Save();
			}

			var note2 = rcn.FindOrCreateCRESAStmNote();
			AssertEquals("Note text", note2.ST_NoteText);
		}

		#endregion

		WhsTransitTestHelper Helper => new WhsTransitTestHelper(Factory);
	}
}
