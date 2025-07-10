using System;
using Enterprise.Warehouse.Web.WebService.Common;

namespace Enterprise.Warehouse.Web.WebService.Business
{
#if DEBUG
	[Serializable]
#endif
	public class WhsEquipmentParameters : DataObjectInfo
	{
		#region Constructors

		public WhsEquipmentParameters()
		{
			PickMethods = new WhsPickMethodInfoCollection();
			PickAreas = new WhsAreaInfoCollection();
			PickGroups = new WhsPickGroupInfoCollection();
			Printers = System.Array.Empty<PrinterInfo>();
		}

		public WhsEquipmentParameters(WhsPickMethodInfoCollection pickMethods, WhsAreaInfoCollection pickAreas, WhsPickGroupInfoCollection pickGroups, PrinterInfo[] printers)
		{
			PickMethods = pickMethods;
			PickAreas = pickAreas;
			PickGroups = pickGroups;
			Printers = printers;
		}

		#endregion

		#region Properties

		public WhsPickMethodInfoCollection PickMethods { get; set; }
		public WhsAreaInfoCollection PickAreas { get; set; }
		public WhsPickGroupInfoCollection PickGroups { get; set; }
		public PrinterInfo[] Printers { get; set; }

		#endregion
	}
}
