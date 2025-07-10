using CargoWise.EntityFramework;
using Enterprise.Customs.NZ.Business.MAFeBACCa.CodedLists;

namespace Enterprise.Customs.NZ.Business.MAFeBACCa
{
	public class MAFMessagingBOLookups : ZLookups
	{
		public MAFMessagingBOLookups(MAFMessagingBO parent)
			: base(parent)
		{
		}

		public CargoTypeList CargoTypes
		{
			get { return Factory.GetCachedValue<CargoTypeList>(); }
		}

		public YesNoUnknownList YesNoUnknowns
		{
			get { return Factory.GetCachedValue<YesNoUnknownList>(); }
		}

		public MeasurementUQList MeasurementUQs
		{
			get { return Factory.GetCachedValue<MeasurementUQList>(); }
		}

		public MAFProcessingOfficeList MAFProcessingOffices
		{
			get { return Factory.GetCachedValue<MAFProcessingOfficeList>(); }
		}

		public MAFPaymentMethodList MAFPaymentMethods
		{
			get { return Factory.GetCachedValue<MAFPaymentMethodList>(); }
		}

		public ConsignmentTypeList ConsignmentTypes
		{
			get { return Factory.GetCachedValue<ConsignmentTypeList>(); }
		}

		public MessagingStatusList MessagingStatuses
		{
			get { return Factory.GetCachedValue<MessagingStatusList>(); }
		}

		public MsgTransportList MsgTransModeList
		{
			get { return Factory.GetCachedValue<MsgTransportList>(); }
		}

		public ConsolIPIStatusList TSW_IPIMessagingStatuses
		{
			get { return Factory.GetCachedValue<ConsolIPIStatusList>(); }
		}
	}
}
