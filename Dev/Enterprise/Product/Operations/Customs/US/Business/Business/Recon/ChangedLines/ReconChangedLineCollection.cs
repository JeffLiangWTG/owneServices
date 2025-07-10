using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	public class ReconChangedLineCollection : NonPersistentBusinessObjectCollection<ReconChangedLine>
	{
		public ReconChangedLineCollection(BusinessObjectFactory factory)
			: base(factory) { }

		#region Implementation

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ReconChangedLine(Factory);
		}

		#endregion
	}
}
