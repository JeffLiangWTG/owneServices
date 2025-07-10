using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.Services.Scim.Api.Helpers;
using Enterprise.Services.Scim.Api.ScimHttpResults;
using Enterprise.Services.Scim.Business;
using Enterprise.Services.Scim.Contracts;
using Enterprise.Services.Scim.Helpers;
using Enterprise.Services.Scim.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SimpleIdServer.Scim;
using SimpleIdServer.Scim.Domain;
using SimpleIdServer.Scim.Domains;
using SimpleIdServer.Scim.DTOs;
using SimpleIdServer.Scim.Exceptions;
using SimpleIdServer.Scim.Helpers;
using SimpleIdServer.Scim.Parser;
using SimpleIdServer.Scim.Parser.Exceptions;
using SimpleIdServer.Scim.Parser.Expressions;
using SimpleIdServer.Scim.Persistence;

namespace Enterprise.Services.Scim.Api.Controllers
{
	public class ScimController<T> : ApiController where T : ScimBase
	{
		readonly IPersistanceRepository<T> persistanceRepository;
		readonly IScimRepresentationConverter<T> scimRepresentationConverter;
		readonly IScimToScimRepresentation scimToScimRepresentation;
		readonly ISCIMSchemaQueryRepository scimSchemaQueryRepository;
		readonly IResourceTypeResolver resourceTypeResolver;
		readonly Helpers.IUriProvider uriProvider;
		readonly IAttributeReferenceEnricher attributeReferenceEnricher;
		readonly SCIMHostOptions options;
		readonly ISCIMRepresentationHelper scimRepresentationHelper;
		readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		public ScimController(IPersistanceRepository<T> persistanceRepository,
					 IScimRepresentationConverter<T> scimRepresentationConverter,
					 IScimToScimRepresentation scimToScimRepresentation,
					 ISCIMSchemaQueryRepository scimSchemaQueryRepository,
					 IResourceTypeResolver resourceTypeResolver,
					 Helpers.IUriProvider uriProvider,
					 IAttributeReferenceEnricher attributeReferenceEnricher,
					 SCIMHostOptions options,
					 ISCIMRepresentationHelper scimRepresentationHelper)
		{
			this.persistanceRepository = persistanceRepository;
			this.scimRepresentationConverter = scimRepresentationConverter;
			this.scimToScimRepresentation = scimToScimRepresentation;
			this.scimSchemaQueryRepository = scimSchemaQueryRepository;
			this.resourceTypeResolver = resourceTypeResolver;
			this.uriProvider = uriProvider;
			this.attributeReferenceEnricher = attributeReferenceEnricher;
			this.options = options;
			this.scimRepresentationHelper = scimRepresentationHelper;
		}

		[HttpGet]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Logging")]
		public async virtual Task<IHttpActionResult> GetByIdAsync(string id, [FromUri] GetSCIMResourceRequest parameter)
		{
			var resourceType = ReturnResourceType();
			try
			{
				using (DisposableActionForDbConnection())
				{
					var schema = await scimSchemaQueryRepository.FindRootSCIMSchemaByResourceType(resourceType);
					if (schema == null)
					{
						return NotFound();
					}

					var resource = await persistanceRepository.FindSCIMByResourceId(id);
					if (resource is null)
					{
						logger.Warn(string.Format(SimpleIdServer.Scim.Resources.Global.ResourceNotFound, id));
						return new ScimErrorResult(string.Format(SimpleIdServer.Scim.Resources.Global.ResourceNotFound, id), HttpStatusCode.NotFound, null);
					}

					var representation = await scimToScimRepresentation.ScimToSCIMRepresentation(resource, true);
					representation.ApplyEmptyArray();

					await attributeReferenceEnricher.Enrich(resourceType, ReturnControllerName(), new List<SCIMRepresentation> { representation }, uriProvider.GetAbsoluteUriWithVirtualPath(Request));
					var schemaIds = new List<string> { schema.Id };
					schemaIds.AddRange(schema.SchemaExtensions.Select(s => s.Schema));

					var schemas = (await scimSchemaQueryRepository.FindSCIMSchemaByIdentifiers(schemaIds)).ToList();
					var standardSchemas = new List<SCIMSchema>
					{
						StandardSchemas.StandardResponseSchemas
					};
					standardSchemas.AddRange(schemas);
					var includedAttributes = parameter.Attributes == null ? new List<SCIMAttributeExpression>() : parameter.Attributes.Select(a => SCIMFilterParser.Parse(a, standardSchemas)).Cast<SCIMAttributeExpression>().ToList();
					var excludedAttributes = parameter.ExcludedAttributes == null ? new List<SCIMAttributeExpression>() : parameter.ExcludedAttributes.Select(a => SCIMFilterParser.Parse(a, standardSchemas)).Cast<SCIMAttributeExpression>().ToList();
					representation.FilterAttributes(includedAttributes, excludedAttributes);
					var location = GetLocation(representation);

					await attributeReferenceEnricher.Enrich(SCIMResourceTypes.User, ReturnControllerName(), new List<SCIMRepresentation> { representation }, uriProvider.GetAbsoluteUriWithVirtualPath(Request));
					var content = representation.ToResponse(location, true, mergeExtensionAttributes: options.MergeExtensionAttributes);

					return new ScimHttpResult(content, location, representation.Version, HttpStatusCode.OK);
				}
			}
			catch (SCIMAttributeException ex)
			{
				logger.Error(ex, "{ErrorScim} - {Message}", SCIMConstants.ErrorSCIMTypes.InvalidValue, ex.Message);
				return new ScimErrorResult(ex.Message, HttpStatusCode.BadRequest, SCIMConstants.ErrorSCIMTypes.InvalidValue);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				logger.Error(ex, "{ErrorScim} - {Message}", SCIMConstants.ErrorSCIMTypes.InternalServerError, ex.Message);
				return new ScimErrorResult(ex.Message, HttpStatusCode.InternalServerError, SCIMConstants.ErrorSCIMTypes.InternalServerError);
			}
		}

