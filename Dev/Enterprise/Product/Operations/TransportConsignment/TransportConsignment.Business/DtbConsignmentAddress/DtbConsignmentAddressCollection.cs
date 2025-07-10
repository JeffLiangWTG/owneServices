using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportConsignment.Business
{
	public class DtbConsignmentAddressCollection : ActiveBusinessObjectCollection<DtbConsignmentAddress>
	{
		public DtbConsignmentAddressCollection(DtbConsignment consignment)
			: base(consignment.Factory, consignment, null, DtbConsignmentAddressSchema.LTS_LTC_Consignment)
		{
		}

		#region AddNew

		public DtbConsignmentAddress AddNew(ZString instructionType)
		{
			var address = AddNew();
			address.LTS_InstructionType = instructionType;

			return address;
		}

		#endregion

	}
}
