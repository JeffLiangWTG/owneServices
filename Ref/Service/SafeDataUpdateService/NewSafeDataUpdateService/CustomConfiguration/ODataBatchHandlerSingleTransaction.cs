using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Batch;
using Microsoft.AspNetCore.OData.Extensions;

namespace CargoWise.RefDbRepo.NewSafeDataUpdateService
{
	public class ODataBatchHandlerSingleTransaction : DefaultODataBatchHandler
	{
		public ODataBatchHandlerSingleTransaction(Func<IReferenceDataRepository> getRepository) : base()
		{
			Argument.NotNull(getRepository, nameof(getRepository));

			this.getRepository = getRepository;
			MessageQuotas.MaxOperationsPerChangeset = 65000;
			MessageQuotas.MaxPartsPerBatch = 65000;
		}

		string userId;

		public override Task ProcessBatchAsync(HttpContext context, RequestDelegate nextHandler)
		{
			userId = context.User.Claims.FirstOrDefault(x => x.Type == AuthClaimType.UniqueName)?.Value;
			return base.ProcessBatchAsync(context, nextHandler);
		}

		public override async Task<IList<ODataBatchResponseItem>> ExecuteRequestMessagesAsync(IEnumerable<ODataBatchRequestItem> requests, RequestDelegate handler)
		{
			Argument.NotNull(requests, nameof(requests));

			var responses = new List<ODataBatchResponseItem>();
			foreach (var request in requests)
			{
				var operation = request as OperationRequestItem;
				if (operation != null)
				{
					responses.Add(await request.SendRequestAsync(handler));
				}
				else
				{
					await ExecuteChangeSet((ChangeSetRequestItem)request, responses, handler);
				}
			}

			return responses;
		}

		async Task ExecuteChangeSet(ChangeSetRequestItem changeSet, IList<ODataBatchResponseItem> responses, RequestDelegate handler)
		{
			using (var repository = getRepository())
			{
				var requests = changeSet.Contexts;
				foreach (var request in requests)
				{
					request.SetContext(repository, true);
				}

				ChangeSetResponseItem changeSetResponse = null;
				changeSetResponse = (ChangeSetResponseItem)await changeSet.SendRequestAsync(handler);
				responses.Add(changeSetResponse);

				var success = changeSetResponse.Contexts.All(c => c.Response.IsSuccessStatusCode());
				if (success)
				{
					await repository.SaveChangesAsync(userId, true);
				}
			}
		}

		readonly Func<IReferenceDataRepository> getRepository;
	}
}