		[HttpGet]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Logging")]
		public async Task<IHttpActionResult> SearchAsync([FromUri] SearchSCIMResourceParameter searchRequest)
		{
			var resourceType = ReturnResourceType();
			try
			{
				using (DisposableActionForDbConnection())
				{
					if (searchRequest.Count > options.MaxResults || searchRequest.Count == null)
					{
						searchRequest.Count = options.MaxResults;
					}

					var schema = await scimSchemaQueryRepository.FindRootSCIMSchemaByResourceType(resourceType);
					var schemaIds = new List<string> { schema.Id };
					schemaIds.AddRange(schema.SchemaExtensions.Select(s => s.Schema));
					var schemas = (await scimSchemaQueryRepository.FindSCIMSchemaByIdentifiers(schemaIds)).ToList();
					var sortByFilter = SCIMFilterParser.Parse(searchRequest.SortBy, schemas);

					if (searchRequest.StartIndex <= 0)
					{
						logger.Error(SimpleIdServer.Scim.Resources.Global.StartIndexMustBeSuperiorOrEqualTo1);
						return new ScimErrorResult(SimpleIdServer.Scim.Resources.Global.StartIndexMustBeSuperiorOrEqualTo1, HttpStatusCode.BadRequest, null);
					}

					var standardSchemas = new List<SCIMSchema>
					{
						StandardSchemas.StandardResponseSchemas
					};
					standardSchemas.AddRange(schemas);
					var includedAttributes = searchRequest.Attributes == null ? new List<SCIMAttributeExpression>() : searchRequest.Attributes.Select(a => SCIMFilterParser.Parse(a, standardSchemas)).Cast<SCIMAttributeExpression>().ToList();
					var excludedAttributes = searchRequest.ExcludedAttributes == null ? new List<SCIMAttributeExpression>() : searchRequest.ExcludedAttributes.Select(a => SCIMFilterParser.Parse(a, standardSchemas)).Cast<SCIMAttributeExpression>().ToList();
					var searchSCIMRepresentationsParameter = new SearchSCIMRepresentationsParameter(resourceType, searchRequest.StartIndex, searchRequest.Count.Value, sortByFilter, searchRequest.SortOrder, SCIMFilterParser.Parse(searchRequest.Filter, schemas), includedAttributes, excludedAttributes);

					var resource = await persistanceRepository.FindSCIMResource(searchSCIMRepresentationsParameter);

					var representations = new List<SCIMRepresentation>();
					foreach (var user in resource.Content)
					{
						var scimRepresentation = await scimToScimRepresentation.ScimToSCIMRepresentation(user, true);

						if (scimRepresentation != null)
						{
							representations.Add(scimRepresentation);
						}
					}
					representations.FilterAttributes(includedAttributes, excludedAttributes);

					var jObj = new JObject
					{
						{ StandardSCIMRepresentationAttributes.Schemas, new JArray(new [] { StandardSchemas.ListResponseSchemas.Id } ) },
						{ StandardSCIMRepresentationAttributes.TotalResults, resource.TotalResults },
						{ StandardSCIMRepresentationAttributes.ItemsPerPage, searchRequest.Count },
						{ StandardSCIMRepresentationAttributes.StartIndex, searchRequest.StartIndex }
					};
					var resources = new JArray();
					var baseUrl = uriProvider.GetAbsoluteUriWithVirtualPath(Request);
					foreach (var representation in representations)
					{
						representation.Schemas = schemas;
					}

					await attributeReferenceEnricher.Enrich(resourceType, ReturnControllerName(), representations, baseUrl);

					foreach (var record in representations)
					{
						JObject newJObj = null;
						var location = $"{baseUrl}/{resourceTypeResolver.ResolveByResourceType(resourceType).ControllerName}/{record.Id}";
						bool includeStandardRequest = true;
						if (searchRequest.Attributes.Any())
						{
							record.AddStandardAttributes(location, searchRequest.Attributes, true, false);
							includeStandardRequest = false;
						}
						else if (searchRequest.ExcludedAttributes.Any())
						{
							record.AddStandardAttributes(location, searchRequest.ExcludedAttributes, false, false);
							includeStandardRequest = false;
						}
						else
						{
							record.ApplyEmptyArray();
						}

						newJObj = record.ToResponse(location, true, includeStandardRequest, mergeExtensionAttributes: options.MergeExtensionAttributes);
						resources.Add(newJObj);
					}

					jObj.Add(StandardSCIMRepresentationAttributes.Resources, resources);

					var response = new HttpResponseMessage(HttpStatusCode.OK);
					response.Content = new StringContent(jObj.ToString());
					response.Content.Headers.ContentType = new MediaTypeHeaderValue(SCIMConstants.STANDARD_SCIM_CONTENT_TYPE);

					return ResponseMessage(response);
				}
			}
			catch (SCIMFilterException ex)
			{
				logger.Error(ex, "{ErrorScim} - {Message}", SCIMConstants.ErrorSCIMTypes.InvalidFilter, ex.Message);
				return new ScimErrorResult(ex.Message, HttpStatusCode.BadRequest, SCIMConstants.ErrorSCIMTypes.InvalidFilter);
			}
			catch (SCIMAttributeException ex)
			{
				logger.Error(ex, "{ErrorScim} - {Message}", SCIMConstants.ErrorSCIMTypes.InvalidValue, ex.Message);
				return new ScimErrorResult(ex.Message, HttpStatusCode.BadRequest, SCIMConstants.ErrorSCIMTypes.InvalidValue);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				logger.Error(ex, "{ErrorScim} - {Message}", SCIMConstants.ErrorSCIMTypes.InternalServerError, ex.Message);
				return new ScimErrorResult(ex.Message, HttpStatusCode.InternalServerError, SCIMConstants.ErrorSCIMTypes.InternalServerError);
			}
		}

