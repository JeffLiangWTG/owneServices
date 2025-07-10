using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.ZA.Business
{
	public class STATACEDIMessage : SARSEDIMessage
	{
		public STATACEDIMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_MessageType = MessageTypes.STATAC;
		}
	}
}
