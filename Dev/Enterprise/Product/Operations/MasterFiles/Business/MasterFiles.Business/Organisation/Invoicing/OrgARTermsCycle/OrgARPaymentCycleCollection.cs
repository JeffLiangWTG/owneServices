using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class OrgARPaymentCycleCollection : ActiveBusinessObjectCollection<OrgARPaymentCycle>
	{
		public OrgARPaymentCycleCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public OrgARPaymentCycleCollection(OrgARTerms master)
			: base(master)
		{
		}

		protected override void SetDefaultsForNewElementCore(OrgARPaymentCycle newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);

			if (ReadOnly)
			{
				newElement.SetReadOnlyIncludingChildren(true);
			}
		}
	}
}
