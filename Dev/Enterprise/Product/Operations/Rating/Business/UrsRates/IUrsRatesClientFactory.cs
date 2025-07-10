#nullable enable
using System;
using System.Threading;
using Enterprise.Integration;
using Urs.Api.Integration;

namespace Enterprise.Rating.Business;

public interface IUrsRatesClientFactory
{
	IUrsClient? TryCreate(string transportMode, string containerMode, string correlationID, ILogger logger, TimeSpan secondsBeforeTokenExpiry, CancellationToken cancellationToken);
}

