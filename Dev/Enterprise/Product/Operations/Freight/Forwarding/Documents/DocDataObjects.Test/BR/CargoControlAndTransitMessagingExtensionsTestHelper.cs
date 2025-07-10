using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Freight.Forwarding.Documents.Testing.BR
{
	class CargoControlAndTransitMessagingExtensionsTestHelper
	{
		public CargoControlAndTransitMessagingExtensionsTestHelper(BusinessObjectFactory factory)
		{
			Factory = factory;
		}

		BusinessObjectFactory Factory { get; set; }

		public void CreateNewCCTPassword()
		{
			var password = Factory.New<IGlbExternalPassword>();
			password.GP_PasswordType = PasswordTypesList.Codes.CCT;
			password.GP_GC = GlbCompany.CurrentCompany.PK;
			password.GP_GS = GlbStaff.CurrentUser.PK;
			Factory.Save();
		}
	}
}
