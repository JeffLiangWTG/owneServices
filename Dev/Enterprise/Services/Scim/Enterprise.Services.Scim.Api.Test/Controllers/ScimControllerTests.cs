using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Results;
using Enterprise.Services.Scim.Api.Helpers;
using Enterprise.Services.Scim.Api.ScimHttpResults;
using Enterprise.Services.Scim.Api.Test.Controllers;
using Enterprise.Services.Scim.Contracts;
using Enterprise.Services.Scim.Helpers;
using Enterprise.Services.Scim.Models;
using Enterprise.Services.Scim.Tests.Helpers;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using SimpleIdServer.Scim;
using SimpleIdServer.Scim.Domains;
using SimpleIdServer.Scim.DTOs;
using SimpleIdServer.Scim.Exceptions;
using SimpleIdServer.Scim.Helpers;
using SimpleIdServer.Scim.Parser.Exceptions;
using SimpleIdServer.Scim.Persistence;
using SimpleIdServer.Scim.Persistence.InMemory;
using SimpleIdServer.Scim.Resources;

namespace Enterprise.Services.Scim.Tests.Controllers
{
	[TestFixture(typeof(ScimUser))]
	[TestFixture(typeof(ScimGroup))]
	public class ScimControllerTests<T> where T : ScimBase, new()
	{
		Mock<IPersistanceRepository<T>> persistanceRepository;
		Mock<Api.Helpers.IUriProvider> uriProviderMock;
		Mock<IResourceTypeResolver> resourceTypeResolverMock;
		Mock<IScimRepresentationConverter<T>> scimRepresentationConverterMock;
		Mock<IScimToScimRepresentation> scimToScimRepresentationMock;
		Mock<SCIMHostOptions> optionsMonitorMock;
		Mock<IAttributeReferenceEnricher> attributeReferenceEnricherMock;
		Mock<ISCIMRepresentationHelper> scimRepresentationHelperMock;
		TestableScimController<T> scimController;

		[Test]
		public async Task TestGetByIdAsyncInvalidSchema()
		{
			Mock<ISCIMSchemaQueryRepository> sCIMSchemaQueryRepositoryMock = new Mock<ISCIMSchemaQueryRepository>();
			sCIMSchemaQueryRepositoryMock.Setup(s => s.FindRootSCIMSchemaByResourceType(It.IsAny<string>())).Returns(Task.FromResult<SCIMSchema>(null));

			scimController = new TestableScimController<T>(persistanceRepository.Object, scimRepresentationConverterMock.Object, scimToScimRepresentationMock.Object, sCIMSchemaQueryRepositoryMock.Object,
				resourceTypeResolverMock.Object, uriProviderMock.Object, attributeReferenceEnricherMock.Object, optionsMonitorMock.Object, scimRepresentationHelperMock.Object);

			var resourceRequest = new GetSCIMResourceRequest();
			var res = await scimController.GetByIdAsync("1", resourceRequest);
			Assert.That(res, Is.TypeOf<NotFoundResult>());
		}

		[Test]
		public async Task TestGetByIdAsyncNotFound()
		{
			var resourceRequest = new GetSCIMResourceRequest();
			var res = await scimController.GetByIdAsync("1", resourceRequest);
			var httpRes = await res.ExecuteAsync(CancellationToken.None);
			var responseBody = await httpRes.Content.ReadAsStringAsync();

			Assert.That(res, Is.TypeOf(typeof(ScimErrorResult)));
			var jToken = Utils.ExtractJsonFromBody(responseBody);
			Assert.That(httpRes.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
			Assert.That(jToken.GetTokenAsString("status"), Is.EqualTo("404"));
			Assert.That(jToken.GetTokenAsString("schemas[0]"), Is.EqualTo("urn:ietf:params:scim:api:messages:2.0:Error"));
			Assert.That(jToken.GetTokenAsString("detail"), Is.EqualTo("resource 1 not found"));
		}

		[Test]
		public async Task TestGetByIdAsyncFound()
		{
			var resourceRequest = new GetSCIMResourceRequest();
			var scimResource = new T { };
			persistanceRepository.Setup(u => u.FindSCIMByResourceId(It.IsAny<string>())).Returns(Task.FromResult(scimResource));
			var res = await scimController.GetByIdAsync("1", resourceRequest);
			var httpRes = await res.ExecuteAsync(CancellationToken.None);
			var responseBody = await httpRes.Content.ReadAsStringAsync();

			Assert.That(res, Is.TypeOf(typeof(ScimHttpResult)));
			var jToken = Utils.ExtractJsonFromBody(responseBody);
			Assert.That(httpRes.StatusCode, Is.EqualTo(HttpStatusCode.OK));
		}

		[Test]
		public async Task TestGetByIdAsyncThrowsException()
		{
			var resourceRequest = new GetSCIMResourceRequest();
			persistanceRepository.Setup(u => u.FindSCIMByResourceId(It.IsAny<string>())).ThrowsAsync(new Exception());
			var res = await scimController.GetByIdAsync("1", resourceRequest);
			var httpRes = await res.ExecuteAsync(CancellationToken.None);
			var responseBody = await httpRes.Content.ReadAsStringAsync();

			Assert.That(res, Is.TypeOf(typeof(ScimErrorResult)));
			var jToken = Utils.ExtractJsonFromBody(responseBody);
			Assert.That(httpRes.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));
			Assert.That(jToken.GetTokenAsString("scimType"), Is.EqualTo(SCIMConstants.ErrorSCIMTypes.InternalServerError));
		}

