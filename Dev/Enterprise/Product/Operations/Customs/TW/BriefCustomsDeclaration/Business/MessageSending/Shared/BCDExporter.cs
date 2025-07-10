using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Business
{
	public class BCDExporter : BCDPartyShared
	{
		public BCDExporter(AsycudaBill masterBill)
		{
			MasterBill = Argument.NotNull(masterBill, nameof(masterBill));
		}

		protected AsycudaBill MasterBill { get; }

		protected override ZString GetIDCore() => MasterBill.ABL_ShipperRegNo;

		protected override ZString GetNameCore() => MasterBill.ABL_ShipperName;

		protected override ZString GetChineseNameCore() => MasterBill.ABL_ShipperLocalName;

		protected override ZString RegNoType => MasterBill.ABL_ShipperRegNoType;

		protected override ZString GetCustomsControlIDCore() => MasterBill.ShipperBondedID;

		protected override ZString GetLineCore() => GetLine(MasterBill.ABL_ShipperPostcode, MasterBill.ShipperCountry, MasterBill.ABL_ShipperState, MasterBill.ABL_ShipperCity, MasterBill.ABL_ShipperStreet1, MasterBill.ABL_ShipperStreet2);

		protected override ZString GetChineseLineCore() => GetChineseLine(MasterBill.ABL_ShipperPostcode, MasterBill.ShipperCountry, MasterBill.ABL_ShipperLocalState, MasterBill.ABL_ShipperLocalCity, MasterBill.ABL_ShipperLocalStreet1, MasterBill.ABL_ShipperLocalStreet2);

		protected override ZString GetCountryCodeCore() => ZString.Empty;
	}
}
