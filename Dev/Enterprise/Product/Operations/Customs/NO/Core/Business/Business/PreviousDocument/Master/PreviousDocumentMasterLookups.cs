using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NO.Business;

public sealed class PreviousDocumentMasterLookups : ZLookups
{
	public PreviousDocumentMasterLookups(BusinessObject parent) : base(parent)
	{
	}

	public CodeDescriptionPairList ProcedureList => Factory.GetCachedValue<PreviousProcedureList>();
}
