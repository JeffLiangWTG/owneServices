
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.MessageBuilders
{
	public interface IDrawbackTrailerScheduleBNumber
	{
		ZString FirstScheduleBNumber { get; }
		ZString AdditionalScheduleBNumber { get; }
		ZString AdditionalScheduleBNumber1 { get; }
		ZString AdditionalScheduleBNumber2 { get; }
		ZString AdditionalScheduleBNumber3 { get; }
	}
}
