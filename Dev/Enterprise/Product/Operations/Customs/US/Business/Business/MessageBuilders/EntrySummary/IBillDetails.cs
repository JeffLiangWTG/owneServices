using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.MessageBuilders
{
	public interface IBillDetails
	{
		ZString ITNumber { get; }
		ZDate ITDate { get; }
		ZString MasterBillNumber { get; }
		ZString IssuerCodeOfMasterBillNumber { get; }
		ZString HouseBillNumber { get; }
		ZString IssuerCodeOfHouseBillNumber { get; }
		ZString SubHouseBillNumber { get; }
		ZString IssuerCodeOfSubHouseBillNumber { get; }
		BusinessObjectFactory Factory { get; }
		ZInt PackageQuantity { get; }
		ZString PackageType { get; }
		ZBool IsNonAMS { get; }
		ZBool IsSplit { get; }
		ZBool IsExpressTracking { get; }

		IEnumerable<IConveyanceOrSplitDetails> ConveyanceOrSplitDetails { get; }
		IEnumerable<IContainer> Containers { get; }
	}

	public interface IConveyanceOrSplitDetails
	{
		ZDateTime ArrivalDate { get; }
		ZString FlightNumber { get; }
		ZInt Qty { get; }
		ZString UQ { get; }
		ZString CarrierCode { get; }
		ZString PipelineName { get; }
	}

	public static class IBillDetailsExtensionMethod
	{
		public static bool IsSE16Required(this IBillDetails bill)
		{
			return bill.IsSplit || bill.IsNonAMS;
		}
	}

	public interface IBillOfLadingDetail
	{
		ZString BillType { get; }
		ZString IssuerCodeOfBillOfLading { get; }
		ZString BillOfLadingNumber { get; }
		IEnumerable<IBillOfLadingDetail> ChildBills { get; }
	}
}
