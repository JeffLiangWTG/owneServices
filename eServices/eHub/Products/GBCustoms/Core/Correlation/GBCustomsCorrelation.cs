using CargoWise.eHub.DataAccess.Integration;
using CargoWise.eHub.DataAccess.Sql;
using CargoWise.eHub.Products.GBCustoms.Core.Correlation.Handlers;

using Common.Logging;

using System;
using System.Collections.Generic;

namespace CargoWise.eHub.Products.GBCustoms.Core.Correlation
{
    public class GBCustomsCorrelation
    {
        private ProviderType Provider;
        private string Service;
        private GBCustomsSource Source;
        private string Content;
        private readonly IClientRegistrationAccessor ClientRegistration;
        private readonly ILog Logger;
        private readonly bool IsProd;

        private const string GBCClientRegistrationType = "GBCustoms-Transport";

        public List<GBCustomsCorrelationIdentifier> GBCustomsCorrelationIdentifiers { get; protected set; }

        public GBCustomsCorrelation(ILog logger, bool isProd, List<GBCustomsCorrelationIdentifier> correlationIdentifiers)
        {
            Logger = logger;
            IsProd = isProd;
            GBCustomsCorrelationIdentifiers = correlationIdentifiers;
        }

        public GBCustomsCorrelation(ILog logger, ProviderType providerType, string service, GBCustomsSource source, string content, bool isProd, IClientRegistrationAccessor clientRegistrationAccessor = null)
        {
            Logger = logger;
            ClientRegistration = clientRegistrationAccessor ?? new ClientRegistrationAccessor();
            IsProd = isProd;
            Initialise(providerType, service, source, content);
        }

        public GBCustomsCorrelation(ILog logger, string provider, string service, string source, string content, bool isProd, IClientRegistrationAccessor clientRegistrationAccessor = null)
        {
            Logger = logger;
            var providerParsed = ParseProvider(provider);
            var sourceParsed = ParseSource(source);
            ClientRegistration = clientRegistrationAccessor ?? new ClientRegistrationAccessor();
            IsProd = isProd;
            Initialise(providerParsed, service, sourceParsed, content);
        }

        private void Initialise(ProviderType provider, string service, GBCustomsSource source, string content)
        {
            Provider = provider;
            Service = service;
            Source = source;
            Content = content;

            Logger.Info($"GBCustomsCorrelation - Getting the correlation identifier(s) for Provider {provider.ConvertToString()}, Service {service ?? "NULL"}, Source {source.ConvertToString()}, Content {content ?? "NULL"}.");

            GBCustomsCorrelationIdentifiers = GetCorrelations();

            var identifierCount = GBCustomsCorrelationIdentifiers.Count;
            var valueCount = GBCustomsCorrelationIdentifiers.FindAll(x => !string.IsNullOrEmpty(x.Value)).Count;
            if (identifierCount > 0)
            {
                var identifierList = "";
                GBCustomsCorrelationIdentifiers.ForEach(x => identifierList += $"Name:{x.Name}, Type:{x.Type.ConvertToString()}, Value:{x.Value}.{Environment.NewLine}");
                Logger.Info($"GBCustomsCorrelation - Found {identifierCount} identifier(s). Successfully extracted {valueCount} value(s).");
                Logger.Info($"GBCustomsCorrelation - {identifierList}");
            }
            else
            {
                Logger.Warn("GBCustomsCorrelation - No identifier could be found");
            }
        }

        private ProviderType ParseProvider(string provider)
        {
            var providerParsed = provider.ConvertToEnum<ProviderType>();
            if (providerParsed == ProviderType.Unknown)
            {
                Logger.Error($"GBCustomsCorrelation - Provider {provider ?? "NULL"} is invalid");
                throw new ArgumentException($"Provider {provider ?? "NULL"} is invalid");
            }
            return providerParsed;
        }

