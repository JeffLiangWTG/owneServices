using System;
using CargoWise.Glow.Model.Interfaces;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterData.Business
{
	public class DeduplicationCertifcateOrAccreditation<T> : ICertificateOrAccreditation<T>, IDeduplicationGlowObject
	{
		[Newtonsoft.Json.JsonConstructor]
		DeduplicationCertifcateOrAccreditation() { }

		public DeduplicationCertifcateOrAccreditation(GenRegCertAccredMaintList certificate)
		{
			XZ_PK = certificate.PK.ToGuid();
			XZ_RefNumber = certificate.XZ_RefNumber;
			XZ_Type = certificate.XZ_Type;
			XZ_ParentID = certificate.XZ_ParentID.ToGuid();
		}

		public Guid PK => XZ_PK;

		public string TablePrefix => GenRegCertAccredMaintListSchema.Constants.Prefix;

		public bool IsInDatabase { get; }

		public Type BizoType => typeof(GenRegCertAccredMaintList);

		public string CountryCode => string.Empty;

		public T Parent { get; set; }

		public Guid XZ_PK { get; }

		public string XZ_Comment { get; set; }

		public DateTime? XZ_ExpiryOrDueDate { get; set; }

		public DateTime? XZ_IssueDate { get; set; }

		public bool XZ_IsValid { get; set; }

		public Guid? XZ_ParentID { get; set; }

		public string XZ_RefNumber { get; set; }

		public string XZ_RN_NKCountryOfIssuance { get; set; }

		public string XZ_StateOrProvinceOfIssuance { get; set; }

		public DateTime? XZ_SystemCreateTimeUtc { get; set; }

		public string XZ_SystemCreateUser { get; set; }

		public DateTime? XZ_SystemLastEditTimeUtc { get; set; }

		public string XZ_SystemLastEditUser { get; set; }

		public string XZ_Type { get; set; }

		public IRefCountryInfo CountryOfIssuance { get; set; }

		public IGlbStaffInfo CreatedByStaff { get; set; }

		public IGlbStaffInfo LastEditedByStaff { get; set; }

		public string ReferenceNumberForDeduplication
		{
			get
			{
				var country = !string.IsNullOrEmpty(XZ_RN_NKCountryOfIssuance) ? XZ_RN_NKCountryOfIssuance : "NCS";
				return string.Join("_", XZ_Type, XZ_RefNumber, country);
			}
		}
	}
}
