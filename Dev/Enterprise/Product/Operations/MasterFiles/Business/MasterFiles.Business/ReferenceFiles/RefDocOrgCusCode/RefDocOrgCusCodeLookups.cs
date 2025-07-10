using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class RefDocOrgCusCodeLookups : AutoRefDocOrgCusCodeLookups
	{
		public RefDocOrgCusCodeLookups(AutoRefDocOrgCusCode parent) : base(parent)
		{
		}

		public CodeDescriptionPairList RegistrationTypeList
		{
			get
			{
				var countryCode = ((AutoRefDocOrgCusCode)Parent).DOC_RN_NKCodeCountry;

				return Factory.GetCachedValue($"RefDocOrgCusCode_RegistrationTypeList_{countryCode}", () =>
				{
					var country = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, countryCode);

					return new OrgCodeLists().CustomsCodes_List(country);
				});
			}
		}

		public CodeDescriptionPairList DocumentTypeList => GetDocumentTypeList(Factory);

		public static CodeDescriptionPairList GetDocumentTypeList(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("RefDocOrgCusCode_DocumentTypeList", () =>
			{
				var list = new CodeDescriptionPairList();
				list.AddPair("AWB", Res.GetString("35178d2c-97f2-450a-874e-4a28e8e61541", "Master Air Waybill"));
				list.AddPair("HAW", Res.GetString("d178434d-0d37-4385-b565-c353d70a2240", "House Air Waybill"));
				list.AddPair("ESI", Res.GetString("2b3d9256-1a90-4745-a94b-c4bdc719df40", "Shipping Instruction"));
				list.AddPair("HBL", Res.GetString("F969871F-7D5F-4BCF-86D0-48F4797A5062", "House Bill"));

				return list;
			});
		}

		public CodeDescriptionPairList DirectionList => GetDirectionList(Factory);

		public static CodeDescriptionPairList GetDirectionList(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("RefDocOrgCusCode_DirectionList", () =>
			{
				var list = new CodeDescriptionPairList();
				list.AddPair("BTH", Res.GetString("622E2C81-428C-44F3-85A1-6F75E0E728D7", "Both"));
				list.AddPair("IMP", Res.GetString("37B1C48A-D18E-4E00-B0DA-87D394884EF8", "Import"));
				list.AddPair("EXP", Res.GetString("A196CC52-603F-407D-B7DA-AEE58529C10D", "Export"));

				return list;
			});
		}
	}
}
