namespace Enterprise.Customs.Business.MessageBuilders.eManifest
{
	using CargoWise.Types;

	public interface IAddress
	{
		/// <summary>(M/105)</summary>
		ZString Address { get; }

		/// <summary>(M/35)</summary>
		ZString City { get; }

		/// <summary>(M/3)</summary>
		ZString StateOrProvince { get; }

		/// <summary>(M/3)</summary>
		ZString Country { get; }

		/// <summary>(M/10)</summary>
		ZString Postcode { get; }

		ZBool IsEmpty { get; }
	}
}
