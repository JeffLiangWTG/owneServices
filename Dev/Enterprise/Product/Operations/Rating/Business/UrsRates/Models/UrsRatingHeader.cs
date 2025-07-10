#nullable enable
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Rating.Business;

public class UrsRatingHeader : WiseHeader
{
	UrsCarrier serviceProvider = null!;
	public UrsCarrier ServiceProvider
	{
		get => serviceProvider;
		set
		{
			serviceProvider = value;
			TH_OH = value.OrgHeader?.PK ?? ZGuid.Empty;
		}
	}

	public UrsRatingHeader(BusinessObjectFactory factory, UrsCarrier serviceProvider) : base(factory)
	{
		ServiceProvider = serviceProvider;
	}
}
