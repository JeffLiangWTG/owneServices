using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Universal
{
	public static class CustomsStatusAttributeHelper
	{
		public static bool ShouldUpdateEntryNumber(BusinessObjectFactory factory, ZString code, ZString dataGroupingCode, ZDateTime date)
		{
			return HasAttribute(factory, RefCusCodeListAttributeTypes.Codes.IUpdateEntryNumber, code, dataGroupingCode, date);
		}

		public static bool ShouldUpdateReleaseDate(BusinessObjectFactory factory, ZString code, ZString dataGroupingCode, ZDateTime date)
		{
			return HasAttribute(factory, RefCusCodeListAttributeTypes.Codes.IUpdateReleaseDate, code, dataGroupingCode, date);
		}

		public static bool ShouldUpdateCustomsStatus(BusinessObjectFactory factory, ZString code, ZString dataGroupingCode, ZDateTime date)
		{
			return HasAttribute(factory, RefCusCodeListAttributeTypes.Codes.IUpdateCustomsStatus, code, dataGroupingCode, date);
		}

		public static bool ShouldUpdateCIQStatus(BusinessObjectFactory factory, ZString code, ZString dataGroupingCode, ZDateTime date)
		{
			return HasAttribute(factory, RefCusCodeListAttributeTypes.Codes.IUpdateCIQStatus, code, dataGroupingCode, date);
		}

		public static bool ShouldNotify(BusinessObjectFactory factory, ZString code, ZString dataGroupingCode, ZDateTime date)
		{
			return HasAttribute(factory, RefCusCodeListAttributeTypes.Codes.INotify, code, dataGroupingCode, date);
		}

		public static bool ShouldSendEntryDocs(BusinessObjectFactory factory, ZString code, ZString dataGroupingCode, ZDateTime date)
		{
			return HasAttribute(factory, RefCusCodeListAttributeTypes.Codes.ISendEntryDocs, code, dataGroupingCode, date);
		}

		public static bool ShouldAddEntryDocsToEDocs(BusinessObjectFactory factory, ZString code, ZString dataGroupingCode, ZDateTime date)
		{
			return HasAttribute(factory, RefCusCodeListAttributeTypes.Codes.IAddEntryDocsToEDocs, code, dataGroupingCode, date);
		}

		public static bool AllowCancel(BusinessObjectFactory factory, ZString code, ZString dataGroupingCode, ZDateTime date)
		{
			var attrValue = GetAttributeValue(factory, RefCusCodeListAttributeTypes.Codes.IAllowCancel, code, dataGroupingCode, date, bool.FalseString);
			return attrValue.IsEmpty ? ZBool.False : ZBool.ParseSafe(attrValue, false);
		}

		public static bool IsStatusCancelled(BusinessObjectFactory factory, ZString code, ZString dataGroupingCode, ZDateTime date)
		{
			return HasAttribute(factory, RefCusCodeListAttributeTypes.Codes.ICustomsCancelled, code, dataGroupingCode, date);
		}

		public static bool IsStatusCommenced(BusinessObjectFactory factory, ZString code, ZString dataGroupingCode, ZDateTime date)
		{
			return HasAttribute(factory, RefCusCodeListAttributeTypes.Codes.CustomsCommenced, code, dataGroupingCode, date);
		}

		public static bool IsStatusCleared(BusinessObjectFactory factory, ZString code, ZString dataGroupingCode, ZDateTime date)
		{
			var attrValue = GetAttributeValue(factory, RefCusCodeListAttributeTypes.Codes.CustomsCleared, code, dataGroupingCode, date, bool.FalseString);
			return attrValue.IsEmpty ? ZBool.False : ZBool.ParseSafe(attrValue, false);
		}

		public static bool ShouldPostCustomsAPInvoice(BusinessObjectFactory factory, ZString code, ZString dataGroupingCode, ZDateTime date)
		{
			var attrValue = GetAttributeValue(factory, RefCusCodeListAttributeTypes.Codes.IPostCustomsAPInvoice, code, dataGroupingCode, date, bool.FalseString);
			return attrValue.IsEmpty ? ZBool.False : ZBool.ParseSafe(attrValue, false);
		}

		public static bool IsStatusRejected(BusinessObjectFactory factory, ZString code, ZString dataGroupingCode, ZDateTime date)
		{
			var attrValue = GetAttributeValue(factory, RefCusCodeListAttributeTypes.Codes.CustomsRejected, code, dataGroupingCode, date, bool.FalseString);
			return attrValue.IsEmpty ? ZBool.False : ZBool.ParseSafe(attrValue, false);
		}

		public static bool HasAttribute(BusinessObjectFactory factory, ZString attrName, ZString code, ZString dataGroupingCode, ZDateTime date)
		{
			var statusCode = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(factory, code, dataGroupingCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, date);
			return statusCode?.HasAttribute(attrName) ?? false;
		}

		public static bool ShouldUpdateBondedWhs(BusinessObjectFactory factory, ZString code, ZString dataGroupingCode, ZDateTime date)
		{
			return HasAttribute(factory, RefCusCodeListAttributeTypes.Codes.IUpdateBondedWhs, code, dataGroupingCode, date);
		}

		public static bool ShouldCancelBondedWhs(BusinessObjectFactory factory, ZString code, ZString dataGroupingCode, ZDateTime date)
		{
			return HasAttribute(factory, RefCusCodeListAttributeTypes.Codes.ICancelBondedWhs, code, dataGroupingCode, date);
		}

		public static bool IsStatusFitToMarkPayInfoAsAwaitingResponse(BusinessObjectFactory factory, ZString code, ZString dataGroupingCode, ZDateTime date)
		{
			var attrValue = GetAttributeValue(factory, RefCusCodeListAttributeTypes.Codes.IMarkEntryPayInfoAwaitingResp, code, dataGroupingCode, date, bool.FalseString);
			return attrValue.IsEmpty ? ZBool.False : ZBool.ParseSafe(attrValue, false);
		}

		public static bool ShouldExecuteAutoBilling(BusinessObjectFactory factory, ZString code, ZString dataGrouping, ZDateTime date)
		{
			var statusCode = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(factory, code, dataGrouping, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, date);
			return statusCode?.HasAttribute(RefCusCodeListAttributeTypes.Codes.IExecuteAutoBilling) ?? false;
		}

		static ZString GetAttributeValue(BusinessObjectFactory factory, ZString attrName, ZString code, ZString dataGroupingCode, ZDateTime date, ZString defaultValue)
		{
			var statusCode = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(factory, code, dataGroupingCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, date);
			var attribute = statusCode?.Attributes?.Find(x => x.ZZE_ZXE_NKName.EqualsIgnoringCase(attrName))?.FirstOrDefault();
			return attribute?.ZZE_Value ?? defaultValue;
		}
	}
}
