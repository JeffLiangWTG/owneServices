using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class PGAEntityIdentificationCodeAndNumberDetails
	{
		public PGAEntityIdentificationCodeAndNumberDetails(string entityIdentificationCode, string entityNumber)
		{
			this.entityNumber = entityNumber;
			this.entityIdentificationCode = entityIdentificationCode;
		}
		readonly ZString entityNumber;
		readonly ZString entityIdentificationCode;

		public ZString EntityIdentificationCode => entityNumber.IsEmpty ? ZString.Empty : entityIdentificationCode;
		public ZString EntityNumber => entityNumber;
	}
}
