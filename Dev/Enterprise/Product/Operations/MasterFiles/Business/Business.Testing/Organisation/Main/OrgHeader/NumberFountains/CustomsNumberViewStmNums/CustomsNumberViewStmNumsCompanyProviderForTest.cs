using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Moq;
using Moq.Protected;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class CustomsNumberViewStmNumsCompanyProviderForTest : CustomsNumberViewStmNumsCompanyProvider
	{
		public CustomsNumberViewStmNumsCompanyProviderForTest(BusinessObjectFactory factory, ZString countryCode, ZGuid companyPk, bool? enableCompanyLevelForTesting = true, bool? enableBranchLevelForTesting = true)
			: base(factory, countryCode, companyPk)
		{
			this.enableCompanyLevelForTesting = enableCompanyLevelForTesting.GetValueOrDefault();
			this.enableBranchLevelForTesting = enableBranchLevelForTesting.GetValueOrDefault();
		}

		public static CustomsNumberViewStmNumsCompanyProviderForTest New(BusinessObjectFactory factory)
		{
			return new CustomsNumberViewStmNumsCompanyProviderForTest(factory, Core.Constants.CountryCodes.Eritrea, GlbCompany.CurrentCompany.PK);
		}

		protected override ZString GetReasonForNotAbleToModifyCore(CustomsNumberViewStmNumsWrapper wrapper) => getReasonForNotAbleToModifyForTesting?.Invoke(wrapper) ?? ZString.Empty;
		public Func<CustomsNumberViewStmNumsWrapper, ZString> getReasonForNotAbleToModifyForTesting;

		public CustomsNumberViewStmNums NewCustomsNumber()
		{
			var stmNums = CustomsNumberViewStmNumsHelper.NewStmNums(Factory, this, Parent.PK);
			stmNums.SN_Type = "CEN";
			stmNums.SN_FountainName = "BOB NUMBER";
			return stmNums;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1199", Justification = "Must be translatable by Enterprise.ZArchitecture.GUI.Testing.BasherTest.TestFormIsFullyTranslatable()")]
		protected internal override CustomsNumberViewStmNumsLookups GetNewLookups(CustomsNumberViewStmNums stmNums)
		{
			var mock = new Mock<CustomsNumberViewStmNumsLookups>(stmNums);
			mock.CallBase = true;
			mock.Setup(m => m.TypeList)
				.Returns(stmNums.Factory.GetCachedValue("MockCustomsNumberViewStmNumsTypeList",
					() =>
					{
						var list = new CodeDescriptionPairList();
						list.AddPair("CEN", ResString.GetMultilingualString("{A4774E4E-16D9-463C-A280-948E268FA977}", "CEN"));
						return list;
					}));
			return mock.Object;
		}

		protected override CustomsNumberViewStmNumsSetting GetSettingCore(ZString rangeType)
		{
			var settingMock = new Mock<CustomsNumberViewStmNumsSetting>(new object[] { Parent, rangeType });
			settingMock.CallBase = true;
			if (ThresholdRunOutWarningForTesting.HasValue)
			{
				settingMock.Protected().Setup<ZLong>("GetThresholdRunOutWarningCore").Returns(ThresholdRunOutWarningForTesting.Value);
			}

			if (AllowDuplicatedTypeForTesting.HasValue)
			{
				settingMock.Protected().Setup<bool>("AllowDuplicateCore").Returns(AllowDuplicatedTypeForTesting.Value);
			}

			if (CanRolloverForTesting.HasValue)
			{
				settingMock.Protected().Setup<bool>("CanRolloverCore").Returns(CanRolloverForTesting.Value);
			}
			if (UsedNumbersForTesting != null)
			{
				settingMock.Protected()
					.Setup<bool>("IsNumberUsedCore", ItExpr.IsAny<CustomsNumberViewStmNums>(), ItExpr.IsAny<ZString>())
					.Returns<CustomsNumberViewStmNums, ZString>((stmNums, number) =>
					{
						return UsedNumbersForTesting.TryGetValue(stmNums.SN_Type, out var list) && list.Contains(number);
					});
			}

			return settingMock.Object;
		}

		public ZLong? ThresholdRunOutWarningForTesting;
		public bool? AllowDuplicatedTypeForTesting;
		public bool? CanRolloverForTesting;
		public Dictionary<ZString, List<ZString>> UsedNumbersForTesting;

		public override bool EnableBranchLevel => enableBranchLevelForTesting;
		readonly bool enableBranchLevelForTesting;

		public override bool EnableCompanyLevel => enableCompanyLevelForTesting;
		readonly bool enableCompanyLevelForTesting;

		protected override CustomsNumberViewStmNumsWrapper CreateWrapperCore(CustomsNumberViewStmNums stmNums)
		{
			return WrapperTypeForTesting == null ? base.CreateWrapperCore(stmNums) : (CustomsNumberViewStmNumsWrapper)Activator.CreateInstance(WrapperTypeForTesting, new object[] { stmNums });
		}

		protected override Type WrapperType => WrapperTypeForTesting ?? base.WrapperType;
		public Type WrapperTypeForTesting;
	}
}
