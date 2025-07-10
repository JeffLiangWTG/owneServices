using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.NZ.Registry;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.NZ.Business.Declaration
{
	public class NZCMessageCollection : EDIMessageCollection
	{
		public NZCMessageCollection(BusinessObject master)
			: base(master, master.Factory)
		{
		}

		public new NZCMessage this[int index]
		{
			get { return (NZCMessage)Elements[index]; }
		}

		public virtual new NZCMessage AddNew()
		{
			return (NZCMessage)base.AddNew();
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			var filter = base.CreateAdditionalFilter();
			filter.AddToFilter(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.NZCustoms);
			return filter;
		}

#if DEBUG
		public NZCMessage AddNewTestTransmitMessage()
		{
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "00009917B");
			NZCMessage result = AddNew();
			result.EM_MessageText = NZCMessage.MessageNumberPlaceHolder;
			result.EM_ReceiveTransmit = NZCMessage.Direction.Transmit;
			return result;
		}
#endif
	}
}
