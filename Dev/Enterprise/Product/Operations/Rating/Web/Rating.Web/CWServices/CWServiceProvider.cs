using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.Billing.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.Rating;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Web.Model;
using Enterprise.Rating.Web.Model.Conversion;
using Enterprise.Rating.Web.Model.Validation;
using Enterprise.ZArchitecture.Schema;
using FluentValidation;
using WiseRatesModel = WiseRates.Api.Model;

namespace Enterprise.Rating.Web
{
	class CWServiceProvider : ICWServiceProvider
	{
		BusinessObjectFactory Factory { get; }
		public CWServiceProvider(BusinessObjectFactory factory = null)
		{
			Factory = factory ?? new ReadOnlyBusinessObjectFactory();
		}

		public IReadOnlyCollection<Rate> GetCosts(string userName, string branchCode, string departmentCode, RateQuery query, ILogger logger)
		{
			using (Db.DisposableActionForDbConnection())
			using (SetTemporaryUserContext(userName, branchCode, departmentCode))
			{
				ValidateRateQuery(query, SourceEndpoint.Costing);

				var rates = new List<IRateEntry>();

				rates.AddRange(GetCW1InternalCosts(query, logger));
				rates.AddRange(GetCW1ExternalProvidersCosts(query, logger));

				return new RateEntryToRateConverter(Factory).Convert(rates, RatingConstants.RatingHeaderTypes.Costing, logger);
			}
		}

		public IReadOnlyCollection<Rate> GetCompanyTariffs(string userName, string branchCode, string departmentCode, RateQuery query, ILogger logger)
		{
			using (Db.DisposableActionForDbConnection())
			using (SetTemporaryUserContext(userName, branchCode, departmentCode))
			{
				ValidateRateQuery(query, SourceEndpoint.CompanyTariffs);

				var cw1Rates = new SimpleRateEntryCollection(Factory);

				var queryToLoadCW1Rates = new RateQueryToZQueryConverter(Factory, logger).Convert(Env.CurrentCompanyPK, query, SourceEndpoint.CompanyTariffs);
				if (queryToLoadCW1Rates != null)
				{
					cw1Rates.Load(queryToLoadCW1Rates);
				}

				return new RateEntryToRateConverter(Factory).Convert(cw1Rates.Select(r => r), RatingConstants.RatingHeaderTypes.Tariff, logger);
			}
		}

		public IReadOnlyCollection<Rate> GetIntercompanyTariffs(string userName, string branchCode, string departmentCode, RateQuery query, ILogger logger)
		{
			using (Db.DisposableActionForDbConnection())
			using (SetTemporaryUserContext(userName, branchCode, departmentCode))
			{
				ValidateRateQuery(query, SourceEndpoint.IntercompanyTariffs);

				var cw1Rates = new SimpleRateEntryCollection(Factory);

				var queryToLoadCW1Rates = new RateQueryToZQueryConverter(Factory, logger).Convert(Env.CurrentCompanyPK, query, SourceEndpoint.IntercompanyTariffs);
				if (queryToLoadCW1Rates != null)
				{
					cw1Rates.Load(queryToLoadCW1Rates);
				}

				return new RateEntryToRateConverter(Factory).Convert(cw1Rates.Select(r => r), RatingConstants.RatingHeaderTypes.IntercompanyTariff, logger);
			}
		}

