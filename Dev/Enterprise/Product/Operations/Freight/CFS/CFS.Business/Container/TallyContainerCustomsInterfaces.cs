using CargoWise.Types;

namespace Enterprise.Freight.CFS.Business
{
	public interface IOutturnLinkable
	{
		void SetOutturnLink(IOutturnLink listener);
	}

	public interface IOutturnProvider
	{
		IOutturn GetOutturnFor(PackUnpackShipment shipment);
	}

	public interface IOutturnLink : IOutturnProvider
	{
		ZDateTime ReceiptDate { get; set; }
		void SetSealNumber(ZString sealNumber);
		void SetSealIntact(ZBool sealIntact);
	}

	public interface IOutturn
	{
		ZInt NumberOfPackages { get; }
		void SetPackageType(ZString packageType);
		ZString MarksAndNumbers { get; }

		void SetPackagesOutturned(ZInt packagesOutturned);
		void SetPillaged(ZBool pillaged);
		void SetDamaged(ZBool damaged);

		ZDateTime UnpackDate { get; set; }

		ZBool IsDeleted { get; }
	}
}
