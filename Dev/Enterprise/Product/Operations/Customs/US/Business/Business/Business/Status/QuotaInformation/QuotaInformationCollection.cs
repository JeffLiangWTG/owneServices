using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	public class QuotaInformationCollection : NonPersistentBusinessObjectCollection<QuotaInformation>
	{
		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new QuotaInformation();
		}
	}
}
