using System;
using System.Globalization;
using System.Linq;
using System.Web.Services;
using System.Web.Services.Protocols;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService
{
	public partial class WhsSecureService
	{
		#region GetASNLinesFromPalletID

		[WebMethod(Description = "Get Receive ASN Lines")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public WhsGroupedUnloadLinesWebServiceResponse GetASNLinesFromPalletID(Guid receivePK, string palletID)
		{
			return HandleWebServiceRequest<WhsGroupedUnloadLinesWebServiceResponse>(result => GetASNLinesFromPalletIDCore(result, receivePK, palletID));
		}

		#region GetASNLinesFromPalletIDCore

		void GetASNLinesFromPalletIDCore(WhsGroupedUnloadLinesWebServiceResponse response, Guid receivePK, string palletID)
		{
			palletID = palletID.ToUpper(CultureInfo.InvariantCulture);
			var receive = WhsGroupedUnloadHelper.LoadValidNotFinalizedReceive(response, Factory, receivePK);
			if (response.Error == ErrorTypes.None)
			{
				WhsGroupedUnloadHelper.CheckASNLinesPalletInfo(response, receive, palletID, isPerformingUnload: false);
			}

			switch (response.Error)
			{
				case ErrorTypes.None:
					response.GroupedLines = new WhsGroupedUnloadLineInfoCollection(receive.AsnLines.Cast<WhsAsnLine>().Where(al => al.WN_PalletId.ToUpper() == palletID), receive.Client, receive.Warehouse);
					break;
				case ErrorTypes.YesNoEnquiry:
					response.GroupedLines = new WhsGroupedUnloadLineInfoCollection(receive.Inventory.Cast<WhsInventoryView>().Where(i => i.WI_PalletID.ToUpper() == palletID), receive.Client, receive.Warehouse);
					break;
			}
		}

		#endregion

		#endregion
	}
}
