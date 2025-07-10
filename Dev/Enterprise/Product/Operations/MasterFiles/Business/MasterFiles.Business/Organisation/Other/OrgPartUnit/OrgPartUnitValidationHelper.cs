using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public static class OrgPartUnitValidationHelper
	{
		static string GetMessage(IEnumerable<string> undefinedPackTypes) => Res.GetString("c51f565a-d9f1-4f15-8afd-c7355dc2d403", "Error during import: Invalid package types in unit conversion: {0}", string.Join(", ", undefinedPackTypes));

		public static bool ValidatePackTypes<TProduct, TUnitConversion>(
			BusinessObjectFactory factory,
			TProduct product,
			Func<TProduct, IEnumerable<TUnitConversion>> getUnitConversions,
			Func<TUnitConversion, string> getPackType,
			Func<TUnitConversion, string> getParentPackType,
			Action<TProduct, string> failAction)
		{
			var isValid = true;

			if (DataRegistry.Instance.UnitConversionPackTypesValidation)
			{
				var conversions = getUnitConversions(product);
				var packTypesToCheck = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
				foreach (var conversion in conversions)
				{
					packTypesToCheck.Add(getPackType(conversion).Trim());
					packTypesToCheck.Add(getParentPackType(conversion).Trim());
				}

				var referencePackTypes = RefPackTypeCollection.GetAsCodeDescriptionPairWithStandardUnits(factory).GetAllCodes();
				packTypesToCheck.ExceptWith(referencePackTypes);

				if (packTypesToCheck.Any())
				{
					isValid = false;
					var message = GetMessage(packTypesToCheck);
					failAction(product, message);
				}
			}

			return isValid;
		}
	}
}
