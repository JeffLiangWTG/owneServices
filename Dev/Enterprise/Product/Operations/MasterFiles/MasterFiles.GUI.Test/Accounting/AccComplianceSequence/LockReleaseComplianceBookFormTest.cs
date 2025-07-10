using System.Windows.Forms;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(LockReleaseComplianceBookForm))]
	sealed class LockReleaseComplianceBookFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var book = new LockComplianceBook(Factory);
			var result = new LockReleaseComplianceBookForm(book);
			result.ControllerID = ControllerIDs.LockComplianceBook;
			return result;
		}

		public void TestFormCaptionAndButtonSaveAndButtonCaption()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Peru);

			var existedSequenceWithinTheValidtyPeriod = Factory.NewWithValidTestData<AccComplianceSequence>();
			existedSequenceWithinTheValidtyPeriod.XD_SequenceClass = "NCR";
			existedSequenceWithinTheValidtyPeriod.XD_Code = "LM5";
			existedSequenceWithinTheValidtyPeriod.XD_StartNumber = 200;
			existedSequenceWithinTheValidtyPeriod.XD_EndNumber = 300;
			existedSequenceWithinTheValidtyPeriod.XD_MaximumNumberDigits = 6;
			existedSequenceWithinTheValidtyPeriod.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			existedSequenceWithinTheValidtyPeriod.XD_AllocationLevel = Core.Constants.ComplianceBookAllocationLevel.Counter;
			existedSequenceWithinTheValidtyPeriod.XD_StartDate = new ZDate(2018, 07, 01);
			existedSequenceWithinTheValidtyPeriod.XD_ExpiryDate = new ZDateTime(2018, 07, 31);
			existedSequenceWithinTheValidtyPeriod.XD_IsActive = true;
			Factory.Save();

			AssertEquals(existedSequenceWithinTheValidtyPeriod.XD_LockBy, ZGuid.Empty);

			var lockComplianceBookController = ZControllerFactory.Create(ControllerIDs.LockComplianceBook);

			using (var newLockComplianceBookForm = (LockReleaseComplianceBookForm)lockComplianceBookController.ShowNewForm())
			{
				((LockReleaseComplianceBook)newLockComplianceBookForm.DataSource).XD_Calc_PK = existedSequenceWithinTheValidtyPeriod.PK;

				AssertEquals(newLockComplianceBookForm.CaptionResourceString, NoResourceStringData.GetData("Lock Counter Compliance Book"));
				AssertEquals(newLockComplianceBookForm.SaveButton.CaptionResourceString, NoResourceStringData.GetData("Lock"));

				newLockComplianceBookForm.SaveButton.PerformClick();

				AssertEquals(existedSequenceWithinTheValidtyPeriod.XD_LockBy, GlbStaff.CurrentUser.PK);
			}

			AssertEquals(existedSequenceWithinTheValidtyPeriod.XD_LockBy, GlbStaff.CurrentUser.PK);

			var releaseComplianceBookController = ZControllerFactory.Create(ControllerIDs.ReleaseComplianceBook);

			using (var newReleaseComplianceBookForm = (LockReleaseComplianceBookForm)releaseComplianceBookController.ShowNewForm())
			{
				((ReleaseComplianceBook)newReleaseComplianceBookForm.DataSource).XD_Calc_PK = existedSequenceWithinTheValidtyPeriod.PK;

				AssertEquals(newReleaseComplianceBookForm.CaptionResourceString, NoResourceStringData.GetData("Release Counter Compliance Book"));
				AssertEquals(newReleaseComplianceBookForm.SaveButton.CaptionResourceString, NoResourceStringData.GetData("Release"));

				newReleaseComplianceBookForm.SaveButton.PerformClick();

				AssertEquals(existedSequenceWithinTheValidtyPeriod.XD_LockBy, ZGuid.Empty);
			}
		}
	}
}
