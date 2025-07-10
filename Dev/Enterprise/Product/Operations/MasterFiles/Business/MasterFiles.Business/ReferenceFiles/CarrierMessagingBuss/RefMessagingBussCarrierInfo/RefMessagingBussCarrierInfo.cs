using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[CodeProperty(RefMessagingBussCarrierInfoSchema.Constants.ZMC_CarrierCode), DescriptionProperty(RefMessagingBussCarrierInfoSchema.Constants.ZMC_CarrierName)]
	public class RefMessagingBussCarrierInfo : AutoRefMessagingBussCarrierInfo
	{
		public RefMessagingBussCarrierInfo(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[ResourceStringData("RefMessagingBussCarrierInfo|ZMC_CarrierCode", Caption = "Carrier Code")]
		public override ZString ZMC_CarrierCode => base.ZMC_CarrierCode;

		[ResourceStringData("RefMessagingBussCarrierInfo|ZMC_CarrierName", Caption = "Carrier Name")]
		public override ZString ZMC_CarrierName => base.ZMC_CarrierName;

		[ResourceStringData("RefMessagingBussCarrierInfo|ZMC_CountryCode", Caption = "Country Code")]
		public override ZString ZMC_CountryCode => base.ZMC_CountryCode;

		public RefMessagingBussPackageInfo Package => Factory.Load<RefMessagingBussPackageInfo>(ZMC_ZMP_PackageInfo);

		[RelatedBusinessObject("Package")]
		public override ZGuid ZMC_ZMP_PackageInfo
		{
			get { return base.ZMC_ZMP_PackageInfo; }
			set
			{
				if (value != base.ZMC_ZMP_PackageInfo)
				{
					base.ZMC_ZMP_PackageInfo = value;
				}
			}
		}

		protected override ZString HumanReadableNameCore => Res.GetString("ecd07079-05eb-4c92-8cc4-cc966f7cb4ae", "Carrier - {0}", CalculateShortcutName());

		#region For Test
#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, System.ComponentModel.PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			ZMC_CountryCode = "ZZ";
		}
#endif
		#endregion
	}
}
