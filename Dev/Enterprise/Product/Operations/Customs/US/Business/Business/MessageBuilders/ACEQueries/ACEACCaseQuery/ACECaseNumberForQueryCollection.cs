using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	public class ACECaseNumberForQueryCollection : NonPersistentBusinessObjectCollection<ACECaseNumberForQuery>, IObsoleteValidation
	{
		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ACECaseNumberForQuery();
		}
	}
}