        private GBCustomsSource ParseSource(string source)
        {
            var sourceParsed = source.ConvertToEnum<GBCustomsSource>();
            if (sourceParsed == GBCustomsSource.Unknown)
            {
                Logger.Error($"GBCustomsCorrelation - Source {source ?? "NULL"} is invalid");
                throw new ArgumentException($"Source {source ?? "NULL"} is invalid");
            }
            return sourceParsed;
        }

        private List<GBCustomsCorrelationIdentifier> GetCorrelations()
        {
            var identifiers = new List<GBCustomsCorrelationIdentifier>();

            var registrations = ClientRegistration.ReadRegistrations(GetClientId(Provider.ConvertToString()), GBCClientRegistrationType, true, qualifier: $"{Service}%", flag1: (int?)Source);

            if (registrations == null)
            {
                Logger.Error($"GBCustomsCorrelation - Could not find a valid ClientRegistraion record for Client [{GetClientId(Provider.ConvertToString())}],  RegistrationType [{GBCClientRegistrationType}], Qualifier [{Service}], Flag1 [{Source}]");
                throw new NullReferenceException("Could not find a valid ClientRegistration record for CorrelationID extraction");
            }

            foreach (var registration in registrations)
            {
                var typeAndPath = registration["CX_Code"].ToString();
                var identifierName = registration["CX_Attr1"].ToString();
                var identifierType = registration["CX_Flag2"].ToString().ConvertToEnum<CorrelationType>();
                if (identifierType == CorrelationType.Unknown)
                {
                    Logger.Warn($"GBCustomsCorrelation - Could not find a CorrelationType from string '{registration["CX_Flag2"]}'");
                }

                var typeAndPathSplit = typeAndPath.Split(':');
                var extractionTypeString = typeAndPathSplit[0].Trim();

                var path = "";
                if (typeAndPathSplit.Length > 1)
                {
                    path = typeAndPathSplit[1].Trim();
                }
                else
                {
                    Logger.Warn($"GBCustomsCorrelation - Could not find a path from CX_Code Value [{typeAndPath}]");
                }

                var extractionType = extractionTypeString.ConvertToEnum<ExtractionType>();
                if (extractionType != ExtractionType.Unknown)
                {
                    var handler = GetExtractionHandler(extractionType);
                    var identifierValue = handler.Extract(Logger, Content, path);

                    var identifier = new GBCustomsCorrelationIdentifier
                    {
                        Name = identifierName,
                        Type = identifierType,
                        Value = identifierValue
                    };
                    identifiers.Add(identifier);
                }
                else
                {
                    Logger.Warn($"GBCustomsCorrelation - Could not find an ExtractionType from string '{extractionTypeString}'");
                }
            }

            return identifiers;
        }

        private IExtractionHandler GetExtractionHandler(ExtractionType extractionType)
        {
            switch (extractionType)
            {
                case ExtractionType.XMLBody:
                    Logger.Info("GBCustomsCorrelation - Using XMLBodyExtractionHandler");
                    return new XMLBodyExtractionHandler();
                case ExtractionType.JSONBody:
                    Logger.Info("GBCustomsCorrelation - Using JSONBodyExtractionHandler");
                    return new JSONBodyExtractionHandler();
                case ExtractionType.Header:
                    Logger.Info("GBCustomsCorrelation - Using HeaderExtractionHandler");
                    return new HeaderExtractionHandler();
                case ExtractionType.Parameter:
                    Logger.Info("GBCustomsCorrelation - Using ParameterExtractionHandler");
                    return new ParameterExtractionHandler();
                default:
                    Logger.Error($"GBCustomsCorrelation - ExtractionType {extractionType.ConvertToString()} is unhandled.");
                    throw new InvalidOperationException($"ExtractionType {extractionType.ConvertToString()} is unhandled.");
            }
        }

        private string GetClientId(string provider)
        {
            var prodTest = IsProd ? string.Empty : "Test";
            return $"GBCustoms{prodTest}-{provider}";
        }
    }
}
