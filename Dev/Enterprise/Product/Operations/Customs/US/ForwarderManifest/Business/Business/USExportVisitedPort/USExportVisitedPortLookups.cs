using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.US.ForwarderManifest.Business
{
	public class USExportVisitedPortLookups : CusCodeDataLookups
	{
		public USExportVisitedPortLookups(CusCodeData parent) : base(parent)
		{
		}

		public RefUNLOCOCollection UNLOCOCodeList => new RefUNLOCOCollection(Factory);

		public ZZRefCusCodeListCombinedCollection ScheduleKCodeList
		{
			get
			{
				ZZRefCusCodeListCombinedCollection result = null;
				var parent = (USExportVisitedPort)Parent;
				if (parent.PortCodeFieldType == nameof(FieldType.TextDropEdit))
				{
					result = parent.PortDefaulterRefLocoMappings as ZZRefCusCodeListCombinedCollection;
				}
				return result ?? ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, ZDateTime.Today);
			}
		}
	}
}
