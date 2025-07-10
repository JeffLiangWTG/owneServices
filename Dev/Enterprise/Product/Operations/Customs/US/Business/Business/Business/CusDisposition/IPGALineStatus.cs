using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.US.Business
{
	public interface IPGALineStatus
	{
		ZString PGALineStatusAgencyCode { get; }
		ZInt PGALineNumber { get; }
		CusDispositionCollection PGALineCusDispositions { get; }
	}
}
