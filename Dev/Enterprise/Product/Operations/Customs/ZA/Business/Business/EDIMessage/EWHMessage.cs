using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.ZA.Business
{
	public class EWHMessage : EDIMessage
	{
		#region Constructor
		public EWHMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
		#endregion

		public new static readonly EDIMessageTypeDecider TypeDecider = new EDIMessageTypeDecider();

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_MessageType = ZAEDIMessageTypeList.Codes.ExternalWarehouse;
			EM_ApplicationCode = EDIInterchange.ApplicationCodes.SouthAfricanTransactionOrders;
		}

		protected override string GetMessageReferenceNumber()
		{
			return Env.NumberFountains.ZACustomsEDIFACTNumberFountain("M", EM_ApplicationCode).GetNextFormatted(Factory);
		}
	}
}
