using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.ResourceStrings.Grammar;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business
{
	public class NX101GovernmentProcedure : IGovernmentProcedure
	{
		readonly ZString description;

		NX101GovernmentProcedure(ZString description)
		{
			this.description = description;
		}

		public ZString TransportTypeCode => null;

		public ZString CurrentCode => null;

		public ZString Description => description;

		public static IEnumerable<NX101GovernmentProcedure> GovernmentProcedures(CusTWControllingMessageHeader header)
		{
			var remarks = GetFullDescription(header).Split(256).Take(4);
			foreach (var remark in remarks)
			{
				yield return new NX101GovernmentProcedure(remark);
			}
		}

		static ZString GetFullDescription(CusTWControllingMessageHeader header)
		{
			var declaration = header.Declaration;
			var result = ZString.Empty;
			if (declaration != null)
			{
				var totalNoOfPacks = declaration.JE_TotalNoOfPacks;
				var packType = declaration.JE_TotalNoOfPacksPackType.ToUpper();
				if (totalNoOfPacks > 1)
				{
					packType = Grammar.Instance.Pluralize(packType).ToUpper(CultureInfo.InvariantCulture);
				}
				var quantityInEnglish = DocumentEngine.MacroValueProviders.Utilities.NumberToString_EN.ConvertNumberToWords(totalNoOfPacks).ToUpper(CultureInfo.InvariantCulture);
				var remarks = header.TW1_Remarks;
				var sayTotal = remarks.IsEmpty ? DocumentWrappers.DocumentWrapperHelper.SayTotalConst.SayTotalString : $"\r\n{DocumentWrappers.DocumentWrapperHelper.SayTotalConst.SayTotalString}";
				result = $@"{remarks}{sayTotal} {quantityInEnglish} ({totalNoOfPacks}) {packType}{DocumentWrappers.DocumentWrapperHelper.SayTotalConst.SayTotalOnly}";
			}
			return result;
		}
	}
}
