using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.Universal.Messaging.CUSCAR
{
	public interface ICusCarLine
	{
		ZString BillNumber { get; }
		ZString BillNumberWithHyphen { get; }
		ZString BillType { get; }
		ZString BillStatus { get; }
		ZString BillStatusDescription { get; }
		ZDateTime BillIssueDate { get; }
		ZWeight BillWeight { get; }
		ZString Origin { get; }
		ZString FinalDestination { get; }
		ZString ExternalReference { get; }
		IEnumerable<ICustomsNumber> CustomsNumbers { get; }
		ZString UCRNumber { get; }
		ZString LocationOfGoods { get; }
		ZString PlaceOfDispatch { get; }
		ZString DepotOfUnpack { get; }
		ZString TerminalOfDischarge { get; }
		IEnumerable<ICusCarParty> Parties { get; }
		IEnumerable<ICusCarPackage> Packages { get; }
		ZString CargoReleaseStatus { get; }
		ZString CargoReleaseStatusDescription { get; }
		ZString ShipmentType { get; }
	}
}
