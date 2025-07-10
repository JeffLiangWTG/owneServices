using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class RefExchangeRateLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestGetExchangeRateTypeList()
		{
			var oldGlobalCreditSecurity = Env.Security.GCBExchangeRateUpdate.IsAllowed;
			Env.Security.GCBExchangeRateUpdate.IsAllowed = false;

			var expectedListWithNoCustomsType = new string[]
			{
				Core.Constants.ExchangeRateTypes.Code.BuyRate,
				Core.Constants.ExchangeRateTypes.Code.SellRate,
				Core.Constants.ExchangeRateTypes.Code.PeriodEndRate,
				Core.Constants.ExchangeRateTypes.Code.IATARate,
			};

			Env.Security.CustomsExchangeRateUpdate.IsAllowed = false;
			var exchangeRate = RefExchangeRate.New(Factory);
			ClearCachedExRateTypes(Factory);
			AssertContainsExactElementsInAnyOrder(expectedListWithNoCustomsType, exchangeRate.Lookups.ExRateTypes.ToArray().Select(p => p.Code));
			AssertContainsExactElementsInAnyOrder(expectedListWithNoCustomsType, RefExchangeRateLookups.GetExchangeRateTypeList().ToArray().Select(p => p.Code));

			var expectedListWithCustomsType = new string[]
			{
				Core.Constants.ExchangeRateTypes.Code.CustomsRate,
				Core.Constants.ExchangeRateTypes.Code.CustomsRateSecondary,
				Core.Constants.ExchangeRateTypes.Code.CustomsMeasureEURExRate,
				Core.Constants.ExchangeRateTypes.Code.BuyRate,
				Core.Constants.ExchangeRateTypes.Code.SellRate,
				Core.Constants.ExchangeRateTypes.Code.PeriodEndRate,
				Core.Constants.ExchangeRateTypes.Code.IATARate,
			};

			Env.Security.CustomsExchangeRateUpdate.IsAllowed = true;
			exchangeRate = RefExchangeRate.New(Factory);
			ClearCachedExRateTypes(Factory);
			AssertContainsExactElementsInAnyOrder(expectedListWithCustomsType, exchangeRate.Lookups.ExRateTypes.ToArray().Select(p => p.Code));
			AssertContainsExactElementsInAnyOrder(expectedListWithCustomsType, RefExchangeRateLookups.GetExchangeRateTypeList().ToArray().Select(p => p.Code));

			var expectedListWithGlobalCreditType = new string[]
			{
				Core.Constants.ExchangeRateTypes.Code.CustomsRate,
				Core.Constants.ExchangeRateTypes.Code.CustomsRateSecondary,
				Core.Constants.ExchangeRateTypes.Code.CustomsMeasureEURExRate,
				Core.Constants.ExchangeRateTypes.Code.BuyRate,
				Core.Constants.ExchangeRateTypes.Code.SellRate,
				Core.Constants.ExchangeRateTypes.Code.PeriodEndRate,
				Core.Constants.ExchangeRateTypes.Code.IATARate,
				Core.Constants.ExchangeRateTypes.Code.GlobalCreditControl,
			};

			Env.Security.GCBExchangeRateUpdate.IsAllowed = true;
			exchangeRate = RefExchangeRate.New(Factory);
			ClearCachedExRateTypes(Factory);
			AssertContainsExactElementsInAnyOrder(expectedListWithGlobalCreditType, exchangeRate.Lookups.ExRateTypes.ToArray().Select(p => p.Code));
			AssertContainsExactElementsInAnyOrder(expectedListWithGlobalCreditType, RefExchangeRateLookups.GetExchangeRateTypeList().ToArray().Select(p => p.Code));

			// With default registry
			var expectedList = new string[]
			{
				Core.Constants.ExchangeRateTypes.Code.BuyRate,
				Core.Constants.ExchangeRateTypes.Code.SellRate,
				Core.Constants.ExchangeRateTypes.Code.CustomsRate,
				Core.Constants.ExchangeRateTypes.Code.CustomsRateSecondary,
				Core.Constants.ExchangeRateTypes.Code.CustomsMeasureEURExRate,
				Core.Constants.ExchangeRateTypes.Code.GlobalCreditControl,
				Core.Constants.ExchangeRateTypes.Code.PeriodEndRate,
				Core.Constants.ExchangeRateTypes.Code.IATARate,
			};
			ClearCachedExRateTypes(Factory);
			AssertContainsExactElementsInAnyOrder(expectedList, exchangeRate.Lookups.ExRateTypes.ToArray().Select(p => p.Code));
			AssertContainsExactElementsInAnyOrder(expectedList, RefExchangeRateLookups.GetExchangeRateTypeList().ToArray().Select(p => p.Code));
			AssertEquals(8, RefExchangeRateLookups.GetExchangeRateTypeList().Count);

			// With all custom exchange rates enabled
			var itemsSystemLevel = AccountingMasterFilesRegistry.Instance.CurrencyExchangeRateTypes.Value;
			itemsSystemLevel.Cast<CodeDescriptionBool>()
				.ForEach(x =>
				{
					x.Bool = true;
				});
			AccountingMasterFilesRegistry.Instance.CurrencyExchangeRateTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, itemsSystemLevel);

			var itemsCompanyLevel = AccountingMasterFilesRegistry.Instance.CurrencyExchangeRateTypesCompanySpecific.Value;
			itemsCompanyLevel.Cast<CodeDescriptionBool>()
				.ForEach(x =>
				{
					x.Bool = true;
				});
			AccountingMasterFilesRegistry.Instance.CurrencyExchangeRateTypesCompanySpecific.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, itemsCompanyLevel);

			expectedList = new string[]
			{
				Core.Constants.ExchangeRateTypes.Code.BuyRate,
				Core.Constants.ExchangeRateTypes.Code.SellRate,
				Core.Constants.ExchangeRateTypes.Code.CustomsRate,
				Core.Constants.ExchangeRateTypes.Code.CustomsRateSecondary,
				Core.Constants.ExchangeRateTypes.Code.CustomsMeasureEURExRate,
				Core.Constants.ExchangeRateTypes.Code.GlobalCreditControl,
				Core.Constants.ExchangeRateTypes.Code.PeriodEndRate,
				Core.Constants.ExchangeRateTypes.Code.IATARate,
				Core.Constants.ExchangeRateTypes.Code.C01Rate,
				Core.Constants.ExchangeRateTypes.Code.C02Rate,
				Core.Constants.ExchangeRateTypes.Code.C03Rate,
				Core.Constants.ExchangeRateTypes.Code.C04Rate,
				Core.Constants.ExchangeRateTypes.Code.C05Rate,
				Core.Constants.ExchangeRateTypes.Code.C06Rate,
				Core.Constants.ExchangeRateTypes.Code.C07Rate,
				Core.Constants.ExchangeRateTypes.Code.C08Rate,
				Core.Constants.ExchangeRateTypes.Code.C09Rate,
				Core.Constants.ExchangeRateTypes.Code.C10Rate,
				Core.Constants.ExchangeRateTypes.Code.C11Rate,
				Core.Constants.ExchangeRateTypes.Code.C12Rate,
				Core.Constants.ExchangeRateTypes.Code.C13Rate,
				Core.Constants.ExchangeRateTypes.Code.C14Rate,
				Core.Constants.ExchangeRateTypes.Code.C15Rate,
				Core.Constants.ExchangeRateTypes.Code.C16Rate,
				Core.Constants.ExchangeRateTypes.Code.C17Rate,
				Core.Constants.ExchangeRateTypes.Code.C18Rate,
				Core.Constants.ExchangeRateTypes.Code.C19Rate,
				Core.Constants.ExchangeRateTypes.Code.C20Rate,
				Core.Constants.ExchangeRateTypes.Code.C21Rate,
				Core.Constants.ExchangeRateTypes.Code.C22Rate,
				Core.Constants.ExchangeRateTypes.Code.C23Rate,
				Core.Constants.ExchangeRateTypes.Code.C24Rate,
				Core.Constants.ExchangeRateTypes.Code.C25Rate,
				Core.Constants.ExchangeRateTypes.Code.C26Rate,
				Core.Constants.ExchangeRateTypes.Code.C27Rate,
				Core.Constants.ExchangeRateTypes.Code.C28Rate,
				Core.Constants.ExchangeRateTypes.Code.C29Rate,
				Core.Constants.ExchangeRateTypes.Code.C30Rate,
				Core.Constants.ExchangeRateTypes.Code.C31Rate,
				Core.Constants.ExchangeRateTypes.Code.C32Rate,
				Core.Constants.ExchangeRateTypes.Code.C33Rate,
				Core.Constants.ExchangeRateTypes.Code.C34Rate,
				Core.Constants.ExchangeRateTypes.Code.C35Rate,
				Core.Constants.ExchangeRateTypes.Code.C36Rate,
				Core.Constants.ExchangeRateTypes.Code.C37Rate,
				Core.Constants.ExchangeRateTypes.Code.C38Rate,
				Core.Constants.ExchangeRateTypes.Code.C39Rate,
				Core.Constants.ExchangeRateTypes.Code.C40Rate,
				Core.Constants.ExchangeRateTypes.Code.C41Rate,
				Core.Constants.ExchangeRateTypes.Code.C42Rate,
				Core.Constants.ExchangeRateTypes.Code.C43Rate,
				Core.Constants.ExchangeRateTypes.Code.C44Rate,
				Core.Constants.ExchangeRateTypes.Code.C45Rate,
				Core.Constants.ExchangeRateTypes.Code.C46Rate,
				Core.Constants.ExchangeRateTypes.Code.C47Rate,
				Core.Constants.ExchangeRateTypes.Code.C48Rate,
				Core.Constants.ExchangeRateTypes.Code.C49Rate,
				Core.Constants.ExchangeRateTypes.Code.C50Rate,
				Core.Constants.ExchangeRateTypes.Code.C51Rate,
				Core.Constants.ExchangeRateTypes.Code.C52Rate,
				Core.Constants.ExchangeRateTypes.Code.C53Rate,
				Core.Constants.ExchangeRateTypes.Code.C54Rate,
				Core.Constants.ExchangeRateTypes.Code.C55Rate,
				Core.Constants.ExchangeRateTypes.Code.C56Rate,
				Core.Constants.ExchangeRateTypes.Code.C57Rate,
				Core.Constants.ExchangeRateTypes.Code.C58Rate,
				Core.Constants.ExchangeRateTypes.Code.C59Rate,
				Core.Constants.ExchangeRateTypes.Code.C60Rate,
				Core.Constants.ExchangeRateTypes.Code.C61Rate,
				Core.Constants.ExchangeRateTypes.Code.C62Rate,
				Core.Constants.ExchangeRateTypes.Code.C63Rate,
				Core.Constants.ExchangeRateTypes.Code.C64Rate,
				Core.Constants.ExchangeRateTypes.Code.C65Rate,
				Core.Constants.ExchangeRateTypes.Code.C66Rate,
				Core.Constants.ExchangeRateTypes.Code.C67Rate,
				Core.Constants.ExchangeRateTypes.Code.C68Rate,
				Core.Constants.ExchangeRateTypes.Code.C69Rate,
				Core.Constants.ExchangeRateTypes.Code.C70Rate,
				Core.Constants.ExchangeRateTypes.Code.C71Rate,
				Core.Constants.ExchangeRateTypes.Code.C72Rate,
				Core.Constants.ExchangeRateTypes.Code.C73Rate,
				Core.Constants.ExchangeRateTypes.Code.C74Rate,
				Core.Constants.ExchangeRateTypes.Code.C75Rate,
				Core.Constants.ExchangeRateTypes.Code.C76Rate,
				Core.Constants.ExchangeRateTypes.Code.C77Rate,
				Core.Constants.ExchangeRateTypes.Code.C78Rate,
				Core.Constants.ExchangeRateTypes.Code.C79Rate,
				Core.Constants.ExchangeRateTypes.Code.C80Rate,
				Core.Constants.ExchangeRateTypes.Code.C81Rate,
				Core.Constants.ExchangeRateTypes.Code.C82Rate,
				Core.Constants.ExchangeRateTypes.Code.C83Rate,
				Core.Constants.ExchangeRateTypes.Code.C84Rate,
				Core.Constants.ExchangeRateTypes.Code.C85Rate,
				Core.Constants.ExchangeRateTypes.Code.C86Rate,
				Core.Constants.ExchangeRateTypes.Code.C87Rate,
				Core.Constants.ExchangeRateTypes.Code.C88Rate,
				Core.Constants.ExchangeRateTypes.Code.C89Rate,
				Core.Constants.ExchangeRateTypes.Code.C90Rate,
				Core.Constants.ExchangeRateTypes.Code.C91Rate,
				Core.Constants.ExchangeRateTypes.Code.C92Rate,
				Core.Constants.ExchangeRateTypes.Code.C93Rate,
				Core.Constants.ExchangeRateTypes.Code.C94Rate,
				Core.Constants.ExchangeRateTypes.Code.C95Rate,
				Core.Constants.ExchangeRateTypes.Code.C96Rate,
				Core.Constants.ExchangeRateTypes.Code.C97Rate,
				Core.Constants.ExchangeRateTypes.Code.C98Rate,
				Core.Constants.ExchangeRateTypes.Code.C99Rate,
				Core.Constants.ExchangeRateTypes.Code.L01Rate,
				Core.Constants.ExchangeRateTypes.Code.L02Rate,
				Core.Constants.ExchangeRateTypes.Code.L03Rate,
				Core.Constants.ExchangeRateTypes.Code.L04Rate,
				Core.Constants.ExchangeRateTypes.Code.L05Rate,
				Core.Constants.ExchangeRateTypes.Code.L06Rate,
				Core.Constants.ExchangeRateTypes.Code.L07Rate,
				Core.Constants.ExchangeRateTypes.Code.L08Rate,
				Core.Constants.ExchangeRateTypes.Code.L09Rate,
				Core.Constants.ExchangeRateTypes.Code.L10Rate,
				Core.Constants.ExchangeRateTypes.Code.L11Rate,
				Core.Constants.ExchangeRateTypes.Code.L12Rate,
				Core.Constants.ExchangeRateTypes.Code.L13Rate,
				Core.Constants.ExchangeRateTypes.Code.L14Rate,
				Core.Constants.ExchangeRateTypes.Code.L15Rate,
				Core.Constants.ExchangeRateTypes.Code.L16Rate,
				Core.Constants.ExchangeRateTypes.Code.L17Rate,
				Core.Constants.ExchangeRateTypes.Code.L18Rate,
				Core.Constants.ExchangeRateTypes.Code.L19Rate,
				Core.Constants.ExchangeRateTypes.Code.L20Rate,
				Core.Constants.ExchangeRateTypes.Code.L21Rate,
				Core.Constants.ExchangeRateTypes.Code.L22Rate,
				Core.Constants.ExchangeRateTypes.Code.L23Rate,
				Core.Constants.ExchangeRateTypes.Code.L24Rate,
				Core.Constants.ExchangeRateTypes.Code.L25Rate,
				Core.Constants.ExchangeRateTypes.Code.L26Rate,
				Core.Constants.ExchangeRateTypes.Code.L27Rate,
				Core.Constants.ExchangeRateTypes.Code.L28Rate,
				Core.Constants.ExchangeRateTypes.Code.L29Rate,
				Core.Constants.ExchangeRateTypes.Code.L30Rate,
				Core.Constants.ExchangeRateTypes.Code.L31Rate,
				Core.Constants.ExchangeRateTypes.Code.L32Rate,
				Core.Constants.ExchangeRateTypes.Code.L33Rate,
				Core.Constants.ExchangeRateTypes.Code.L34Rate,
				Core.Constants.ExchangeRateTypes.Code.L35Rate,
				Core.Constants.ExchangeRateTypes.Code.L36Rate,
				Core.Constants.ExchangeRateTypes.Code.L37Rate,
				Core.Constants.ExchangeRateTypes.Code.L38Rate,
				Core.Constants.ExchangeRateTypes.Code.L39Rate,
				Core.Constants.ExchangeRateTypes.Code.L40Rate,
				Core.Constants.ExchangeRateTypes.Code.L41Rate,
				Core.Constants.ExchangeRateTypes.Code.L42Rate,
				Core.Constants.ExchangeRateTypes.Code.L43Rate,
				Core.Constants.ExchangeRateTypes.Code.L44Rate,
				Core.Constants.ExchangeRateTypes.Code.L45Rate,
				Core.Constants.ExchangeRateTypes.Code.L46Rate,
				Core.Constants.ExchangeRateTypes.Code.L47Rate,
				Core.Constants.ExchangeRateTypes.Code.L48Rate,
				Core.Constants.ExchangeRateTypes.Code.L49Rate,
				Core.Constants.ExchangeRateTypes.Code.L50Rate,
				Core.Constants.ExchangeRateTypes.Code.L51Rate,
				Core.Constants.ExchangeRateTypes.Code.L52Rate,
				Core.Constants.ExchangeRateTypes.Code.L53Rate,
				Core.Constants.ExchangeRateTypes.Code.L54Rate,
				Core.Constants.ExchangeRateTypes.Code.L55Rate,
				Core.Constants.ExchangeRateTypes.Code.L56Rate,
				Core.Constants.ExchangeRateTypes.Code.L57Rate,
				Core.Constants.ExchangeRateTypes.Code.L58Rate,
				Core.Constants.ExchangeRateTypes.Code.L59Rate,
				Core.Constants.ExchangeRateTypes.Code.L60Rate,
				Core.Constants.ExchangeRateTypes.Code.L61Rate,
				Core.Constants.ExchangeRateTypes.Code.L62Rate,
				Core.Constants.ExchangeRateTypes.Code.L63Rate,
				Core.Constants.ExchangeRateTypes.Code.L64Rate,
				Core.Constants.ExchangeRateTypes.Code.L65Rate,
				Core.Constants.ExchangeRateTypes.Code.L66Rate,
				Core.Constants.ExchangeRateTypes.Code.L67Rate,
				Core.Constants.ExchangeRateTypes.Code.L68Rate,
				Core.Constants.ExchangeRateTypes.Code.L69Rate,
				Core.Constants.ExchangeRateTypes.Code.L70Rate,
				Core.Constants.ExchangeRateTypes.Code.L71Rate,
				Core.Constants.ExchangeRateTypes.Code.L72Rate,
				Core.Constants.ExchangeRateTypes.Code.L73Rate,
				Core.Constants.ExchangeRateTypes.Code.L74Rate,
				Core.Constants.ExchangeRateTypes.Code.L75Rate,
				Core.Constants.ExchangeRateTypes.Code.L76Rate,
				Core.Constants.ExchangeRateTypes.Code.L77Rate,
				Core.Constants.ExchangeRateTypes.Code.L78Rate,
				Core.Constants.ExchangeRateTypes.Code.L79Rate,
				Core.Constants.ExchangeRateTypes.Code.L80Rate,
				Core.Constants.ExchangeRateTypes.Code.L81Rate,
				Core.Constants.ExchangeRateTypes.Code.L82Rate,
				Core.Constants.ExchangeRateTypes.Code.L83Rate,
				Core.Constants.ExchangeRateTypes.Code.L84Rate,
				Core.Constants.ExchangeRateTypes.Code.L85Rate,
				Core.Constants.ExchangeRateTypes.Code.L86Rate,
				Core.Constants.ExchangeRateTypes.Code.L87Rate,
				Core.Constants.ExchangeRateTypes.Code.L88Rate,
				Core.Constants.ExchangeRateTypes.Code.L89Rate,
				Core.Constants.ExchangeRateTypes.Code.L90Rate,
				Core.Constants.ExchangeRateTypes.Code.L91Rate,
				Core.Constants.ExchangeRateTypes.Code.L92Rate,
				Core.Constants.ExchangeRateTypes.Code.L93Rate,
				Core.Constants.ExchangeRateTypes.Code.L94Rate,
				Core.Constants.ExchangeRateTypes.Code.L95Rate,
				Core.Constants.ExchangeRateTypes.Code.L96Rate,
				Core.Constants.ExchangeRateTypes.Code.L97Rate,
				Core.Constants.ExchangeRateTypes.Code.L98Rate,
				Core.Constants.ExchangeRateTypes.Code.L99Rate,
			};
			AssertEquals(206, RefExchangeRateLookups.GetExchangeRateTypeList().Count);
			AssertContainsExactElementsInAnyOrder(expectedList, RefExchangeRateLookups.GetExchangeRateTypeList().ToArray().Select(p => p.Code));

			// With few custom rates disabled
			itemsSystemLevel.Cast<CodeDescriptionBool>()
				.ForEach(x =>
				{
					if (x.Code.Equals(Core.Constants.ExchangeRateTypes.Code.C01Rate) || x.Code.Equals(Core.Constants.ExchangeRateTypes.Code.C05Rate) || x.Code.Equals(Core.Constants.ExchangeRateTypes.Code.C09Rate))
					{
						x.Bool = false;
					}
				});
			AccountingMasterFilesRegistry.Instance.CurrencyExchangeRateTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, itemsSystemLevel);

			itemsCompanyLevel.Cast<CodeDescriptionBool>()
				.ForEach(x =>
				{
					if (x.Code.Equals(Core.Constants.ExchangeRateTypes.Code.L01Rate) || x.Code.Equals(Core.Constants.ExchangeRateTypes.Code.L05Rate) || x.Code.Equals(Core.Constants.ExchangeRateTypes.Code.L09Rate))
					{
						x.Bool = false;
					}
				});
			AccountingMasterFilesRegistry.Instance.CurrencyExchangeRateTypesCompanySpecific.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, itemsCompanyLevel);

			expectedList = new string[]
			{
				Core.Constants.ExchangeRateTypes.Code.BuyRate,
				Core.Constants.ExchangeRateTypes.Code.SellRate,
				Core.Constants.ExchangeRateTypes.Code.CustomsRate,
				Core.Constants.ExchangeRateTypes.Code.CustomsRateSecondary,
				Core.Constants.ExchangeRateTypes.Code.CustomsMeasureEURExRate,
				Core.Constants.ExchangeRateTypes.Code.GlobalCreditControl,
				Core.Constants.ExchangeRateTypes.Code.PeriodEndRate,
				Core.Constants.ExchangeRateTypes.Code.IATARate,
				Core.Constants.ExchangeRateTypes.Code.C02Rate,
				Core.Constants.ExchangeRateTypes.Code.C03Rate,
				Core.Constants.ExchangeRateTypes.Code.C04Rate,
				Core.Constants.ExchangeRateTypes.Code.C06Rate,
				Core.Constants.ExchangeRateTypes.Code.C07Rate,
				Core.Constants.ExchangeRateTypes.Code.C08Rate,
				Core.Constants.ExchangeRateTypes.Code.C10Rate,
				Core.Constants.ExchangeRateTypes.Code.C11Rate,
				Core.Constants.ExchangeRateTypes.Code.C12Rate,
				Core.Constants.ExchangeRateTypes.Code.C13Rate,
				Core.Constants.ExchangeRateTypes.Code.C14Rate,
				Core.Constants.ExchangeRateTypes.Code.C15Rate,
				Core.Constants.ExchangeRateTypes.Code.C16Rate,
				Core.Constants.ExchangeRateTypes.Code.C17Rate,
				Core.Constants.ExchangeRateTypes.Code.C18Rate,
				Core.Constants.ExchangeRateTypes.Code.C19Rate,
				Core.Constants.ExchangeRateTypes.Code.C20Rate,
				Core.Constants.ExchangeRateTypes.Code.C21Rate,
				Core.Constants.ExchangeRateTypes.Code.C22Rate,
				Core.Constants.ExchangeRateTypes.Code.C23Rate,
				Core.Constants.ExchangeRateTypes.Code.C24Rate,
				Core.Constants.ExchangeRateTypes.Code.C25Rate,
				Core.Constants.ExchangeRateTypes.Code.C26Rate,
				Core.Constants.ExchangeRateTypes.Code.C27Rate,
				Core.Constants.ExchangeRateTypes.Code.C28Rate,
				Core.Constants.ExchangeRateTypes.Code.C29Rate,
				Core.Constants.ExchangeRateTypes.Code.C30Rate,
				Core.Constants.ExchangeRateTypes.Code.C31Rate,
				Core.Constants.ExchangeRateTypes.Code.C32Rate,
				Core.Constants.ExchangeRateTypes.Code.C33Rate,
				Core.Constants.ExchangeRateTypes.Code.C34Rate,
				Core.Constants.ExchangeRateTypes.Code.C35Rate,
				Core.Constants.ExchangeRateTypes.Code.C36Rate,
				Core.Constants.ExchangeRateTypes.Code.C37Rate,
				Core.Constants.ExchangeRateTypes.Code.C38Rate,
				Core.Constants.ExchangeRateTypes.Code.C39Rate,
				Core.Constants.ExchangeRateTypes.Code.C40Rate,
				Core.Constants.ExchangeRateTypes.Code.C41Rate,
				Core.Constants.ExchangeRateTypes.Code.C42Rate,
				Core.Constants.ExchangeRateTypes.Code.C43Rate,
				Core.Constants.ExchangeRateTypes.Code.C44Rate,
				Core.Constants.ExchangeRateTypes.Code.C45Rate,
				Core.Constants.ExchangeRateTypes.Code.C46Rate,
				Core.Constants.ExchangeRateTypes.Code.C47Rate,
				Core.Constants.ExchangeRateTypes.Code.C48Rate,
				Core.Constants.ExchangeRateTypes.Code.C49Rate,
				Core.Constants.ExchangeRateTypes.Code.C50Rate,
				Core.Constants.ExchangeRateTypes.Code.C51Rate,
				Core.Constants.ExchangeRateTypes.Code.C52Rate,
				Core.Constants.ExchangeRateTypes.Code.C53Rate,
				Core.Constants.ExchangeRateTypes.Code.C54Rate,
				Core.Constants.ExchangeRateTypes.Code.C55Rate,
				Core.Constants.ExchangeRateTypes.Code.C56Rate,
				Core.Constants.ExchangeRateTypes.Code.C57Rate,
				Core.Constants.ExchangeRateTypes.Code.C58Rate,
				Core.Constants.ExchangeRateTypes.Code.C59Rate,
				Core.Constants.ExchangeRateTypes.Code.C60Rate,
				Core.Constants.ExchangeRateTypes.Code.C61Rate,
				Core.Constants.ExchangeRateTypes.Code.C62Rate,
				Core.Constants.ExchangeRateTypes.Code.C63Rate,
				Core.Constants.ExchangeRateTypes.Code.C64Rate,
				Core.Constants.ExchangeRateTypes.Code.C65Rate,
				Core.Constants.ExchangeRateTypes.Code.C66Rate,
				Core.Constants.ExchangeRateTypes.Code.C67Rate,
				Core.Constants.ExchangeRateTypes.Code.C68Rate,
				Core.Constants.ExchangeRateTypes.Code.C69Rate,
				Core.Constants.ExchangeRateTypes.Code.C70Rate,
				Core.Constants.ExchangeRateTypes.Code.C71Rate,
				Core.Constants.ExchangeRateTypes.Code.C72Rate,
				Core.Constants.ExchangeRateTypes.Code.C73Rate,
				Core.Constants.ExchangeRateTypes.Code.C74Rate,
				Core.Constants.ExchangeRateTypes.Code.C75Rate,
				Core.Constants.ExchangeRateTypes.Code.C76Rate,
				Core.Constants.ExchangeRateTypes.Code.C77Rate,
				Core.Constants.ExchangeRateTypes.Code.C78Rate,
				Core.Constants.ExchangeRateTypes.Code.C79Rate,
				Core.Constants.ExchangeRateTypes.Code.C80Rate,
				Core.Constants.ExchangeRateTypes.Code.C81Rate,
				Core.Constants.ExchangeRateTypes.Code.C82Rate,
				Core.Constants.ExchangeRateTypes.Code.C83Rate,
				Core.Constants.ExchangeRateTypes.Code.C84Rate,
				Core.Constants.ExchangeRateTypes.Code.C85Rate,
				Core.Constants.ExchangeRateTypes.Code.C86Rate,
				Core.Constants.ExchangeRateTypes.Code.C87Rate,
				Core.Constants.ExchangeRateTypes.Code.C88Rate,
				Core.Constants.ExchangeRateTypes.Code.C89Rate,
				Core.Constants.ExchangeRateTypes.Code.C90Rate,
				Core.Constants.ExchangeRateTypes.Code.C91Rate,
				Core.Constants.ExchangeRateTypes.Code.C92Rate,
				Core.Constants.ExchangeRateTypes.Code.C93Rate,
				Core.Constants.ExchangeRateTypes.Code.C94Rate,
				Core.Constants.ExchangeRateTypes.Code.C95Rate,
				Core.Constants.ExchangeRateTypes.Code.C96Rate,
				Core.Constants.ExchangeRateTypes.Code.C97Rate,
				Core.Constants.ExchangeRateTypes.Code.C98Rate,
				Core.Constants.ExchangeRateTypes.Code.C99Rate,
				Core.Constants.ExchangeRateTypes.Code.L02Rate,
				Core.Constants.ExchangeRateTypes.Code.L03Rate,
				Core.Constants.ExchangeRateTypes.Code.L04Rate,
				Core.Constants.ExchangeRateTypes.Code.L06Rate,
				Core.Constants.ExchangeRateTypes.Code.L07Rate,
				Core.Constants.ExchangeRateTypes.Code.L08Rate,
				Core.Constants.ExchangeRateTypes.Code.L10Rate,
				Core.Constants.ExchangeRateTypes.Code.L11Rate,
				Core.Constants.ExchangeRateTypes.Code.L12Rate,
				Core.Constants.ExchangeRateTypes.Code.L13Rate,
				Core.Constants.ExchangeRateTypes.Code.L14Rate,
				Core.Constants.ExchangeRateTypes.Code.L15Rate,
				Core.Constants.ExchangeRateTypes.Code.L16Rate,
				Core.Constants.ExchangeRateTypes.Code.L17Rate,
				Core.Constants.ExchangeRateTypes.Code.L18Rate,
				Core.Constants.ExchangeRateTypes.Code.L19Rate,
				Core.Constants.ExchangeRateTypes.Code.L20Rate,
				Core.Constants.ExchangeRateTypes.Code.L21Rate,
				Core.Constants.ExchangeRateTypes.Code.L22Rate,
				Core.Constants.ExchangeRateTypes.Code.L23Rate,
				Core.Constants.ExchangeRateTypes.Code.L24Rate,
				Core.Constants.ExchangeRateTypes.Code.L25Rate,
				Core.Constants.ExchangeRateTypes.Code.L26Rate,
				Core.Constants.ExchangeRateTypes.Code.L27Rate,
				Core.Constants.ExchangeRateTypes.Code.L28Rate,
				Core.Constants.ExchangeRateTypes.Code.L29Rate,
				Core.Constants.ExchangeRateTypes.Code.L30Rate,
				Core.Constants.ExchangeRateTypes.Code.L31Rate,
				Core.Constants.ExchangeRateTypes.Code.L32Rate,
				Core.Constants.ExchangeRateTypes.Code.L33Rate,
				Core.Constants.ExchangeRateTypes.Code.L34Rate,
				Core.Constants.ExchangeRateTypes.Code.L35Rate,
				Core.Constants.ExchangeRateTypes.Code.L36Rate,
				Core.Constants.ExchangeRateTypes.Code.L37Rate,
				Core.Constants.ExchangeRateTypes.Code.L38Rate,
				Core.Constants.ExchangeRateTypes.Code.L39Rate,
				Core.Constants.ExchangeRateTypes.Code.L40Rate,
				Core.Constants.ExchangeRateTypes.Code.L41Rate,
				Core.Constants.ExchangeRateTypes.Code.L42Rate,
				Core.Constants.ExchangeRateTypes.Code.L43Rate,
				Core.Constants.ExchangeRateTypes.Code.L44Rate,
				Core.Constants.ExchangeRateTypes.Code.L45Rate,
				Core.Constants.ExchangeRateTypes.Code.L46Rate,
				Core.Constants.ExchangeRateTypes.Code.L47Rate,
				Core.Constants.ExchangeRateTypes.Code.L48Rate,
				Core.Constants.ExchangeRateTypes.Code.L49Rate,
				Core.Constants.ExchangeRateTypes.Code.L50Rate,
				Core.Constants.ExchangeRateTypes.Code.L51Rate,
				Core.Constants.ExchangeRateTypes.Code.L52Rate,
				Core.Constants.ExchangeRateTypes.Code.L53Rate,
				Core.Constants.ExchangeRateTypes.Code.L54Rate,
				Core.Constants.ExchangeRateTypes.Code.L55Rate,
				Core.Constants.ExchangeRateTypes.Code.L56Rate,
				Core.Constants.ExchangeRateTypes.Code.L57Rate,
				Core.Constants.ExchangeRateTypes.Code.L58Rate,
				Core.Constants.ExchangeRateTypes.Code.L59Rate,
				Core.Constants.ExchangeRateTypes.Code.L60Rate,
				Core.Constants.ExchangeRateTypes.Code.L61Rate,
				Core.Constants.ExchangeRateTypes.Code.L62Rate,
				Core.Constants.ExchangeRateTypes.Code.L63Rate,
				Core.Constants.ExchangeRateTypes.Code.L64Rate,
				Core.Constants.ExchangeRateTypes.Code.L65Rate,
				Core.Constants.ExchangeRateTypes.Code.L66Rate,
				Core.Constants.ExchangeRateTypes.Code.L67Rate,
				Core.Constants.ExchangeRateTypes.Code.L68Rate,
				Core.Constants.ExchangeRateTypes.Code.L69Rate,
				Core.Constants.ExchangeRateTypes.Code.L70Rate,
				Core.Constants.ExchangeRateTypes.Code.L71Rate,
				Core.Constants.ExchangeRateTypes.Code.L72Rate,
				Core.Constants.ExchangeRateTypes.Code.L73Rate,
				Core.Constants.ExchangeRateTypes.Code.L74Rate,
				Core.Constants.ExchangeRateTypes.Code.L75Rate,
				Core.Constants.ExchangeRateTypes.Code.L76Rate,
				Core.Constants.ExchangeRateTypes.Code.L77Rate,
				Core.Constants.ExchangeRateTypes.Code.L78Rate,
				Core.Constants.ExchangeRateTypes.Code.L79Rate,
				Core.Constants.ExchangeRateTypes.Code.L80Rate,
				Core.Constants.ExchangeRateTypes.Code.L81Rate,
				Core.Constants.ExchangeRateTypes.Code.L82Rate,
				Core.Constants.ExchangeRateTypes.Code.L83Rate,
				Core.Constants.ExchangeRateTypes.Code.L84Rate,
				Core.Constants.ExchangeRateTypes.Code.L85Rate,
				Core.Constants.ExchangeRateTypes.Code.L86Rate,
				Core.Constants.ExchangeRateTypes.Code.L87Rate,
				Core.Constants.ExchangeRateTypes.Code.L88Rate,
				Core.Constants.ExchangeRateTypes.Code.L89Rate,
				Core.Constants.ExchangeRateTypes.Code.L90Rate,
				Core.Constants.ExchangeRateTypes.Code.L91Rate,
				Core.Constants.ExchangeRateTypes.Code.L92Rate,
				Core.Constants.ExchangeRateTypes.Code.L93Rate,
				Core.Constants.ExchangeRateTypes.Code.L94Rate,
				Core.Constants.ExchangeRateTypes.Code.L95Rate,
				Core.Constants.ExchangeRateTypes.Code.L96Rate,
				Core.Constants.ExchangeRateTypes.Code.L97Rate,
				Core.Constants.ExchangeRateTypes.Code.L98Rate,
				Core.Constants.ExchangeRateTypes.Code.L99Rate,
			};
			AssertEquals(200, RefExchangeRateLookups.GetExchangeRateTypeList().Count);
			AssertContainsExactElementsInAnyOrder(expectedList, RefExchangeRateLookups.GetExchangeRateTypeList().ToArray().Select(p => p.Code));

			Env.Security.GCBExchangeRateUpdate.IsAllowed = oldGlobalCreditSecurity;
		}

		static void ClearCachedExRateTypes(BusinessObjectFactory factory)
		{
			factory.ClearCachedValue<CodeDescriptionPairList>("RefExchangeRateLookups.ExRateTypes");
		}

		public void TestRefExchangeRateLookupsColumnTypes()
		{
			var exchangeRate = RefExchangeRate.New(Factory);
			AssertType<RefCurrencyCollection>("Should be RefCurrencyCollection", exchangeRate.Lookups.Currencies);
			AssertType<DebtorCollection>("Should be DebtorCollection", exchangeRate.Lookups.LocalClients);
		}

		public void TestCurrencies_IN()
		{
			var helper = ObjectFactory.Get<Enterprise.Integration.Customs.Shared.Universal.IUniversalReferenceTestDataHelper>("Universal.IUniversalReferenceTestDataHelper", Factory);
			helper.CreateNewOrGetExistingDataGrouping("IN", "India");
			helper.CreateNewOrGetExistingCusCodeType("SDCUR", "IN Customs Standard Currency List", "IN");
			helper.CreateNewOrGetExistingCusCodeList("IN", "SDCUR", "USD", "United States Dollar", new ZDateTime(2019, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList("IN", "SDCUR", "KRW", "South Korean Won", new ZDateTime(2019, 1, 1), new ZDateTime(2079, 6, 6));
			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			{
				var exchangeRate = RefExchangeRate.New(Factory);
				exchangeRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRate;
				var currencies = exchangeRate.Lookups.Currencies;
				AssertContainsExactElementsInExactOrder(new string[] { "KRW", "USD" }, currencies.Select(x => x.Code));
				exchangeRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRateSecondary;
				var currencies2 = exchangeRate.Lookups.Currencies;
				AssertSame("collection is same for CUE and cached", currencies, currencies2);
			}
		}
	}
}
