using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business
{
	public class CusAuthorisationHeaderLookups : CusPermitHeaderLookups
	{
		public CusAuthorisationHeaderLookups(CusAuthorisationHeader parent) : base(parent)
		{
		}

		public new CusAuthorisationHeader Parent => (CusAuthorisationHeader)base.Parent;

		public CodeDescriptionPairList AuthorisationTypeList => Parent.Provider.GetAuthorisationTypeList(Factory);

		public OrgHeaderCollection AppliesToList => new OrgHeaderCollection(Factory);
	}
}
