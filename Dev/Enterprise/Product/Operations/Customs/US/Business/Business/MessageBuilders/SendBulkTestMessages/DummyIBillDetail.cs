using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.MessageBuilders
{
	public class DummyIBillDetail : IBillDetails
	{
		#region IEntryBillDetails Members

		public ZInt PackageQuantity
		{
			get { return 1; }
		}

		public ZString PackageType
		{
			get { return "PK"; }
		}

		#endregion

		#region IBillDetails Members

		public ZString ITNumber
		{
			get { return ZString.Empty; }
		}

		public ZDate ITDate
		{
			get { return ZDate.Empty; }
		}

		public ZString MasterBillNumber
		{
			get { return "OBL123"; }
		}

		public ZString IssuerCodeOfMasterBillNumber
		{
			get { return "MSCU"; }
		}

		public ZString HouseBillNumber
		{
			get { return ZString.Empty; }
		}

		public ZString IssuerCodeOfHouseBillNumber
		{
			get { return ZString.Empty; }
		}

		public ZString SubHouseBillNumber
		{
			get { return ZString.Empty; }
		}

		public ZString IssuerCodeOfSubHouseBillNumber
		{
			get { return ZString.Empty; }
		}

		public BusinessObjectFactory Factory
		{
			get { return null; }
		}

		ZBool IBillDetails.IsSplit
		{
			get { return false; }
		}

		IEnumerable<IConveyanceOrSplitDetails> IBillDetails.ConveyanceOrSplitDetails
		{
			get { return Enumerable.Empty<IConveyanceOrSplitDetails>(); }
		}

		IEnumerable<IContainer> IBillDetails.Containers
		{
			get { return Enumerable.Empty<IContainer>(); }
		}

		ZBool IBillDetails.IsNonAMS
		{
			get { return false; }
		}

		ZBool IBillDetails.IsExpressTracking
		{
			get { return false; }
		}

		#endregion
	}
}