		[HttpPost]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Logging")]
		public async Task<IHttpActionResult> AddAsync([FromBody()] RepresentationParameter jobj)
		{
			var resourceType = ReturnResourceType();
			if (jobj == null)
			{
				logger.Error(SimpleIdServer.Scim.Resources.Global.HttpPostNotWellFormatted);
				return new ScimErrorResult(SimpleIdServer.Scim.Resources.Global.HttpPostNotWellFormatted, HttpStatusCode.BadRequest, SCIMConstants.ErrorSCIMTypes.InvalidSyntax);
			}

			logger.Info("{operation} {externalId} {CreateObj}", SimpleIdServer.Scim.Resources.Global.AddResource, jobj.ExternalId, jobj == null ? string.Empty : JsonConvert.SerializeObject(jobj.Attributes));
			try
			{
				var scimResource = scimRepresentationConverter.JsonToScim(jobj.Attributes.ToString());

				if (scimResource == null)
				{
					logger.Error("Could not convert to user representation");
					return new ScimErrorResult("Could not convert to user representation", HttpStatusCode.BadRequest, SCIMConstants.ErrorSCIMTypes.InvalidSyntax);
				}

				scimResource.ExternalId = jobj.ExternalId;

				using (DisposableActionForDbConnection())
				{
					var schema = await scimSchemaQueryRepository.FindRootSCIMSchemaByResourceType(resourceType);
					if (schema == null)
					{
						return NotFound();
					}

					_ = scimRepresentationHelper.ExtractSCIMRepresentationFromJSON(jobj.Attributes, null, schema, scimSchemaQueryRepository.GetExtensions(schema));
					var createdResource = await persistanceRepository.CreateSCIMResource(scimResource);

					var representation = await scimToScimRepresentation.ScimToSCIMRepresentation(createdResource);
					representation.ApplyEmptyArray();

					var location = GetLocation(representation);
					var content = representation.ToResponse(location, false, mergeExtensionAttributes: options.MergeExtensionAttributes);
					return new ScimHttpResult(content, location, representation.Version, HttpStatusCode.Created);
				}
			}
			catch (SCIMSchemaViolatedException ex)
			{
				logger.Error(ex, "{ErrorScim} - {Message}", SCIMConstants.ErrorSCIMTypes.InvalidValue, ex.Message);
				return new ScimErrorResult(ex.Message, HttpStatusCode.BadRequest, SCIMConstants.ErrorSCIMTypes.InvalidValue);
			}
			catch (SCIMBadSyntaxException ex)
			{
				logger.Error(ex, "{ErrorScim} - {Message}", SCIMConstants.ErrorSCIMTypes.InvalidSyntax, ex.Message);
				return new ScimErrorResult(ex.Message, HttpStatusCode.BadRequest, SCIMConstants.ErrorSCIMTypes.InvalidSyntax);
			}
			catch (SCIMNoTargetException ex)
			{
				logger.Error(ex, "{ErrorScim} - {Message}", SCIMConstants.ErrorSCIMTypes.NoTarget, ex.Message);
				return new ScimErrorResult(ex.Message, HttpStatusCode.BadRequest, SCIMConstants.ErrorSCIMTypes.NoTarget);
			}
			catch (SCIMUniquenessAttributeException ex)
			{
				logger.Error(ex, "{ErrorScim} - {Message}", SCIMConstants.ErrorSCIMTypes.Uniqueness, ex.Message);
				return new ScimErrorResult(ex.Message, HttpStatusCode.Conflict, SCIMConstants.ErrorSCIMTypes.Uniqueness);
			}
			catch (SCIMAttributeException ex)
			{
				logger.Error(ex, "{ErrorScim} - {Message}", SCIMConstants.ErrorSCIMTypes.InvalidValue, ex.Message);
				return new ScimErrorResult(ex.Message, HttpStatusCode.BadRequest, SCIMConstants.ErrorSCIMTypes.InvalidValue);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				logger.Error(ex, "{ErrorScim} - {Message}", SCIMConstants.ErrorSCIMTypes.InternalServerError, ex.Message);
				return new ScimErrorResult(ex.Message, HttpStatusCode.InternalServerError, SCIMConstants.ErrorSCIMTypes.InternalServerError);
			}
		}

