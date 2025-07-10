using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.NZ.Business
{
	public class NZCInterchange : EDIInterchange, Integration.Customs.NZ.INZCustomsInterchange
	{
		public NZCInterchange(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EI_ApplicationCode = EDIInterchange.ApplicationCodes.NewZealandCustoms;
		}

		protected override bool ShouldSendViaEHubCore => true;
	}
}
