using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using static Enterprise.Integration.Customs.TW;

namespace Enterprise.Customs.TW.Business
{
	public class TWOrgImpAddInfo : AutoTWOrgImpAddInfo, IOrgImpAddInfo
	{
		public TWOrgImpAddInfo(ZPropertyInfoString parentPropertyInfo) : base(parentPropertyInfo.BizObj.Factory)
		{
			ParentPropertyInfo = parentPropertyInfo;
			using (GetValidationSuspender())
			using (SuspendSettingHasChanges())
			{
				Deserialise();
			}
		}

		public static TWOrgImpAddInfo Get(OrgHeader orgHeader)
		{
			TWOrgImpAddInfo tWOrgImpAddInfo = null;
			if (orgHeader != null)
			{
				var countryData = orgHeader.GetCountryData(Core.Constants.CountryCodes.Taiwan);
				orgHeader.RegisterEditableChildObject(countryData);
				tWOrgImpAddInfo = (TWOrgImpAddInfo)countryData.ImpAddInfo;
			}
			return tWOrgImpAddInfo;
		}

		[ResourceStringData("Enterprise.Customs.TW.Business.TWOrgImpAddInfo|ZO_TWHideEXPBuyerZHTAddr", Caption = "Hide EXP Buyer Trad. Chinese Addr.")]
		public override ZBool ZO_TWHideEXPBuyerZHTAddr { get => base.ZO_TWHideEXPBuyerZHTAddr; set => base.ZO_TWHideEXPBuyerZHTAddr = value; }

		[ResourceStringData("Enterprise.Customs.TW.Business.TWOrgImpAddInfo|ZO_TWHideEXPExporterZHTAddr", Caption = "Hide EXP Exporter Trad. Chinese Addr.")]
		public override ZBool ZO_TWHideEXPExporterZHTAddr { get => base.ZO_TWHideEXPExporterZHTAddr; set => base.ZO_TWHideEXPExporterZHTAddr = value; }

		[ResourceStringData("Enterprise.Customs.TW.Business.TWOrgImpAddInfo|ZO_TWHideIMPImporterZHTAddr", Caption = "Hide IMP Importer Trad. Chinese Addr.")]
		public override ZBool ZO_TWHideIMPImporterZHTAddr { get => base.ZO_TWHideIMPImporterZHTAddr; set => base.ZO_TWHideIMPImporterZHTAddr = value; }

		[ResourceStringData("Enterprise.Customs.TW.Business.TWOrgImpAddInfo|ZO_TWHideIMPImporterAddr", Caption = "Hide IMP Importer English Addr.")]
		public override ZBool ZO_TWHideIMPImporterAddr { get => base.ZO_TWHideIMPImporterAddr; set => base.ZO_TWHideIMPImporterAddr = value; }

		[ResourceStringData("Enterprise.Customs.TW.Business.TWOrgImpAddInfo|ZO_TWHideIMPSellerZHTAddr", Caption = "Hide IMP Seller Trad. Chinese Addr.")]
		public override ZBool ZO_TWHideIMPSellerZHTAddr { get => base.ZO_TWHideIMPSellerZHTAddr; set => base.ZO_TWHideIMPSellerZHTAddr = value; }

		[List(nameof(Lookups) + "." + nameof(TWOrgImpAddInfoLookups.ExamModeList))]
		[ResourceStringData("Enterprise.Customs.TW.Business.TWOrgImpAddInfo|ZO_TWDefaultExamMode", Caption = "Default Exam Mode")]
		public override ZString ZO_TWDefaultExamMode { get => base.ZO_TWDefaultExamMode; set => base.ZO_TWDefaultExamMode = value; }

		[List(nameof(Lookups) + "." + nameof(TWOrgImpAddInfoLookups.IMPPaymentMethodList))]
		[ResourceStringData("Enterprise.Customs.TW.Business.TWOrgImpAddInfo|ZO_TWDefaultIMPPaymentMethod", Caption = "Default IMP Payment Method")]
		public override ZString ZO_TWDefaultIMPPaymentMethod { get => base.ZO_TWDefaultIMPPaymentMethod; set => base.ZO_TWDefaultIMPPaymentMethod = value; }

		[List(nameof(Lookups) + "." + nameof(TWOrgImpAddInfoLookups.EXPPaymentMethodList))]
		[ResourceStringData("Enterprise.Customs.TW.Business.TWOrgImpAddInfo|ZO_TWDefaultEXPPaymentMethod", Caption = "Default EXP Payment Method")]
		public override ZString ZO_TWDefaultEXPPaymentMethod { get => base.ZO_TWDefaultEXPPaymentMethod; set => base.ZO_TWDefaultEXPPaymentMethod = value; }

		[ResourceStringData("Enterprise.Customs.TW.Business.TWOrgImpAddInfo|ZO_TWHideEXPExporterAddr", Caption = "Hide EXP Exporter English Addr.")]
		public override ZBool ZO_TWHideEXPExporterAddr { get => base.ZO_TWHideEXPExporterAddr; set => base.ZO_TWHideEXPExporterAddr = value; }

		[ResourceStringData("Enterprise.Customs.TW.Business.TWOrgImpAddInfo|ZO_TWHideEXPBuyerAddr", Caption = "Hide EXP Buyer English Addr.")]
		public override ZBool ZO_TWHideEXPBuyerAddr { get => base.ZO_TWHideEXPBuyerAddr; set => base.ZO_TWHideEXPBuyerAddr = value; }
	}
}
