using CargoWise.Types;

namespace Enterprise.Freight.Forwarding.Business.AWB
{
	public interface ISendCIMP
	{
		ZBool SendFWB { get; set; }
		ZBool SendFHL { get; set; }
		ZBool IncludeSecurityDeclaration { get; set; }
		ZString DateLastSent { get; set; }
	}
}
