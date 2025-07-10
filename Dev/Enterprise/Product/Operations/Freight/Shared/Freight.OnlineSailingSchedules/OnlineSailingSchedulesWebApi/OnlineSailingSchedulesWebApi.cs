using Enterprise.Freight.Integration;
using Enterprise.Freight.Integration.OnlineSailingSchedulesWebApi;

namespace Enterprise.Freight.OnlineSailingSchedules
{
	public class OnlineSailingSchedulesWebApi : IOnlineSailingSchedulesWebApi
	{
		IImportGSSResponse IOnlineSailingSchedulesWebApi.ImportGSSFromNaturalKey(IImportGSSPayload importGSSPayload) => ImportGSSFromNaturalKey(importGSSPayload, null);

		IImportManyGSSResponse IOnlineSailingSchedulesWebApi.ImportManyGSS(IImportManyGSSPayload importManyPayload) => ImportManyGSS(importManyPayload, null);

		public IImportGSSResponse ImportGSSFromNaturalKey(IImportGSSPayload importGSSPayload, OnlineSchedules onlineSchedules)
		{
			var import = new OnlineSailingSchedulesWebApiImport(importGSSPayload, onlineSchedules);
			return import.Import();
		}

		public IImportManyGSSResponse ImportManyGSS(IImportManyGSSPayload importManyGSSPayload, OnlineSchedules onlineSchedules)
		{
			var importMany = new OnlineSailingSchedulesWebApiImportMany(importManyGSSPayload, onlineSchedules);
			return importMany.Import();
		}
	}
}
