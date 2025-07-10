using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.SG;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.SG.V4.Business
{
	public class JobDeclarationLookups : Customs.Business.JobDeclarationLookups
	{
		public JobDeclarationLookups(JobDeclaration parent)
			: base(parent)
		{
		}

		public JobDeclaration Declaration
		{
			get { return Parent; }
		}

		protected new JobDeclaration Parent
		{
			get { return (JobDeclaration)base.Parent; }
		}

		public override CodeDescriptionPairList JE_TotalNoOfPacksPackType_List => Factory.GetCachedValue<UnitOfQuantityCodeList>();

		public override CodeDescriptionPairList PaymentPartyList => Factory.GetCachedValue<BGIndicatorCodeList>();

		public override CodeDescriptionPairList MessageSubTypeList
		{
			get
			{
				CodeDescriptionPairList result = new CodeDescriptionPairList();
				switch (Declaration.JE_MessageType)
				{
					case MessageTypeCodeList.Codes.IPT:
						result = IPTMessageSubTypeList;
						break;
					case MessageTypeCodeList.Codes.INP:
						result = INPMessageSubTypeList;
						break;
					case MessageTypeCodeList.Codes.OUT:
						result = OUTMessageSubTypeList;
						break;
					case MessageTypeCodeList.Codes.TNP:
						result = TNPMessageSubTypeList;
						break;
					case MessageTypeCodeList.Codes.COO:
						result = EmptyMessageSubTypeList;  // Certificate of Origin does not have any sub-type
						break;
					default:
						result = new DeclarationTypeCodeList();
						break;
				}

				return result;
			}
		}

		public CodeDescriptionPairList IPTMessageSubTypeList
		{
			get
			{
				CodeDescriptionPairList result = new CodeDescriptionPairList();
				result.AddPair(DeclarationTypeCodeList.Codes.DUT, DeclarationTypeCodeList.Descriptions.DUT);
				result.AddPair(DeclarationTypeCodeList.Codes.GST, DeclarationTypeCodeList.Descriptions.GST);
				result.AddPair(DeclarationTypeCodeList.Codes.DNG, DeclarationTypeCodeList.Descriptions.DNG);
				result.AddPair(DeclarationTypeCodeList.Codes.BKP, DeclarationTypeCodeList.Descriptions.BKP);

				return result;
			}
		}

		public CodeDescriptionPairList INPMessageSubTypeList
		{
			get
			{
				CodeDescriptionPairList result = new CodeDescriptionPairList();
				result.AddPair(DeclarationTypeCodeList.Codes.SFZ, DeclarationTypeCodeList.Descriptions.SFZ);
				result.AddPair(DeclarationTypeCodeList.Codes.APS, DeclarationTypeCodeList.Descriptions.APS);
				result.AddPair(DeclarationTypeCodeList.Codes.BKN, DeclarationTypeCodeList.Descriptions.BKN);
				result.AddPair(DeclarationTypeCodeList.Codes.GTR, DeclarationTypeCodeList.Descriptions.GTR);
				result.AddPair(DeclarationTypeCodeList.Codes.SHO, DeclarationTypeCodeList.Descriptions.SHO);
				result.AddPair(DeclarationTypeCodeList.Codes.DES, DeclarationTypeCodeList.Descriptions.DES);
				result.AddPair(DeclarationTypeCodeList.Codes.REX, DeclarationTypeCodeList.Descriptions.REX);
				result.AddPair(DeclarationTypeCodeList.Codes.TCS, DeclarationTypeCodeList.Descriptions.TCS);
				result.AddPair(DeclarationTypeCodeList.Codes.TCR, DeclarationTypeCodeList.Descriptions.TCR);
				result.AddPair(DeclarationTypeCodeList.Codes.TCE, DeclarationTypeCodeList.Descriptions.TCE);
				result.AddPair(DeclarationTypeCodeList.Codes.TCO, DeclarationTypeCodeList.Descriptions.TCO);
				result.AddPair(DeclarationTypeCodeList.Codes.TCI, DeclarationTypeCodeList.Descriptions.TCI);

				return result;
			}
		}

		public CodeDescriptionPairList OUTMessageSubTypeList
		{
			get
			{
				CodeDescriptionPairList result = new CodeDescriptionPairList();
				result.AddPair(DeclarationTypeCodeList.Codes.DRT, DeclarationTypeCodeList.Descriptions.DRT);
				result.AddPair(DeclarationTypeCodeList.Codes.BKO, DeclarationTypeCodeList.Descriptions.BKO);
				result.AddPair(DeclarationTypeCodeList.Codes.APS, DeclarationTypeCodeList.Descriptions.APS);
				result.AddPair(DeclarationTypeCodeList.Codes.TCS, DeclarationTypeCodeList.Descriptions.TCS);
				result.AddPair(DeclarationTypeCodeList.Codes.TCR, DeclarationTypeCodeList.Descriptions.TCR);
				result.AddPair(DeclarationTypeCodeList.Codes.TCE, DeclarationTypeCodeList.Descriptions.TCE);
				result.AddPair(DeclarationTypeCodeList.Codes.TCO, DeclarationTypeCodeList.Descriptions.TCO);
				result.AddPair(DeclarationTypeCodeList.Codes.TCI, DeclarationTypeCodeList.Descriptions.TCI);

				return result;
			}
		}

		public CodeDescriptionPairList TNPMessageSubTypeList
		{
			get
			{
				CodeDescriptionPairList result = new CodeDescriptionPairList();
				result.AddPair(DeclarationTypeCodeList.Codes.TTI, DeclarationTypeCodeList.Descriptions.TTI);
				result.AddPair(DeclarationTypeCodeList.Codes.TTF, DeclarationTypeCodeList.Descriptions.TTF);
				result.AddPair(DeclarationTypeCodeList.Codes.IGM, DeclarationTypeCodeList.Descriptions.IGM);
				result.AddPair(DeclarationTypeCodeList.Codes.REM, DeclarationTypeCodeList.Descriptions.REM);
				result.AddPair(DeclarationTypeCodeList.Codes.BRE, DeclarationTypeCodeList.Descriptions.BRE);

				return result;
			}
		}

		public CodeDescriptionPairList EmptyMessageSubTypeList
		{
			get
			{
				CodeDescriptionPairList result = new CodeDescriptionPairList();
				result.AddPair("", "");

				return result;
			}
		}

		public override CodeDescriptionPairList CargoIdTypeList => Declaration.IsTradeNet4Point1 ? new CargoPackingCodeList() : new CargoPackingTypeCodeList();

		public ApplicationProductTypeCodeList ApplicationProductTypes => new ApplicationProductTypeCodeList();

		public RefCountryCollection CountryCodeList => new RefCountryCollection(Factory);

		public ZZRefCusCodeListCombinedCollection SGPlacesCodeList
		{
			get
			{
				return ZZRefCusCodeListCombinedCollection.GetCachedCollection(Parent.Factory, Core.Constants.CountryCodes.Singapore, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, ZDateTime.Now);
			}
		}

		public ZZRefCusCodeListCombinedCollection SGLocoList
		{
			get
			{
				var result = ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory,
					Core.Constants.CountryCodes.Singapore,
					Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port,
					ZDateTime.Now);

				result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Universal.Constants.ZZRefCusCodeListFilters.CountryOrGrouping, "Property", new ZString(Core.Constants.CountryCodes.Singapore), false));
				result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Universal.Constants.ZZRefCusCodeListFilters.ListType, "Property", new ZString(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port), false));

				return result;
			}
		}

		public override CodeDescriptionPairList TransportTypeList => Factory.GetCachedValue<TransportModeCodeList>();

		public CodeDescriptionPairList VesselTypeList => Factory.GetCachedValue<VesselTypeCodeList>();

		public override OrgHeaderCollection Organisations => Factory.GetCachedValue("SG.V4.Business.JobDeclarationLookups.Organisations", () => new OrgHeaderCollection(Factory));

		public SupplyIndicatorCodeList SupplyIndicators => Factory.GetCachedValue<SupplyIndicatorCodeList>();

		public CodeForExtensionOfPermitValidityCodeList ExtensionOfPermitValidityCodes => Factory.GetCachedValue<CodeForExtensionOfPermitValidityCodeList>();

		public ZZRefCusCodeListCombinedCollection CertificateTypes => COTypesList;

		ZZRefCusCodeListCombinedCollection COTypesList
		{
			get
			{
				var collection = ZZRefCusCodeListCombinedCollection.GetCachedCollection(Parent.Factory, Core.Constants.CountryCodes.Singapore, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CertificateOfOriginType, ZDateTime.Now);
				collection.Load();
				collection.Sort<ZZRefCusCodeListCombined>(new CertificateCodeComparer());

				return collection;
			}
		}

		sealed class CertificateCodeComparer : IComparer<ICodeDescription>
		{
			public int Compare(ICodeDescription x, ICodeDescription y)
			{
				var codeX = NumberRegex.Replace(x.Code, match => match.Value.PadLeft(2, '0'));
				var codeY = NumberRegex.Replace(y.Code, match => match.Value.PadLeft(2, '0'));

				return codeX.CompareTo(codeY);
			}

			readonly static Regex NumberRegex = new Regex(@"\d+", RegexOptions.Compiled);
		}

		protected override ZQuery OriginPortFilter() => new ZQuery();

		protected override ZQuery DestinationPortFilter() => new ZQuery();

		protected override ZQuery DischargePortFilter() => new ZQuery();

		protected override ZQuery FinalDestinationPortFilter() => new ZQuery();

		public override CodeDescriptionPairList ApplicationCodeList => Factory.GetCachedValue("5a25bc9b-cf5b-4dde-bf71-8cc1d5a3ca50|SGApplicationCodeList", () =>
		{
			var result = new JobApplicationCodeList();
			result.AddPair(DeclarationApplicationCodeList.Codes.Interfaced, DeclarationApplicationCodeList.Descriptions.Interfaced);
			return result;
		});

		public CodeDescriptionPairList GlobalManifestStatusList => Factory.GetCachedValue<GlobalManifestStatusList>();
	}
}
