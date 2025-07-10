using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DeniedPartyScreening.Business
{
	public class DpsResultModel
	{
		public DpsResultModel(List<DpsResponseWithScreeningParty> responseWithScreeningParties, BusinessObjectFactory factory, bool forceAllLists = false)
		{
			Argument.NotNull(responseWithScreeningParties, nameof(responseWithScreeningParties));
			Argument.NotNull(factory, nameof(factory));

			Factory = factory;
			ScreenedPartyModels = responseWithScreeningParties.Select(u => new ScreenedPartyModel(u, Factory, forceAllLists)).ToList();
		}

		public List<ScreenedPartyModel> ScreenedPartyModels { get; }

		public BusinessObjectFactory Factory { get; }
	}
}