		public IReadOnlyCollection<Rate> GetClientRates(string userName, string branchCode, string departmentCode, RateQuery query, ILogger logger)
		{
			using (Db.DisposableActionForDbConnection())
			using (SetTemporaryUserContext(userName, branchCode, departmentCode))
			{
				ValidateRateQuery(query, SourceEndpoint.ClientRates);

				var cw1Rates = new SimpleRateEntryCollection(Factory);

				var queryToLoadCW1Rates = new RateQueryToZQueryConverter(Factory, logger).Convert(Env.CurrentCompanyPK, query, SourceEndpoint.ClientRates);
				if (queryToLoadCW1Rates != null)
				{
					cw1Rates.Load(queryToLoadCW1Rates);
				}

				return new RateEntryToRateConverter(Factory).Convert(cw1Rates.Select(r => r), RatingConstants.RatingHeaderTypes.ClientRate, logger);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Logging message")]
		public IReadOnlyCollection<Rate> GetJobCharges(string userName, string branchCode, string departmentCode, RateQuery query, ILogger logger, RatesAPIsAutoRater ratesAPIAutoRater = null)
		{
			using (Db.DisposableActionForDbConnection())
			using (SetTemporaryUserContext(userName, branchCode, departmentCode))
			{
				ValidateRateQuery(query, SourceEndpoint.JobCharges);

				try
				{
					var autoRater = ratesAPIAutoRater
						?? new RatesAPIsAutoRater();

					return autoRater.AutoRate(Factory, query, logger);
				}
				catch (AutoRaterException ex)
				{
					logger.Error("An exception occured during Autorating: " + ex.Message);
					return Array.Empty<Rate>();
				}
			}
		}

		public void ReportUsage(string userName, string branchCode, string departmentCode, SourceEndpoint endpoint, HttpStatusCode status, IEnumerable<Rate> rates, TimeSpan processTime)
		{
			using (Db.DisposableActionForDbConnection())
			using (SetTemporaryUserContext(userName, branchCode, departmentCode))
			{
				var properties = new List<(string name, object value)>();
				properties.Add((UsageProperties.RequestUrl, endpoint.ToString()));
				properties.Add((UsageProperties.StatusCode, ((int)status).ToString()));
				properties.Add((UsageProperties.RateEntriesCount, rates.Count().ToString()));
				properties.Add((UsageProperties.RateLinesCount, rates.Sum(rate => rate.Charges.Length).ToString()));
				properties.Add((UsageProperties.ElapsedTime, (long)processTime.TotalMilliseconds));

				try
				{
					UsageReporter(properties);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					var message = $"Could not report usage. Error: {ex.Message}";
					ErrorReporter.ReportOnce(message, ex);
				}
			}
		}

		internal Action<List<(string name, object value)>> UsageReporter = properties => UsageCollector.Report(UsageFeatures.Codes.RatesAPI, properties.ToArray());

		IEnumerable<IRateEntry> GetCW1InternalCosts(RateQuery query, ILogger logger)
		{
			var cw1Rates = new SimpleRateEntryCollection(Factory);

			var queryToLoadCW1Rates = new RateQueryToZQueryConverter(Factory, logger).Convert(Env.CurrentCompanyPK, query, SourceEndpoint.Costing);
			if (queryToLoadCW1Rates != null)
			{
				cw1Rates.Load(queryToLoadCW1Rates);
			}

			return cw1Rates.Select(r => r);
		}

		IEnumerable<IRateEntry> GetCW1ExternalProvidersCosts(RateQuery query, ILogger logger)
		{
			var wiseRatesQuery = new RateQueryToRateServiceQueryConverter(Factory, logger).Convert(query, out var criteria);

			if (wiseRatesQuery != null && (query.RateProviders?.Where(rp => RatesAPIsConstants.RateProviders.CGCS.Equals(rp, StringComparison.InvariantCultureIgnoreCase))?.Any() ?? false))
			{
				// For now, when Urs is enabled, we will still use WiseRatesProvider (WiseRates & URS conversion).
				// ToDo: We will support UrsRatesProvider in WF5 of WI00882761
				WiseRatesProvider wiseProvider = null;
				var externalProvider = RatingContext.CreateWRProviderIfAllowed(logger, Factory);

				if (externalProvider is WiseRatesProvider existingWiseProvider)
				{
					wiseProvider = existingWiseProvider;
				}
				else if (externalProvider != null)
				{
					wiseProvider = new WiseRatesProvider(Factory, ObjectFactory.Get<IWiseRatesClientFactory>(), logger);
				}

				if (wiseProvider != null)
				{
					return wiseProvider.GetRates(wiseRatesQuery, criteria, WiseRatesModel.RatesSearchRequest.Operation.WiseRateSearch, out _);
				}
			}

			return Enumerable.Empty<IRateEntry>();
		}

		IDisposable SetTemporaryUserContext(string userName, string branchCode, string departmentCode)
		{
			var staff = GetUserInformation(userName);

			Guid branchPK, departmentPK;

			if (string.IsNullOrEmpty(branchCode) || string.IsNullOrEmpty(departmentCode))
			{
				branchPK = staff?.HomeBranch?.PK.ToGuid() ?? throw new InvalidBranchException();
				departmentPK = staff?.HomeDepartment?.PK.ToGuid() ?? throw new InvalidDepartmentException();
			}
			else
			{
				branchPK = GetBranchPK(branchCode) ?? throw new InvalidBranchException(branchCode);
				departmentPK = GetDepartmentPK(departmentCode) ?? throw new InvalidDepartmentException(departmentCode);
			}

			return Env.SetTemporaryUserContext(staff?.PK.ToGuid() ?? Guid.Empty, branchPK, departmentPK);
		}

		GlbStaff GetUserInformation(string userName)
		{
			return GlbStaff.LoadFromLoginName(Factory, userName);
		}

		Guid? GetBranchPK(string branchCode)
		{
			return Factory.LoadFromNaturalKey<GlbBranch>(GlbBranchSchema.GB_Code, branchCode)?.PK.ToGuid();
		}

		Guid? GetDepartmentPK(string departmentCode)
		{
			return Factory.LoadFromNaturalKey<GlbDepartment>(GlbDepartmentSchema.GE_Code, departmentCode)?.PK.ToGuid();
		}

		void ValidateRateQuery(RateQuery rateQuery, SourceEndpoint sourceEndpoint)
		{
			var direction = ImportExportHelper.GetJobDirection(rateQuery.Origin.Value, rateQuery.Destination.Value);

			var validator = new RateQueryValidator(sourceEndpoint, direction);
			validator.ValidateAndThrow(rateQuery);
		}
	}
}
