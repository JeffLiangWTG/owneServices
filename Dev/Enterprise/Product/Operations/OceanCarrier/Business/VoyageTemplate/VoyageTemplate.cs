using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.OceanCarrier.Business
{
	public sealed class VoyageTemplate : AutoVoyageTemplate
	{
		public VoyageTemplate(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
		public CarrierService CarrierService => Factory.Load<CarrierService>(VTV_CSV_Service);

		[RelatedBusinessObject(nameof(CarrierService))]
		public override ZGuid VTV_CSV_Service
		{
			get => base.VTV_CSV_Service;
			set => base.VTV_CSV_Service = value;
		}
	}
}
