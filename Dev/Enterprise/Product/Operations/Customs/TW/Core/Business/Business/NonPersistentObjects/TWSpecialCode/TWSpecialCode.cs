using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes;

namespace Enterprise.Customs.TW.Business
{
	[CodeProperty(ZZRefCusCodeListWrapper.Schema.Code), DescriptionProperty(ZZRefCusCodeListWrapper.Schema.Description)]
	public class TWSpecialCode : ZZRefCusCodeListWrapper
	{
		public TWSpecialCode(ZZRefCusCodeListCombined cusCodeList)
			: base(cusCodeList)
		{
		}

		#region Schema

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors")]
		public new class Schema : ZZRefCusCodeListWrapper.Schema
		{
			public const string SC_CodeType = "SC_CodeType";
			public const string SC_CodeTypeDesc = "SC_CodeTypeDesc";
			public const string SC_Country = "SC_Country";
			public const string SC_ControllingAgency = "SC_ControllingAgency";
			public const string SC_ControllingAgencyDescription = "SC_ControllingAgencyDescription";
			public const string SC_Remarks = "SC_Remarks";
			public const string SC_Source = "SC_Source";
			public const string SC_Description = "SC_Description";
		}

		#endregion

		#region Properties

		public ZString SC_CodeType => CusCodeList.ZZD_CodeType;
		public ZPropertyInfo SC_CodeTypeInfo => GetZPropertyInfo(TWSpecialCode.Schema.SC_CodeType);

		public ZString SC_CodeTypeDesc => RefCusCodeTypeList.GetListByCountry(Factory, SC_Country, true).GetDescriptionFromCode(SC_CodeType);
		public ZPropertyInfo SC_CodeTypeDescInfo => GetZPropertyInfo(TWSpecialCode.Schema.SC_CodeTypeDesc);

		public ZString SC_Country => CusCodeList.ZZD_CountryOrGrouping;
		public ZPropertyInfo SC_CountryInfo => GetZPropertyInfo(TWSpecialCode.Schema.SC_Country);

		public ZString SC_ControllingAgency => CusCodeList.GetAttribute(UniversalReferenceConstants.RefCusCodeListAttributes.ControllingAgency);
		public ZPropertyInfo SC_ControllingAgencyInfo => GetZPropertyInfo(TWSpecialCode.Schema.SC_ControllingAgency);

		public ZString SC_ControllingAgencyDescription => ControllingAgencyTypeForValueList.GetDescriptionFromCode(SC_ControllingAgency);
		public ZPropertyInfo SC_ControllingAgencyDescriptionInfo => GetZPropertyInfo(TWSpecialCode.Schema.SC_ControllingAgencyDescription);

		public ZString SC_Remarks => CusCodeList.GetAttribute(UniversalReferenceConstants.RefCusCodeListAttributes.Remarks);
		public ZPropertyInfo SC_RemarksInfo => GetZPropertyInfo(TWSpecialCode.Schema.SC_Remarks);

		public ZString SC_Source => CusCodeList.GetAttribute(UniversalReferenceConstants.RefCusCodeListAttributes.Source);
		public ZPropertyInfo SC_SourceInfo => GetZPropertyInfo(TWSpecialCode.Schema.SC_Source);

		public ZString SC_Description => CusCodeList.ZZD_Description;
		public ZPropertyInfo SC_DescriptionInfo => GetZPropertyInfo(TWSpecialCode.Schema.SC_Description);

		#endregion

		public CodeDescriptionPairList ControllingAgencyTypeForValueList => GetControllingAgencyTypeForValueList(Factory);

		public static CodeDescriptionPairList GetControllingAgencyTypeForValueList(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("Enterprise.Customs.TW.Business.TWSpecialCode.ControllingAgencyTypeForValueList", () =>
			{
				var result = new CodeDescriptionPairList();
				var query = new ZQuery(Enterprise.ZArchitecture.Schema.RefCusCodeListAttributeNameSchema.ZXE_ZZK_NKCodeType, Codes.SpecialCodesForExemptionOfControllingAgencies);
				query.AddToFilter(Enterprise.ZArchitecture.Schema.RefCusCodeListAttributeNameSchema.ZXE_ZZZ_NKDataGrouping, Core.Constants.CountryCodes.Taiwan);
				query.AddToFilter(Enterprise.ZArchitecture.Schema.RefCusCodeListAttributeNameSchema.ZXE_Name, UniversalReferenceConstants.RefCusCodeListAttributes.ControllingAgency);
				var typeForValueList = factory.LoadTop1<RefCusCodeListAttributeName>(query)?.ZXE_ZZK_NKCodeTypeForValueList ?? ZString.Empty;
				if (!typeForValueList.IsEmpty)
				{
					result = RefCusCodeListTypes.GetCachedList(factory, Core.Constants.CountryCodes.Taiwan, typeForValueList, ZDateTime.Today);
				}
				return result;
			});
		}

		protected override ZString HumanReadableNameCore
		{
			get { return Res.GetString("17828A01-E762-4C30-8A58-4A838D9601CC", "Special Code: '{0}'", Code); }
		}
	}
}
