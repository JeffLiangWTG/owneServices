using System;
using System.Data;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Services.ServiceHost
{
	public sealed class InstanceIdHandler : DelegatingHandler
	{
		public InstanceIdHandler()
		{
			instanceId = new Lazy<string>(GetInstanceID, LazyThreadSafetyMode.PublicationOnly);
		}

		readonly Lazy<string> instanceId;

		protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
			if (response.StatusCode == HttpStatusCode.Unauthorized)
			{
				try
				{
					response.Headers.Add("wtg-instid", instanceId.Value); // string constant
				}
				catch (SqlException ex)
				{
					ErrorReporter.Instance.Report(null, (NoResString)"Unable to retrieve database Instance Id", ex); // string constant
				}
			}
			return response;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		static string GetInstanceID()
		{
			using (Db.DisposableActionForDbConnection())
			using (var command = Db.Connection.Command("GetInstanceId"))
			{
				command.CommandType = CommandType.StoredProcedure;

				command.AddOutputParameter("@InstanceId", SqlDbType.VarChar, 10, 0, 0, null);
				command.ExecuteNonQuery();

				return (string)command.GetParameterValue("@InstanceId");
			}
		}
	}
}
