using CargoWise.Customs.PL.MessageContracts.Interfaces;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.PL.Business;

public sealed class NUPMessageInterpreter<TLinkedObject>(TLinkedObject linkedObject) : UPOInterpreterBase<TLinkedObject>(linkedObject)
	where TLinkedObject : EnterpriseBusinessObject
{
	public override string GetMessageTitle(IUpo dataProvider) => Constants.InterpretationStrings.MessageTitles.NUP;
}
