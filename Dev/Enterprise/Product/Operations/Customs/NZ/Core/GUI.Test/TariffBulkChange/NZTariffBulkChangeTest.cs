using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.NZ.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.NZ.GUI.Testing;

class NZTariffBulkChangeTest : TestCaseWithFactory
{
	public void TestAdditionalContinueWithSave()
	{
		ZDateTime allowDate = new ZDateTime(2006, 12, 30);
		ZDateTime priorToAllowDate = new ZDateTime(2006, 12, 29);
		const string DemoCompanyCode = "DEM";
		const string NonDemoCompanyCode = "EDI";
		TariffBulkChangeTestHelper topLevelObject = new TariffBulkChangeTestHelper(Factory);

		RunOneAdditionalContinueWithSavetest(topLevelObject, setProductionDataBase: false, DemoCompanyCode, allowDate, automaticConvert: false, DialogResult.Yes, "", ContinueWithSave.Yes, "", "", "");
		RunOneAdditionalContinueWithSavetest(topLevelObject, setProductionDataBase: false, DemoCompanyCode, allowDate, automaticConvert: true, DialogResult.Yes, "", ContinueWithSave.Yes, "", "", "");
		RunOneAdditionalContinueWithSavetest(topLevelObject, setProductionDataBase: false, NonDemoCompanyCode, allowDate, automaticConvert: false, DialogResult.Yes, "", ContinueWithSave.Yes, "", "", "");
		RunOneAdditionalContinueWithSavetest(topLevelObject, setProductionDataBase: false, NonDemoCompanyCode, allowDate, automaticConvert: true, DialogResult.Yes, "", ContinueWithSave.Yes, "", "", "");

		RunOneAdditionalContinueWithSavetest(topLevelObject, setProductionDataBase: true, DemoCompanyCode, allowDate, automaticConvert: false, DialogResult.Yes, BaseTariffBulkChange.DemoBranchInitialWarningMessage, ContinueWithSave.No, BaseTariffBulkChange.DemoBranchMessage, "", "");
		RunOneAdditionalContinueWithSavetest(topLevelObject, setProductionDataBase: true, DemoCompanyCode, allowDate, automaticConvert: true, DialogResult.Yes, BaseTariffBulkChange.DemoBranchInitialWarningMessage, ContinueWithSave.No, BaseTariffBulkChange.DemoBranchMessage, "", "");
		RunOneAdditionalContinueWithSavetest(topLevelObject, setProductionDataBase: true, DemoCompanyCode, priorToAllowDate, automaticConvert: false, DialogResult.Yes, BaseTariffBulkChange.DemoBranchInitialWarningMessage, ContinueWithSave.No, BaseTariffBulkChange.DemoBranchMessage, "", "");
		RunOneAdditionalContinueWithSavetest(topLevelObject, setProductionDataBase: true, DemoCompanyCode, priorToAllowDate, automaticConvert: true, DialogResult.Yes, BaseTariffBulkChange.DemoBranchInitialWarningMessage, ContinueWithSave.No, BaseTariffBulkChange.DemoBranchMessage, "", "");
		RunOneAdditionalContinueWithSavetest(topLevelObject, setProductionDataBase: true, NonDemoCompanyCode, allowDate, automaticConvert: false, DialogResult.Yes, "", ContinueWithSave.Yes, "", "", "");
		RunOneAdditionalContinueWithSavetest(topLevelObject, setProductionDataBase: true, NonDemoCompanyCode, allowDate, automaticConvert: true, DialogResult.Yes, "", ContinueWithSave.Yes, "", "", "");
	}

	void RunOneAdditionalContinueWithSavetest(TariffBulkChangeTestHelper topLevelObject, bool setProductionDataBase, ZString companyCode, ZDateTime currentDate, bool automaticConvert, DialogResult questionAnswer,
		string expectedLastWarnTextMessage, ContinueWithSave expectedResult, string expectedLastErrorTextMessage, string expectedFinalConfirmationContains1, string expectedFinalConfirmationContains2)
	{
		topLevelObject.SetIsProductionDataBase = setProductionDataBase;
		GlbCompany.CurrentCompany.GC_Code = companyCode;
		topLevelObject.SetDateTimeNow = currentDate;

		topLevelObject.WarnIfDemoOrDateInvalid();
		if (string.IsNullOrEmpty(expectedLastWarnTextMessage))
		{
			Assert("There should not be any warning", UnitTestUserNotification.Instance.LastMessage.WasNone);
		}
		else
		{
			Assert("Sould be a warning", UnitTestUserNotification.Instance.LastMessage.WasWarning);
			AssertEquals(expectedLastWarnTextMessage, UnitTestUserNotification.Instance.LastMessage.Text);
		}
		UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

		UnitTestUserNotification.Instance.AddAnswer(questionAnswer);
		AssertEquals("Expected Result", expectedResult, topLevelObject.AdditionalContinueWithSave(automaticConvert));
		if (!string.IsNullOrEmpty(expectedLastErrorTextMessage))
		{
			Assert("Sould be an error", UnitTestUserNotification.Instance.LastMessage.WasError);
			AssertEquals(expectedLastErrorTextMessage, UnitTestUserNotification.Instance.LastMessage.Text);
		}
		if (!string.IsNullOrEmpty(expectedFinalConfirmationContains1))
		{
			Assert("There should be a question", UnitTestUserNotification.Instance.LastMessage.WasQuestion);
			Assert("Question should contain '" + expectedFinalConfirmationContains1 + "'", UnitTestUserNotification.Instance.LastMessage.Text.Contains(expectedFinalConfirmationContains1));
		}
		if (!string.IsNullOrEmpty(expectedFinalConfirmationContains2))
		{
			Assert("There should be a question", UnitTestUserNotification.Instance.LastMessage.WasQuestion);
			Assert("Question should contain '" + expectedFinalConfirmationContains2 + "'", UnitTestUserNotification.Instance.LastMessage.Text.Contains(expectedFinalConfirmationContains2));
		}
		UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
	}

	class TariffBulkChangeTestHelper : NZTariffBulkChange
	{
		public TariffBulkChangeTestHelper(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public override bool IsProductionDataBase
		{
			get
			{
				return fisProductionDataBase;
			}
		}

		public bool SetIsProductionDataBase
		{
			set
			{
				fisProductionDataBase = value;
			}
		}
		bool fisProductionDataBase;

		protected override ZDateTime DateTimeNow
		{
			get
			{
				return fDateTimeNow;
			}
		}

		public ZDateTime SetDateTimeNow
		{
			set
			{
				fDateTimeNow = value;
			}
		}
		ZDateTime fDateTimeNow;
	}
}
