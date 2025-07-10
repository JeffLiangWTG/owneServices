using System.Collections.Generic;

namespace Enterprise.Freight.Agency.Business
{
	public interface IPortAuthorityConsignmentData
	{
		/// <summary>
		/// May be sequential, but must always be unique within the message.
		/// (n..4)
		/// </summary>
		int ConsignmentNumber { get; }

		string BillOfLading { get; }
		bool IsWaybill { get; }
		string PackingMode { get; }

		/// <summary>
		/// Export Only:
		/// </summary>
		string ConsignorNameAndAddress { get; }

		/// <summary>
		/// Import Only:
		/// </summary>
		string ConsigneeNameAndAddress { get; }

		/// <summary>
		/// Import Only:
		/// LOC 7 -- Place Of Delivery
		/// </summary>
		string PortOfDestination { get; }

		/// <summary>
		/// Import Only:
		/// LOC 27 -- Country Of Origin
		/// </summary>
		string CountryOfOrigin { get; }

		/// <summary>
		/// Import Only:
		/// LOC 9 -- Port Of Loading
		/// </summary>
		string PortOfLoading { get; }

		/// <summary>
		/// Exports Only:
		/// LOC 88 -- Place Of Receipt
		/// </summary>
		string PortOfOrigin { get; }

		/// <summary>
		/// Exports Only:
		/// LOC 28 -- Country Of Destination
		/// </summary>
		string CountryOfDestination { get; }

		/// <summary>
		/// Exports Only:
		/// LOC 11 -- Port Of Discharge
		/// </summary>
		string PortOfDischarge { get; }

		IEnumerable<IPortAuthorityGoodsData> Goods { get; }
	}
}
