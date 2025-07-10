using System;
using Enterprise.Services.Scim.Api.Controllers;
using Enterprise.Services.Scim.Api.Helpers;
using Enterprise.Services.Scim.Contracts;
using Enterprise.Services.Scim.Helpers;
using Enterprise.Services.Scim.Models;
using SimpleIdServer.Scim;
using SimpleIdServer.Scim.Helpers;
using SimpleIdServer.Scim.Persistence;

namespace Enterprise.Services.Scim.Api.Test.Controllers
{
	internal class TestableScimController<T> : ScimController<T> where T : ScimBase
	{
		public TestableScimController(IPersistanceRepository<T> persistanceRepository, IScimRepresentationConverter<T> scimRepresentationConverter, IScimToScimRepresentation scimToScimRepresentation, ISCIMSchemaQueryRepository scimSchemaQueryRepository, IResourceTypeResolver resourceTypeResolver, Helpers.IUriProvider uriProvider, IAttributeReferenceEnricher attributeReferenceEnricher, SCIMHostOptions options, ISCIMRepresentationHelper scimRepresentationHelper) : base(persistanceRepository, scimRepresentationConverter, scimToScimRepresentation, scimSchemaQueryRepository, resourceTypeResolver, uriProvider, attributeReferenceEnricher, options, scimRepresentationHelper)
		{
		}

		protected override IDisposable DisposableActionForDbConnection()
		{
			return new MockDisposable();
		}
	}

	internal class MockDisposable : IDisposable
	{
		bool _disposed;

		protected virtual void Dispose(bool disposing)
		{
			if (!_disposed) // only dispose once!
			{
				if (disposing)
				{
					// Not in destructor, OK to reference other objects
				}
			}
			_disposed = true;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		~MockDisposable()
		{
			Dispose(false);
		}
	}
}
