using Enterprise.Customs.PL.Business.Declaration;

namespace Enterprise.Customs.PL.Business;

sealed class UPOMessageInterpreter(CusEntryHeader entryHeader) : UPOInterpreterBase<CusEntryHeader>(entryHeader);
