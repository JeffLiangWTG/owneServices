using System.Collections.Generic;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	public class MaximumCreditLimitCollection : RegistryBusinessObjectCollectionTemplate
	{
		public MaximumCreditLimitCollection()
		{
		}

		public MaximumCreditLimitCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		protected override bool AllowNewCore => CurrentFallbackLevel?.Level != RegistryStorageFlags.Company || Count < 1;

		public new MaximumCreditLimitItem AddNew() => base.AddNew() as MaximumCreditLimitItem;

		public new MaximumCreditLimitItem this[int index] => Elements[index] as MaximumCreditLimitItem;

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new MaximumCreditLimitItem(CurrentFallbackLevel, CurrentFactory);
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new MaximumCreditLimitCollection(fallbackLevel, factory);
		}

		protected override void PerformPostCloneAction(IRegistryBusiness registryBusiness)
		{
			if (registryBusiness is MaximumCreditLimitCollection creditLimitCollection && creditLimitCollection?.CurrentFallbackLevel?.Level == RegistryStorageFlags.Company)
			{
				var itemsToRemove = new List<MaximumCreditLimitItem>();
				var currencyPk = CurrentFactory?.Load<GlbCompany>(creditLimitCollection.CurrentFallbackLevel.CompanyPK(false))?.LocalCurrency?.PK;
				foreach (MaximumCreditLimitItem creditLimitItem in creditLimitCollection)
				{
					if (creditLimitItem.CurrencyPK != currencyPk)
					{
						itemsToRemove.Add(creditLimitItem);
					}
				}

				foreach (var creditLimitItem in itemsToRemove)
				{
					creditLimitCollection.Remove(creditLimitItem);
				}
			}
			base.PerformPostCloneAction(registryBusiness);
		}
	}
}
