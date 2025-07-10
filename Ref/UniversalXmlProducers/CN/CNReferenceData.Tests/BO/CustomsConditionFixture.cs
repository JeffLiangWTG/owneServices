using System;
using CargoWise.RefDbRepo.CNReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.CNReferenceData.Tests
{
	[TestFixture]
	public class CustomsConditionFixture
	{
		[Test]
		public void TestCustomsCondition()
		{
			TestCustomsCondition(CustomsConditionList.Get('0'), "00", "反制措施排除代码", true, false, "CNDOC", "DOC");
			TestCustomsCondition(CustomsConditionList.Get('1'), "01", "进口许可证", true, false, "CNDOC", "DOC");
			TestCustomsCondition(CustomsConditionList.Get('2'), "02", "两用物项和技术进口许可证", true, false, "CNDOC", "DOC");
			TestCustomsCondition(CustomsConditionList.Get('3'), "03", "两用物项和技术出口许可证", false, true, "CNDOC", "DOC");
			TestCustomsCondition(CustomsConditionList.Get('4'), "04", "出口许可证", false, true, "CNDOC", "DOC");
			TestCustomsCondition(CustomsConditionList.Get('5'), "05", "纺织品临时出口许可证", false, true, "CNDOC", "DOC");
			TestCustomsCondition(CustomsConditionList.Get('6'), "06", "旧机电产品禁止进口", true, false, "IMPPH", "PROH");
			TestCustomsCondition(CustomsConditionList.Get('7'), "07", "自动进口许可证", true, false, "CNDOC", "DOC");
			TestCustomsCondition(CustomsConditionList.Get('8'), "08", "禁止出口商品", false, true, "EXPPH", "PROH");
			TestCustomsCondition(CustomsConditionList.Get('9'), "09", "禁止进口商品", true, false, "IMPPH", "PROH");

			TestCustomsCondition(CustomsConditionList.Get('a'), "0a", "保税核注清单", true, true, "CNDOC", "DOC");
			TestCustomsCondition(CustomsConditionList.Get('b'), "0b", "进口广播电影电视节目带(片)提取单", true, false, "CNDOC", "DOC");
			TestCustomsCondition(CustomsConditionList.Get('c'), "0c", "内销征税联系单", true, false, "CNDOC", "DOC");
			TestCustomsCondition(CustomsConditionList.Get('d'), "0d", "援外项目任务通知函", true, true, "CNDOC", "DOC");
			TestCustomsCondition(CustomsConditionList.Get('e'), "0e", "关税配额外优惠税率进口棉花配额", true, false, "CNDOC", "DOC");
			TestCustomsCondition(CustomsConditionList.Get('f'), "0f", "音像制品（成品）进口批准单", true, false, "CNDOC", "DOC");
			TestCustomsCondition(CustomsConditionList.Get('g'), "0g", "技术出口合同登记证", false, true, "CNDOC", "DOC");
			TestCustomsCondition(CustomsConditionList.Get('h'), "0h", "核增核扣表", true, true, "CNDOC", "DOC");
			TestCustomsCondition(CustomsConditionList.Get('i'), "0i", "技术出口许可证", false, true, "CNDOC", "DOC");
			Assert.Throws<NotSupportedException>(() => CustomsConditionList.Get('j'));
			TestCustomsCondition(CustomsConditionList.Get('k'), "0k", "民用爆炸物品进出口审批单", true, true, "CNDOC", "DOC");
			Assert.Throws<NotSupportedException>(() => CustomsConditionList.Get('l'));
			TestCustomsCondition(CustomsConditionList.Get('m'), "0m", "银行调运人民币现钞进出境证明", true, true, "CNDOC", "DOC");
			TestCustomsCondition(CustomsConditionList.Get('n'), "0n", "音像制品（版权引进）批准单", true, false, "CNDOC", "DOC");
			Assert.Throws<NotSupportedException>(() => CustomsConditionList.Get('o'));
			Assert.Throws<NotSupportedException>(() => CustomsConditionList.Get('p'));
			TestCustomsCondition(CustomsConditionList.Get('q'), "0q", "国别关税配额证明", true, false, "CNDOC", "DOC");
			TestCustomsCondition(CustomsConditionList.Get('r'), "0r", "预归类标志", true, true, "CNDOC", "DOC");
			TestCustomsCondition(CustomsConditionList.Get('s'), "0s", "适用ITA税率的商品用途认定证明", true, false, "CNDOC", "DOC");
			TestCustomsCondition(CustomsConditionList.Get('t'), "0t", "关税配额证明", true, false, "CNDOC", "DOC");
			TestCustomsCondition(CustomsConditionList.Get('u'), "0u", "钟乳石出口批件", true, false, "CNDOC", "DOC");
			TestCustomsCondition(CustomsConditionList.Get('v'), "0v", "自动进口许可证(加工贸易)", true, false, "CNDOC", "DOC");
			Assert.Throws<NotSupportedException>(() => CustomsConditionList.Get('w'));
			TestCustomsCondition(CustomsConditionList.Get('x'), "0x", "出口许可证(加工贸易)", false, true, "CNDOC", "DOC");
			TestCustomsCondition(CustomsConditionList.Get('y'), "0y", "出口许可证(边境小额贸易)", false, true, "CNDOC", "DOC");
			TestCustomsCondition(CustomsConditionList.Get('z'), "0z", "古生物化石出境批件", false, true, "CNDOC", "DOC");

			TestCustomsCondition(CustomsConditionList.Get('A'), "1A", "检验检疫", true, false, "CNDOC", "DOC");
			TestCustomsCondition(CustomsConditionList.Get('B'), "1B", "电子底账", false, true, "CNDOC", "DOC");
			Assert.Throws<NotSupportedException>(() => CustomsConditionList.Get('C'));
			TestCustomsCondition(CustomsConditionList.Get('D'), "1D", "出/入境货物通关单（毛坯钻石用）", true, true, "CNDOC", "DOC");
			TestCustomsCondition(CustomsConditionList.Get('E'), "1E", "濒危物种允许出口证明书", false, true, "CNDOC", "DOC");
			TestCustomsCondition(CustomsConditionList.Get('F'), "1F", "濒危物种允许进口证明书", true, false, "CNDOC", "DOC");
			TestCustomsCondition(CustomsConditionList.Get('G'), "1G", "两用物项和技术出口许可证（定向）", false, true, "CNDOC", "DOC");
			TestCustomsCondition(CustomsConditionList.Get('H'), "1H", "港澳OPA纺织品证明", false, true, "CNDOC", "DOC");
			TestCustomsCondition(CustomsConditionList.Get('I'), "1I", "麻醉精神药品进出口准许证", true, true, "CNDOC", "DOC");
			TestCustomsCondition(CustomsConditionList.Get('J'), "1J", "黄金及黄金制品进出口准许证", true, true, "CNDOC", "DOC");
			TestCustomsCondition(CustomsConditionList.Get('K'), "1K", "深加工结转申请表", true, true, "CNDOC", "DOC");
			TestCustomsCondition(CustomsConditionList.Get('L'), "1L", "药品进出口准许证", true, true, "CNDOC", "DOC");
			TestCustomsCondition(CustomsConditionList.Get('M'), "1M", "密码产品和设备进口许可证", true, false, "CNDOC", "DOC");
			Assert.Throws<NotSupportedException>(() => CustomsConditionList.Get('N'));
			TestCustomsCondition(CustomsConditionList.Get('O'), "1O", "自动进口许可证(新旧机电产品)", true, false, "CNDOC", "DOC");
			TestCustomsCondition(CustomsConditionList.Get('P'), "1P", "固体废物进口许可证", true, false, "CNDOC", "DOC");
			TestCustomsCondition(CustomsConditionList.Get('Q'), "1Q", "进口药品通关单", true, false, "CNDOC", "DOC");
			TestCustomsCondition(CustomsConditionList.Get('R'), "1R", "进口兽药通关单", true, false, "CNDOC", "DOC");
			TestCustomsCondition(CustomsConditionList.Get('S'), "1S", "进出口农药登记证明", true, true, "CNDOC", "DOC");
			TestCustomsCondition(CustomsConditionList.Get('T'), "1T", "银行调运现钞进出境许可证", true, true, "CNDOC", "DOC");
			TestCustomsCondition(CustomsConditionList.Get('U'), "1U", "合法捕捞产品通关证明", true, false, "CNDOC", "DOC");
			TestCustomsCondition(CustomsConditionList.Get('V'), "1V", "人类遗传资源材料出口、出境证明", false, true, "CNDOC", "DOC");
			TestCustomsCondition(CustomsConditionList.Get('W'), "1W", "麻醉药品进出口准许证", false, true, "CNDOC", "DOC");
			TestCustomsCondition(CustomsConditionList.Get('X'), "1X", "有毒化学品环境管理放行通知单", true, true, "CNDOC", "DOC");
			TestCustomsCondition(CustomsConditionList.Get('Y'), "1Y", "原产地证明", true, false, "CNDOC", "DOC");
			TestCustomsCondition(CustomsConditionList.Get('Z'), "1Z", "赴境外加工光盘进口备案证明", true, false, "CNDOC", "DOC");
		}

		[Test]
		public void TestIsIgnorable()
		{
			Assert.AreEqual(true, new CustomsCondition("A", "", false, false).IsIgnorable);
			Assert.AreEqual(false, new CustomsCondition("a", "", false, false).IsIgnorable);
		}

		internal static void TestCustomsCondition(CustomsCondition condition
			, string code
			, string description
			, bool isImport
			, bool isExport
			, string conditionType
			, string conditionValueType)
		{
			Assert.AreEqual(code, condition.Code);
			Assert.AreEqual(description, condition.Description);
			Assert.AreEqual(isImport, condition.IsImport);
			Assert.AreEqual(isExport, condition.IsExport);
			Assert.AreEqual(conditionType, condition.ConditionType);
			Assert.AreEqual(conditionValueType, condition.ConditionValueType);
		}
	}
}
