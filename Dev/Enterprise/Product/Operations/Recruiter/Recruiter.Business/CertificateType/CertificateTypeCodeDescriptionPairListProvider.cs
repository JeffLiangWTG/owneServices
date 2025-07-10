using Enterprise.Recruiter.Integration;
using Enterprise.ZArchitecture.Core;
using ICodeDescriptionPairListProvider = Enterprise.DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider;

namespace Enterprise.Recruiter.Business
{
	public class CertificateTypeCodeDescriptionPairListProvider : ICodeDescriptionPairListProvider, ICertificateTypeCodeDescriptionPairListProvider
	{
		public ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList()
		{
			return RecruiterDataRegistry.Instance.CertificateTypesExtra.Value.GetActiveCodeDescriptionPairList();
		}
	}
}
