using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;

namespace Enterprise.Customs.US.Business
{
	public class EDIMessageCollection : CBPEDIMessageCollection
	{
		public EDIMessageCollection(BusinessObject master)
			: base(master, CBPEDIMessage.AllCBPFilter)
		{
		}

		public new CBPEDIMessage this[int index]
		{
			get { return base[index]; }
		}

		public new CBPEDIMessage AddNew(Type bizOType)
		{
			return base.AddNew(bizOType);
		}

		#region New Methods

		internal void DiscardAll(IMessageAttachee newMaster)
		{
			foreach (CBPEDIMessage message in this.ToArray())
			{
				if (message.EM_LinkedObject != newMaster)
				{
					message.EM_LinkedObject = (BusinessObject)newMaster;
				}
				message.EM_Status = EDIMessage.Status.Discarded;
			}

			this.Load();
			newMaster.Messages.Load();
		}

		public MQEDIMessage GetLastMessageWithSpecificMessageBlock(ZString applicationCode, ZString messageType, ZString tRXorRCV, Type type)
		{
			return GetLastMessageWithSpecificMessageBlock<MQEDIMessage>(applicationCode, messageType, tRXorRCV, type);
		}

		#endregion

		protected override BusinessObject CreateBusinessObjectFromRow(DataRow row)
		{
			return new EDIMessage(Factory, row);
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}
	}
}
