#nullable enable
using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Rating.CarrierConnect;
using Enterprise.Rating.CarrierConnect.RateSelection.Models;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Rating.GUI;

static class JobTypes
{
	public const string JobConsol = "JobConsol";
	public const string JobShipment = "JobShipment";
	public const string JobOneOffQuote = "JobOneOffQuote";
	public const string JobBookingWithQuote = "JobBookingWithQuote";
}

public class CreateAutoratedJobUrlHandler : UrlHandler
{
	public static CreateAutoratedJobUrlHandler Instance => instance ??= new ();
	[ThreadStatic]
	static CreateAutoratedJobUrlHandler? instance;

	protected override string ExpectedCommandText => "CreateAutoratedJob";

	protected override bool HandleCore(QueryString queryString)
	{
		var factory = new BusinessObjectFactory();

		if (!Guid.TryParse(queryString["Id"] ?? "", out var guid))
		{
			return false;
		}

		var rateSearchResult = factory.Load<RateSearchResult>(guid);
		if (rateSearchResult is null)
		{
			return false;
		}

		if (!rateSearchResult.TryGetDto<CreateJobDto>(out var dto))
		{
			return false;
		}

		var job = CreateJob(queryString["Job"] ?? "", dto);
		ZControllerFactory.Instance
			.GetControllerForBizo(job)
			.ShowFormForNewEntity(job);

		rateSearchResult.Delete();
		factory.Save();

		RateSearchResult.DeleteExpiredRecords();

		return true;
	}

	static BusinessObject CreateJob(string jobType, CreateJobDto dto) => jobType switch
	{
		JobTypes.JobConsol => new AutoratedConsolCreator(dto).CreateJob(),
		JobTypes.JobShipment => new AutoratedShipmentCreator(dto).CreateJob(),
		JobTypes.JobOneOffQuote => new AutoratedOneOffQuoteCreator(dto).CreateJob(),
		JobTypes.JobBookingWithQuote => new AutoratedBookingWithQuoteCreator(dto).CreateJob(),
		_ => throw new InvalidQueryStringException($"Invalid Job type {jobType}."),
	};
}
