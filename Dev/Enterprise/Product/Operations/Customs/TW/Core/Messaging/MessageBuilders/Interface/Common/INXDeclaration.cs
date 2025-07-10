using CargoWise.Types;

namespace Enterprise.Customs.TW.Messaging
{
	public interface INXDeclaration
	{
		ZString FunctionalReferenceID { get; }

		ZString FunctionCode { get; }

		IApplication Application { get; }
	}
}