		[Test]
		public async Task TestSearchAsyncNegativeStartIndex()
		{
			var searchRequest = new SearchSCIMResourceParameter();
			searchRequest.StartIndex = -1;
			var res = await scimController.SearchAsync(searchRequest);
			var httpRes = await res.ExecuteAsync(CancellationToken.None);
			var responseBody = await httpRes.Content.ReadAsStringAsync();

			Assert.That(res, Is.TypeOf(typeof(ScimErrorResult)));
			var jToken = Utils.ExtractJsonFromBody(responseBody);
			Assert.That(httpRes.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
			Assert.That(jToken.GetTokenAsString("detail"), Is.EqualTo(Global.StartIndexMustBeSuperiorOrEqualTo1));
		}

		[Test]
		public async Task TestSearchAsyncEmptyResult()
		{
			var scimResources = new List<T>();
			persistanceRepository.Setup(u => u.FindSCIMResource(It.IsAny<SearchSCIMRepresentationsParameter>())).ReturnsAsync(new SearchScimResponse<T>(scimResources.Count, scimResources));

			var searchRequest = new SearchSCIMResourceParameter();
			var res = await scimController.SearchAsync(searchRequest);
			var httpRes = await res.ExecuteAsync(CancellationToken.None);
			var responseBody = await httpRes.Content.ReadAsStringAsync();

			var jToken = Utils.ExtractJsonFromBody(responseBody);
			Assert.That(httpRes.StatusCode, Is.EqualTo(HttpStatusCode.OK));

			Assert.That(jToken.GetTokenAsString("schemas[0]"), Is.EqualTo("urn:ietf:params:scim:api:messages:2.0:ListResponse"));
			Assert.That(jToken.GetTokenAsString("totalResults"), Is.EqualTo("0"));
		}

		[Test]
		public async Task TestSearchAsyncOneResult()
		{
			var scimResources = new List<T> { new T() { ExternalId = "andrew@wtg.com" } };
			persistanceRepository.Setup(u => u.FindSCIMResource(It.IsAny<SearchSCIMRepresentationsParameter>())).ReturnsAsync(new SearchScimResponse<T>(scimResources.Count, scimResources));

			var searchRequest = new SearchSCIMResourceParameter();
			var res = await scimController.SearchAsync(searchRequest);
			var httpRes = await res.ExecuteAsync(CancellationToken.None);
			var responseBody = await httpRes.Content.ReadAsStringAsync();

			var jToken = Utils.ExtractJsonFromBody(responseBody);
			Assert.That(httpRes.StatusCode, Is.EqualTo(HttpStatusCode.OK));

			Assert.That(jToken.GetTokenAsString("schemas[0]"), Is.EqualTo("urn:ietf:params:scim:api:messages:2.0:ListResponse"));
			Assert.That(jToken.GetTokenAsString("totalResults"), Is.EqualTo("1"));
		}

		[Test]
		public async Task TestSearchAsyncWithFiltersNoResult()
		{
			var scimResources = new List<T> { new T() { ExternalId = "andrew@wtg.com" } };
			persistanceRepository.Setup(u => u.FindSCIMResource(It.IsAny<SearchSCIMRepresentationsParameter>())).ReturnsAsync(new SearchScimResponse<T>(scimResources.Count, scimResources));
			scimToScimRepresentationMock.Setup(s => s.ScimToSCIMRepresentation(It.IsAny<T>(), true)).ReturnsAsync((SCIMRepresentation)null);

			var searchRequest = new SearchSCIMResourceParameter { Attributes = new List<string> { "userName" } };
			var res = await scimController.SearchAsync(searchRequest);
			var httpRes = await res.ExecuteAsync(CancellationToken.None);
			var responseBody = await httpRes.Content.ReadAsStringAsync();

			var jToken = Utils.ExtractJsonFromBody(responseBody);
			Assert.That(httpRes.StatusCode, Is.EqualTo(HttpStatusCode.OK));

			Assert.That(jToken.GetTokenAsString("schemas[0]"), Is.EqualTo("urn:ietf:params:scim:api:messages:2.0:ListResponse"));
			Assert.That(jToken.GetTokenAsString("totalResults"), Is.EqualTo("1"));
		}

		[Test]
		public async Task TestSearchAsyncMultipleResult()
		{
			var scimResources = new List<T> { new T() { ExternalId = "andrew@wtg.com" }, new T() { ExternalId = "ryan@wtg.com" } };
			persistanceRepository.Setup(u => u.FindSCIMResource(It.IsAny<SearchSCIMRepresentationsParameter>())).ReturnsAsync(new SearchScimResponse<T>(scimResources.Count, scimResources));

			var searchRequest = new SearchSCIMResourceParameter();
			var res = await scimController.SearchAsync(searchRequest);
			var httpRes = await res.ExecuteAsync(CancellationToken.None);
			var responseBody = await httpRes.Content.ReadAsStringAsync();

			var jToken = Utils.ExtractJsonFromBody(responseBody);
			Assert.That(httpRes.StatusCode, Is.EqualTo(HttpStatusCode.OK));

			Assert.That(jToken.GetTokenAsString("schemas[0]"), Is.EqualTo("urn:ietf:params:scim:api:messages:2.0:ListResponse"));
			Assert.That(jToken.GetTokenAsString("totalResults"), Is.EqualTo("2"));
			Assert.That(jToken.SelectToken("Resources").Count(), Is.EqualTo(2));
		}

		[Test]
		public async Task TestSearchAsyncInvalidFilter()
		{
			persistanceRepository.Setup(u => u.FindSCIMResource(It.IsAny<SearchSCIMRepresentationsParameter>())).ThrowsAsync(new SCIMFilterException(""));

			var searchRequest = new SearchSCIMResourceParameter();
			var res = await scimController.SearchAsync(searchRequest);
			var httpRes = await res.ExecuteAsync(CancellationToken.None);
			var responseBody = await httpRes.Content.ReadAsStringAsync();

			Assert.That(res, Is.TypeOf(typeof(ScimErrorResult)));
			var jToken = Utils.ExtractJsonFromBody(responseBody);
			Assert.That(httpRes.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
			Assert.That(jToken.GetTokenAsString("scimType"), Is.EqualTo(SCIMConstants.ErrorSCIMTypes.InvalidFilter));
		}

		[Test]
		public async Task TestAddAsync()
		{
			var representation = new SCIMRepresentation { ResourceType = ReturnResourceType(), Version = 1 };
			var jobj = GetRepresentationParameter();

			if (typeof(ScimUser) == typeof(T))
			{
				var schema = StandardSchemas.UserSchema;
				representation.AddAttribute(new SCIMRepresentationAttribute(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), schema.GetAttribute("userName"), schema.Id)
				{
					Id = Guid.NewGuid().ToString(),
					ValueString = "andrew@wg.com"
				});
				representation.ExternalId = jobj.ExternalId;
			}

			if (typeof(ScimGroup) == typeof(T))
			{
				var schema = StandardSchemas.GroupSchema;
				representation.AddAttribute(new SCIMRepresentationAttribute(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), schema.GetAttribute("displayName"), schema.Id)
				{
					Id = Guid.NewGuid().ToString(),
					ValueString = "group1"
				});
				representation.ExternalId = jobj.ExternalId;
			}

			scimToScimRepresentationMock.Setup(s => s.ScimToSCIMRepresentation(It.IsAny<ScimUser>(), false)).ReturnsAsync(representation);

			var res = await scimController.AddAsync(jobj);
			var httpRes = await res.ExecuteAsync(CancellationToken.None);
			var responseBody = await httpRes.Content.ReadAsStringAsync();

			Assert.That(res, Is.TypeOf(typeof(ScimHttpResult)));
			var jToken = Utils.ExtractJsonFromBody(responseBody);
			Assert.That(httpRes.StatusCode, Is.EqualTo(HttpStatusCode.Created));

			if (typeof(ScimUser) == typeof(T))
			{
				Assert.That(jToken.GetTokenAsString("userName"), Is.EqualTo("andrew@wg.com"));
				Assert.That(jToken.GetTokenAsString("externalId"), Is.EqualTo("1"));
			}

			if (typeof(ScimGroup) == typeof(T))
			{
				Assert.That(jToken.GetTokenAsString("displayName"), Is.EqualTo("group1"));
				Assert.That(jToken.GetTokenAsString("externalId"), Is.EqualTo("1"));
			}
		}

