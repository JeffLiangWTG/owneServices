using CargoWise.Common;
using Enterprise.Core;
using Enterprise.Registry.Business.eServices;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.Registry
{
	public class CargoIMPServiceProviderList : CodeDescriptionPairList
	{
		#region New

		protected CargoIMPServiceProviderList()
		{
			AddPair(Constants.AWB.CargoIMPServiceProviderConstants.CCN, ResString.GetMultilingualString("2e6d4ea9-956f-4648-adf2-30fb8aec28ab", "Cargo Community Network"));
			AddPair(Constants.AWB.CargoIMPServiceProviderConstants.Descartes, ResString.GetMultilingualString("1851c4d6-8005-48a6-afba-6b019afe4bac", "Descartes"));
			AddPair(Constants.AWB.CargoIMPServiceProviderConstants.HUB, ResString.GetMultilingualString("1851c4d6-8005-48a6-afba-7d019afe4bac", "CargoWise eService Bureau"));

			if (eAdaptorRegistry.Instance.CanSendCargoImpMessagesThroughEAdaptor.Value)
			{
				AddPair(Constants.AWB.CargoIMPServiceProviderConstants.EDP, ResString.GetMultilingualString("1851c4d6-8005-48a6-afba-8t019afe4bac", "eAdaptor"));
			}
		}

		public static CargoIMPServiceProviderList New()
		{
			CargoIMPServiceProviderList result = null;

			var overridden = OverridableNewDelegate.Value;
			if (overridden != null)
			{
				result = overridden();
			}
			else
			{
				result = new CargoIMPServiceProviderList();
			}

			return result;
		}

		protected delegate CargoIMPServiceProviderList NewDelegate();
		protected static readonly Overridable<NewDelegate> OverridableNewDelegate = new Overridable<NewDelegate>();

		#endregion
	}
}
