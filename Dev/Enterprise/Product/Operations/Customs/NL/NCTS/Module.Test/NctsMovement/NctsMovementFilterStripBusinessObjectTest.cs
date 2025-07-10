using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.NL.NCTS.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.NL.NCTS.Module.Testing;

[TestedType(typeof(NctsMovementFilterStripBusinessObject))]
sealed class NctsMovementFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
{
	public void TestNctsArrivalStatusList_Phase5() => CombineAssertions(() =>
	{
		var mockSettings = new Mock<Integration.Customs.Shared.INctsSettings>();
		mockSettings.Setup(x => x.IsUsingPhase5(It.IsAny<string>())).Returns(true);
		using (ObjectFactory.Substitute(mockSettings.Object))
		{
			var filterStrip = (NctsMovementFilterStripBusinessObject)GetNewFilterStripBusinessObject();

			var textFilterApplicationCode = (ModuleTextFilter)filterStrip[NctsMovementFilterStripBusinessObject.FilterConstants.ApplicationCode];
			textFilterApplicationCode.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact;
			textFilterApplicationCode.Property = ZString.Empty;

			var arrivalStatusFilter = (ModuleTextFilter)filterStrip[NctsMovementFilterStripBusinessObject.FilterConstants.ArrivalStatus];
			var arrivalStatusFilterCodeList = (CodeDescriptionPairList)arrivalStatusFilter.List;
			AssertContainsExactElementsInAnyOrder("Arrival when application code is empty", AddNcst4Codes(new NLNCTS5ArrivalCustomsStatusList()).GetAllCodes(), arrivalStatusFilterCodeList.GetAllCodes());
			textFilterApplicationCode.Property = CusInBondApplicationCodeList.Codes.NCTS5;

			arrivalStatusFilter = (ModuleTextFilter)filterStrip[NctsMovementFilterStripBusinessObject.FilterConstants.ArrivalStatus];
			arrivalStatusFilterCodeList = (CodeDescriptionPairList)arrivalStatusFilter.List;
			AssertContainsExactElementsInAnyOrder("Arrival when application code is NCTS5", new NLNCTS5ArrivalCustomsStatusList().GetAllCodes(), arrivalStatusFilterCodeList.GetAllCodes());
		}

		static CodeDescriptionPairList AddNcst4Codes(CodeDescriptionPairList codeList)
		{
			codeList.AddRange(new NctsTransitStatusList());
			return codeList;
		}
	});

	protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new NctsMovementFilterStripBusinessObject();
}
