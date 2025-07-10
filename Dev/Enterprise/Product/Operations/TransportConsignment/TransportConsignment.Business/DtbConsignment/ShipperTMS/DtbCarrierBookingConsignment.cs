using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportConsignment.Business
{
	[UserDefinedValues]
	[CodeProperty(DtbConsignmentSchema.Constants.LTC_JobID)]
	[DescriptionProperty(DtbConsignmentSchema.Constants.LTC_ConnoteNumber)]
	[UniversalDataContext(DataContextType.CarrierBookingConsignment)]
	public class DtbCarrierBookingConsignment : DtbConsignment
	{
		public DtbCarrierBookingConsignment(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region SetDefaultValues

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			LTC_ConsignmentType = DtbConsignmentTypes.Codes.CarrierBookingConsignment;
			LTC_Direction = Core.Constants.CartageDirection.Local;
		}

		#endregion

	}
}
