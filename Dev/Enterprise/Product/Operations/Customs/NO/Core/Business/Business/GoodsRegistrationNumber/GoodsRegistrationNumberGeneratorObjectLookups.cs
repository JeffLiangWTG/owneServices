using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NO.Business;

public sealed class GoodsRegistrationNumberGeneratorObjectLookups(GoodsRegistrationNumberGeneratorObject parent) : ZLookups(parent)
{
	public CodeDescriptionPairList AuthorizationsList => GetAuthorizationsList();

	CodeDescriptionPairList GetAuthorizationsList()
	{
		var factory = parent.Factory;
		return factory.GetCachedValue("NO.GoodsNumberGeneratorObjectLookups.AuthorizationsList.CWP", () =>
		{
			var authorizationList = new CodeDescriptionPairList();

			foreach (var auth in new CusAuthorisationHeaderCollection(factory)
				.Where(a => a.CPH_Type == CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP))
			{
				authorizationList.AddPair(auth.CPH_Number, auth.CPH_PermitDescription);
			}

			return authorizationList;
		});
	}

	readonly GoodsRegistrationNumberGeneratorObject parent = Argument.NotNull(parent, nameof(parent));
}
