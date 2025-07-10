using CargoWise.Types;

namespace Enterprise.Customs.US.AMS.Messaging.Interface
{
	public interface IEntity : IEntityAddress
	{
		ZString EntityCode { get; }
		ZString EntityName { get; }
		ZString CodeQualifier { get; }
		ZString IDCode { get; }
	}

	public interface IEntityAddress
	{
		ZString AddressLine1 { get; }
		ZString AddressLine2 { get; }
		ZString AddressLine1Part2 { get; }
		ZString AddressLine2Part2 { get; }
		ZString CityName { get; }
		ZString StateProvince { get; }
		ZString PostalCode { get; }
		ZString CountryCode { get; }

		INotifyPartyContact AdminContact { get; }
	}
}
