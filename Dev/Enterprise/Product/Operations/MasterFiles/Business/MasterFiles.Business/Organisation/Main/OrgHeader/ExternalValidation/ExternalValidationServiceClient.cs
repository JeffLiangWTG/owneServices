using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Native.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public interface IExternalValidationServiceClient
	{
		/// <summary>
		/// Validates the current organization.
		/// </summary>
		/// <returns>The validation result.</returns>
		Task<ExternalValidationResult> ValidateAsync(BusinessObjectFactory factory, CancellationToken cancellationToken);
	}

	/// <summary>
	/// Represents an external validation service client for a specific organizationXml.
	/// </summary>
	public class ExternalValidationServiceClient : IExternalValidationServiceClient
	{
		#region Constructors
		public ExternalValidationServiceClient(ZGuid organizationPK)
		{
			OrganizationPK = organizationPK;
			eventFactory = new BusinessObjectFactory();
			eventFactory.RelinquishThreadOwnership();
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets the primary key of the organization to be validated.
		/// </summary>
		public ZGuid OrganizationPK { get; }

		/// <summary>
		/// Gets/Sets the service timeout.
		/// </summary>
		public int ServiceTimeout
		{
			get
			{
				return serviceTimeout >= 0 ? serviceTimeout : (serviceTimeout = DataRegistry.Instance.ExternalValidationServiceTimeout * 1000);
			}
			set
			{
				serviceTimeout = value;
			}
		}

		/// <summary>
		/// Gets/Sets the service URL.
		/// </summary>
		public Uri ServiceUri
		{
			get
			{
				return serviceUri ?? (serviceUri = new Uri(DataRegistry.Instance.ExternalValidationServiceUrl));
			}
			set
			{
				serviceUri = value;
			}
		}

		#endregion

		#region Public/Internal Instance Methods

		/// <summary>
		/// Validates the current organization.
		/// </summary>
		/// <returns>The validation result.</returns>
		public async Task<ExternalValidationResult> ValidateAsync(BusinessObjectFactory factory, CancellationToken cancellationToken)
		{
			try
			{
				var organizationXml = SerializeOrganizationToXml(factory);
				var resultXml = await GetWebServiceResult(ServiceUri, organizationXml, cancellationToken, ServiceTimeout);
				var result = DeserializeValidationResult(resultXml);

				if (result != null)
				{
					result.ErrorSource = result.HasErrors || result.HasWarnings
						? ErrorSource.External
						: ErrorSource.None;
				}

				return result;
			}
			catch (OperationCanceledException)
			{
				if (cancellationToken.IsCancellationRequested)
				{
					return new ExternalValidationResult
					{
						ErrorSource = ErrorSource.Cancelled,
						Warnings = Array.Empty<string>(),
						Errors = Array.Empty<string>(),
					};
				}
				else
				{
					AddExternalValidationEvent(AutoEvents.ExternalValidationNotCompleted, (NoResString)"Web service failure");

					return new ExternalValidationResult
					{
						ErrorSource = ErrorSource.Timeout,
						Warnings = Array.Empty<string>(),
						Errors = new[]
						{
							Res.GetString("F5544CAF-4EFA-4AC0-8181-DA40677D3345", "A timeout occurred during the validation with external web service.")
						}
					};
				}
			}
			catch (WebException ex)
			{
				AddExternalValidationEvent(AutoEvents.ExternalValidationNotCompleted, (NoResString)"Web service failure");

				return new ExternalValidationResult
				{
					ErrorSource = ErrorSource.Other,
					Warnings = Array.Empty<string>(),
					Errors = new[] { ex.Message }
				};
			}
			catch (UriFormatException)
			{
				return new ExternalValidationResult
				{
					ErrorSource = ErrorSource.Other,
					Warnings = Array.Empty<string>(),
					Errors = new[]
					{
						Res.GetString("89595FFC-7EAA-4AA1-8A5B-1036DA8F2615", "Invalid external validation web service URL. Please see Maintain -> System -> Registry -> Organizations -> External Validation Service -> External Validation Service URL, should you wish to edit the URL.")
					}
				};
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				return new ExternalValidationResult
				{
					ErrorSource = ErrorSource.Other,
					Warnings = Array.Empty<string>(),
					Errors = new[] { ex.Message }
				};
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "ContentType parameter")]
		public static async Task<string> GetWebServiceResult(Uri serviceUri, string organizationXml, CancellationToken cancellationToken, int serviceTimeout = -1)
		{
			using (var client = new HttpClient())
			{
				client.Timeout = Timeout.InfiniteTimeSpan;
				client.DefaultRequestHeaders.Connection.Add("Keep-Alive");
				var content = new StringContent(orgXMLParamName + "=" + organizationXml);
				content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

				using (var internalTimeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
				{
					internalTimeoutSource.CancelAfter(serviceTimeout);

					using (var response = await client.PostAsync(serviceUri.ToString(), content, internalTimeoutSource.Token))
					{
						response.EnsureSuccessStatusCode();

						return await response.Content.ReadAsStringAsync();
					}
				}
			}
		}

		readonly BusinessObjectFactory eventFactory;

#if DEBUG
		internal
#endif
		void AddExternalValidationEvent(Event @event, string reference)
		{
			eventFactory?.TakeThreadOwnership();
			try
			{
				eventFactory?.Load<OrgHeader>(OrganizationPK)?.Logs?.AddNew(@event, reference);
				eventFactory?.Save();
			}
			finally
			{
				eventFactory?.RelinquishThreadOwnership();
			}
		}

		#endregion

		#region Fields

		const string orgXMLParamName = "orgXML";
		static readonly IExportService exportService = ObjectFactory.Get<IExportService>("NativeXmlExportService");
		int serviceTimeout = -1;
		Uri serviceUri;

		#endregion

		#region Private/Protected Members

		string SerializeOrganizationToXml(BusinessObjectFactory factory)
		{
			using (var organizationXmlStream = new MemoryStream())
			{
				var organization = factory.Load<OrgHeader>(OrganizationPK);
				exportService.Export(new IBusiness[] { organization }, organizationXmlStream, factory);
				organizationXmlStream.Seek(0, SeekOrigin.Begin);
				using (var organizationXmlStreamReader = new StreamReader(organizationXmlStream))
				{
					return organizationXmlStreamReader.ReadToEnd();
				}
			}
		}

		ExternalValidationResult DeserializeValidationResult(string resultXml)
		{
			var xmlSerializer = new XmlSerializer(typeof(ExternalValidationResult));
			try
			{
				using (var serviceResultStream = new StringReader(resultXml))
				{
					return xmlSerializer.Deserialize(serviceResultStream) as ExternalValidationResult;
				}
			}
			catch (Exception)
			{
				throw new InvalidOperationException("The returned data from the external validation service is corrupted.");
			}
		}
		#endregion
	}
}
