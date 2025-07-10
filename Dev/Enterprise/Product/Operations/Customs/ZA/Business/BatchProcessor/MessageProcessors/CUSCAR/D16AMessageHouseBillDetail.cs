using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Edifact.D16A.Elements;
using Enterprise.Edifact.D16A.Messages.CUSCAR;
using Enterprise.Edifact.D16A.Segments;

namespace Enterprise.Customs.ZA.Business.MessageProcessor
{
	public class D16AMessageHouseBillDetail
	{
		public D16AMessageHouseBillDetail(SegmentGroup8 sg8Bill)
		{
			this.grp8 = sg8Bill;
		}

		readonly SegmentGroup8 grp8;

		public ZString BillNumber
		{
			get
			{
				if (billNumber == null)
				{
					var rffSegment = grp8.RFF.Cast<RFFSegment>()
										 .FirstOrDefault(rff => rff.Reference.ReferenceCodeQualifier == ReferenceCodeQualifierList.BillOfLadingNumber);

					if (rffSegment != null)
					{
						billNumber = rffSegment.Reference.ReferenceIdentifier;
					}
				}

				return (ZString)billNumber;
			}
		}
		string billNumber;

		public ZString TerminalOfDischarge
		{
			get
			{
				return (ZString)(terminalOfDischarge ?? (terminalOfDischarge = GetLocationByQualifier(LocationFunctionCodeQualifierList.FinalPortOrPlaceOfDischarge)));
			}
		}
		string terminalOfDischarge;

		public ZString PlaceOfDeconsolidation
		{
			get
			{
				return (ZString)(placeOfDeconsolidation ?? (placeOfDeconsolidation = GetLocationByQualifier(LocationFunctionCodeQualifierList.PlaceOfDeconsolidation)));
			}
		}
		string placeOfDeconsolidation;

		ZString GetLocationByQualifier(LocationFunctionCodeQualifierList qualifier)
		{
			ZString location = ZString.Empty;

			var locSegment = grp8.LOC.Cast<LOCSegment>()
								 .FirstOrDefault(loc => loc.LocationFunctionCodeQualifier == qualifier);
			if (locSegment != null)
			{
				location = locSegment.LocationIdentification.LocationIdentifier;
			}

			return location;
		}

		public ZInt TotalNumberOfPackages => GetPackLines().Sum(l => ZInt.ParseSafe(l.NumberOfPackages, 0));

		// Assume all packages are the same type.
		public ZString TypeOfPackages => GetPackLines()[0]?.TypeOfPackages ?? ZString.Empty;

		// TBA
		public ZString BillGoodsDescription => ZString.Empty;

		public D16AMessagePackLineDetail[] GetPackLines()
		{
			if (items == null)
			{
				items = grp8.Group14.Cast<SegmentGroup14>()
							.Select(grp14 => new D16AMessagePackLineDetail(grp14))
							.ToArray();
			}
			return items;
		}
		D16AMessagePackLineDetail[] items;

		public SegmentGroup14MessageSection Group14 => grp8.Group14;
	}
}


