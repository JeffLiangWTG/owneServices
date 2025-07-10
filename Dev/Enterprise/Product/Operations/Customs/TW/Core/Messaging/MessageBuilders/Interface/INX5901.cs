using CargoWise.Types;

namespace Enterprise.Customs.TW.Messaging
{
	public interface INX5901Declaration
	{
		ZString FunctionalReferenceID { get; }

		ZString ID { get; }

		ZString TypeCode { get; }

		IAdditionalDocument AdditionalDocument { get; }

		ZString ContactOffice { get; }

		IGoodsShipment GoodsShipment { get; }

		IGovernmentProcedure GovernmentProcedure { get; }

		IPreviousDocument PreviousDocument { get; }

		ZString ResponsibleGovernmentAgency { get; }
	}
}
