using CargoWise.EntityFramework;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ConsolTemplateCollection : NonPersistentBusinessObjectCollection<ConsolTemplate>
	{
		public ConsolTemplateCollection(MultiDaysSelection multiDaysSelection)
			: base(multiDaysSelection.Factory)
		{
			this.multiDaysSelection = multiDaysSelection;
		}
		readonly MultiDaysSelection multiDaysSelection;

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ConsolTemplate(multiDaysSelection);
		}
	}
}
