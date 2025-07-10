using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class CusRefPacks : BaseRefPacks, IUnitConverter
	{
		public CusRefPacks(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Loader : BusinessObject.Loader
		{
			public Loader(BusinessObjectFactory factory) : base(factory)
			{
			}

			protected override Type GetTypeOfBusinessObjectToLoad()
			{
				return typeof(CusRefPacks);
			}

			public CusRefPacks Load(string commercialUQ, string countryCode, string type, IEnumerable<string> acceptableCustomsUQs)
			{
				var maps = CusRefPacksHelper.LoadFilteredRefPacks(Factory, countryCode, type, commercialUQ);

				if (maps.Count == 1)
				{
					return maps[0];
				}
				else if (maps.Count > 1)
				{
					return (from CusRefPacks p in maps where acceptableCustomsUQs.Contains(p.RP_CustomsPack.ToString()) select p).FirstOrDefault();
				}
				else
				{
					return null;
				}
			}
		}

		protected override ZString HumanReadableNameCore => Res.GetString("71F084C9-9C02-4983-B21E-A87F4DDC8602", "Packs Conversion {0}", RP_Code);

		IRefCusPackListProvider CachedRefCusPackListProvider => RefCusPackListProvider.Loader.GetRefCusPackListProvider(Factory, RP_CustomsCountry) ?? new RefCusPackListProvider();

		public override CodeDescriptionPairList RP_CustomsPack_List
		{
			get
			{
				var cachedProvider = CachedRefCusPackListProvider;
				if (cachedProvider != null)
				{
					return cachedProvider.GetCustomsPackList(Factory, RP_Type, RP_CustomsCountry);
				}

				return Factory.GetCachedValue<BaseCusUQList>(); // Default behaviour...
			}
		}

		public override CodeDescriptionPairList RP_CommercialPack_List
		{
			get
			{
				var cachedProvider = CachedRefCusPackListProvider;
				if (cachedProvider != null)
				{
					return cachedProvider.GetCommercialPackList(Factory, RP_Type);
				}

				return base.RP_CommercialPack_List;
			}
		}

		public override CodeDescriptionPairList RP_Type_List => CachedRefCusPackListProvider.GetPackConversionTypeList(Factory) ?? base.RP_Type_List;

		#region Overrides

		#region RP_ConversionFactor

		[DecimalPlaces(9)]
		public override ZDecimal RP_ConversionFactor
		{
			get { return base.RP_ConversionFactor; }
			set { base.RP_ConversionFactor = value; }
		}

		#endregion

		#endregion

		#region IUnitConverter Members

		public ZString ParentUnit
		{
			get { return RP_CommercialPack; }
		}

		public ZString ChildUnit
		{
			get { return RP_CustomsPack; }
		}

		public ZDecimal ConversionFactor
		{
			get { return RP_ConversionFactor; }
		}

		#endregion
	}
}
