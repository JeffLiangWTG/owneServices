using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.TW.Business
{
	public class ControllingMessageHeaderImporterLocalAddressRequirement : ImporterLocalAddressRequirement
	{
		public ControllingMessageHeaderImporterLocalAddressRequirement(CusTWControllingMessageHeader header, DocAddressType defaultDocAddressType)
			: base(defaultDocAddressType)
		{
			CMHeader = header;
		}

		CusTWControllingMessageHeader CMHeader { get; }

		protected override void ValidateE2_CompanyNameCore(JobDocAddressValidation validation)
		{
			base.ValidateE2_CompanyNameCore(validation);
			if (validation.Parent is TWJobDocAddress parent)
			{
				var targetInfo = parent.E2_CompanyNameInfo;
				if (parent.E2_AddressOverride &&
				((CMHeader.IsNX101 && (CMHeader.ImporterDocumentaryAddress.CompanyName.IsEmpty || CMHeader.IsCertificate15)) || IsImporterLocalCompanyNameReruired(CMHeader)))
				{
					MandatoryValidation.MessageErrorIfNotEntered(targetInfo, ValidationConstants.CusTWControllingMessageHeader.Importer.LocalCompanyNameResString);
				}
			}
		}

		protected override void ValidateE2_Address1Core(JobDocAddressValidation validation)
		{
			base.ValidateE2_Address1Core(validation);
			if (validation.Parent is TWJobDocAddress parent &&
				parent.E2_AddressOverride &&
				((CMHeader.IsNX101 && (CMHeader.ImporterDocumentaryAddress.E2_Address1.IsEmpty || CMHeader.IsCertificate15)) || IsImporterLocalAddressReruired(CMHeader)))
			{
				MandatoryValidation.MessageErrorIfNotEntered(parent.E2_Address1Info, ValidationConstants.CusTWControllingMessageHeader.Importer.LocalAddressResString);
			}
		}

		public static bool IsImporterLocalCompanyNameReruired(CusTWControllingMessageHeader cMHeader) =>
			cMHeader.IsNX301 ||
			cMHeader.IsNX301_AX ||
			cMHeader.IsNX301_DN ||
			(cMHeader.IsNX401 && cMHeader.IsImport) ||
			cMHeader.IsNX601 ||
			cMHeader.IsNX603;

		public static bool IsImporterLocalAddressReruired(CusTWControllingMessageHeader cMHeader) =>
			cMHeader.IsNX301 ||
			cMHeader.IsNX301_AX ||
			cMHeader.IsNX301_DN ||
			cMHeader.IsNX601 ||
			cMHeader.IsNX603;
	}
}
