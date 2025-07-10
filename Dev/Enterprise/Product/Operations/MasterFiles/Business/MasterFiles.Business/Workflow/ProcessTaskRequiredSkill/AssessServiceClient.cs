using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Enterprise.ZArchitecture.Environment;
using WTG.DevTools.Definitions;
using WTG.DevTools.ServiceClient.Assess;
using WTG.DevTools.ServiceClient.Common;

namespace Enterprise.MasterFiles.Business
{
	class AssessServiceClient : IAssessServiceClient
	{
		public AssessServiceClient()
			: this(new HttpClientHandler())
		{
		}

		public AssessServiceClient(HttpMessageHandler messageHandler)
		{
			innerServiceClient = new WTG.DevTools.ServiceClient.Assess.AssessServiceClient(messageHandler, GetBaseAddress());
		}

		readonly IAssessServiceClient innerServiceClient;

		static string GetBaseAddress()
		{
			return Globals.IsTest ? "http://nothing/" : RawDataRegistry.Instance.AssessApiBaseAddress.Value;
		}

		#region IAssessServiceClient Members

		public async Task<ServiceResponse<IReadOnlyCollection<Aspect>>> GetActiveAssessAspectsAsync(ServiceRequestOptions options = null)
		{
			return await innerServiceClient.GetActiveAssessAspectsAsync(options).ConfigureAwait(false);
		}

		public async Task<ServiceResponse<string>> GetLearningUnitNameAsync(Guid aspectPK, ServiceRequestOptions options = null)
		{
			return await innerServiceClient.GetLearningUnitNameAsync(aspectPK, options).ConfigureAwait(false);
		}

		public async Task<ServiceResponse<string>> GetLearningUnitUrlAsync(Guid aspectPK, ServiceRequestOptions options = null)
		{
			return await innerServiceClient.GetLearningUnitUrlAsync(aspectPK, options).ConfigureAwait(false);
		}

		public async Task<ServiceResponse<bool>> HasPassedLearningUnitAsync(Guid aspectPK, Guid personPK, ServiceRequestOptions options = null)
		{
			return await innerServiceClient.HasPassedLearningUnitAsync(aspectPK, personPK, options).ConfigureAwait(false);
		}

		public async Task<ServiceResponse<bool>> HasPassedLearningUnitAsync(Guid aspectPK, string loginName, ServiceRequestOptions options = null)
		{
			return await innerServiceClient.HasPassedLearningUnitAsync(aspectPK, loginName, options).ConfigureAwait(false);
		}

		public async Task<ServiceResponse> EnsureEnrolledAsync(Guid aspectPK, Guid personPK, EnrolmentParameters enrolmentParameters, ServiceRequestOptions options = null)
		{
			return await innerServiceClient.EnsureEnrolledAsync(aspectPK, personPK, enrolmentParameters, options).ConfigureAwait(false);
		}

		#endregion
	}

	public static class IAssessServiceClientExtensionMethods
	{
		public static string GetLearningUnitNameOrPlaceholder(this IAssessServiceClient apiClient, Guid aspectPK, string placeholderText = "Unknown course")
		{
			if (apiClient == null)
			{
				throw new ArgumentNullException(nameof(apiClient));
			}
			var learningUnitNameResult = apiClient.GetLearningUnitNameAsync(aspectPK).GetAwaiter().GetResult();
			if (!learningUnitNameResult.IsSuccess || string.IsNullOrEmpty(learningUnitNameResult.Content))
			{
				return placeholderText;
			}
			return learningUnitNameResult.Content;
		}
	}
}
