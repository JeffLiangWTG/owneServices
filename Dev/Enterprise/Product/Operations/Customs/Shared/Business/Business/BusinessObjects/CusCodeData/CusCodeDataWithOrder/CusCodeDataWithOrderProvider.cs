using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public abstract class CusCodeDataWithOrderProvider
	{
		protected CusCodeDataWithOrderProvider(ZString countryCode)
		{
			CountryCode = countryCode;
		}

		public ZString CountryCode { get; }

		public virtual ZShort CodesStartingOrder => 3;

		public virtual ZShort NumberOfCodes => 8;
	}
}
