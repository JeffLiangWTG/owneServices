using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.Universal.Messaging.CUSCAR;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.ZA.Manifest.Business.EDIFACT
{
	public class CusCarEntryNum : ICusCarLine
	{
		public CusCarEntryNum(ABLEntryNum entryNum)
			: this(entryNum, new CusCarBill((AsycudaBill)entryNum.Bill))
		{
		}

		public CusCarEntryNum(ABLEntryNum entryNum, CusCarBill bill)
		{
			EntryNum = entryNum;
			Bill = bill;
		}

		public ABLEntryNum EntryNum;
		public CusCarBill Bill;

		#region ICusCarLine - for messaging

		ZString ICusCarLine.BillNumber => Bill.BillNumber;

		ZString ICusCarLine.BillNumberWithHyphen => Bill.BillNumberWithHyphen;

		ZString ICusCarLine.Origin => Bill.Origin;

		ZString ICusCarLine.FinalDestination => Bill.FinalDestination;

		IEnumerable<ICusCarParty> ICusCarLine.Parties => Bill.Parties;

		ZString ICusCarLine.UCRNumber => Bill.UCRNumber;

		ZString ICusCarLine.LocationOfGoods => Bill.LocationOfGoods;

		ZString ICusCarLine.PlaceOfDispatch => Bill.PlaceOfDispatch;

		IEnumerable<ICusCarPackage> ICusCarLine.Packages
		{
			get
			{
				var packages = EntryNum.PackPivots
					.OfType<ABLEntryNumRelatedPacksGenPivot>()
					.Where(x => x.Relation2Object != null)
					.Select(x => new CusCarPack((AsycudaPack)x.Relation2Object))
					.ToArray();
				return packages.Any() ? packages : Bill.Packages;
			}
		}

		ZString ICusCarLine.CargoReleaseStatus => Bill.CargoReleaseStatus;

		ZString ICusCarLine.CargoReleaseStatusDescription => Bill.CargoReleaseStatusDescription;

		IEnumerable<ICustomsNumber> ICusCarLine.CustomsNumbers
		{
			get
			{
				yield return EntryNum;
			}
		}

		ZString ICusCarLine.ExternalReference => Bill.ExternalReference;

		ZWeight ICusCarLine.BillWeight => Bill.BillWeight;

		public ZString BillType => Bill.BillType;

		public ZDateTime BillIssueDate => Bill.BillIssueDate;

		public ZString BillStatus => Bill.BillStatus;

		public ZString BillStatusDescription => Bill.BillStatusDescription;

		public ZString DepotOfUnpack => Bill.DepotOfUnpack;

		public ZString TerminalOfDischarge => Bill.TerminalOfDischarge;

		ZString ICusCarLine.ShipmentType => Bill.ShipmentType;

		#endregion
	}
}