		[HttpPut]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Logging")]
		public async Task<IHttpActionResult> UpdateAsync(string id, RepresentationParameter representationParameter)
		{
			var resourceType = ReturnResourceType();
			if (representationParameter == null)
			{
				logger.Error(SimpleIdServer.Scim.Resources.Global.HttpPutNotWellFormatted);
				return new ScimErrorResult(SimpleIdServer.Scim.Resources.Global.HttpPutNotWellFormatted, HttpStatusCode.BadRequest, SCIMConstants.ErrorSCIMTypes.InvalidSyntax);
			}

			logger.Info(SimpleIdServer.Scim.Resources.Global.UpdateResource, id);
			try
			{
				using (DisposableActionForDbConnection())
				{
					var scimResource = scimRepresentationConverter.JsonToScim(representationParameter.Attributes.ToString());

					if (scimResource == null)
					{
						logger.Error("Could not convert to user representation");
						return new ScimErrorResult("Could not convert to user representation", HttpStatusCode.BadRequest, SCIMConstants.ErrorSCIMTypes.InvalidSyntax);
					}

					scimResource.ExternalId = representationParameter.ExternalId;

					var schema = await scimSchemaQueryRepository.FindRootSCIMSchemaByResourceType(resourceType);
					if (schema == null)
					{
						return NotFound();
					}

					_ = scimRepresentationHelper.ExtractSCIMRepresentationFromJSON(representationParameter.Attributes, null, schema, scimSchemaQueryRepository.GetExtensions(schema));
					var updatedResource = await persistanceRepository.UpdateSCIMResourceById(id, scimResource);

					var newRepresentation = await scimToScimRepresentation.ScimToSCIMRepresentation(updatedResource);
					var location = GetLocation(newRepresentation);
					var content = newRepresentation.ToResponse(location, false, mergeExtensionAttributes: options.MergeExtensionAttributes);
					return new ScimHttpResult(content, location, newRepresentation.Version, HttpStatusCode.OK);
				}
			}
			catch (SCIMUniquenessAttributeException ex)
			{
				logger.Error(ex, "{ErrorScim} - {Message}", SCIMConstants.ErrorSCIMTypes.Uniqueness, ex.Message);
				return new ScimErrorResult(ex.Message, HttpStatusCode.Conflict, SCIMConstants.ErrorSCIMTypes.Uniqueness);
			}
			catch (SCIMSchemaViolatedException ex)
			{
				logger.Error(ex, "{ErrorScim} - {Message}", SCIMConstants.ErrorSCIMTypes.InvalidValue, ex.Message);
				return new ScimErrorResult(ex.Message, HttpStatusCode.BadRequest, SCIMConstants.ErrorSCIMTypes.InvalidValue);
			}
			catch (SCIMBadSyntaxException ex)
			{
				logger.Error(ex, "{ErrorScim} - {Message}", SCIMConstants.ErrorSCIMTypes.InvalidSyntax, ex.Message);
				return new ScimErrorResult(ex.Message, HttpStatusCode.BadRequest, SCIMConstants.ErrorSCIMTypes.InvalidSyntax);
			}
			catch (SCIMImmutableAttributeException ex)
			{
				logger.Error(ex, "{ErrorScim} - {Message}", SCIMConstants.ErrorSCIMTypes.Mutability, ex.Message);
				return new ScimErrorResult(ex.Message, HttpStatusCode.BadRequest, SCIMConstants.ErrorSCIMTypes.Mutability);
			}
			catch (SCIMNotFoundException ex)
			{
				logger.Error(ex, "{ErrorScim} - {Message}", SCIMConstants.ErrorSCIMTypes.Unknown, ex.Message);
				return new ScimErrorResult(ex.Message, HttpStatusCode.NotFound, SCIMConstants.ErrorSCIMTypes.Unknown);
			}
			catch (SCIMAttributeException ex)
			{
				logger.Error(ex, "{ErrorScim} - {Message}", SCIMConstants.ErrorSCIMTypes.InvalidValue, ex.Message);
				return new ScimErrorResult(ex.Message, HttpStatusCode.BadRequest, SCIMConstants.ErrorSCIMTypes.InvalidValue);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				logger.Error(ex, "{ErrorScim} - {Message}", SCIMConstants.ErrorSCIMTypes.InternalServerError, ex.Message);
				return new ScimErrorResult(ex.Message, HttpStatusCode.InternalServerError, SCIMConstants.ErrorSCIMTypes.InternalServerError);
			}
		}

