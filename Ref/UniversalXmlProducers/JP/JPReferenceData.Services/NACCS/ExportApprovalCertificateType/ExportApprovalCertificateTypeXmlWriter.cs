using System;

namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	public class ExportApprovalCertificateTypeXmlWriter : NaccsXmlWriter
	{
		protected override string DataSource => "Japan Export Approval Certificate Type";
		protected override string FileNameWithoutExtension => "RefCusCodeList_JP_ExportApprovalCertificateType";
		protected override string DownloadUrl => AppConfig.NACCS.CodeLists.ExportApprovalCertificateCsvFileDownloadUrl;
		protected override RefCusCodeListParser GetRefCusCodeListParserCore() => new ExportApprovalCertificateTypeRefCusCodeListParser();
		protected override bool IsCodeTypeConstant => false;
	}
}