		[Test]
		public async Task TestSearchAsyncThrowsException()
		{
			persistanceRepository.Setup(u => u.FindSCIMResource(It.IsAny<SearchSCIMRepresentationsParameter>())).ThrowsAsync(new Exception(""));

			var searchRequest = new SearchSCIMResourceParameter();
			var res = await scimController.SearchAsync(searchRequest);
			var httpRes = await res.ExecuteAsync(CancellationToken.None);
			var responseBody = await httpRes.Content.ReadAsStringAsync();

			Assert.That(res, Is.TypeOf(typeof(ScimErrorResult)));
			var jToken = Utils.ExtractJsonFromBody(responseBody);
			Assert.That(httpRes.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));
			Assert.That(jToken.GetTokenAsString("scimType"), Is.EqualTo(SCIMConstants.ErrorSCIMTypes.InternalServerError));
		}

		public static IEnumerable<TestCaseData> AddAsyncExceptions
		{
			get
			{
				yield return new TestCaseData(new SCIMSchemaViolatedException(""), HttpStatusCode.BadRequest, SCIMConstants.ErrorSCIMTypes.InvalidValue) { TestName = "{m}_SchemaViolatedException" };
				yield return new TestCaseData(new Exception(""), HttpStatusCode.InternalServerError, SCIMConstants.ErrorSCIMTypes.InternalServerError) { TestName = "{m}_Exception" };
				yield return new TestCaseData(new SCIMBadSyntaxException(""), HttpStatusCode.BadRequest, SCIMConstants.ErrorSCIMTypes.InvalidSyntax) { TestName = "{m}_BadSyntaxException" };
				yield return new TestCaseData(new SCIMUniquenessAttributeException(""), HttpStatusCode.Conflict, SCIMConstants.ErrorSCIMTypes.Uniqueness) { TestName = "{m}_UniquenessAttributeException" };
				yield return new TestCaseData(new SCIMNoTargetException(""), HttpStatusCode.BadRequest, SCIMConstants.ErrorSCIMTypes.NoTarget) { TestName = "{m}_NoTargetException" };
			}
		}

