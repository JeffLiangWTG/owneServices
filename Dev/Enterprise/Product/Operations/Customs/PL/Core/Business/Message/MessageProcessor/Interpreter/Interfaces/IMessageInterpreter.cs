using CargoWise.Types;

namespace Enterprise.Customs.PL.Business;

public interface IMessageInterpreter<in TDataProvider>
	where TDataProvider : class
{
	public ZString Interpret(TDataProvider dataProvider);
}
