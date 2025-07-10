using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ZA.Business
{
	public class CustomsResponseCollection : NonDependentEDIMessageCollection
	{
		public CustomsResponseCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public virtual new CUSRESEDIMessage AddNew()
		{
			return (CUSRESEDIMessage)base.AddNew();
		}

		public new CUSRESEDIMessage this[int index]
		{
			get { return (CUSRESEDIMessage)base[index]; }
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			var result = base.CreateAdditionalFilter();
			result.AddToFilter(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.SouthAfricanCustoms);
			result.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Receive);
			result.AddToFilter(EDIMessageSchema.EM_MessageType, new ZString[] { CustomsResponseMessageTypeList.Codes.RES });
			return result;
		}
	}
}
