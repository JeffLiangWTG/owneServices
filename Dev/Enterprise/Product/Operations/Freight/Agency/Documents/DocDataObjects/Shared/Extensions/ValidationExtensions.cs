using CargoWise.EntityFramework;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.Freight.Agency.Documents.DocDataObjects
{
	public static class ValidationExtensions
	{
		#region IsPartyNameAndAddressEmpty

		public static bool IsPartyNameAndAddressEmpty(this Address address)
		{
			if (address == null)
			{
				return true;
			}

			return address.CompanyName.IsEmpty
				|| address.AddressLine1.IsEmpty
				|| address.Country.Name.IsEmpty;
		}

		#endregion

		#region AddValidationDependencies

		public static void AddValidationDependencies(this DocDataObject docDataObject, ZPropertyInfo infoToValidate, params ZPropertyInfo[] infosOnWhichValidationDepends)
		{
			foreach (var info in infosOnWhichValidationDepends)
			{
				if (info?.BizObj is DocDataObject valueChangedDataObject)
				{
					valueChangedDataObject.OnValueChanged(info.Name).Do(() => docDataObject.Validate(infoToValidate.Name));
				}
			}
		}

		#endregion
	}
}
