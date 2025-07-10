using System;
using System.Collections.Generic;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Web.WebService.Business
{
#if DEBUG
	[Serializable]
#endif
	public class UnitConversionCollection
	{
		#region Constructors

		public UnitConversionCollection()
		{
		}

		public UnitConversionCollection(OrgSupplierPart part)
		{
			ProductPK = part.PK.ToGuid();

			var table = new ConversionsToSKUTable(part);

			foreach (var conversion in table)
			{
				Conversions.Add(new UnitConversion(conversion.PackType, conversion.QtySKU));
			}
		}

		#endregion

		#region Properties

		#region ProductPK

		public Guid ProductPK { get; set; }

		#endregion

		#region Conversions

		public List<UnitConversion> Conversions
		{
			get { return conversions ?? (conversions = new List<UnitConversion>()); }
			set { conversions = value; }
		}

		List<UnitConversion> conversions;

		#endregion

		#endregion
	}
}
