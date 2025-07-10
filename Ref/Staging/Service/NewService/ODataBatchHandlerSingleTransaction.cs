using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Staging.Schema_New;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Batch;
using Microsoft.AspNetCore.OData.Extensions;

namespace CargoWise.RefDbRepo.Staging.NewService
{
	public class ODataBatchHandlerSingleTransaction : DefaultODataBatchHandler
	{
		public ODataBatchHandlerSingleTransaction(Func<IStagingRepository> getRepository) : base()
		{
			Argument.NotNull(getRepository, nameof(getRepository));
			this.getRepository = getRepository;
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
			ChangeSetResponseItem changeSetResponse = null;
			using (var repository = getRepository())
			{
				var requests = changeSet.Contexts;

				foreach (var request in requests)
				{
					request.SetContext(repository);
				}

				using (var transaction = repository.BeginTransaction())
				{
					changeSetResponse = (ChangeSetResponseItem)await changeSet.SendRequestAsync(handler);
					responses.Add(changeSetResponse);

					if (changeSetResponse.Contexts.All(r => r.Response.IsSuccessStatusCode()))
					{
						repository.Commit(transaction);
					}
				}
			}
		}

		readonly Func<IStagingRepository> getRepository;
	}
}
