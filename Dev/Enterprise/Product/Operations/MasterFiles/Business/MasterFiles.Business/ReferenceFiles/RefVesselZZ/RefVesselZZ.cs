using System;
using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[DescriptionProperty(RefVesselZZSchema.Constants.ZZO_LloydsNumber)]
	public class RefVesselZZ : AutoRefVesselZZ
	{
		public RefVesselZZ(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region ZZO_ZZZ_NKDataGrouping

		[RelatedBusinessObject("DataGrouping")]
		public override ZString ZZO_ZZZ_NKDataGrouping
		{
			get { return base.ZZO_ZZZ_NKDataGrouping; }
			set { base.ZZO_ZZZ_NKDataGrouping = value; }
		}

		public RefDataGrouping DataGrouping
		{
			get { return Factory.LoadFromNaturalKey<RefDataGrouping>(RefDataGroupingSchema.ZZZ_DataGrouping, ZZO_ZZZ_NKDataGrouping); }
		}

		#endregion

		#region Lookups

		public static RefVesselZZ LookupVesselByCode(ZString vesselCode, string countryCode, BusinessObjectFactory factory)
		{
			if (factory == null)
			{
				throw new ArgumentNullException(nameof(factory));
			}

			RefVesselZZ result = null;
			if (!vesselCode.IsEmpty)
			{
				ZQuery filter = new ZQuery(RefVesselZZSchema.ZZO_Code, vesselCode.ToUpper().Trim());
				if (!string.IsNullOrWhiteSpace(countryCode))
				{
					filter.AddToFilter(RefVesselZZSchema.ZZO_ZZZ_NKDataGrouping, countryCode);
				}
				filter.OrderBy = RefVesselZZSchema.PK.Name;
				result = factory.LoadTop1<RefVesselZZ>(filter);
			}

			return result;
		}

		public static RefVesselZZ[] LookupVesselsByCode(ZString vesselCode, string countryCode, BusinessObjectFactory factory)
		{
			if (factory == null)
			{
				throw new ArgumentNullException(nameof(factory));
			}

			ZQuery filter = new ZQuery(RefVesselZZSchema.ZZO_Code, vesselCode.ToUpper().Trim());
			if (!vesselCode.IsEmpty)
			{
				if (!string.IsNullOrWhiteSpace(countryCode))
				{
					filter.AddToFilter(RefVesselZZSchema.ZZO_ZZZ_NKDataGrouping, countryCode);
				}
				filter.OrderBy = RefVesselZZSchema.PK.Name;
			}
			return factory.Load<RefVesselZZ>(filter);
		}

		public static RefVesselZZ GetCachedVesselByCode(ZString vesselCode, BusinessObjectFactory factory)
		{
			RefVesselZZ result = null;

			if (!vesselCode.IsEmpty)
			{
				result = factory.GetCachedValue("RefVesselZZ_" + vesselCode, () =>
				{
					return RefVesselZZ.LookupVesselByCode(vesselCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, factory);
				});
			}

			return result;
		}

		public static RefVesselZZ[] GetCachedVesselsByCode(ZString vesselCode, BusinessObjectFactory factory)
		{
			if (factory == null)
			{
				throw new ArgumentNullException(nameof(factory));
			}

			return factory.GetCachedValue("RefVesselZZ_" + vesselCode ?? ZString.Empty, () =>
				{
					return RefVesselZZ.LookupVesselsByCode(vesselCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, factory);
				});
		}

		#endregion

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			ZZO_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.SouthAfrica;
		}
#endif

		protected override ZString HumanReadableNameCore => Res.GetString("B9BAF110-CB42-4025-A96A-4F7125E2F7C3", "Global Vessels - {0}", CalculateShortcutName());

		#region RefVesselZZ Carrier Codes and Carrier Names

		public ZString CarrierCodes => CarrierCodesCore;

		protected virtual ZString CarrierCodesCore => ZString.Empty;

		public ZString CarrierNames => CarrierNamesCore;

		protected virtual ZString CarrierNamesCore => ZString.Empty;

		#endregion
	}
}
