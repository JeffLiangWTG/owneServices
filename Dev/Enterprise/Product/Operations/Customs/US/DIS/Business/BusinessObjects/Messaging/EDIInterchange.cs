using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.DIS.Business
{
	public class EDIInterchange : Enterprise.Messaging.Business.EDIInterchange, Integration.Customs.US.DIS.IEDIInterchange
	{
		public EDIInterchange(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override bool ShouldSendViaEHubCore
		{
			get { return true; }
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EI_ApplicationCode = EDIInterchange.ApplicationCodes.USCustomsDIS;
		}
	}
}
