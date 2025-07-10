using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	[CodeProperty(JobConsolTransportSchema.Constants.JW_VoyageFlight), DescriptionProperty(JobConsolTransportSchema.Constants.JW_VoyageFlight)]
	public class TransportNonDependent : Transport
	{
		public TransportNonDependent(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override Type GetParentTypeCore()
		{
			return typeof(CommonConsol);
		}

		protected override void SetIsDomestic()
		{
			this.SuspendValidation();
			base.SetIsDomestic();
		}
	}
}
