using CargoWise.Types;

namespace Enterprise.Customs.ZA.Manifest.Business
{
	public class ABLEntryNumProvider : ASYCUDA.Business.ABLEntryNumProvider
	{
		public ABLEntryNumProvider(ZString countryCode)
			: base(countryCode)
		{
		}

		public override ASYCUDA.Business.ABLEntryNumValidation GetNewValidation(ASYCUDA.Business.ABLEntryNum entryNumber) => new ABLEntryNumValidation(entryNumber);
	}
}
