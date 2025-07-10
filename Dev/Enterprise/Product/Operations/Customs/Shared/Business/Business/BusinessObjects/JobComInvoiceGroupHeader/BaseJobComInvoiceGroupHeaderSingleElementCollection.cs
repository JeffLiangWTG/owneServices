using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	public class BaseJobComInvoiceGroupHeaderSingleElementCollection : BusinessObjectCollection<BaseJobComInvoiceGroupHeader>
	{
		public BaseJobComInvoiceGroupHeaderSingleElementCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public void SwapGroup(BaseJobComInvoiceGroupHeader group)
		{
			bool hasADifferentGroupBeenPassedIn = group != (Count > 0 ? this[0] : null);

			if (hasADifferentGroupBeenPassedIn)
			{
				using (SuspendListChanged())
				{
					RemoveAll();
					if (group != null)
					{
						Add(group);
					}
				}
				//when it is disposed, FireListResetEvent is called
			}
		}

		#region SetOverrideDeclaration

		public void SetOverrideDeclaration(BaseJobDeclaration declaration)
		{
			if (declaration == null || declaration.SupportAdditionalInvoices)
			{
				foreach (BaseJobComInvoiceGroupHeader header in this)
				{
					header.OverrideParent = declaration;
				}
			}
		}

		#endregion
	}
}
