using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NL.Business;

public class AlternativeEvidenceLookups(AlternativeEvidence parent) : JobDeclarationAlternativeEvidenceLookups(parent)
{
	public CodeDescriptionPairList DocTypeList => Factory.GetCachedValue<AlternativeEvidenceDocTypeList>();
}
