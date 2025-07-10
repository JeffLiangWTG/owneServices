using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class SealNumberBusinessObject : AutoSealNumberBusinessObject
	{
		public SealNumberBusinessObject(ZString sealNumber)
		{
			this.SealNumber = sealNumber.Trim().Left(SealNumberBusinessObject.Schema.SealNumberMaxLength);
		}
	}
}
