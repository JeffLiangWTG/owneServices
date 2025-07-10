namespace Enterprise.Customs.Business.MessageBuilders.eManifest
{
	using CargoWise.Types;

	public interface IParty : IAddress
	{
		/// <summary>
		/// Whether the Party is Consignee/Shipper/Importer etc. (M/3)
		/// </summary>
		ZString PartyType { get; }

		/// <summary>(M/40)</summary>
		ZString PartyName { get; }

		/// <summary>(O/40)</summary>
		ZString Phone { get; }

		/// <summary>(O/255)</summary>
		ZString Email { get; }
	}
}