		[HttpPatch]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Logging")]
		public async Task<IHttpActionResult> PatchAsync(string id, PatchRepresentationParameter patchRepresentation)
		{
			logger.Info("{Id} {PatchObj}", id, patchRepresentation == null ? string.Empty : JsonConvert.SerializeObject(patchRepresentation));
			try
			{
				using (DisposableActionForDbConnection())
				{
					var patchedResource = await persistanceRepository.PatchSCIMResourceById(id, patchRepresentation);
					if (patchedResource == null)
					{
						return StatusCode(HttpStatusCode.NoContent);
					}

					var newRepresentation = await scimToScimRepresentation.ScimToSCIMRepresentation(patchedResource);
					var location = GetLocation(newRepresentation);
					var content = newRepresentation.ToResponse(location, false, mergeExtensionAttributes: options.MergeExtensionAttributes);

					return new ScimHttpResult(content, location, newRepresentation.Version, HttpStatusCode.OK);
				}
			}
			catch (SCIMDuplicateAttributeException ex)
			{
				logger.Error(ex, "{ErrorScim} - {Message}", SCIMConstants.ErrorSCIMTypes.Uniqueness, ex.Message);
				return new ScimErrorResult(ex.Message, HttpStatusCode.NoContent, SCIMConstants.ErrorSCIMTypes.Uniqueness);
			}
			catch (SCIMUniquenessAttributeException ex)
			{
				logger.Error(ex, "{ErrorScim} - {Message}", SCIMConstants.ErrorSCIMTypes.Uniqueness, ex.Message);
				return new ScimErrorResult(ex.Message, HttpStatusCode.Conflict, SCIMConstants.ErrorSCIMTypes.Uniqueness);
			}
			catch (SCIMFilterException ex)
			{
				logger.Error(ex, "{ErrorScim} - {Message}", SCIMConstants.ErrorSCIMTypes.InvalidFilter, ex.Message);
				return new ScimErrorResult(ex.Message, HttpStatusCode.BadRequest, SCIMConstants.ErrorSCIMTypes.InvalidFilter);
			}
			catch (SCIMBadSyntaxException ex)
			{
				logger.Error(ex, "{ErrorScim} - {Message}", SCIMConstants.ErrorSCIMTypes.InvalidSyntax, ex.Message);
				return new ScimErrorResult(ex.Message, HttpStatusCode.BadRequest, SCIMConstants.ErrorSCIMTypes.InvalidSyntax);
			}
			catch (SCIMNoTargetException ex)
			{
				logger.Error(ex, "{ErrorScim} - {Message}", SCIMConstants.ErrorSCIMTypes.NoTarget, ex.Message);
				return new ScimErrorResult(ex.Message, HttpStatusCode.NotFound, SCIMConstants.ErrorSCIMTypes.NoTarget);
			}
			catch (SCIMNotFoundException ex)
			{
				logger.Error(ex, "{ErrorScim} - {Message}", SCIMConstants.ErrorSCIMTypes.Unknown, ex.Message);
				return new ScimErrorResult(ex.Message, HttpStatusCode.NotFound, SCIMConstants.ErrorSCIMTypes.Unknown);
			}
			catch (SCIMAttributeException ex)
			{
				logger.Error(ex, "{ErrorScim} - {Message}", SCIMConstants.ErrorSCIMTypes.InvalidValue, ex.Message);
				return new ScimErrorResult(ex.Message, HttpStatusCode.BadRequest, SCIMConstants.ErrorSCIMTypes.InvalidValue);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				logger.Error(ex, "{ErrorScim} - {Message}", SCIMConstants.ErrorSCIMTypes.InternalServerError, ex.Message);
				return new ScimErrorResult(ex.Message, HttpStatusCode.InternalServerError, SCIMConstants.ErrorSCIMTypes.InternalServerError);
			}
		}

