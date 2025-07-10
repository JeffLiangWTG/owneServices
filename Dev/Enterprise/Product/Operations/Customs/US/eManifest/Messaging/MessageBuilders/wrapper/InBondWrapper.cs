using CargoWise.Types;
using Enterprise.Customs.US.eManifest.Business;

namespace Enterprise.Customs.US.eManifest.Messaging
{
	internal class InBondWrapper : IInBond
	{
		public InBondWrapper(InBond inBond)
		{
			this.inBond = inBond;
		}

		#region Implementation of IInBond

		public ZString InbondType
		{
			get { return inBond.BM_InBondEntryType; }
		}

		public ZString InbondDestination
		{
			get { return inBond.BM_DestinationPortCode; }
		}

		public ZString OnwardCarrier
		{
			get { return inBond.BM_OnwardCarrier; }
		}

		public ZString BondedCarrier
		{
			get { return inBond.BM_InBondCarrierID; }
		}

		public ZString Inbond7512Number
		{
			get { return inBond.InBondNumber; }
		}

		public ZString TransferCarrier
		{
			get { return inBond.BM_TransferCarrier; }
		}

		public ZString ForeignPortOfDestination
		{
			get { return inBond.BM_ForeignDestPortKCode; }
		}

		public ZString ForeignPortOfDestinationCodeType
		{
			get { return PortCodeTypes.Codes.ScheduleK; }
		}

		public ZDate EstimatedDateOfUSExit
		{
			get { return inBond.BM_ExportDate.Date; }
		}

		public ZString MexicanPedimentoNumber
		{
			get { return inBond.BM_PedimentoNumber; }
		}

		#endregion

		readonly InBond inBond;
	}
}
