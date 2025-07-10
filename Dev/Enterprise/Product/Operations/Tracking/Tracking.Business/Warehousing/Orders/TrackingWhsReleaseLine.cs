using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Tracking.Business
{
	public class TrackingWhsReleaseLine : NonPersistentBusinessObject
	{
		public static class Constants
		{
			public const string ProductCode = "ProductCode";
			public const string ProductDescription = "ProductDescription";
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Its a constant")]
			public const string Packs = "Packs";
			public const string PacksUQ = "PacksUQ";
			public const string UnitOfQuantity = "UnitOfQuantity";
			public const string QtyOrdered = "QtyOrdered";
			public const string UnitsQName = "UnitsQName";

			public const string W1_ExpiryDate = "W1_ExpiryDate";
			public const string W1_PackingDate = "W1_PackingDate";
			public const string W1_PartAttrib1 = "W1_PartAttrib1";
			public const string W1_PartAttrib2 = "W1_PartAttrib2";
			public const string W1_PartAttrib3 = "W1_PartAttrib3";
			public const string W1_SerialNumber = "W1_SerialNumber";
			public const string W1_Units = "W1_Units";
			public const string W1_UnitsUQ = "W1_UnitsUQ";
		}

		public TrackingWhsReleaseLine(WhsReleaseLine releaseLine, PartAttributeManager attributeManager, ZInt decimalPlaces)
		{
			if (releaseLine != null)
			{
				W1_PartAttrib1 = releaseLine.PartAttribute1;
				W1_PartAttrib2 = releaseLine.PartAttribute2;
				W1_PartAttrib3 = releaseLine.PartAttribute3;
				W1_SerialNumber = releaseLine.SerialNumber;
				W1_ExpiryDate = attributeManager != null && attributeManager.IsExpiryDateUsedByOrganisation ? releaseLine.ExpiryDate.ToShortDateString() : "";
				W1_PackingDate = attributeManager != null && attributeManager.IsPackingDateUsedByOrganisation ? releaseLine.PackingDate.ToShortDateString() : "";
				W1_UnitsUQ = releaseLine.UnitsUQ;
				W1_Units = releaseLine.Quantity.ToString(decimalPlaces);
			}
		}

		internal TrackingWhsReleaseLine() : base() { }

		public ZString W1_PartAttrib1 { get; set; }
		public ZString W1_PartAttrib2 { get; set; }
		public ZString W1_PartAttrib3 { get; set; }
		public ZString W1_SerialNumber { get; set; }
		public ZString W1_ExpiryDate { get; set; }
		public ZString W1_PackingDate { get; set; }
		public ZString W1_Units { get; set; }
		public ZString W1_UnitsUQ { get; set; }

		public ZString ProductCode { get; set; }
		public ZString ProductDescription { get; set; }
		public ZDecimal Packs { get; set; }
		public ZString PacksUQ { get; set; }
		public ZString UnitOfQuantity { get; set; }
		public ZString QtyOrdered { get; set; }
		public ZString UnitsQName { get; set; }
	}
}
