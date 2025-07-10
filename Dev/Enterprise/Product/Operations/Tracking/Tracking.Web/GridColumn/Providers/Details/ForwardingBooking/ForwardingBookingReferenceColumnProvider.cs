using Enterprise.Customs.Common;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
	public class ForwardingBookingReferenceColumnProvider : GridColumnProvider
	{
		public ForwardingBookingReferenceColumnProvider()
		{
		}

		protected override void CustomizeDictionaryCore()
		{
			base.CustomizeDictionaryCore();
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("09e63eea-d4e5-4a66-888f-b0f5fbf454b8", "Country/Region"), CusEntryNumber.Schema.CE_RN_NKCountryCode) { ColumnKey = WebTracker.Grids.ReferenceNumbers.Country });
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("1046968e-fdce-4fd3-89bc-dfd8479ee245", "Number Type"), CusEntryNumber.Schema.CE_EntryType) { ColumnKey = WebTracker.Grids.ReferenceNumbers.NumberType });
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("e73b87d3-b024-4558-9d8c-620f1cbcab2f", "Number"), CusEntryNumber.Schema.CE_EntryNum) { ColumnKey = WebTracker.Grids.ReferenceNumbers.Number });
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("f11d31f5-a3a0-4a35-9e46-86ffc2929cc2", "Type Description"), CusEntryNumber.Schema.AdditionalReferenceNumberTypeDescription) { ColumnKey = WebTracker.Grids.ReferenceNumbers.TypeDescription });
			AddToDictionary(new ZDateTimeColumn(Res.GetString("0b399433-41c6-4d0e-9d99-83ba966921a0", "Issue Date"), CusEntryNumber.Schema.CE_IssueDate, ZDateTimePickerFormat.Short) { ColumnKey = WebTracker.Grids.ReferenceNumbers.IssueDate });
			AddToDictionary(new ZTextEditColumn(Res.GetString("5b86ef86-9c42-4c09-b6cd-7e2f39cbb288", "Information"), CusEntryNumber.Schema.CE_EntryLineReference) { ColumnKey = WebTracker.Grids.ReferenceNumbers.Information });
		}
	}
}
