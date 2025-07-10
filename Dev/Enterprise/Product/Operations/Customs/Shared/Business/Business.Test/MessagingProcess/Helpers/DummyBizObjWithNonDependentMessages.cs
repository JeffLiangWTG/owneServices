using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.Testing.MessagingProcess.Helpers
{
	internal class DummyBizObjWithNonDependentMessages : DummyBusinessObject, IEDIMessageCollectionOwner
	{
		public DummyBizObjWithNonDependentMessages(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public BusinessObject Child
		{
			get
			{
				return fChild;
			}
			set
			{
				fChild = value;
				fMessages = null;
			}
		}
		BusinessObject fChild;

		BusinessObject IEDIMessageCollectionOwner.MessageOwner => this;
		IBusinessObjectCollection IEDIMessageCollectionOwner.Messages => Messages;

		[ChildEditable(true)]
		public BusinessObjectCollection<EDIMessage> Messages
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
		BusinessObjectCollection<EDIMessage> fMessages;

		public bool MessagesAreLoaded
		{
			get { return fMessages != null; }
		}

		BusinessObjectCollection<EDIMessage> GetNewMessageCollection()
		{
			return new ParentAndChildEDIMessageCollection(this, Child, Factory);
		}
	}

	public class ParentAndChildEDIMessageCollection : BusinessObjectCollection<EDIMessage>
	{
		public ParentAndChildEDIMessageCollection(BusinessObject parent, BusinessObject child, BusinessObjectFactory factory) : base(factory)
		{
			this.parent = parent;
			this.child = child;
		}

		readonly BusinessObject parent;
		readonly BusinessObject child;

		protected override ZQuery CreateRelationshipFilter()
		{
			var query = base.CreateRelationshipFilter();

			var fks = new List<ZGuid>() { parent.PK };
			if (child != null)
			{
				fks.Add(child.PK);
			}

			query.AddToFilter(new ZQuery(EDIMessageSchema.EM_LinkUniqueID, fks.ToArray()));

			return query;
		}
	}
}
