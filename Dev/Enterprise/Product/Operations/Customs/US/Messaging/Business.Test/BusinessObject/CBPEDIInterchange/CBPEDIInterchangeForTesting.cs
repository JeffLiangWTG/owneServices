using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Messaging.Business.Testing
{
	sealed class CBPEDIInterchangeForTesting : CBPEDIInterchange
	{
		public CBPEDIInterchangeForTesting(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EI_ApplicationCode = CBPEDIInterchange.ApplicationCodeForTesting;
		}
	}
}
