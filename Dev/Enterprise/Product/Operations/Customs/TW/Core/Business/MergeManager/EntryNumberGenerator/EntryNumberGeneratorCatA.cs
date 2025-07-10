using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.TW.Business
{
	public class EntryNumberGeneratorCatA : EntryNumberGenerator
	{
		public EntryNumberGeneratorCatA(IEntryNumberGeneratorProvider provider)
			: base(provider)
		{
		}

		protected override ZString Category => RangeTypeList.Codes.A;

		public override ZString Part2 => GetPart2();

		ZString GetPart2()
		{
			var result = new ZString("  ");
			var declCustomsOffice = DeclarationCustomsOffice;
			if (declCustomsOffice != EntryNumberPart1 && !declCustomsOffice.IsEmpty)
			{
				result = declCustomsOffice;
			}
			return result;
		}

		ZString DeclarationCustomsOffice
		{
			get
			{
				var result = ZString.Empty;
				var entryNumberGeneratorProviderBusinessObject = Provider.EntryNumberGeneratorProviderBusinessObject;
				if (entryNumberGeneratorProviderBusinessObject is JobDeclaration decl)
				{
					result = decl.JE_CustomsOffice;
				}
				else if (entryNumberGeneratorProviderBusinessObject is CusEntryHeader header)
				{
					result = header.Declaration?.JE_CustomsOffice ?? ZString.Empty;
				}
				return result;
			}
		}

		protected override IEnumerable<ZPropertyInfo> GetValidationPropertyInfosCore()
		{
			yield return Provider.EntryNumberPart1Info;
			yield return Provider.CustomsBrokerageBoxNumberInfo;
		}
	}
}
