using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.PL.Business;

namespace Enterprise.Customs.PL.NCTS.Business;

sealed class UPOMessageInterpreter(NctsCommonMovementHeader attachedObject) : UPOInterpreterBase<NctsCommonMovementHeader>(attachedObject);
