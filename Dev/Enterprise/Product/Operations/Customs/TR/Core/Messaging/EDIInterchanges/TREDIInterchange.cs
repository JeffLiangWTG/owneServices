using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.TR.Messaging.EDIInterchanges
{
	public class TREDIInterchange : EDIInterchange, Integration.Customs.TR.IEDIInterchange
	{
		public TREDIInterchange(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			EI_ApplicationCode = ApplicationCodes.TRCustoms;
		}

		protected override bool ShouldSendViaEHubCore => EI_TransportType != EDIInterchange.TransportType.xT;
	}
}
