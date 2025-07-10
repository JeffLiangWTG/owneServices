using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture;

namespace Enterprise.MasterFiles.Business
{
	[ProvideMetaDataProperty("ReadOnlySecurity", MetaDataTypes.ReadOnly)]
	public class OrgCusCodeValidity : AutoOrgCusCodeValidity
	{
		public OrgCusCodeValidity(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new class Schema : AutoOrgCusCodeValidity.Schema
		{
			public const string VerificationStatus = "VerificationStatus";
			public const string LastVerifiedTime = "LastVerifiedTime";
			public const string VerificationAuthority = "VerificationAuthority";
		}

		public static readonly OrgCusCodeValidityTypeDecider TypeDecider = new OrgCusCodeValidityTypeDecider();

		public OrgCusCode RegistrationNumber => Factory.Load<OrgCusCode>(OCV_OK_OrgCusCode);

		#region IReadOnlySecurity Members

		protected bool GetReadOnlySecurity(PropertyDescriptor property)
		{
			bool shouldBeReadOnly = false;

			if (RegistrationNumber?.Header is OrgHeader header)
			{
				shouldBeReadOnly = !header.SecurityProvider.HasModifyConfigRegistrationNumbersSecurity;
			}
			return shouldBeReadOnly || MetaData.GetReadOnlyExcludingMethodProvider(this, property);
		}

		#endregion

		#region New Properties

		protected virtual ZBool VerificationSupported => RegistrationNumber?.VerificationSupported ?? false;

		public virtual ZBool IsVerified => OCV_Verified;

		#region Verification Status

		public virtual ZString VerificationStatus
		{
			get
			{
				var result = ZString.Empty;

				if (VerificationSupported)
				{
					result = OCV_Verified ? OrgConstants.CusCodeValidityVerification.Verified : OrgConstants.CusCodeValidityVerification.NotVerified;
				}

				return result;
			}
			set
			{
				if (VerificationSupported)
				{
					OCV_Verified = value.EqualsIgnoringCase(OrgConstants.CusCodeValidityVerification.Verified.GetUnresolvedString());
				}

				VerificationStatusInfo.RefreshBinding();
			}
		}

		public virtual ZPropertyInfo VerificationStatusInfo => GetWrappedZPropertyInfo(Schema.VerificationStatus, x => OCV_VerifiedInfo);

		public virtual ZBool VerificationStatus_ReadOnly => true;

		#endregion

		#region Last Verified Time

		public virtual ZDateTime LastVerifiedTime
		{
			get
			{
				var result = ZDateTime.Empty;

				if (VerificationSupported)
				{
					result = OCV_LastVerifiedTimeUTC;
				}

				return result;
			}
			set
			{
				if (VerificationSupported)
				{
					OCV_LastVerifiedTimeUTC = value;
				}

				LastVerifiedTimeInfo.RefreshBinding();
			}
		}

		public virtual ZPropertyInfo LastVerifiedTimeInfo => GetWrappedZPropertyInfo(Schema.LastVerifiedTime, x => OCV_LastVerifiedTimeUTCInfo);

		public virtual ZBool LastVerifiedTime_ReadOnly => true;

		#endregion

		#region Verification Authority

		public virtual ZString VerificationAuthority
		{
			get
			{
				var result = ZString.Empty;
				if (VerificationSupported)
				{
					result = OCV_VerificationAuthority;
				}

				return result;
			}
			set
			{
				if (VerificationSupported)
				{
					OCV_VerificationAuthority = value;
				}

				VerificationAuthorityInfo.RefreshBinding();
			}
		}

		public virtual ZPropertyInfo VerificationAuthorityInfo => GetWrappedZPropertyInfo(Schema.VerificationAuthority, x => OCV_VerificationAuthorityInfo);

		public virtual ZBool VerificationAuthority_ReadOnly => true;

		#endregion

		public virtual ZString VerificationAuthorityFieldType
		{
			get
			{
				var result = nameof(FieldType.Text);
				if (VerificationSupported && OCV_Verified && !OCV_SnapShotOfWhatIsVerified.IsEmpty)
				{
					result = nameof(FieldType.LinkLabel);
				}

				return result;
			}
		}

		#region Snapshot Context For Display

		public virtual ZString SnapshotContextForDisplay => OCV_SnapShotOfWhatIsVerified;

		#endregion

		#endregion

		#region New Methods

		public void MarkVerifiedOrUnVerified(ZBool verified, ZString verificationAuthority, ZString snapshotContext)
		{
			if (VerificationSupported)
			{
				if (verified)
				{
					MarkVerified(verificationAuthority, snapshotContext);
				}
				else
				{
					MarkUnVerified();
				}
			}
		}

		protected virtual void MarkVerified(ZString verificationAuthority, ZString snapshotContext)
		{
			OCV_Verified = true;
			OCV_LastVerifiedTimeUTC = ZDateTime.UtcNow;
			OCV_VerificationAuthority = verificationAuthority;
			OCV_SnapShotOfWhatIsVerified = snapshotContext;
		}

		protected virtual void MarkUnVerified()
		{
			OCV_Verified = false;
			OCV_LastVerifiedTimeUTC = ZDateTime.Empty;
			OCV_VerificationAuthority = ZString.Empty;
			OCV_SnapShotOfWhatIsVerified = ZString.Empty;
		}

		#endregion
	}
}
