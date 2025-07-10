using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business
{
	public partial class JobMessageTypeList : Common.Shared.SharedJobMessageTypeList, Integration.Customs.IDeclarationTypeListProvider
	{
		public JobMessageTypeList()
		{
			RemoveCode(Codes.WarehousedByExternalAgent);
			RemoveCode(Codes.ExportDeclarationByExternalBroker);
			RemoveCode(Codes.ImportDeclarationByExternalBroker);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors")]
		public new class MoreCodes : Common.Shared.SharedJobMessageTypeList.MoreCodes
		{
			public static bool RefreshDefaultWhenAttachedToDeclaration(string invoiceMessageType)
			{
				return string.IsNullOrEmpty(invoiceMessageType) || invoiceMessageType == MoreCodes.AdvanceShippingNotice;
			}
		}

		public static CodeDescriptionPairList GetListWithAdvanceShippingNotice(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("InvoiceHeaderMessageTypes", delegate
			{
				var result = new JobMessageTypeList();
				result.AddAdvanceShippingNotice();
				return result;
			});
		}

		public static CodeDescriptionPairList GetCachedListFor(BusinessObjectFactory factory, ZString country)
		{
			return factory.GetCachedValue("JobMessageTypeList_" + country, () =>
			{
				return GetNewListFor(country);
			});
		}

		public static CodeDescriptionPairList GetNewListFor(ZString country)
		{
			var customsCountry = Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(country);
			CodeDescriptionPairList result;
			switch (customsCountry)
			{
				case Core.Constants.CountryCodes.UnitedArabEmirates:
					result = new Common.AE.AEJobMessageTypeList();
					break;
				case Core.Constants.CountryCodes.Australia:
					result = new Common.AU.AUJobMessageTypeList();
					break;
				case Core.Constants.CountryCodes.Canada:
					result = new Common.CA.CAJobMessageTypeList();
					break;
				case Core.Constants.CountryCodes.Switzerland:
					result = new Common.CH.CHJobMessageTypeList();
					break;
				case Core.Constants.CountryCodes.NewZealand:
					result = new Common.NZ.NZJobMessageTypeList();
					break;
				case Core.Constants.CountryCodes.Singapore:
					result = new Common.SG.SGJobMessageTypeList();
					break;
				case Core.Constants.CountryCodes.UnitedStates:
					result = new Common.US.USJobMessageTypeList();
					break;
				case Core.Constants.CountryCodes.SouthAfrica:
					result = new Common.ZA.ZAJobMessageTypeList();
					break;
				case Core.Constants.CountryCodes.Ireland:
					result = new Common.IE.IEJobMessageTypeList();
					break;
				case Core.Constants.CountryCodes.Italy:
					result = new Common.IT.ITJobMessageTypeList();
					break;
				case Core.Constants.CountryCodes.Taiwan:
					result = new Common.TW.TWJobMessageTypeList();
					break;
				case Core.Constants.CountryCodes.Japan:
					result = new Common.JP.JPJobMessageTypeList();
					break;
				case Core.Constants.CountryCodes.KoreaSouth:
					result = new Common.KR.KRJobMessageTypeList();
					break;
				case Core.Constants.CountryCodes.Germany:
					result = new Common.DE.DEJobMessageTypeList();
					break;
				case Core.Constants.CountryCodes.Brazil:
					result = new Common.BR.BRJobMessageTypeList();

					var brDataRegistry = ObjectFactory.Get<Integration.Customs.BR.IBRCustomsDataRegistry>();
					if (!brDataRegistry.EnableImportLicense)
					{
						result.RemoveCode(Customs.Common.BR.BRJobMessageTypeList.Codes.ImportLicense);
					}
					if (!brDataRegistry.EnableLPCO)
					{
						result.RemoveCode(Customs.Common.BR.BRJobMessageTypeList.Codes.LPCO);
					}
					if (!brDataRegistry.EnableImportSiscomex)
					{
						result.RemoveCode(Customs.Common.BR.BRJobMessageTypeList.Codes.ImportSiscomex);
					}

					break;
				case Core.Constants.CountryCodes.Belgium:
					result = new Common.BE.BEJobMessageTypeList();
					break;
				case Core.Constants.CountryCodes.Poland:
					result = new Common.PL.PLJobMessageTypeList();
					break;
				default:
					if (ObjectFactory.Get<Integration.Customs.Shared.IEuropeanUnionCustomsMembersProvider>().IsInEuropeanCustomsUnionOrInheritsFromEU(customsCountry))
					{
						result = new Common.EU.EUJobMessageTypeList();
					}
					else if (ObjectFactory.Get<Integration.Customs.Shared.IAsycudaCustomsCountryProvider>().IsAsycudaCustomsCountry(customsCountry))
					{
						result = new Common.AsycudaCustoms.AsycudaJobMessageTypeList();
					}
					else
					{
						result = new JobMessageTypeList();
					}
					break;
			}
			result.Sort();
			return result;
		}

		#region IDeclarationTypeListProvider

		ICodeDescriptionPairList Integration.Customs.IDeclarationTypeListProvider.GetListFor(string country)
		{
			var result = new CodeDescriptionPairList();

			switch (country)
			{
				case Core.Constants.CountryCodes.Canada:
					{
						result.AddPair(Customs.Common.CA.CAJobMessageTypeList.Codes.Import, Customs.Common.CA.CAJobMessageTypeList.Descriptions.Import);
						result.AddPair(Customs.Common.CA.CAJobMessageTypeList.Codes.Export, Customs.Common.CA.CAJobMessageTypeList.Descriptions.Export);
						break;
					}

				case Core.Constants.CountryCodes.UnitedStates:
					{
						result.AddPair(Customs.Common.US.USJobMessageTypeList.Codes.Import, Customs.Common.US.USJobMessageTypeList.Descriptions.Import);
						result.AddPair(Customs.Common.US.USJobMessageTypeList.Codes.ImportByExternalBroker, Customs.Common.US.USJobMessageTypeList.Descriptions.ImportByExternalBroker);
						result.AddPair(Customs.Common.US.USJobMessageTypeList.Codes.Export, Customs.Common.US.USJobMessageTypeList.Descriptions.Export);
						result.AddPair(Customs.Common.US.USJobMessageTypeList.Codes.FTZ, Customs.Common.US.USJobMessageTypeList.Descriptions.FTZ);
						break;
					}

				default:
					{
						result = GetNewListFor(country);
						break;
					}
			}

			result.RemoveCode(Codes.MiscellaneousCustoms);

			return result;
		}

		#endregion
	}

	public static class JobMessageTypeListExtensions
	{
		public static void AddAdvanceShippingNotice(this CodeDescriptionPairList messageTypeList)
		{
			messageTypeList.AddPair(JobMessageTypeList.MoreCodes.AdvanceShippingNotice, JobMessageTypeList.MoreDescriptions.AdvanceShippingNotice);
		}
	}
}
