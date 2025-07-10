using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public class CusTransportMeans : AutoCusTransportMeans
	{
		public CusTransportMeans(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[List(nameof(Lookups) + "." + nameof(CusTransportMeansLookups.TransportStateList))]
		public override ZString TPM_TransportState
		{
			get => base.TPM_TransportState;
			set => base.TPM_TransportState = value;
		}
	}
}