		[TestCaseSource(nameof(AddAsyncExceptions))]
		public async Task TestUserAddAsyncThrowsException(Exception ex, HttpStatusCode statusCode, string scimType)
		{
			persistanceRepository.Setup(u => u.CreateSCIMResource(It.IsAny<T>())).ThrowsAsync(ex);
			var jobj = GetRepresentationParameter();

			var res = await scimController.AddAsync(jobj);
			var httpRes = await res.ExecuteAsync(CancellationToken.None);
			var responseBody = await httpRes.Content.ReadAsStringAsync();

			Assert.That(res, Is.TypeOf(typeof(ScimErrorResult)));
			var jToken = Utils.ExtractJsonFromBody(responseBody);
			Assert.That(httpRes.StatusCode, Is.EqualTo(statusCode));
			Assert.That(jToken.GetTokenAsString("scimType"), Is.EqualTo(scimType));
		}

		[Test]
		public async Task TestUpdateAsync()
		{
			persistanceRepository
				.Setup(r => r.UpdateSCIMResourceById(It.IsAny<string>(), It.Is<T>(x => string.IsNullOrEmpty(x.ExternalId))))
				.ThrowsAsync(new ArgumentException("ExternalId cannot be null or empty"));

			var representation = new SCIMRepresentation { ResourceType = ReturnResourceType(), Version = 1 };

			if (typeof(ScimUser) == typeof(T))
			{
				var schema = StandardSchemas.UserSchema;
				representation.AddAttribute(new SCIMRepresentationAttribute(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), schema.GetAttribute("userName"), schema.Id)
				{
					Id = Guid.NewGuid().ToString(),
					ValueString = "andrew@wg.com"
				});
			}

