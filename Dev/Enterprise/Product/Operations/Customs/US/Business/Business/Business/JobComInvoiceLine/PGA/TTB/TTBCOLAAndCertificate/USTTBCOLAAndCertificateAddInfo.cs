using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.US.Business
{
	[CusAddInfoType(CusAddInfoTypeAttribute.Codes.USTTBCOLAAndCertificate)]
	public class USTTBCOLAAndCertificateAddInfo : AutoUSTTBCOLAAndCertificateAddInfo
	{
		public USTTBCOLAAndCertificateAddInfo(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty.BizObj.Factory)
		{
			SetupEventsAndLoadValues(addInfoProperty);
		}

		public new class Schema : AutoUSTTBCOLAAndCertificateAddInfo.Schema
		{
			public const string HasForeignCertificate = "HasForeignCertificate";
		}

		public new TTBCOLAAndCertificate Parent
		{
			get { return (TTBCOLAAndCertificate)base.Parent; }
			protected set { base.Parent = value; }
		}

		public ZBool HasForeignCertificate
		{
			get
			{
				if (!hasForeignCertificate.HasValue)
				{
					hasForeignCertificate = !US_ForeignCertificateCountry.IsEmpty;
				}
				return hasForeignCertificate.Value;
			}
			set
			{
				var oldValue = HasForeignCertificate;
				if (!hasForeignCertificateSettingInProgress)
				{
					try
					{
						hasForeignCertificateSettingInProgress = true;
						hasForeignCertificate = value;
						if (!IsCopying && oldValue != HasForeignCertificate)
						{
							if (!value)
							{
								US_ForeignCertificateCountry = ZString.Empty;
							}
						}
						HasForeignCertificateInfo.RefreshBinding(oldValue);
						if (!IsValidationSuspended)
						{
							Validation.ValidateHasForeignCertificate();
						}
					}
					finally
					{
						hasForeignCertificateSettingInProgress = false;
					}
				}
			}
		}
		ZBool? hasForeignCertificate;
		bool hasForeignCertificateSettingInProgress;

		public ZPropertyInfo HasForeignCertificateInfo
		{
			get { return GetZPropertyInfo(Schema.HasForeignCertificate); }
		}

		public override ZString US_ForeignCertificateCountry
		{
			get { return base.US_ForeignCertificateCountry; }
			set
			{
				var oldValue = US_ForeignCertificateCountry;
				base.US_ForeignCertificateCountry = value;
				if (!IsCopying && oldValue != US_ForeignCertificateCountry)
				{
					HasForeignCertificate = !US_ForeignCertificateCountry.IsEmpty;
				}
			}
		}
	}
}
