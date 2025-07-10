using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public interface IAcknowledgeAndSign
	{
		ZBool US_CertifyCargoRelease { get; }
		ZBool US_AcknowledgeAndSign { get; set; }
		ZDateTime US_DateOfDeclaration { get; set; }
		ZBool CertifyTIB { get; }
	}
}
