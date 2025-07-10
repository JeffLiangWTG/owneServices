using System.Windows.Forms;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.US.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.GUI.Testing
{
	[TestedType(typeof(AllocateInBondNumberForm))]
	sealed class AllocateInBondNumberFormTest : ZFormBasherTest
	{
		public void TestOKButtonClick()
		{
			Enterprise.Customs.US.Business.Testing.DeclarationTestHelper.SetupBranchSpecificInBondNumberRanges();
			var branch = Factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK);
			var allocateInBondNumber = new AllocateInBondNumber(branch);
			using (var form = new AllocateInBondNumberForm(allocateInBondNumber))
			{
				form.Show();
				form.OKButton.PerformClick();
				AssertEquals(DialogResult.OK, form.DialogResult);
				form.Show();
				allocateInBondNumber.AI_InBondNumber = "BLAH";
				form.OKButton.PerformClick();
				AssertEquals("Error There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.ToString());
				AssertEquals(DialogResult.None, form.DialogResult);
			}
		}

		public void TestReuseConfirmation()
		{
			Enterprise.Customs.US.Business.Testing.DeclarationTestHelper.SetupBranchSpecificInBondNumberRanges();
			var branch = Factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK);
			var inBondNumberSetting = InBondNumberSetting.New(branch);
			var connection = ((IDbConnected)Factory).Connection;
			var fountain = Env.NumberFountains.USInBondNumberFountain(branch.PK.ToGuid());
			var nextNumber = fountain.PeekPreliminary(connection);

			var header = Factory.New<Integration.Customs.US.InBond.ICusInBondHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.InBond;
			header.BH_JobReference = "INB0000001";
			var moveHeader = Factory.New<Integration.Customs.US.InBond.ICusInBondMoveHeader>();
			moveHeader.InBondNumber = "69300140";
			moveHeader.BM_BH = header.PK;

			var entryNum = Factory.New<CusEntryNumber>();
			entryNum.CE_EntryType = CusEntryHeaderMessageTypeList.Codes.InBond;
			entryNum.CE_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			entryNum.CE_EntryNum = inBondNumberSetting.GetNumberWithCheckDigit(nextNumber);
			entryNum.CE_ParentTable = CusInBondMoveHeaderSchema.Constants.TableName;
			entryNum.CE_ParentID = moveHeader.PK;
			entryNum.CE_SystemCreateTimeUtc = ZDateTime.UtcNow.AddYears(-4);

			var allocateInBondNumber = new AllocateInBondNumber(branch);
			using (var form = new AllocateInBondNumberForm(allocateInBondNumber))
			{
				form.Show();
				form.OKButton.PerformClick();
				AssertEquals("Question In-bond number '000000011' was previously under on 69300140-INB0000001 but was re-issued by Customs.\r\nOK to proceed with re-using this number?", UnitTestUserNotification.Instance.LastMessage.ToString());
			}

			allocateInBondNumber.AI_InBondNumber = "999999991";
			using (var form = new AllocateInBondNumberForm(allocateInBondNumber))
			{
				form.Show();
				form.OKButton.PerformClick();
				AssertEquals(DialogResult.OK, form.DialogResult);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var allocateInBondNumber = new AllocateInBondNumber(Factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK));
			return new AllocateInBondNumberForm(allocateInBondNumber);
		}
	}
}
