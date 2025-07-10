using CargoWise.Types;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.PL.Business;

public abstract class MessageInterpreterBase<TLinkedObject, TDataProvider>(TLinkedObject linkedObject) : IMessageInterpreter<TDataProvider>
	where TLinkedObject : EnterpriseBusinessObject
	where TDataProvider : class
{
	protected TLinkedObject LinkedObject { get; } = linkedObject;

	public ZString Interpret(TDataProvider dataProvider)
		=> InterpretCore(dataProvider);

	protected abstract ZString InterpretCore(TDataProvider dataProvider);
}
