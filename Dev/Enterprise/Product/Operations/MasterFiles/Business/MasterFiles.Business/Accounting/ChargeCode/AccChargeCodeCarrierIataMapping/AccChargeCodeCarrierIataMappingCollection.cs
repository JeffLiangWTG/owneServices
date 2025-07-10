using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class AccChargeCodeCarrierIataMappingCollection : ActiveBusinessObjectCollection<AccChargeCodeCarrierIataMapping>, IAccChargeCodeCollection
	{
		public AccChargeCodeCarrierIataMappingCollection(AccChargeCode accChargeCode)
			: base(accChargeCode.Factory, new DependentRelationship(accChargeCode, typeof(AccChargeCodeCarrierIataMapping), new ZQuery(AccChargeCodeCarrierIataMappingSchema.ACI_AC_ChargeCode, accChargeCode.PK), AccChargeCodeCarrierIataMappingSchema.ACI_AC_ChargeCode))
		{
		}
	}
}
