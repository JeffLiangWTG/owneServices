using System;
using CargoWise.Common;
using WTG.ProductionRules.Business.Common;

namespace Enterprise.Warehouse.Transactions.Facts
{
	public abstract class LocationFact : IWarehouseLocationFact
	{
		public LocationFact(
			Guid pk,
			string locationTypeCode,
			string locationClass,
			string areaName,
			string rowName,
			int column,
			int level,
			int tray,
			string locationStatus,
			bool isFixedPickFace,
			bool isDynamicPickFace)
		{
			PK = pk;
			LocationTypeCode = Argument.NotNull(locationTypeCode, nameof(locationTypeCode));
			LocationClass = Argument.NotNull(locationClass, nameof(locationClass));
			AreaName = Argument.NotNull(areaName, nameof(areaName));
			RowName = Argument.NotNull(rowName, nameof(rowName));
			Column = column;
			Level = level;
			Tray = tray;
			LocationStatus = Argument.NotNull(locationStatus, nameof(locationStatus));
			IsFixedPickFace = isFixedPickFace;
			IsDynamicPickFace = isDynamicPickFace;
		}

		public Guid PK { get; }

		public string LocationTypeCode { get; }

		public string LocationClass { get; }

		public bool IsDynamicPickFace { get; }

		public bool IsFixedPickFace { get; }

		public string AreaName { get; }

		public string RowName { get; }

		public string LocationStatus { get; }

		public int Column { get; }

		public int Level { get; }

		public int Tray { get; }
	}
}
