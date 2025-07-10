using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ZA.Business
{
	public class GenralMessageCollection : NonDependentEDIMessageCollection
	{
		public GenralMessageCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public virtual new GENRALMessage AddNew()
		{
			return (GENRALMessage)base.AddNew();
		}

		public new GENRALMessage this[int index]
		{
			get { return (GENRALMessage)base[index]; }
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			var result = base.CreateAdditionalFilter();
			result.AddToFilter(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.SouthAfricanCustoms);
			result.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Receive);
			result.AddToFilter(EDIMessageSchema.EM_MessageType, new ZString[] { CustomsResponseMessageTypeList.Codes.GEN });
			return result;
		}
	}
}
