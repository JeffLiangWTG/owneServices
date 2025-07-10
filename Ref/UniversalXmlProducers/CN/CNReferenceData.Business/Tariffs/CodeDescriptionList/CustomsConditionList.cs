
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;

namespace CargoWise.RefDbRepo.CNReferenceData.Business
{
	/*
SELECT 'Add("' + ZZD_Code + '", new CustomsCondition("' + ZZD_Code + '", "' + ZZD_Description + '", '
	+ CASE WHEN (imp = 1) THEN 'true' ELSE 'false' END + ', '
	+ CASE WHEN (exp = 1) THEN 'true' ELSE 'false' END + '));'
FROM
(
	select ZZD_Code, imp.imp, exp.exp , ZZD_Description FROM RefCusCodeList
	outer apply ( select count(*) imp from [dbo].[RefCusCodeListAttribute] where ZZD_PK = ZZE_ZZD_CodeList and ZZE_ZXE_NKName='Import') as imp
	outer apply ( select count(*) exp from [dbo].[RefCusCodeListAttribute] where ZZD_PK = ZZE_ZZD_CodeList and ZZE_ZXE_NKName='Export') as exp
	where ZZD_ZZK_NKCodeType = 'CNDOC'
) docs order by ZZD_Code
	*/
	public class CustomsConditionList : Dictionary<string, CustomsCondition>
	{
		CustomsConditionList()
		{
			Add("00", new CustomsCondition("00", "反制措施排除代码", true, false));
			Add("01", new CustomsCondition("01", "进口许可证", true, false));
			Add("02", new CustomsCondition("02", "两用物项和技术进口许可证", true, false));
			Add("03", new CustomsCondition("03", "两用物项和技术出口许可证", false, true));
			Add("04", new CustomsCondition("04", "出口许可证", false, true));
			Add("05", new CustomsCondition("05", "纺织品临时出口许可证", false, true));
			Add("06", new CustomsCondition("06", "旧机电产品禁止进口", true, false));
			Add("07", new CustomsCondition("07", "自动进口许可证", true, false));
			Add("08", new CustomsCondition("08", "禁止出口商品", false, true));
			Add("09", new CustomsCondition("09", "禁止进口商品", true, false));
			Add("0a", new CustomsCondition("0a", "保税核注清单", true, true));
			Add("0b", new CustomsCondition("0b", "进口广播电影电视节目带(片)提取单", true, false));
			Add("0c", new CustomsCondition("0c", "内销征税联系单", true, false));
			Add("0d", new CustomsCondition("0d", "援外项目任务通知函", true, true));
			Add("0e", new CustomsCondition("0e", "关税配额外优惠税率进口棉花配额", true, false));
			Add("0f", new CustomsCondition("0f", "音像制品（成品）进口批准单", true, false));
			Add("0g", new CustomsCondition("0g", "技术出口合同登记证", false, true));
			Add("0h", new CustomsCondition("0h", "核增核扣表", true, true));
			Add("0i", new CustomsCondition("0i", "技术出口许可证", false, true));
			Add("0k", new CustomsCondition("0k", "民用爆炸物品进出口审批单", true, true));
			Add("0m", new CustomsCondition("0m", "银行调运人民币现钞进出境证明", true, true));
			Add("0n", new CustomsCondition("0n", "音像制品（版权引进）批准单", true, false));
			Add("0q", new CustomsCondition("0q", "国别关税配额证明", true, false));
			Add("0r", new CustomsCondition("0r", "预归类标志", true, true));
			Add("0s", new CustomsCondition("0s", "适用ITA税率的商品用途认定证明", true, false));
			Add("0t", new CustomsCondition("0t", "关税配额证明", true, false));
			Add("0u", new CustomsCondition("0u", "钟乳石出口批件", true, false));
			Add("0v", new CustomsCondition("0v", "自动进口许可证(加工贸易)", true, false));
			Add("0x", new CustomsCondition("0x", "出口许可证(加工贸易)", false, true));
			Add("0y", new CustomsCondition("0y", "出口许可证(边境小额贸易)", false, true));
			Add("0z", new CustomsCondition("0z", "古生物化石出境批件", false, true));
			Add("1A", new CustomsCondition("1A", "检验检疫", true, false));
			Add("1B", new CustomsCondition("1B", "电子底账", false, true));
			Add("1D", new CustomsCondition("1D", "出/入境货物通关单（毛坯钻石用）", true, true));
			Add("1E", new CustomsCondition("1E", "濒危物种允许出口证明书", false, true));
			Add("1F", new CustomsCondition("1F", "濒危物种允许进口证明书", true, false));
			Add("1G", new CustomsCondition("1G", "两用物项和技术出口许可证（定向）", false, true));
			Add("1H", new CustomsCondition("1H", "港澳OPA纺织品证明", false, true));
			Add("1I", new CustomsCondition("1I", "麻醉精神药品进出口准许证", true, true));
			Add("1J", new CustomsCondition("1J", "黄金及黄金制品进出口准许证", true, true));
			Add("1K", new CustomsCondition("1K", "深加工结转申请表", true, true));
			Add("1L", new CustomsCondition("1L", "药品进出口准许证", true, true));
			Add("1M", new CustomsCondition("1M", "密码产品和设备进口许可证", true, false));
			Add("1O", new CustomsCondition("1O", "自动进口许可证(新旧机电产品)", true, false));
			Add("1P", new CustomsCondition("1P", "固体废物进口许可证", true, false));
			Add("1Q", new CustomsCondition("1Q", "进口药品通关单", true, false));
			Add("1R", new CustomsCondition("1R", "进口兽药通关单", true, false));
			Add("1S", new CustomsCondition("1S", "进出口农药登记证明", true, true));
			Add("1T", new CustomsCondition("1T", "银行调运现钞进出境许可证", true, true));
			Add("1U", new CustomsCondition("1U", "合法捕捞产品通关证明", true, false));
			Add("1V", new CustomsCondition("1V", "人类遗传资源材料出口、出境证明", false, true));
			Add("1W", new CustomsCondition("1W", "麻醉药品进出口准许证", false, true));
			Add("1X", new CustomsCondition("1X", "有毒化学品环境管理放行通知单", true, true));
			Add("1Y", new CustomsCondition("1Y", "原产地证明", true, false));
			Add("1Z", new CustomsCondition("1Z", "赴境外加工光盘进口备案证明", true, false));
		}

		public static ImmutableDictionary<string, CustomsCondition> All { get; set; } = ImmutableDictionary.CreateRange(new CustomsConditionList());

		public static CustomsCondition Get(char code)
		{
			var twoCharCode = (char.ToLower(code, CultureInfo.InvariantCulture) == code ? "0" : "1") + code;
			if (!All.TryGetValue(twoCharCode, out var value))
			{
				throw new NotSupportedException($"Condition Type {code} is not defined.");
			}

			return value;
		}
	}
}
