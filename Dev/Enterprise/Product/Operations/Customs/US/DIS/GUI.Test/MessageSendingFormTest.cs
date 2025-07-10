using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.US.DIS;
using Enterprise.Customs.GUI;
using Enterprise.Customs.GUI.Testing;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.DIS.Business;
using Enterprise.Customs.US.DIS.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.DIS.GUI.Testing
{
	[TestedType(typeof(MessageSendingForm))]
	sealed class MessageSendingFormTest : MessageSendingFormBaseTest<MessageSendingAction, DISDocument>
	{
		public new void TestSendButtonWithErrors()
		{
			var docManagerSupport = (IDocManagerSupport)JobDeclaration;
			var storageMain = docManagerSupport.DocManagerInfo.MasterFactory.RetrieveExistingOrCreateStorageMain(JobDeclaration, "TST");
			var eDocs1 = storageMain.AddFileOrDocument(new byte[] { 1, 2, 3 }, "ABㅕC.pdf", "ABC", false);
			var eDocs2 = storageMain.AddFileOrDocument(new byte[] { 1, 2, 3 }, "ABC×.pdf", "ABC", false);

			var disDocument1 = HostWrapper.DISDocuments.AddNew();
			disDocument1.Status = StatusList.Codes.AOS;
			disDocument1.EDocsDocumentPK = eDocs1.UniqueKey;
			var disDocument2 = HostWrapper.DISDocuments.AddNew();
			disDocument2.Status = StatusList.Codes.AOS;
			disDocument2.EDocsDocumentPK = eDocs2.UniqueKey;

			var coll = GetCollection();

			using (var form = GetForm(coll))
			{
				form.Show();
				coll[0].Send = true;
				coll[1].Send = true;
				Assert("1.Warnings showed on DIS SendingAction Form", !coll[0].MessageSendingWarning.IsEmpty);
				Assert("2.Warnings showed on DIS SendingAction Form", !coll[1].MessageSendingWarning.IsEmpty);
			}
		}

		protected override Form GetFormToBashCore()
		{
			return new MessageSendingForm(new MessageSendingActionCollection(HostWrapper as DISHostWrapper));
		}

		protected override MessageSendingFormBase<MessageSendingAction, DISDocument> GetForm(MessageSendingActionCollectionBase<MessageSendingAction, DISDocument> collection)
		{
			return new MessageSendingForm(collection as MessageSendingActionCollection);
		}

		protected override MessageSendingActionCollectionBase<MessageSendingAction, DISDocument> GetCollection()
		{
			return new MessageSendingActionCollection(HostWrapper as DISHostWrapper);
		}

		protected override DISHostWrapperBase<DISDocument> HostWrapper
		{
			get { return hostWrapper ?? (hostWrapper = new DISHostWrapper((MasterFiles.Business.DIS.IUSDISHost)JobDeclaration)); }
		}
		DISHostWrapper hostWrapper;

		protected override BusinessObject JobDeclaration
		{
			get
			{
				if (jobDeclaration == null)
				{
					jobDeclaration = new TestHelper(Factory).GetJobDeclaration();
					jobDeclaration["JE_AddInfo"] = "EntryFilerCode=XJ5";

					USCustomsDataRegistry.Instance.EntryFiler.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new EntryFiler() { EntryFilerCode = "XJ6" });
				}
				return jobDeclaration;
			}
		}
		BusinessObject jobDeclaration;
	}
}
