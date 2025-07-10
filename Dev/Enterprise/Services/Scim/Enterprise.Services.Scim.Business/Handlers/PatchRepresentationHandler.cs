using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Enterprise.Services.Scim.Business.Handlers;
using Enterprise.Services.Scim.Contracts;
using Enterprise.Services.Scim.Models;
using SimpleIdServer.Scim;
using SimpleIdServer.Scim.Domain;
using SimpleIdServer.Scim.Domains;
using SimpleIdServer.Scim.DTOs;
using SimpleIdServer.Scim.Exceptions;
using SimpleIdServer.Scim.Persistence;
using SimpleIdServer.Scim.Persistence.InMemory;
using SimpleIdServer.Scim.Resources;

namespace Enterprise.Services.Scim.Business
{
	public class PatchRepresentationHandler
	{
		readonly ISCIMAttributeMappingQueryRepository attributeMappingQueryRepository;

		public PatchRepresentationHandler(IScimToScimRepresentation scimToScimRepresentation)
		{
			attributeMappingQueryRepository = new DefaultAttributeMappingQueryRepository(SCIMConstants.StandardAttributeMapping);
			Converter = scimToScimRepresentation;
		}

		public IScimToScimRepresentation Converter { get; }

		public async Task<PatchRepresentationResult> Handle(ScimBase scim, PatchRepresentationParameter patchRepresentation)
		{
			if (scim == null)
			{
				throw new SCIMNoTargetException("Scim object is null");
			}

			var schema = await Converter.QueryRepository.FindRootSCIMSchemaByResourceType(SCIMResourceTypes.User)
				?? throw new SCIMSchemaNotFoundException();

			CheckParameter(patchRepresentation);
			var existingRepresentation = await Converter.ScimToSCIMRepresentation(scim)
				?? throw new SCIMNotFoundException(string.Format(Global.ResourceNotFound, scim.Id));

			return await UpdateRepresentation(existingRepresentation, patchRepresentation);
		}

		async Task<PatchRepresentationResult> UpdateRepresentation(SCIMRepresentation existingRepresentation, PatchRepresentationParameter patchRepresentation)
		{
			var attributeMappings = await attributeMappingQueryRepository.GetBySourceResourceType(existingRepresentation.ResourceType);

			var legalOperations = GetLegalOperations(patchRepresentation.Operations);
			var patchResult = existingRepresentation.ApplyPatches(legalOperations, attributeMappings, true);

			if (!patchResult.Any())
			{
				return PatchRepresentationResult.NoPatch();
			}

			return PatchRepresentationResult.Ok(existingRepresentation);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "part of json request")]
		readonly string[] unsupportedOperations =
			[
				"addresses[type eq \"work\"]",
				"addresses[type eq \"home\"].formatted",
				"addresses.formatted",
				"emails[type eq \"home\"]"
			];

		ICollection<PatchOperationParameter> GetLegalOperations(ICollection<PatchOperationParameter> operations)
		{
			return operations
				.Where(op => string.IsNullOrEmpty(op.Path) || !unsupportedOperations.Any(uo => op.Path.Contains(uo)))
				.ToList();
		}

		void CheckParameter(PatchRepresentationParameter patchRepresentation)
		{
			if (patchRepresentation == null)
			{
				throw new SCIMBadSyntaxException(string.Format(Global.RequestIsNotWellFormatted, "PATCH"));
			}

			var requestedSchemas = patchRepresentation.Schemas;
			if (!requestedSchemas.Any())
			{
				throw new SCIMBadSyntaxException(string.Format(Global.AttributeMissing, StandardSCIMRepresentationAttributes.Schemas));
			}

			if (!requestedSchemas.SequenceEqual(new List<string> { StandardSchemas.PatchRequestSchemas.Id }))
			{
				throw new SCIMBadSyntaxException(Global.SchemasNotRecognized);
			}

			if (patchRepresentation.Operations == null)
			{
				throw new SCIMBadSyntaxException(string.Format(Global.AttributeMissing, StandardSCIMRepresentationAttributes.Operations));
			}
		}
	}
}
