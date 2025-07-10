using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Freight.Business
{
	public class JobAddressAdditionalInfo : AutoJobAddressAdditionalInfo, IJobAddressAdditionalInfo
	{
		public JobAddressAdditionalInfo(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public ZString TransportMode { get => JAI_TransportMode; set => JAI_TransportMode = value; }
		public ZString AddressType { get => JAI_AddressType; set => JAI_AddressType = value; }
		public DocAddressType DocAddressType => DocAddressTypes.GetDocAddressTypeFromCode(Factory, JAI_AddressType);
	}
}
