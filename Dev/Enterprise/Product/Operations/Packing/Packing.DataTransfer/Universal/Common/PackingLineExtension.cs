using System.Linq;
using System.Reflection;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Packing.DataTransfer.Universal
{
	public static class PackingLineExtension
	{
		public static bool IsLoosePackageIDDataObject(this PackingLine packingLine)
		{
			return packingLine.ReferenceNumber.HasValue && !packingLine.ReferenceNumber.Value.IsEmpty
				&& (!packingLine.PackQty.HasValue || (packingLine.PackQty.HasValue && packingLine.PackQty.Value == 1))
				&& IsOtherPropertiesExceptReferenceNumberAndPackQtyNull(packingLine);
		}

		static bool IsOtherPropertiesExceptReferenceNumberAndPackQtyNull(PackingLine packingLine)
		{
			return packingLine.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
					.Where(p => p.Name != nameof(packingLine.ReferenceNumber) && p.Name != nameof(packingLine.PackQty))
					.All(p => p.GetValue(packingLine) == null);
		}
	}
}
