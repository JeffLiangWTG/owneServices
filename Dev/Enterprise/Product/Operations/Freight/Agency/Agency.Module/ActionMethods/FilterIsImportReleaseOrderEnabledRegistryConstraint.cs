using System;
using System.Linq;
using Enterprise.Freight.Agency.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Agency.Module
{
	public class FilterIsImportReleaseOrderEnabledRegistryConstraint : IFilterConstraint
	{
		public string Name => FilterConstants.IsImportReleaseOrderEnabled;

		public string SingularValueName => Res.GetString("a518b67c-8db2-40ec-9d3e-9b93ba5568ee", "value");

		public string PluralValueName => Res.GetString("eb0e06bc-dd6e-4f6c-a882-bce300e82709", "values");

		public string Description => Res.GetString("07055a63-aa7b-4d21-97b1-37852ff1e861", "Whether the current branch enables Import Release Order messaging for specific ports");

		public string GetDefaultStringValue()
		{
			var currentCompanyCountry = GlbCompany.CurrentCompany.Country.Code;

			var portConfigFound = AgencyRegistry.Instance.ImportReleaseOrderPorts.Value.OfType<PortMessagingPort>().Any(x => x.Port.StartsWith(currentCompanyCountry) && x.Enabled);
			if (!portConfigFound)
			{
				var systemLevelRetriever = new RegistryItemProposedValueAccessor(AgencyRegistry.Instance.ImportReleaseOrderPorts, new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty));
				var systemPortConfigs = (PortMessagingPortCollection)systemLevelRetriever.GetCurrentValue().Value;

				portConfigFound = systemPortConfigs.OfType<PortMessagingPort>().Any(x => x.Port.StartsWith(currentCompanyCountry) && x.Enabled);
			}

			return portConfigFound ? "Y" : "N";
		}

		public object GetValue() => GetDefaultStringValue();
	}
}
