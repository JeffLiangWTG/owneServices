using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Services.ServiceHost
{
	[CodeAlive("Used through TypeDecider")]
	public class IncidentRequestService : IIncidentRequestService
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Need a default list in generic builds")]
		public IEnumerable<Tuple<string, string>> GetModuleList(string product, string criticality, string language, string status, Guid contactPk)
		{
			return new Tuple<string, string>[] { Tuple.Create("UDF", "Undefined") };
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Need a default list in generic builds")]
		public IEnumerable<Tuple<string, string>> GetProductList(string language, Guid contactPk)
		{
			return new Tuple<string, string>[] { Tuple.Create("UDF", "Undefined") };
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Need a default list in generic builds")]
		public IEnumerable<Tuple<string, string>> GetServiceTypeList(string product, string criticality, string module, string sourceModuleId)
		{
			return new Tuple<string, string>[] { Tuple.Create("UDF", "Undefined") };
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Need a default list in generic builds")]
		public IEnumerable<Tuple<string, string, string, string>> GetFullModuleListByProduct()
		{
			return new Tuple<string, string, string, string>[] { Tuple.Create("UDF", string.Empty, "Undefined", "Undefined") };
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Need a default list in generic builds")]
		public IEnumerable<Tuple<string, string>> GetFullProductList()
		{
			return new Tuple<string, string>[] { Tuple.Create("UDF", "Undefined") };
		}

		public void NotificationWhenSubscribe(Guid jobPK, string userType, ICollection<Guid> userPKs, string email)
		{
		}

		public void NotificationWhenUnsubscribe(Guid jobPK, string userType, ICollection<Guid> userPKs, string email)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Need a default list in generic builds")]
		public Dictionary<string, string> GetDocumentUrls(Guid contactPk, ICollection<string> documentIds)
		{
			var result = new Dictionary<string, string>();
			result.Add("UDF", "Undefined");
			return result; // Need a default list in generic builds
		}

		#region Type Decider

		public abstract class MyTypeDecider : TypeDecider
		{
			public static Type GetTypeForCreate()
			{
				return TypeDecider.GetClientTypeDeciderFromType(typeof(IIncidentRequestService))?.GetTypeForNew() ?? typeof(IncidentRequestService);
			}
		}

		#endregion
	}
}
