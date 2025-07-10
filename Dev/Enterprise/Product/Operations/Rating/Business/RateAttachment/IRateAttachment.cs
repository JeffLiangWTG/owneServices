using CargoWise.Types;

namespace Enterprise.Rating.Business
{
	public interface IRateAttachment
	{
		ZInt Sequence { get; }
		ZString TemplateType { get; }
	}
}
