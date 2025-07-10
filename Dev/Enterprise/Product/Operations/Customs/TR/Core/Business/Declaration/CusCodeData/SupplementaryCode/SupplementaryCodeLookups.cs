using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.TR.Business.Declaration;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TR.Business;
sealed class SupplementaryCodeLookups : BaseSupplementaryCodeLookups
{
	public SupplementaryCodeLookups(BaseSupplementaryCode parent) : base(parent)
	{
	}

	public override CodeDescriptionPairList CY_CodeList
	{
		get
		{
			var countryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			var invoiceLine = Parent.Parent as JobComInvoiceLine;
			string shipmentType = invoiceLine.Declaration.JE_MessageType;

			if (string.IsNullOrWhiteSpace(countryCode) || string.IsNullOrWhiteSpace(shipmentType))
			{ return base.CY_CodeList; }

			return Factory.GetCachedValue("Enterprise.Customs." + countryCode + ".Supplementarycodes" + shipmentType, delegate
			{
				var codeList = new CodeDescriptionPairList();
				codeList.AddRange(base.CY_CodeList);
				var exemptions = new RefCusProcedureCollection(Factory, countryCode, ZDateTime.Now);
				string category = "EXM";
				foreach (var item in exemptions)
				{
					if (item.ZZ6_Category == category && item.ZZ6_ShipmentType.Contains(shipmentType))
					{
						codeList.AddPairIfNotExist(string.Concat(item.ZZ6_ProcedureCode, item.ZZ6_PreviousProcedureCode), item.ZZ6_Description);
					}
				}
				codeList.Sort();
				return codeList;
			});
		}
	}
}
