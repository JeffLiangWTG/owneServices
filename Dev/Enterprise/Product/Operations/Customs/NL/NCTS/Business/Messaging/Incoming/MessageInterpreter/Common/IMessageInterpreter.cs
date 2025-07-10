using CargoWise.Customs.NL.MessageContracts.Interfaces;

namespace Enterprise.Customs.NL.NCTS.Business;

public interface IMessageInterpreter<TDataProvider>
	where TDataProvider : INCTSIncomingDataProvider
{
	string Interpret(TDataProvider dataProvider);
}
