using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Business
{
	public static class Suppression
	{
		#region Public Interface

		public static readonly ZString SuppressedString = "*SUPPRESSED*"; // We don't need to translate it yet
		public static readonly ZDateTime SuppressedDate = DateTime.MinValue.AddDays(1);

		public static T GetValue<T>(T value, IFlightDetailsSuppression bizO, SuppressFields suppressingFieldType, ContactType contactType) where T : IZType
		{
			if (value is ZString)
			{
				return Cast<T>(GetObject(Cast<ZString>(value), bizO, suppressingFieldType, SuppressedString, contactType));
			}

			if (value is ZDateTime)
			{
				return Cast<T>(GetObject(Cast<ZDateTime>(value), bizO, suppressingFieldType, SuppressedDate, contactType));
			}

			if (value is ZDateTimeOffset)
			{
				return Cast<T>(GetObject(Cast<ZDateTimeOffset>(value), bizO, suppressingFieldType, new ZDateTimeOffset(SuppressedDate), contactType));
			}

			return value;
		}

		public static T GetWebValue<T>(T value, IFlightDetailsSuppression bizO, SuppressFields suppressingFieldType) where T : IZType
		{
			return GetValue(value, bizO, suppressingFieldType, null);
		}

		public static ZString GetValue(ZString value, IFlightDetailsSuppression bizO, SuppressFields suppressingFieldType, ZString suppressedValue, ContactType contactType)
		{
			return GetObject(value, bizO, suppressingFieldType, suppressedValue, contactType);
		}

		public static T GetObject<T>(T value, IFlightDetailsSuppression bizO, SuppressFields suppressingFieldType, T supressedValue, ContactType contactType)
		{
			return EnabledForAnyOfFields(bizO, new[] { suppressingFieldType }, contactType) ? supressedValue : value;
		}

		public static bool EnabledForAnyOfFieldsWeb(IFlightDetailsSuppression bizO, SuppressFields[] types)
		{
			return EnabledForAnyOfFields(bizO, types, null);
		}

		public static ZBool EnabledForAnyOfFields(IFlightDetailsSuppression bizO, SuppressFields[] types, ContactType contactType)
		{
			//All Suppression calls pass through this point
			//All Suppression decisions are made inside this method

			ZBool result = bizO != null;
			result = result && EnabledByCountrySpecifics(bizO);
			result = result && types.Any(type => EnabledByTypeAndRegistry(bizO, type));
			result = result && ContactTypeIsConsignorOrWeb(contactType);
			return result;
		}

		#endregion

		#region Implementation

		static bool ContactTypeIsConsignorOrWeb(ContactType contactType)
		{
			return Globals.IsWeb || (contactType?.Code == ContactType.Consignor.Code);
		}

		static ZBool EnabledByTypeAndRegistry(IImportExport bizO, SuppressFields type)
		{
			return Globals.IsWeb
					? SuppressFlightDetailsForImportExport(bizO, type)
					: bizO.IsExport() && GetRegistrySetting(DocumentsDataRegistry.Instance.SuppressPrintingOfFlightDate, type);
		}

		static bool EnabledByCountrySpecifics(IFlightDetailsSuppression bizO)
		{
			bool result = bizO != null && bizO.IsAir;

			var countryCode = GlbBranch.CurrentBranch?.Country?.Code ?? GlbCompany.CurrentCompany?.Country?.Code;

			switch (countryCode)
			{
				case Constants.CountryCodes.UnitedStates:
					return result && bizO.IsPassengerFlight && !bizO.HasFinalRoutingLegATDPassed;
				default:
					return result && !bizO.HasActualRCVPassed && !bizO.HasETDPassed;
			}
		}

		internal class ImportExportKey
		{
			public ImportExportKey(int kind, SuppressFields objectType)
			{
				Kind = kind;
				ObjectType = objectType;
			}
			readonly int Kind;
			readonly SuppressFields ObjectType;

			public override int GetHashCode()
			{
				return Kind.GetHashCode() ^ ObjectType.GetHashCode();
			}

			public override bool Equals(object obj)
			{
				var importExportKey = obj as ImportExportKey;
				return importExportKey != null && Kind.Equals(importExportKey.Kind) && ObjectType.Equals(importExportKey.ObjectType);
			}
		}

		[ThreadStatic]
		static Dictionary<ImportExportKey, bool> cacheObject;

		internal static Dictionary<ImportExportKey, bool> CacheObject
		{
			get { return cacheObject ?? (cacheObject = new Dictionary<ImportExportKey, bool>()); }
		}

		static bool SuppressFlightDetailsForImportExport(IImportExport bizO, SuppressFields type)
		{
			var kind = bizO.IsDomestic() || (!bizO.IsCrossTrade() && !bizO.IsImport() && !bizO.IsExport()) ? 1 :
						bizO.IsExport() ? 2 :
						bizO.IsImport() ? 3 :
						0;

			var key = new ImportExportKey(kind, type);
			var value = false;
			if (!CacheObject.TryGetValue(key, out value))
			{
				if (kind == 1)
				{
					value = GetRegistrySetting(WebDataRegistry.Instance.SuppressFlightDetailsForDomestic, type);
				}
				else if (kind == 2)
				{
					value = GetRegistrySetting(WebDataRegistry.Instance.SuppressFlightDetailsForExport, type);
				}
				else if (kind == 3)
				{
					value = GetRegistrySetting(WebDataRegistry.Instance.SuppressFlightDetailsForImport, type);
				}
				else
				{
					value = GetRegistrySetting(WebDataRegistry.Instance.SuppressFlightDetailsForForeign, type);
				}
				CacheObject.Add(key, value);
			}
			return value;
		}

		static bool GetRegistrySetting(CodeDescriptionBoolRegistryItem item, SuppressFields type)
		{
			ICodeDescriptionBoolList list = item.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty);
			return RegistrySuppressionHelper.GetSetting(list, type);
		}

		static T Cast<T>(object value)
		{
			return (T)value;
		}

		#endregion
	}
}