		[HttpDelete]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Logging")]
		public async Task<IHttpActionResult> DeleteAsync(string id)
		{
			logger.Info(string.Format(SimpleIdServer.Scim.Resources.Global.DeleteResource, id));
			try
			{
				using (DisposableActionForDbConnection())
				{
					await persistanceRepository.DeleteSCIMResourceById(id);
					return StatusCode(HttpStatusCode.NoContent);
				}
			}
			catch (SCIMNotFoundException ex)
			{
				logger.Error(ex, "{ErrorScim} - {Message}", SCIMConstants.ErrorSCIMTypes.Unknown, ex.Message);
				return new ScimErrorResult(ex.Message, HttpStatusCode.NotFound, SCIMConstants.ErrorSCIMTypes.Unknown);
			}
			catch (SCIMAttributeException ex)
			{
				logger.Error(ex, "{ErrorScim} - {Message}", SCIMConstants.ErrorSCIMTypes.InvalidValue, ex.Message);
				return new ScimErrorResult(ex.Message, HttpStatusCode.BadRequest, SCIMConstants.ErrorSCIMTypes.InvalidValue);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				logger.Error(ex, "{ErrorScim} - {Message}", SCIMConstants.ErrorSCIMTypes.InternalServerError, ex.Message);
				return new ScimErrorResult(ex.Message, HttpStatusCode.InternalServerError, SCIMConstants.ErrorSCIMTypes.InternalServerError);
			}
		}

		protected string GetLocation(SCIMRepresentation representation)
		{
			return $"{uriProvider.GetAbsoluteUriWithVirtualPath(Request)}/{resourceTypeResolver.ResolveByResourceType(representation.ResourceType)?.ControllerName}/{representation.Id}";
		}

		protected virtual string GetResourceType(string resourceType)
		{
			return !SCIMConstants.MappingScimResourceTypeToCommonType.ContainsKey(resourceType) ? resourceType : SCIMConstants.MappingScimResourceTypeToCommonType[resourceType];
		}

		protected virtual IDisposable DisposableActionForDbConnection()
		{
			return Db.DisposableActionForDbConnection();
		}

		string ReturnResourceType() => typeof(T) == typeof(ScimUser) ? SCIMResourceTypes.User : SCIMResourceTypes.Group;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Controller name")]
		string ReturnControllerName() => typeof(T) == typeof(ScimUser) ? "Users" : "Groups";
	}
}
