using System;
using System.Collections.Generic;

namespace Enterprise.Services.ServiceHost
{
	public interface IIncidentRequestService
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		IEnumerable<Tuple<string, string>> GetProductList(string language, Guid contactPk);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		IEnumerable<Tuple<string, string>> GetModuleList(string product, string criticality, string language, string status, Guid contactPk);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		IEnumerable<Tuple<string, string>> GetServiceTypeList(string product, string criticality, string module, string sourceModuleId);

		IEnumerable<Tuple<string, string>> GetFullProductList();

		IEnumerable<Tuple<string, string, string, string>> GetFullModuleListByProduct();

		void NotificationWhenUnsubscribe(Guid jobPK, string userType, ICollection<Guid> userPKs, string email);

		Dictionary<string, string> GetDocumentUrls(Guid contactPk, ICollection<string> documentIds);
	}
}
