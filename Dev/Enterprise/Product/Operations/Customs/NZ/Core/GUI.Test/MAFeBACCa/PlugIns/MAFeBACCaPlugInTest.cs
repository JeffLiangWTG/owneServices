using System;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.NZ.Business;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Business.Documents.MAFCoverSheet;
using Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ;
using Enterprise.Customs.NZ.Business.MAFeBACCa.MessageBuilders;
using Enterprise.Customs.NZ.GUI.Declaration;
using Enterprise.Customs.NZ.GUI.Documents;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.NZ.GUI.MAFeBACCa.Testing
{
	public class MAFeBACCaPlugInTest : TestCaseWithFactory
	{
		public void TestGetsRightMenuAndUserControl()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			using (var plugin = new MAFeBACCaPlugIn(new MAFPlugInSupportDeclarationWrapper(declaration)))
			{
				AssertEquals("plugin.TopLevelMenu.GetType()", typeof(MAFeBACCaMenu), plugin.TopLevelMenu.GetType());
				AssertEquals("plugin.UserControl.GetType()", typeof(MAFeBACCaUserControl), plugin.UserControl.GetType());
			}
		}

		public void TestMAFCoverSheetEventHandler()
		{
			Business.EDITariff_ReferenceFiles_NZ.Testing.NZCTariffVersionLoaderTest.SetDataVersion(NZCTariffVersionLoader.MinimumDataVersionRequired);
			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_MenuName = JobDeclarationDocumentSupporter.DocNames.MAFCoverSheet;
			var eventArgs = new DocumentCancelEventArgs(menuItem);
			var docEvents = new DocumentEventsForTesting();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.DocumentSupporter.Initialise(docEvents);
			using (var form = new DeclarationForm(declaration))
			{
				form.Show();
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.No;
				docEvents.RaiseDocumentPrintRequested(declaration, eventArgs);
				AssertEquals("Print should be cancelled", true, eventArgs.Cancel);
				AssertEquals("LastFormShownForTest", typeof(MAFCoverSheetForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
				AssertNull("NZDocsMAFCoverSheet should NOT be created in Factory", Factory.GetValue<NZDocsMAFCoverSheet>());
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;
				docEvents.RaiseDocumentPrintRequested(declaration, eventArgs);
				AssertEquals("Print should NOT be cancelled", false, eventArgs.Cancel);
				AssertEquals("LastFormShownForTest", typeof(MAFCoverSheetForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
				AssertNotNull("NZDocsMAFCoverSheet should be created in Factory", Factory.GetValue<NZDocsMAFCoverSheet>());
			}
		}

		public void TestPlugInNotDisplayedMessage_NullUser()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			using (var plugIn = new MAFeBACCaPlugIn(new MAFPlugInSupportDeclarationWrapper(declaration)))
			{
				plugIn.Mutex.Lock();
				AssertEquals("Mutex is locked", true, plugIn.Mutex.HasLock);
				using (var plugIn2 = new MAFeBACCaPlugIn(new MAFPlugInSupportDeclarationWrapper(declaration)))
				{
					AssertEquals(false, plugIn2.Mutex.Lock());
					var lockInfo = MutexIDs.JobBeingCreatedForShipment.Name;
					var emptyGuid = Guid.Empty;
					var sql = $@"UPDATE TOP(1) StmServiceHeartBeat
									SET SV_ParentId = '{emptyGuid}',
										SV_SystemLastEditTimeUtc = GetUtcDate(),
										SV_SystemLastEditUser = 'USR'
									FROM dbo.StmServiceSemaphore
									INNER JOIN dbo.StmServiceHeartBeat ON SS_SV = SV_PK
									WHERE SS_LockInfo LIKE '%{lockInfo}%';";
					TestConnection.ExecuteNonQuery(sql);
					AssertContains("Null User for LockInfo.UserWithLock should not cause a Null Exception", "Unknown", plugIn2.PlugInNotDisplayedMessage);
				}
			}

			using (var plugIn = new MAFeBACCaPlugIn(new MAFPlugInSupportDeclarationWrapper(declaration)))
			{
				AssertContains("Null lock should not cause an Null Exception", "Someone else", plugIn.PlugInNotDisplayedMessage);
			}
		}
	}
}
