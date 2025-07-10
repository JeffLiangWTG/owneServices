using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.TR.Messaging
{
	public interface IBillBL
	{
		ZString BillNo { get; }
		ZString LineNo { get; }
		ZString ShipperName { get; }
		ZString ConsigneeNameAndTitle { get; }
		ZString ConsigneeTaxNo { get; }
		ZBool IsContainer { get; }
		ZString SequenceNo { get; }
		ZString PackType { get; }
		ZInt PackQuantity { get; }
		ZString MarksAndNumbers { get; }
		ZString Unit { get; }
		ZDecimal GrossWeight { get; }
		ZDecimal NetWeight { get; }
		IEnumerable<IPackBL> Packs { get; }
	}
}
