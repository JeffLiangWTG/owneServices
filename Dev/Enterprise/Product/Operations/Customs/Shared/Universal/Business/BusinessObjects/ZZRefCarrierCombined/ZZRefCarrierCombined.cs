using System;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal
{
	public sealed class ZZRefCarrierCombined : AutoZZRefCarrierCombined
		, ICodeDescription
		, ITranslatableZZBusinessObject
	{
		public ZZRefCarrierCombined(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{ }

		#region Collection

		[ChildEditable]
		public ZZRefCarrierAttributeCombinedCollection Attributes
		{
			get
			{
				if (attributes == null)
				{
					attributes = new ZZRefCarrierAttributeCombinedCollection(this);
					RegisterEditableChildObject(attributes);
				}
				return attributes;
			}
		}

		ZZRefCarrierAttributeCombinedCollection attributes;

		#endregion

		#region Override Method

		public override void Delete()
		{
			Attributes.DeleteAll();
			base.Delete();
		}

		#endregion

		[ReadOnly(true)]
		public override ZBool ZZ4_IsSystem
		{
			get => base.ZZ4_IsSystem;
			set
			{
				var oldValue = ZZ4_IsSystem;
				base.ZZ4_IsSystem = value;
				if (!IsCopying && oldValue != ZZ4_IsSystem)
				{
					Attributes.MarkAsNeedingValidation();
				}
			}
		}

		[List("Lookups.CountryOrGroupingList")]
		public override ZString ZZ4_CountryOrGrouping
		{
			get => base.ZZ4_CountryOrGrouping;
			set => base.ZZ4_CountryOrGrouping = value;
		}

		public override bool SupportsNotes => false;

		public override bool ReadOnly
		{
			get => ZZ4_IsSystem || base.ReadOnly;
			set => base.ReadOnly = value;
		}

		public bool HasMatchingAttributes(ZString[] attributeNames)
		{
			var result = false;
			if (attributeNames != null)
			{
				if (attributeNames.Length == 0)
				{
					result = Attributes.Count == 0;
				}
				else
				{
					var attributesToCheck = Attributes.Cast<ZZRefCarrierAttributeCombined>();
					result = attributeNames.All(attributeName => attributesToCheck.Any(y => y.ZZG_Name.EqualsIgnoringCase(attributeName)));
				}
			}
			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new class Loader : AutoZZRefCarrierCombined.Loader
		{
			public Loader(BusinessObjectFactory factory)
				: base(factory)
			{ }

			protected override Type GetTypeOfBusinessObjectToLoad()
			{
				return typeof(ZZRefCarrierCombined);
			}

			public ZZRefCarrierCombined[] LoadCarriersWithAttributes(ZString countryCode, ZString[] attributeNames)
			{
				var query = new ZQuery(ZZRefCarrierCombinedSchema.ZZ4_CountryOrGrouping, countryCode);
				var codes = Factory.Load<ZZRefCarrierCombined>(query);
				if (attributeNames != null)
				{
					foreach (var pk in codes.Select(x => x.PK))
					{
						Factory.AddFetchHint(ZZRefCarrierAttributeCombinedSchema.ZZG_ZZ4_CarrierCode, pk);
					}
					codes = codes.Where(x => x.HasMatchingAttributes(attributeNames)).ToArray();
				}
				return codes;
			}

			public ZZRefCarrierCombined LoadFromCode(ZString countryCode, ZString carrierCode)
			{
				var query = new ZQuery(ZZRefCarrierCombinedSchema.ZZ4_CountryOrGrouping, countryCode);
				query.AddToFilter(ZZRefCarrierCombinedSchema.ZZ4_Code, carrierCode);
				return Factory.LoadTop1<ZZRefCarrierCombined>(query);
			}

			public bool AnyRecordsForCountry(ZString countryCode)
			{
				var query = new ZQuery(ZZRefCarrierCombinedSchema.ZZ4_CountryOrGrouping, countryCode);
				return Factory.LoadTop1<ZZRefCarrierCombined>(query) != null;
			}
		}

		public ZBoolDescriptionPairList TransportModePairList
		{
			get
			{
				return Factory.GetCachedValue(string.Format(CultureInfo.CurrentCulture, "TransportModePairList_{0}_{1}_{2}_{3}", ZZ4_IsAir, ZZ4_IsSea, ZZ4_IsRail, ZZ4_IsRoad), () =>
				{
					var result = new ZBoolDescriptionPairList();
					result.AddNew(RefTransportModeList.Codes.AIR, ZZ4_IsAir);
					result.AddNew(RefTransportModeList.Codes.SEA, ZZ4_IsSea);
					result.AddNew(RefTransportModeList.Codes.RAI, ZZ4_IsRail);
					result.AddNew(RefTransportModeList.Codes.ROA, ZZ4_IsRoad);
					return result;
				});
			}
		}

		public static SchemaColumn GetTransportModePropertySchemaColumn(ZString transportMode)
		{
			switch (transportMode)
			{
				case RefTransportModeList.Codes.AIR:
					return ZZRefCarrierCombinedSchema.ZZ4_IsAir;
				case RefTransportModeList.Codes.SEA:
					return ZZRefCarrierCombinedSchema.ZZ4_IsSea;
				case RefTransportModeList.Codes.RAI:
					return ZZRefCarrierCombinedSchema.ZZ4_IsRail;
				case RefTransportModeList.Codes.ROA:
					return ZZRefCarrierCombinedSchema.ZZ4_IsRoad;
				default:
					return null;
			}
		}

		public static string GetTransportModePropertyName(ZString transportMode)
		{
			switch (transportMode)
			{
				case RefTransportModeList.Codes.AIR:
					return ZZRefCarrierCombinedSchema.Constants.ZZ4_IsAir;
				case RefTransportModeList.Codes.SEA:
					return ZZRefCarrierCombinedSchema.Constants.ZZ4_IsSea;
				case RefTransportModeList.Codes.RAI:
					return ZZRefCarrierCombinedSchema.Constants.ZZ4_IsRail;
				case RefTransportModeList.Codes.ROA:
					return ZZRefCarrierCombinedSchema.Constants.ZZ4_IsRoad;
				default:
					return null;
			}
		}
		public ZString UnTranslatedZZ4_Description => base.ZZ4_Description;
		public override ZString ZZ4_Description
		{
			get => TranslationHelper.GetTranslatedValue(this, base.ZZ4_Description, RefCarrierCodeLanguageSchema.ZCL_Description);
			set => base.ZZ4_Description = value;
		}

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		public override bool CanDelete => !ZZ4_IsSystem;

		public override MultilingualString ReasonForNotAbleToDelete => ZZ4_IsSystem ? CannotDeleteSystemGenerated : (NoResString)string.Empty;

		internal static MultilingualString CannotDeleteSystemGenerated => ResString.GetMultilingualString("B0F4B17B-2C1C-40EE-B44E-7C78B44EFB71", "You cannot delete a system-generated record.");

		ITableSchema ITranslatableZZBusinessObject.LanguageTableSchema => RefCarrierCodeLanguageSchema.Instance;
		Type ITranslatableZZBusinessObject.LanguageTableType => typeof(RefCarrierCodeLanguage);

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new TranslatableZZBusinessObjectFetchStrategy<ZZRefCarrierCombined>(this);
		}
#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			ZZ4_Code = "BOB";
			ZZ4_Description = "BOB THE BUILDER";
			ZZ4_CountryOrGrouping = Core.Constants.CountryCodes.Cambodia;
		}

#endif
	}
}
