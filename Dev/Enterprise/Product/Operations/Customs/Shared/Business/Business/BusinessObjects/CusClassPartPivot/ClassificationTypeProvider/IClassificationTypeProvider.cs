using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public interface IClassificationTypeProvider
	{
		ZString HTICode { get; }
		ZString HTECode { get; }
		ZString SHBCode { get; }
		ZString HTBCode { get; }
	}
}
