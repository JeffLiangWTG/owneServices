using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.PL.Business.CusTempStorage;
using Enterprise.Customs.PL.GUI.CusTempStorage;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.PL.GUI.Testing;

[TestedType(typeof(CusTempStorageForm))]
class CusTempStorageFormTest : ZFormBasherTest
{
	public void TestCheckOnShowPreSaveDialogs()
	{
		var (header, line) = SetUpJobHeader();
		line.TSL_OwnerReferenceType = "AWB";
		line.TSL_OwnerReferenceNumber = "UNITTEST";
		line.TSL_LocationOfGoods = "ORIGINAL";
		line.TSL_GoodsDescription = "Original Goods";
		line.TSL_PackageQty = 25;
		line.TSL_GrossWeight = 50.0m;

		using (var form = new CusTempStorageForm(header))
		{
			form.Show();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "TestSave";
			header.SJH_OH_Customer = orgHeader.PK;
			header.SJH_TempStorageEndDateUtc = ZDateTime.Empty;

			CombineAssertions(() =>
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				form.FireSaveButton();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

				header.SJH_TempStorageEndDateUtc = ZDateTime.Now;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				form.FireSaveButton();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			});
		}
	}

	(CusTempStorageJobHeader Header, CusTempStorageLine Line) SetUpJobHeader()
	{
		var header = CusTempStorageJobHeader.New(Factory);
		var storageDec = header.CusTempStorageDec;
		var line = storageDec.CusTempStorageLines.AddNew();

		var customer = Factory.NewWithValidTestData<OrgHeader>();
		header.SJH_OH_Customer = customer.PK;

		return (header, line);
	}

	protected override Form GetFormToBashCore()
	{
		var (header, _) = SetUpJobHeader();
		Factory.Save();

		return new CusTempStorageForm(header)
		{
			ControllerID = ControllerIDs.Customs.TemporaryStorage
		};
	}
}
