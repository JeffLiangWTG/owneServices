using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	public class DummyIInBondMessageSendingWP : DummyBusinessObject, IInbondMessageSendingData
	{
		public DummyIInBondMessageSendingWP(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
		{
		}

		public bool IsActive
		{
			get { return true; }
		}

		#region IInbondMessageSendingData Members

		public ZString DiversionPortCode
		{
			get { return diversionPortCode; }
			set { diversionPortCode = value; }
		}
		ZString diversionPortCode;

		public ZDateTime DiversionDateTime
		{
			get { return diversionDateTime; }
			set { diversionDateTime = value; }
		}
		ZDateTime diversionDateTime;

		public ZString DiversionInBondCarrierCode
		{
			get { return carrierCode; }
			set { carrierCode = value; }
		}
		ZString carrierCode;

		public ZString DiversionBondedCarrierID
		{
			get { return carrierID; }
			set { carrierID = value; }
		}
		ZString carrierID;

		ZString IInbondMessageSendingData.PortCode
		{
			get { return this.DiversionPortCode; }
		}

		ZDateTime IInbondMessageSendingData.DiversionDateTime
		{
			get { return this.DiversionDateTime; }
		}

		ZString IInbondMessageSendingData.InBondCarrierCode
		{
			get { return this.DiversionInBondCarrierCode; }
		}

		ZString IInbondMessageSendingData.BondedCarrierID
		{
			get { return this.DiversionBondedCarrierID; }
		}

		#endregion
	}
}
