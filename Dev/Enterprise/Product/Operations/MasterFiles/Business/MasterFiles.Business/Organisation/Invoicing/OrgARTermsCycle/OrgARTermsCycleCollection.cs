using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class OrgARTermsCycleCollection : ActiveBusinessObjectCollection<OrgARTermsCycle>
	{
		public OrgARTermsCycleCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public OrgARTermsCycleCollection(OrgARTerms master)
			: base(master)
		{
		}

		protected override void SetDefaultsForNewElementCore(OrgARTermsCycle newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);

			if (ReadOnly)
			{
				newElement.SetReadOnlyIncludingChildren(true);
			}
		}
	}
}