			if (typeof(ScimGroup) == typeof(T))
			{
				var schema = StandardSchemas.GroupSchema;
				representation.AddAttribute(new SCIMRepresentationAttribute(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), schema.GetAttribute("displayName"), schema.Id)
				{
					Id = Guid.NewGuid().ToString(),
					ValueString = "group1"
				});
			}

			representation.ExternalId = "ext";

			scimToScimRepresentationMock.Setup(s => s.ScimToSCIMRepresentation(It.IsAny<ScimBase>(), false)).ReturnsAsync(representation);
			var jobj = GetRepresentationParameter();

			var res = await scimController.UpdateAsync("1", jobj);
			var httpRes = await res.ExecuteAsync(CancellationToken.None);
			var responseBody = await httpRes.Content.ReadAsStringAsync();

			Assert.That(res, Is.TypeOf(typeof(ScimHttpResult)));
			var jToken = Utils.ExtractJsonFromBody(responseBody);
			Assert.That(httpRes.StatusCode, Is.EqualTo(HttpStatusCode.OK));

			if (typeof(ScimUser) == typeof(T))
			{
				Assert.That(jToken.GetTokenAsString("userName"), Is.EqualTo("andrew@wg.com"));
			}

			if (typeof(ScimGroup) == typeof(T))
			{
				Assert.That(jToken.GetTokenAsString("displayName"), Is.EqualTo("group1"));
			}

			Assert.That(jToken.GetTokenAsString("externalId"), Is.EqualTo("ext"));
		}

		public static IEnumerable<TestCaseData> UpdateAsyncExceptions
		{
			get
			{
				yield return new TestCaseData(new SCIMUniquenessAttributeException(""), HttpStatusCode.Conflict, SCIMConstants.ErrorSCIMTypes.Uniqueness) { TestName = "{m}_UniquenessAttributeException" };
				yield return new TestCaseData(new SCIMSchemaViolatedException(""), HttpStatusCode.BadRequest, SCIMConstants.ErrorSCIMTypes.InvalidValue) { TestName = "{m}_SchemaViolatedException" };
				yield return new TestCaseData(new SCIMBadSyntaxException(""), HttpStatusCode.BadRequest, SCIMConstants.ErrorSCIMTypes.InvalidSyntax) { TestName = "{m}_InvalidSyntaxException" };
				yield return new TestCaseData(new SCIMImmutableAttributeException(""), HttpStatusCode.BadRequest, SCIMConstants.ErrorSCIMTypes.Mutability) { TestName = "{m}_BadSyntaxException" };
				yield return new TestCaseData(new SCIMNotFoundException(""), HttpStatusCode.NotFound, SCIMConstants.ErrorSCIMTypes.Unknown) { TestName = "{m}_NotFoundException" };
				yield return new TestCaseData(new Exception(""), HttpStatusCode.InternalServerError, SCIMConstants.ErrorSCIMTypes.InternalServerError) { TestName = "{m}_Exception" };
			}
		}

		[TestCaseSource(nameof(UpdateAsyncExceptions))]
		public async Task TestUpdateAsyncThrowsException(Exception ex, HttpStatusCode statusCode, string scimType)
		{
			persistanceRepository.Setup(u => u.UpdateSCIMResourceById(It.IsAny<string>(), It.IsAny<T>())).ThrowsAsync(ex);
			var jobj = GetRepresentationParameter();

			var res = await scimController.UpdateAsync("1", jobj);
			var httpRes = await res.ExecuteAsync(CancellationToken.None);
			var responseBody = await httpRes.Content.ReadAsStringAsync();

			Assert.That(res, Is.TypeOf(typeof(ScimErrorResult)));
			var jToken = Utils.ExtractJsonFromBody(responseBody);
			Assert.That(httpRes.StatusCode, Is.EqualTo(statusCode));
			Assert.That(jToken.GetTokenAsString("scimType"), Is.EqualTo(scimType));
		}

		[Test]
		public async Task TestPatchAsync()
		{
			persistanceRepository.Setup(u => u.PatchSCIMResourceById(It.IsAny<string>(), It.IsAny<PatchRepresentationParameter>())).ReturnsAsync(new T());
			var representation = new SCIMRepresentation { ResourceType = ReturnResourceType(), Version = 1 };
			var jobj = GetRepresentationParameter();
			var patchRepresentation = GetPatchRepresentation();

			if (typeof(ScimUser) == typeof(T))
			{
				var schema = StandardSchemas.UserSchema;
				representation.AddAttribute(new SCIMRepresentationAttribute(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), schema.GetAttribute("userName"), schema.Id)
				{
					Id = Guid.NewGuid().ToString(),
					ValueString = "andrew@wg.com"
				});
			}

			if (typeof(ScimGroup) == typeof(T))
			{
				var schema = StandardSchemas.GroupSchema;
				representation.AddAttribute(new SCIMRepresentationAttribute(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), schema.GetAttribute("displayName"), schema.Id)
				{
					Id = Guid.NewGuid().ToString(),
					ValueString = "group1"
				});
			}

			scimToScimRepresentationMock.Setup(s => s.ScimToSCIMRepresentation(It.IsAny<ScimBase>(), false)).ReturnsAsync(representation);

			var res = await scimController.PatchAsync("1", patchRepresentation);
			var httpRes = await res.ExecuteAsync(CancellationToken.None);
			var responseBody = await httpRes.Content.ReadAsStringAsync();

			Assert.That(res, Is.TypeOf(typeof(ScimHttpResult)));
			var jToken = Utils.ExtractJsonFromBody(responseBody);
			Assert.That(httpRes.StatusCode, Is.EqualTo(HttpStatusCode.OK));

			if (typeof(ScimUser) == typeof(T))
			{
				Assert.That(jToken.GetTokenAsString("userName"), Is.EqualTo("andrew@wg.com"));
			}

			if (typeof(ScimGroup) == typeof(T))
			{
				Assert.That(jToken.GetTokenAsString("displayName"), Is.EqualTo("group1"));
			}
		}

		[Test]
		public async Task TestPatchReturnNullAsync()
		{
			persistanceRepository.Setup(u => u.PatchSCIMResourceById(It.IsAny<string>(), It.IsAny<PatchRepresentationParameter>())).Returns(Task.FromResult<T>(null));
			var representation = new SCIMRepresentation { ResourceType = ReturnResourceType(), Version = 1 };
			var jobj = GetRepresentationParameter();
			var patchRepresentation = GetPatchRepresentation();

			if (typeof(ScimUser) == typeof(T))
			{
				var schema = StandardSchemas.UserSchema;
				representation.AddAttribute(new SCIMRepresentationAttribute(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), schema.GetAttribute("userName"), schema.Id)
				{
					Id = Guid.NewGuid().ToString(),
					ValueString = "andrew@wg.com"
				});
			}

			if (typeof(ScimGroup) == typeof(T))
			{
				var schema = StandardSchemas.GroupSchema;
				representation.AddAttribute(new SCIMRepresentationAttribute(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), schema.GetAttribute("displayName"), schema.Id)
				{
					Id = Guid.NewGuid().ToString(),
					ValueString = "group1"
				});
			}

			scimToScimRepresentationMock.Setup(s => s.ScimToSCIMRepresentation(It.IsAny<ScimBase>(), false)).ReturnsAsync(representation);

			var res = await scimController.PatchAsync("1", patchRepresentation);

			Assert.That(res, Is.Not.Null);
			Assert.That(res, Is.InstanceOf(typeof(StatusCodeResult)));
			Assert.That((res as StatusCodeResult).StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
		}

		public static IEnumerable<TestCaseData> PatchAsyncExceptions
		{
			get
			{
				yield return new TestCaseData(new SCIMDuplicateAttributeException(""), HttpStatusCode.NoContent, SCIMConstants.ErrorSCIMTypes.Uniqueness) { TestName = "{m}_DuplicateAttributeException" };
				yield return new TestCaseData(new SCIMUniquenessAttributeException(""), HttpStatusCode.Conflict, SCIMConstants.ErrorSCIMTypes.Uniqueness) { TestName = "{m}_UniquenessAttributeException" };
				yield return new TestCaseData(new SCIMFilterException(""), HttpStatusCode.BadRequest, SCIMConstants.ErrorSCIMTypes.InvalidFilter) { TestName = "{m}_FilterException" };
				yield return new TestCaseData(new SCIMBadSyntaxException(""), HttpStatusCode.BadRequest, SCIMConstants.ErrorSCIMTypes.InvalidSyntax) { TestName = "{m}_BadSyntaxException" };
				yield return new TestCaseData(new SCIMNoTargetException(""), HttpStatusCode.NotFound, SCIMConstants.ErrorSCIMTypes.NoTarget) { TestName = "{m}_NotTargetException" };
				yield return new TestCaseData(new SCIMNotFoundException(""), HttpStatusCode.NotFound, SCIMConstants.ErrorSCIMTypes.Unknown) { TestName = "{m}_NotFoundException" };
				yield return new TestCaseData(new Exception(""), HttpStatusCode.InternalServerError, SCIMConstants.ErrorSCIMTypes.InternalServerError) { TestName = "{m}_Exception" };
			}
		}

		[TestCaseSource(nameof(PatchAsyncExceptions))]
		public async Task TestPatchAsyncThrowsException(Exception ex, HttpStatusCode statusCode, string scimType)
		{
			persistanceRepository.Setup(u => u.PatchSCIMResourceById(It.IsAny<string>(), It.IsAny<PatchRepresentationParameter>())).ThrowsAsync(ex);

			var jobj = GetRepresentationParameter();
			var patchRepresentation = GetPatchRepresentation();

			var res = await scimController.PatchAsync("1", patchRepresentation);
			var httpRes = await res.ExecuteAsync(CancellationToken.None);
			var responseBody = await httpRes.Content.ReadAsStringAsync();

			Assert.That(res, Is.TypeOf(typeof(ScimErrorResult)));
			var jToken = Utils.ExtractJsonFromBody(responseBody);
			Assert.That(httpRes.StatusCode, Is.EqualTo(statusCode));
			Assert.That(jToken.GetTokenAsString("scimType"), Is.EqualTo(scimType));
		}

		public static IEnumerable<TestCaseData> DeleteAsyncExceptions
		{
			get
			{
				yield return new TestCaseData(new SCIMNotFoundException(""), 404, SCIMConstants.ErrorSCIMTypes.Unknown) { TestName = "{m}_NotFoundException" };
				yield return new TestCaseData(new Exception(""), 500, SCIMConstants.ErrorSCIMTypes.InternalServerError) { TestName = "{m}_Exception" };
			}
		}

		[TestCaseSource(nameof(DeleteAsyncExceptions))]
		public async Task TestDeleteAsyncThrowsException(Exception ex, HttpStatusCode statusCode, string scimType)
		{
			persistanceRepository.Setup(u => u.DeleteSCIMResourceById(It.IsAny<string>())).ThrowsAsync(ex);

			var res = await scimController.DeleteAsync("1");
			var httpRes = await res.ExecuteAsync(CancellationToken.None);
			var responseBody = await httpRes.Content.ReadAsStringAsync();

			Assert.That(res, Is.TypeOf(typeof(ScimErrorResult)));
			var jToken = Utils.ExtractJsonFromBody(responseBody);
			Assert.That(httpRes.StatusCode, Is.EqualTo(statusCode));
			Assert.That(jToken.GetTokenAsString("schemas[0]"), Is.EqualTo("urn:ietf:params:scim:api:messages:2.0:Error"));
			Assert.That(jToken.GetTokenAsString("scimType"), Is.EqualTo(scimType));
		}

		[Test]
		public async Task TestDeleteAsync()
		{
			var res = await scimController.DeleteAsync("1");

			Assert.That(res, Is.Not.Null);
			Assert.That(res, Is.InstanceOf(typeof(StatusCodeResult)));
			Assert.That((res as StatusCodeResult).StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
		}

		[SetUp]
		public void Init()
		{
			var basePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "Schemas");
			var userSchema = SCIMSchemaExtractor.Extract(Path.Combine(basePath, "UserSchema.json"), SCIMResourceTypes.User, true);
			var groupSchema = SCIMSchemaExtractor.Extract(Path.Combine(basePath, "GroupSchema.json"), SCIMResourceTypes.Group, true);
			var cwUserSchema = SCIMSchemaExtractor.Extract(Path.Combine(basePath, "CargoWiseUserSchema.json"), SCIMResourceTypes.User, true);
			var cwGroupSchema = SCIMSchemaExtractor.Extract(Path.Combine(basePath, "CargoWiseGroupSchema.json"), SCIMResourceTypes.Group, true);

			userSchema.SchemaExtensions.Add(new SCIMSchemaExtension
			{
				Id = Guid.NewGuid().ToString(),
				Schema = "urn:ietf:params:scim:schemas:extension:cw:2.0:User"
			});

			groupSchema.SchemaExtensions.Add(new SCIMSchemaExtension
			{
				Id = Guid.NewGuid().ToString(),
				Schema = "urn:ietf:params:scim:schemas:extension:cw:2.0:Group"
			});

			var schemas = new List<SCIMSchema>
			{
				userSchema,
				groupSchema,
				cwUserSchema,
				cwGroupSchema
			};

			var schemaQueryRepository = new DefaultSchemaQueryRepository(schemas);
			persistanceRepository = new Mock<IPersistanceRepository<T>>();
			uriProviderMock = new Mock<Api.Helpers.IUriProvider>();
			resourceTypeResolverMock = new Mock<IResourceTypeResolver>();
			scimRepresentationConverterMock = new Mock<IScimRepresentationConverter<T>>();
			scimToScimRepresentationMock = new Mock<IScimToScimRepresentation>();
			attributeReferenceEnricherMock = new Mock<IAttributeReferenceEnricher>();
			scimRepresentationHelperMock = new Mock<ISCIMRepresentationHelper>();
			optionsMonitorMock = new Mock<SCIMHostOptions>();
			scimToScimRepresentationMock.Setup(s => s.ScimToSCIMRepresentation(It.IsAny<T>(), true)).ReturnsAsync(new SCIMRepresentation { ResourceType = ReturnResourceType(), Version = 1 });

			scimRepresentationConverterMock.Setup(s => s.JsonToScim(It.IsAny<string>())).Returns(new T());
			resourceTypeResolverMock.Setup(r => r.ResolveByResourceType(It.IsAny<string>())).Returns(new ResourceTypeResolutionResult() { ControllerName = "Users" });

			scimController = new TestableScimController<T>(persistanceRepository.Object, scimRepresentationConverterMock.Object, scimToScimRepresentationMock.Object, schemaQueryRepository,
				resourceTypeResolverMock.Object, uriProviderMock.Object, attributeReferenceEnricherMock.Object, optionsMonitorMock.Object, scimRepresentationHelperMock.Object);
		}

		static PatchRepresentationParameter GetPatchRepresentation()
		{
			var patchRepresentation = new PatchRepresentationParameter();
			if (typeof(ScimUser) == typeof(T))
			{
				patchRepresentation.Schemas = new List<string> { "urn:ietf:params:scim:schemas:core:2.0:User" };
			}

			if (typeof(ScimGroup) == typeof(T))
			{
				patchRepresentation.Schemas = new List<string> { "urn:ietf:params:scim:schemas:core:2.0:Group" };
			}

			return patchRepresentation;
		}

		static RepresentationParameter GetRepresentationParameter()
		{
			if (typeof(ScimUser) == typeof(T))
			{
				return new RepresentationParameter()
				{
					ExternalId = "1",
					Schemas = new List<string> { "urn:ietf:params:scim:schemas:core:2.0:User" },
					Attributes = new JObject()
				};
			}

			if (typeof(ScimGroup) == typeof(T))
			{
				return new RepresentationParameter()
				{
					ExternalId = "1",
					Schemas = new List<string> { "urn:ietf:params:scim:schemas:core:2.0:Group" },
					Attributes = new JObject()
				};
			}

			throw new ArgumentException();
		}

		string ReturnResourceType() => typeof(T) == typeof(ScimUser) ? SCIMResourceTypes.User : SCIMResourceTypes.Group;
	}
}
