using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.BuildTools.Testing;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static System.Windows.Forms.Control;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(RefDocTypeForm))]
	sealed class RefDocTypeFormTest : ZFormBasherTest
	{
		public void TestCheckBoxSizeShouldBeConsistent()
		{
			var docType = Factory.New<RefDocType>();
			using (var form = new RefDocTypeForm(docType))
			{
				form.Show();

				var previousCheckBoxs = new Dictionary<ZCheckBox, Size>();
				GetAllCheckBoxInTheForm(form.Controls, previousCheckBoxs);

				form.WindowState = FormWindowState.Minimized;
				Application.DoEvents();
				Task.Delay(50).Wait();

				form.WindowState = FormWindowState.Normal;
				Application.DoEvents();
				Task.Delay(50).Wait();

				var currentCheckBoxs = new Dictionary<ZCheckBox, Size>();
				GetAllCheckBoxInTheForm(form.Controls, currentCheckBoxs);

				foreach (var previous in previousCheckBoxs)
				{
					Assert("We can find the same checkbox.", currentCheckBoxs.TryGetValue(previous.Key, out var currentSize));
					AssertEquals("They are the same size.", previous.Value, currentSize);
				}
			}

			void GetAllCheckBoxInTheForm(ControlCollection controlCollection, Dictionary<ZCheckBox, Size> allCheckBox)
			{
				foreach (Control item in controlCollection)
				{
					if (item is ZCheckBox checkBox)
					{
						allCheckBox.Add(checkBox, checkBox.Size);
					}
					else if (item.Controls.Count > 0)
					{
						GetAllCheckBoxInTheForm(item.Controls, allCheckBox);
					}
				}
			}
		}

		public void TestFormIsFixedSingle()
		{
			RefDocType docType = Factory.New<RefDocType>();
			using (RefDocTypeForm form = new RefDocTypeForm(docType))
			{
				AssertEquals("Form should be FixedSingle, because otherwise the form doesn't resize for Events and Notes tabs", FormBorderStyle.FixedSingle, form.FormBorderStyle);
			}
		}

		public void TestShowPreSaveDialogs()
		{
			const string ExpectedWarningMessage = "Allow up to 30 minutes for the 'Force User to Read' change to take effect for other users";

			RefDocType docType = Factory.New<RefDocType>();
			docType.RT_DocType = "AAA";
			docType.RT_ReferenceType = "ALL";
			using (TestRefDocTypeForm form = new TestRefDocTypeForm(docType))
			{
				form.ShowPreSaveDialogs();
				AssertEquals("New and RT_ForceUserToRead=false", null, UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				docType.RT_ForceUserToRead = true;
				form.ShowPreSaveDialogs();
				AssertEquals("New and RT_ForceUserToRead=true", ExpectedWarningMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				Factory.Save();
				form.ShowPreSaveDialogs();
				AssertEquals("Saved and no change to RT_ForceUserToRead", null, UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				Factory.Save();
				docType.RT_ForceUserToRead = false;
				form.ShowPreSaveDialogs();
				AssertEquals("Saved and RT_ForceUserToRead changed from true->false", ExpectedWarningMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				Factory.Save();
				docType.RT_ForceUserToRead = true;
				form.ShowPreSaveDialogs();
				AssertEquals("Saved and RT_ForceUserToRead changed from false->true", ExpectedWarningMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			}
		}

		public void TestMergeDocTypes()
		{
			StmMenuItem mu1 = Factory.NewWithValidTestData<StmMenuItem>();
			StmMenuItem mu2 = Factory.NewWithValidTestData<StmMenuItem>();
			ZGuid pkSU1 = mu1.PK;
			ZGuid pkSU2 = mu2.PK;
			RefDocType docType1 = Factory.New<RefDocType>();
			docType1.RT_DocType = "ZZZ";
			docType1.RT_ReferenceType = "XXX";
			ZGuid pkDocType = docType1.PK;

			RefDocType docType2 = Factory.NewWithValidTestData<RefDocType>();

			StmMenuEDocs edoc1 = Factory.NewWithValidTestData<StmMenuEDocs>();
			edoc1.SX_SU = pkSU1;
			edoc1.SX_RT_DocType = docType1.PK;
			ZGuid pk1 = edoc1.PK;

			StmMenuEDocs edoc2 = Factory.NewWithValidTestData<StmMenuEDocs>();
			edoc2.SX_SU = pkSU2;
			edoc2.SX_RT_DocType = docType2.PK;
			ZGuid pk2 = edoc2.PK;

			StmMenuTemplatePivot pivot1 = Factory.NewWithValidTestData<StmMenuTemplatePivot>();
			pivot1.SI_RT_DocType = docType1.PK;

			Factory.Save();

			RefDocType docType3 = Factory.New<RefDocType>();
			docType3.RT_DocType = "ZZZ";
			docType3.RT_ReferenceType = "ALL";

			StmMenuEDocs edoc3 = Factory.NewWithValidTestData<StmMenuEDocs>();
			edoc3.SX_SU = pkSU1;
			edoc3.SX_RT_DocType = docType3.PK;
			ZGuid pk3 = edoc3.PK;
			edoc3.Factory.Save();

			using (TestRefDocTypeForm form = new TestRefDocTypeForm(docType3))
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				form.ShowPreSaveDialogs();
				Factory.Save();

				BusinessObjectFactory f = new BusinessObjectFactory();
				edoc1 = f.Load<StmMenuEDocs>(pk1);
				edoc2 = f.Load<StmMenuEDocs>(pk2);
				pivot1 = f.Load<StmMenuTemplatePivot>(pivot1.PK);
				edoc3 = f.Load<StmMenuEDocs>(pk3);
				docType1 = f.Load<RefDocType>(pkDocType);

				AssertNull(edoc1);
				AssertNotNull(edoc2);
				AssertNotNull(edoc3);
				AssertNotNull(pivot1);
				AssertNull(docType1);
				AssertEquals(docType2.PK, edoc2.SX_RT_DocType);
				AssertEquals(docType3.PK, pivot1.SI_RT_DocType);
			}
		}

		[RequiresSTA]
		public void TestSaveWithReadOnlyException()
		{
			var docType = Factory.New<RefDocType>();
			using (var form = new TestRefDocTypeFormWithReadOnlyException(docType))
			{
				form.Show();
				AssertNoExceptionThrown(form.ClickSaveButton);
			}
		}

		[UseSnapshotProtection]
		public void TestSaveNewDocumentType()
		{
			RefDocType docType = Factory.NewWithValidTestData<RefDocType>();
			using (TestRefDocTypeForm form = new TestRefDocTypeForm(docType))
			{
				docType.RT_DocType = "ZZZ";
				docType.RT_ReferenceType = "ALL";
				docType.RT_Desc = "ZZZZ";
				docType.RT_IsPublished = true;

				form.ClickSaveButton();
				AssertEquals("There's no confirm message pop up", null, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[UseSnapshotProtection]
		public void TestConfirmUpdateMessage()
		{
			RefDocType docType = Factory.NewWithValidTestData<RefDocType>();
			Factory.Save();

			Assert("Pre-Condition: IsPublished is false in the database.", !docType.RT_IsPublished);
			AssertNotEquals("Pre-Condition: IsPublished is false in the database.", "ZZZ", docType.RT_DocType);

			using (TestRefDocTypeForm form = new TestRefDocTypeForm(docType))
			{
				docType.RT_DocType = "ZZZ";
				docType.RT_ReferenceType = "ALL";
				docType.RT_Desc = "ZZZZ";
				docType.RT_IsPublished = true;

				form.ClickSaveButton();
				Assert("There's confirm message pop up", UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("Do you want to change the status of all eDocs with this Document Type to published?"));
				Assert("There's confirm message pop up", UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("Changing the Document Type and the Description will also change the Document Type and Description for documents that have been previously allocated."));
			}
		}

		public void TestOverrideDuplicatedDocTypesWithResultYes()
		{
			PrepareDocTypesForDuplicatedDocTypesTesting();
			var docType = Factory.NewWithValidTestData<RefDocType>();
			using (var form = new TestRefDocTypeForm(docType))
			{
				docType.RT_DocType = "AAA";
				docType.RT_ReferenceType = "ALL";
				docType.RT_Desc = "ALL AAA";

				form.ClickSaveButton();
				Assert("There's confirm message pop up", UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("There are already document types with a code of 'AAA' for the following categories: BPW,CSR. If you proceed, these document types will be deleted. Are you sure you want to proceed?"));

				var anotherFactory = new BusinessObjectFactory();
				var docTypesAAA = anotherFactory.Load<RefDocType>(new ZQuery(RefDocTypeSchema.RT_DocType, "AAA"));
				AssertEquals(1, docTypesAAA.Length);
				AssertEquals("ALL", docTypesAAA[0].RT_ReferenceType);
			}
		}

		public void TestOverrideDuplicatedDocTypesWithResultNo()
		{
			PrepareDocTypesForDuplicatedDocTypesTesting();
			var docType = Factory.NewWithValidTestData<RefDocType>();
			using (var form = new TestRefDocTypeForm(docType))
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				docType.RT_DocType = "AAA";
				docType.RT_ReferenceType = "ALL";
				docType.RT_Desc = "ALL AAA";

				form.ClickSaveButton();
				Assert("There's confirm message pop up", UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("There are already document types with a code of 'AAA' for the following categories: BPW,CSR. If you proceed, these document types will be deleted. Are you sure you want to proceed?"));

				var anotherFactory = new BusinessObjectFactory();
				var docTypesAAA = anotherFactory.Load<RefDocType>(new ZQuery(RefDocTypeSchema.RT_DocType, "AAA"));
				AssertEquals(2, docTypesAAA.Length);
				AssertEquals("BPW", docTypesAAA[0].RT_ReferenceType);
				AssertEquals("CSR", docTypesAAA[1].RT_ReferenceType);
			}
		}

		public void TestOverrideDuplicatedDocTypesWithSystemDocType()
		{
			PrepareDocTypesForDuplicatedDocTypesTesting(true);
			var docType = Factory.NewWithValidTestData<RefDocType>();
			using (var form = new TestRefDocTypeForm(docType))
			{
				docType.RT_DocType = "AAA";
				docType.RT_ReferenceType = "ALL";
				docType.RT_Desc = "ALL AAA";

				form.ClickSaveButton();
				Assert("There's confirm message pop up", UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(@"There are already document types with a code of 'AAA' for the following categories: BPW,CSR.
The Doc Type AAA with Category CSR is a system defined Doc Type and cannot be deleted. Please save with a category other than ALL."));

				var anotherFactory = new BusinessObjectFactory();
				var docTypesAAA = anotherFactory.Load<RefDocType>(new ZQuery(RefDocTypeSchema.RT_DocType, "AAA"));
				AssertEquals(2, docTypesAAA.Length);
				AssertEquals("BPW", docTypesAAA[0].RT_ReferenceType);
				AssertEquals("CSR", docTypesAAA[1].RT_ReferenceType);
			}
		}

		void PrepareDocTypesForDuplicatedDocTypesTesting(bool createOneSystemDocType = false)
		{
			var docType1 = Factory.NewWithValidTestData<RefDocType>();
			docType1.RT_DocType = "AAA";
			docType1.RT_ReferenceType = "BPW";
			docType1.RT_Desc = "BPW AAA";

			var docType2 = Factory.NewWithValidTestData<RefDocType>();
			docType2.RT_DocType = "AAA";
			docType2.RT_ReferenceType = "CSR";
			docType2.RT_Desc = "CSR AAA";
			docType2.RT_IsSystem = createOneSystemDocType;

			Factory.Save();
		}

		public void TestAuditPluginIsAdded()
		{
			using (var form = (RefDocTypeForm)GetFormToBashCore())
			{
				AssertNotNull("RefDocTypeForm should have audit plugin", form.PlugIns.IsPlugInAvailable(ControllerIDs.Audit));
			}
		}

		#region Test Classes

		class TestRefDocTypeForm : RefDocTypeForm
		{
			public TestRefDocTypeForm(RefDocType docType)
				: base(docType)
			{
			}

			public new ContinueWithSave ShowPreSaveDialogs()
			{
				return base.ShowPreSaveDialogs();
			}

			public void ClickSaveButton()
			{
				PostingButtonsUserControl.SaveButton.PerformClick();
			}
		}

		class TestRefDocTypeFormWithReadOnlyException : RefDocTypeForm
		{
			public TestRefDocTypeFormWithReadOnlyException(RefDocType docType)
				: base(docType)
			{
			}

			public new ContinueWithSave ShowPreSaveDialogs()
			{
				return base.ShowPreSaveDialogs();
			}

			public void ClickSaveButton()
			{
				PostingButtonsUserControl.SaveButton.PerformClick();
			}

			protected override void ShowNewForm()
			{
				var error = SqlExceptionBuilder.CreateSqlError(3906, 1, 1, "", "Failed to update database \"Odyssey_SD007\" because the database is read-only.", "", 1);
				var errorCollection = SqlExceptionBuilder.CreateSqlErrorCollection(error);
				var exception = SqlExceptionBuilder.CreateSqlException(errorCollection);

				throw exception;
			}
		}

		#endregion

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			return new RefDocTypeForm(Factory.New<RefDocType>());
		}

		protected override void SetUp()
		{
			base.SetUp();
			MockSourceControl.Setup();
		}

		protected override void TearDown()
		{
			base.TearDown();
			MockSourceControl.TearDown();
		}

		#endregion
	}
}
