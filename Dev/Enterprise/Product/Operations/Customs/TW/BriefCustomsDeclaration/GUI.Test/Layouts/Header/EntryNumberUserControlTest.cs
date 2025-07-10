using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TW.BriefCustomsDeclaration.Business;
using Enterprise.Customs.TW.Business;
using Enterprise.Customs.TW.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.GUI.Testing
{
	[TestedType(typeof(EntryNumberUserControl))]
	sealed class EntryNumberUserControlTest : TestCaseWithFactory
	{
		public void TestAllocateEntryNumberButton_Click()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Import23;
			header.AMA_CustomsOffice = "AB";
			header.AMA_RecipientReference = "123";
			header.DeclarationDate = new ZDateTime(2020, 07, 22);

			using (var form = new ZForm(header))
			using (var control = new EntryNumberUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				header.Factory.SuspendValidation();
				control.FindSingle<ZButton>("AllocateEntryNumberButton").PerformClick();
				AssertEquals("Entry Number is allocated, and yet it was " + header.DeclarationNumber, "AB  0912300001", header.DeclarationNumber);

				var factory2 = new BusinessObjectFactory();
				var headerLoaded = factory2.Load<AsycudaManifestHeader>(header.PK);
				AssertEquals("AB  0912300001", headerLoaded.DeclarationNumber);

				header.BagNumber = "BAA999";
				control.FindSingle<ZButton>("AllocateEntryNumberButton").PerformClick();
				AssertEquals("Entry Number is allocated, and yet it was " + header.DeclarationNumber, "AB  09123AA999", header.DeclarationNumber);

				headerLoaded = factory2.Load<AsycudaManifestHeader>(header.PK);
				AssertEquals("AB  09123AA999", headerLoaded.DeclarationNumber);

				header.BagNumber = "A999";
				control.FindSingle<ZButton>("AllocateEntryNumberButton").PerformClick();
				AssertEquals("Entry Number is allocated, and yet it was " + header.DeclarationNumber, "AB  091230A999", header.DeclarationNumber);
				headerLoaded = factory2.Load<AsycudaManifestHeader>(header.PK);
				AssertEquals("AB  091230A999", headerLoaded.DeclarationNumber);
			}
		}

		public void TestModifyEntryNumberButton_Click()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Import23;
			header.AMA_CustomsOffice = "AB";
			header.AMA_RecipientReference = "123";
			header.DeclarationDate = new ZDateTime(2020, 07, 22);
			header.DeclarationNumber = "AB  0912300001";
			Factory.Save();

			using (var form = new ZForm(header))
			using (var control = new EntryNumberUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				header.Factory.SuspendValidation();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.SetDelegateToCallOnFormShown(obj =>
				{
					var dialog = (AllocateNumberForm)obj;
					var allocateNumber = (AllocateNumber)dialog.BusinessEntity;
					allocateNumber.Part5Number = "00005";
					dialog.FindSingle<ZButton>("OKButton").PerformClick();
				});
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				control.FindSingle<ZButton>("ModifyEntryNumberButton").PerformClick();
				AssertEquals("AB  0912300005", header.DeclarationNumber);
				Factory.Save();

				var factory2 = new BusinessObjectFactory();
				var headerLoaded = factory2.Load<AsycudaManifestHeader>(header.PK);
				AssertEquals("AB  0912300005", headerLoaded.DeclarationNumber);
			}
		}
	}
}
