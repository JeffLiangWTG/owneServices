using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TR.Business.Declaration
{
	public class CusBondDetailLookups : Enterprise.MasterFiles.Business.CusBondDetailLookups
	{
		public CusBondDetailLookups(AutoCusBondDetail parent) : base(parent)
		{
		}

		protected new CusBondDetail Parent => (CusBondDetail)base.Parent;

		public CusGuaranteeHeaderCollection GuaranteeList
		{
			get
			{
				return Factory.GetCachedValue("Enterprise.Customs.TR.Business.Declaration.CusBondDetailLookups|GuaranteeList", () =>
				new CusGuaranteeHeaderCollection(Factory, new ZString[] { GlbCompany.CurrentCompany.GC_RN_NKCountryCode }, Array.Empty<ZString>())
				{
					AdditionalFilter = new ZQuery(CusPermitHeaderSchema.CPH_OH_PermitHolder, Parent.EntryInstruction?.JobDeclaration?.JE_OH_Importer ?? ZGuid.Empty)
				});
			}
		}

		CodeDescriptionPairList GuaranteeTypeList => Factory.GetCachedValue<GuaranteeTypeList>();

		public CodeDescriptionPairList BondTypeList
		{
			get
			{
				if (bondTypeList == null)
				{
					bondTypeList = new CodeDescriptionPairList(GuaranteeTypeList);
					foreach (var guarantee in GuaranteeList)
					{
						if (!bondTypeList.ContainsCode(guarantee.CPH_Type))
						{
							bondTypeList.AddPair(guarantee.CPH_Type, ResString.GetMultilingualString("0C2289C5-379F-4D9C-9A1C-1ADDB3F233D8", "From Importer"));
						}
					}
				}
				return bondTypeList;
			}
		}
		CodeDescriptionPairList bondTypeList;
	}
}
