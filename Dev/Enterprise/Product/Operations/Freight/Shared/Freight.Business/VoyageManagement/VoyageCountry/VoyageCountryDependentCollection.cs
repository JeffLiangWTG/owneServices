using System;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Freight.Business
{
	public class VoyageCountryDependentCollection : DependentBusinessObjectCollection<VoyageCountry, JobVoyage>
	{
		public VoyageCountryDependentCollection(JobVoyage voyage)
			: base(voyage) { }

		public VoyageCountry GetCountry(string code, bool createIfNotExists)
		{
			if (string.IsNullOrEmpty(code))
			{
				return null;
			}

			VoyageCountry result = null;

			for (int i = Count - 1; i >= 0; i--)
			{
				VoyageCountry country = this[i];

				if (country.J0_RN_NKCountry != code)
				{
					continue;
				}

				if (result == null)
				{
					result = country;
				}
				else if (!country.IsInDatabase)
				{
					country.Delete();
				}
				else if (!result.IsInDatabase)
				{
					result.Delete();
					result = country;
				}
			}

			if (createIfNotExists && result == null && code.Length == 2)
			{
				result = AddNew();
				result.J0_RN_NKCountry = code;
				result.HasChanges = false;
			}

			return result;
		}

		#region BusinessObjectCollection Overrides

		public override Type GetTypeOfElementsFromPK(ZGuid pK)
		{
			return typeof(VoyageCountry);
		}

		#endregion
	}
}
