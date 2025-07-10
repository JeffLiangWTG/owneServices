using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.GUI.MessagingProcess.Testing
{
	sealed class DialogTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("Business Entity Missing", () => { var crash = new DialogForTest(null, typeof(ValidDialogTestForm)); });
			AssertExceptionThrown<ArgumentNullException>("Form Type Missing", () => { var crash = new DialogForTest(bizObj, null); });

			var dlg = new DialogForTest(bizObj, typeof(ValidDialogTestForm));
			AssertSame("Business Entity", bizObj, dlg.BusinessEntity_Exposed);
			AssertEquals("Form Type", typeof(ValidDialogTestForm), dlg.TypeOfForm_Exposed);
		}

		public void TestIsIDialog()
		{
			var iDlg = new DialogForTest(bizObj, typeof(ValidDialogTestForm)) as IDialog;
			AssertSame("DataSource", bizObj, iDlg.DataSource);
			AssertEquals("Form Type", typeof(ValidDialogTestForm), iDlg.TypeOfForm);
		}

		protected override void SetUp()
		{
			base.SetUp();
			bizObj = Factory.New<DummyBusinessObject>();
		}

		DummyBusinessObject bizObj;
	}

	sealed class DialogForTest : Dialog
	{
		public DialogForTest(IBusiness businessEntity, Type typeOfForm) : base(businessEntity, typeOfForm) { }

		public IBusiness BusinessEntity_Exposed => BusinessEntity;
		public Type TypeOfForm_Exposed => TypeOfForm;
	}
}
