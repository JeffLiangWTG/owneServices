using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.OpenApi.Models;

namespace eServices.Dms.Core.ServiceDefaults.OpenApi
{
	public static class BinaryMedia
	{
		public static KeyValuePair<string, OpenApiMediaType> OctetStream { get; } = new(
			"application/octet-stream",
			new OpenApiMediaType()
			{
				Schema = new OpenApiSchema()
				{
					Type = "string",
					Format = "binary",
				},
			});

		public static TBuilder WithOpenApiProducesBinaryContent<TBuilder>(this TBuilder builder, IResult result) where TBuilder : IEndpointConventionBuilder
			=> builder.WithOpenApi(operation =>
			{
				var status = result is IStatusCodeHttpResult statusCodeResult ? statusCodeResult.StatusCode!.Value : StatusCodes.Status200OK;
				operation.Responses.Add(
					status.ToString(),
					new OpenApiResponse
					{
						Description = ReasonPhrases.GetReasonPhrase(status),
						Content = { OctetStream }
					});
				return operation;
			});

		public static TBuilder WithOpenApiConsumesBinaryContent<TBuilder>(this TBuilder builder) where TBuilder : IEndpointConventionBuilder
			=> builder.WithOpenApi(operation =>
			{
				operation.RequestBody = new OpenApiRequestBody()
				{
					Required = true,
					Content = { OctetStream }
				};
				return operation;
			});
	}
}
