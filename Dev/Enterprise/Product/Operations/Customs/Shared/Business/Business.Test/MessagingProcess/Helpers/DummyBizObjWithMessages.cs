using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.Business.MessagingProcess.Testing
{
	public class DummyBizObjWithMessages : DummyBusinessObject, IEDIMessageCollectionOwner
	{
		public DummyBizObjWithMessages(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		BusinessObject IEDIMessageCollectionOwner.MessageOwner => this;
		IBusinessObjectCollection IEDIMessageCollectionOwner.Messages => Messages;

		[ChildEditable(true)]
		public EDIMessageCollection Messages
		{
			get
			{
				if (fMessages == null)
				{
					fMessages = GetNewMessageCollection();
					fMessages.Load();
					fMessages.SetReadOnlyIncludingChildren(true);
					RegisterEditableChildObject(fMessages);
				}
				return fMessages;
			}
		}
		EDIMessageCollection fMessages;

		public bool MessagesAreLoaded
		{
			get { return fMessages != null; }
		}

		EDIMessageCollection GetNewMessageCollection()
		{
			return new EDIMessageCollection(this, Factory);
		}
	}
}
