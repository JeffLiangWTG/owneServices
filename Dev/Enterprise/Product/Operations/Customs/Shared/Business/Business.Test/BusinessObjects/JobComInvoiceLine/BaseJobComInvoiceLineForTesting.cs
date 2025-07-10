using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	public class BaseJobComInvoiceLineForTesting : BaseJobComInvoiceLine
	{
		public BaseJobComInvoiceLineForTesting(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public override VehicleRelationshipType VehicleRelationship => VehicleRelationshipForTesting ?? base.VehicleRelationship;

		public VehicleRelationshipType? VehicleRelationshipForTesting { get; set; }

		public new ICusVehicleCollection<CusVehicle, BaseJobComInvoiceLineForTesting> Vehicles => (ICusVehicleCollection<CusVehicle, BaseJobComInvoiceLineForTesting>)base.Vehicles;

		protected override ICusVehicleCollection<CusVehicle, BaseJobComInvoiceLine> GetNewCusVehicleCollection() => new CusVehicleCollection<CusVehicle, BaseJobComInvoiceLineForTesting>(this);

		public override bool HasOutOfInwardProcessingProcedure => true;

		protected override ICustomsValuationCalculator GetValuationCalculatorCore() => new MockCustomsValuationCalculator();

		protected override ZString GetTariffDescription(ZString tariffCode)
		{
			return JI_Tariff.IsEmpty ? ZString.Empty : new ZString("TEST DESCRIPTION");
		}

		public void ResetCustomsUnitDefaultingStrategyExposed() => ResetCustomsUnitDefaultingStrategy();

		class MockCustomsValuationCalculator : ICustomsValuationCalculator
		{
			public ZDecimal GetAmountToAddToITOTForVatableGstable(RefCurrency currency) => 1000m;
			public ZDecimal GetAmountToAddToITOTForDutiable(RefCurrency currency) => 0m;
			public ZDecimal GetAmountToAddToITOTForStatistical(RefCurrency currency) => 0m;
		}
	}
}
