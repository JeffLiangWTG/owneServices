using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	public class ReconFlattenedDataLineCollection : NonPersistentBusinessObjectCollection<ReconFlattenedDataLine>
	{
		public ReconFlattenedDataLineCollection(BusinessObjectFactory factory)
			: base(factory) { }

		#region Implementation

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ReconFlattenedDataLine();
		}

		#endregion
	}
}
