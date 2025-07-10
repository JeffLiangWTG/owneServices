using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Business
{
	public abstract class CountrySpecificJobSupport<T> where T : BusinessObject
	{
		protected T SupportedBO { get; set; }
		protected abstract ZString CountryCode { get; }

		public bool Register(T bizObj)
		{
			SupportedBO = bizObj;
			if (Apply())
			{
				RegisterCore();
				return true;
			}

			return false;
		}

		public void Unregister()
		{
			UnregisterCore();
		}

		protected virtual bool Apply()
		{
			return GlbCompany.CurrentCompany != null && GlbCompany.CurrentCompany.GC_RN_NKCountryCode == CountryCode || CountryCode.IsEmpty;
		}

		protected virtual void RegisterCore()
		{
		}

		protected virtual void UnregisterCore()
		{
		}
	}
}
