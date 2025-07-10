using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NZ.Business
{
	public abstract class OtherInfo : CodeDataPair
	{
		public OtherInfo(BusinessObjectFactory factory, OtherInfoCollection parentCollection)
			: base(factory, parentCollection)
		{
		}

		public override bool CodeRequiresData
		{
			get
			{
				return CodesRequiringData.Contains(ZO_Code.ToString());
			}
		}

		protected override ZString HumanReadableNameCore
		{
			get { return "Other Info"; }
		}

		#region CodesRequiringData

		protected string[] CodesRequiringData => LoadActiveCodeList(() => ImportCodesRequiringData, () => ExportCodesRequiringData, () => ImportExportCodesRequiringData, () => LegacyCodesRequiringData);

		protected abstract string[] ExportCodesRequiringData { get; }
		protected abstract string[] ImportCodesRequiringData { get; }

		protected string[] ImportExportCodesRequiringData
		{
			get
			{
				return Factory.GetCachedValue(".NZ.Business.OtherInfo.ImportExportCodesRequiringData",
					() => ExportCodesRequiringData.Union(ImportCodesRequiringData).ToArray());
			}
		}

		protected string[] LegacyCodesRequiringData
		{
			get
			{
				return Factory.GetCachedValue(".NZ.Business.OtherInfo.LegacyCodesRequiringData",
					() => new string[]
						{
							HeaderOtherInfoList.Codes.ApprovedTransitionalFacility,
							HeaderOtherInfoList.Codes.DeedOfCovenant,
							HeaderOtherInfoList.Codes.CustomsOfficerID,
							HeaderOtherInfoList.Codes.CertificateOfOrigin,
							HeaderOtherInfoList.Codes.CitizenshipOfImporter1,
							HeaderOtherInfoList.Codes.CitizenshipOfImporter2,
							HeaderOtherInfoList.Codes.CitizenshipOfImporter3,
							HeaderOtherInfoList.Codes.CitizenshipOfImporter4,
							HeaderOtherInfoList.Codes.CitizenshipOfImporter5,
							HeaderOtherInfoList.Codes.DeedOfUndertaking,
							LineOtherInfoList.Codes.MeatProducersBoardEMPICCodes,
							HeaderOtherInfoList.Codes.LetterOfUnderstanding,
							HeaderOtherInfoList.Codes.MemorandumOfUnderstanding,
							HeaderOtherInfoList.Codes.MinistryOfAgricultureSealNo,
							HeaderOtherInfoList.Codes.PassportOfImporter1,
							HeaderOtherInfoList.Codes.PassportOfImporter2,
							HeaderOtherInfoList.Codes.PassportOfImporter3,
							HeaderOtherInfoList.Codes.PassportOfImporter4,
							HeaderOtherInfoList.Codes.PassportOfImporter5,
							HeaderOtherInfoList.Codes.SecureExportPartnership,
							HeaderOtherInfoList.Codes.Passport,
							HeaderOtherInfoList.Codes.Certificate,
							HeaderOtherInfoList.Codes.OtherDocument,
						});
			}
		}

		#endregion
	}
}
