using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ZA.Business
{
	public class OrgHeaderWrapper : NonPersistentBusinessObject, IObsoleteValidation
	{
		protected OrgHeaderWrapper(OrgHeader organisation)
			: base(organisation.Factory)
		{
			this.organisation = organisation;
		}

		readonly OrgHeader organisation;

		public static OrgHeaderWrapper New(OrgHeader organisation)
		{
			OrgHeaderWrapper result = null;

			if (organisation != null)
			{
				result = organisation.Factory.GetCachedValue(organisation.PK.ToStringKey(), delegate
				{
					return new OrgHeaderWrapper(organisation);
				});
			}

			return result;
		}

		public OrgCusAccountCollection FinancialAccountNumbers
		{
			get
			{
				if (financialAccountNumbers == null)
				{
					financialAccountNumbers = new OrgCusAccountCollection(organisation, Core.Constants.CountryCodes.SouthAfrica);
					financialAccountNumbers.Load();
					organisation.RegisterEditableChildObject(financialAccountNumbers);
				}
				return financialAccountNumbers;
			}
		}
		OrgCusAccountCollection financialAccountNumbers;
	}
}
