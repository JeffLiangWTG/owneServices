using CargoWise.Types;

namespace Enterprise.Freight.Forwarding.Business.AWB
{
	public interface IPrintMAWB
	{
		ZBool PrintMasterAirWaybill { get; set; }
		ZBool NeutralAWB { get; set; }
		ZBool CarrierAWB { get; set; }
		ZBool LaserAWB { get; set; }
		ZGuid MAWBPrinter { get; set; }
		ZBool MAWBUseEPrint { get; set; }
		ZString DatePrinted { get; set; }
		ZBool PrintConsignmentSecurityDeclaration { get; set; }
	}
}
