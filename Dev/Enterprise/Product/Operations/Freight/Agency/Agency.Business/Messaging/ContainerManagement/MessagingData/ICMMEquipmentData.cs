using System;
using System.Collections.Generic;

namespace Enterprise.Freight.Agency.Business
{
	public interface ICMMEquipmentData
	{
		string ContainerNumber { get; }
		string ISOType { get; }
		string BookingReference { get; }
		string BillOfLading { get; }
		string GoodsDeclarationNumber { get; }
		bool IsEmpty { get; }
		CMMEquipmentSupplier EquipmentSupplier { get; }

		DateTime PositioningDateTime { get; }
		decimal? GrossWeightKG { get; }

		IEnumerable<string> SealNumbers { get; }
	}
}
