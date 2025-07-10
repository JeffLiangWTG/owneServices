using CargoWise.Application;

namespace Enterprise.MasterFiles.Business.DIS
{
	public static class DISPreFormActionRegistrar
	{
		public static IDISPreFormActionRunner GetDISPreFormActionRunner(IDISHost disHost)
		{
			IDISPreFormActionRegistrar registrar = null;
			IDISPreFormActionRunner runner = null;

			switch (Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(GlbCompany.CurrentCompany.GC_RN_NKCountryCode.ToString()))
			{
				case Core.Constants.CountryCodes.UnitedStates:
					registrar = ObjectFactory.Get<IDISPreFormActionRegistrar>("US.IDISPreFormActionRegistrar");
					break;
				case Core.Constants.CountryCodes.Canada:
					registrar = ObjectFactory.Get<IDISPreFormActionRegistrar>("CA.IDISPreFormActionRegistrar");
					break;
			}

			if (registrar != null)
			{
				runner = registrar.GetDISPreFormActionRunner(disHost);
			}

			return runner ?? new DISPreFormActionRunner(disHost);
		}
	}
}
