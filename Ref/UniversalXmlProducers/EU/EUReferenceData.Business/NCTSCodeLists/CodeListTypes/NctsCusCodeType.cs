using System.Collections.Generic;
using static CargoWise.RefDbRepo.EUReferenceData.Business.Constants;

namespace CargoWise.RefDbRepo.EUReferenceData.Business
{
	public sealed class NctsCusCodeType : NctsCodeListDetails, IUCCExportCodeListDetail
	{
		string IUCCExportCodeListDetail.Domain => Constants.UccConstants.NCTSDomain;

		string IUCCExportCodeListDetail.CodeType => Constants.CUSNumbers.CusCodeAttributeName;

		string IUCCExportCodeListDetail.CodeListType => UccCodeListTypes.CusCode;

		string IUCCExportCodeListDetail.DataSource => UccCodeListTypes.CusCode;

		string IUCCExportCodeListDetail.ExtraType => ExtraType;

		IReadOnlyList<(string attributeName, string attributeValue)> IUCCExportCodeListDetail.AttributeValues => AttributeValues;

		string IUCCExportCodeListDetail.XmlDataItemForCode => UccCodeListTypes.CusCode;
	}
}
