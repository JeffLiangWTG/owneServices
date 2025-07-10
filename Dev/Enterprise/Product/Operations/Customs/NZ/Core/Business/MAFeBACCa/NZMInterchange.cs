using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.NZ.Business.MAFeBACCa
{
	public class NZMInterchange : EDIInterchange, Integration.Customs.NZ.INZEBACCAInterchange
	{
		public NZMInterchange(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EI_ApplicationCode = EDIInterchange.ApplicationCodes.NewZealandMAFeBACCa;
		}

		protected override bool ShouldSendViaEHubCore
		{
			get { return false; }
		}
	}
}
