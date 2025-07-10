using System;
using CargoWise.Types;
using Enterprise.Registry.Business;

namespace Enterprise.MasterFiles.Business.Testing;

public class DummyRefGlbReleaseNote
{
	public DummyRefGlbReleaseNote(string section, string summary, string url, string countryCode, ZDateTime publishedDate)
	{
		Section = section;
		Summary = summary;
		CountryCode = countryCode;
		Url = url;
		PublishedDate = publishedDate;

		if (section == NewsSectionTypeList.Codes.ProductUpdates)
		{
			MinVersion = "1.0.0";
		}
	}

	public Guid PK { get; } = Guid.NewGuid();

	public string Section { get; }

	public string Summary { get; }

	public string CountryCode { get; }

	public string Url { get; }

	public ZDateTime PublishedDate { get; }

	public bool IsRead { get; init; }

	public string MinVersion { get; init; } = string.Empty;
}
