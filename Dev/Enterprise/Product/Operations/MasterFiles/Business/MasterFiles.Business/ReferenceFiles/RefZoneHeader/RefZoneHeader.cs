using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[System.Diagnostics.DebuggerDisplay("Code = {FZ_Code}")]
	[CodeProperty(RefZoneHeader.Schema.FZ_Code)]
	[DescriptionProperty(RefZoneHeader.Schema.FZ_Description)]
	public class RefZoneHeader : AutoRefZoneHeader, IRefZoneHeader, ILocationBiz, ITemplateCopyable, IRatingZone
	{
		public RefZoneHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Logging

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		protected override ZString HumanReadableNameCore
		{
			get { return Res.GetString("8641e390-0a8f-4fe0-a7cf-f5cc37cc28b9", "Zone"); }
		}

		#endregion

		#region Clone

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			RefZoneHeader result = (RefZoneHeader)base.CloneInternal(args);

			if (result.FZ_ZoneType == RefZoneHeaderLookups.ZoneTypeCodes.WiseRatesOcean)
			{
				result.FZ_ZoneType = string.Empty;
			}

			foreach (RefCountry country in Countries)
			{
				result.Countries.Add(country);
			}

			foreach (RefUNLOCO unloco in UNLOCOs)
			{
				result.UNLOCOs.Add(unloco);
			}

			return result;
		}

		protected override IEnumerable<string> GetPropertiesToExcludeFromCloning()
		{
			List<string> result = new List<string>(base.GetPropertiesToExcludeFromCloning());
			result.Add(RefZoneHeaderSchema.Constants.FZ_Code);

			return result;
		}

		#endregion

		#region Properties

		#region FZ_OH_RelatedParty
		[List("Lookups.RelatedParties")]

		public override ZGuid FZ_OH_RelatedParty
		{
			get
			{
				return base.FZ_OH_RelatedParty;
			}
			set
			{
				base.FZ_OH_RelatedParty = value;
			}
		}

		#endregion

		#region FZ_ZoneType
		[List("Lookups.ZoneTypes")]
		public override ZString FZ_ZoneType
		{
			get { return base.FZ_ZoneType; }
			set
			{
				base.FZ_ZoneType = value;
				if (!IsRatingAvailableZone)
				{
					FZ_OH_RelatedParty = ZGuid.Empty;
				}

				var zoneModeList = Lookups.ZoneModes;
				if (zoneModeList != null && zoneModeList.Count > 0)
				{
					FZ_ZoneMode = zoneModeList[0].Code;
				}

				FZ_ZoneModeInfo.RefreshBinding();

				if (FZ_ZoneType == RefZoneHeaderLookups.ZoneTypeCodes.Schedules)
				{
					Countries.RemoveAll();
				}

				Countries.RefreshBindingIncludingChildren();
			}
		}

		public ZBool IsRatingAvailableZone
		{
			get
			{
				return FZ_ZoneType != RefZoneHeaderLookups.ZoneTypeCodes.Reporting
					&& FZ_ZoneType != RefZoneHeaderLookups.ZoneTypeCodes.Tax
					&& FZ_ZoneType != RefZoneHeaderLookups.ZoneTypeCodes.TransitWarehouse
					&& FZ_ZoneType != RefZoneHeaderLookups.ZoneTypeCodes.WiseRatesOcean;
			}
		}

		public ZPropertyInfo IsRatingAvailableZoneInfo
		{
			get { return GetZPropertyInfo(nameof(IsRatingAvailableZone)); }
		}

		[List("Lookups.ZoneModes")]
		public override ZString FZ_ZoneMode
		{
			get => base.FZ_ZoneMode;
			set
			{
				if (base.FZ_ZoneMode != value)
				{
					base.FZ_ZoneMode = value;
				}
			}
		}

		#endregion

		#region Active Filter

		public static ZQuery ActiveFilter
		{
			get { return new ZQuery(RefZoneHeaderSchema.FZ_IsActive, true); }
		}

		#endregion

		#endregion

		#region Related Business Objects

		#region UNLOCOs

		[ChildEditable(true)]
		public RefZoneUNLOCOCollection UNLOCOs
		{
			get
			{
				if (fUNLOCOs == null)
				{
					fUNLOCOs = new RefZoneUNLOCOCollection(this);
					fUNLOCOs.Load();
					RegisterEditableChildObject(fUNLOCOs);
				}

				return fUNLOCOs;
			}
		}
		RefZoneUNLOCOCollection fUNLOCOs;

		#endregion

		#region Countries

		[ChildEditable(true)]
		public RefZoneCountryCollection Countries
		{
			get
			{
				if (fCountries == null)
				{
					fCountries = new RefZoneCountryCollection(this);
					fCountries.Load();
					RegisterEditableChildObject(fCountries);
				}

				return fCountries;
			}
		}
		RefZoneCountryCollection fCountries;

		#endregion

		#endregion

		#region ILocationBiz Members

		public ZString Code
		{
			get { return FZ_Code; }
			set { FZ_Code = value; }
		}

		public ZPropertyInfo CodeInfo
		{
			get { return FZ_CodeInfo; }
		}

		[SuppressMessage("Enterprise.Globalization", "EDI007:CustomizableDataTranslationRule")]
		public ZString Description
		{
			get { return FZ_Description; }
			set { FZ_Description = value; }
		}

		[RefZoneTranslatableDataField(Schema.FZ_Description, Asmid = ResString.AssemblyId)]
		public override ZString FZ_Description
		{
			get { return base.FZ_Description; }
			set { base.FZ_Description = value; }
		}

		public MultilingualString FZ_DescriptionMultilingual => GetMultilingual(FZ_DescriptionInfo);

		public override bool ReadOnly
		{
			get { return FZ_ZoneType == RefZoneHeaderLookups.ZoneTypeCodes.WiseRatesOcean && !FZ_ZoneTypeInfo.HasErrors(); }
			set { base.ReadOnly = value; }
		}

		public ZPropertyInfo DescriptionInfo
		{
			get { return FZ_DescriptionInfo; }
		}

		public ZBool IsActive
		{
			get { return FZ_IsActive; }
		}

		public ZPropertyInfo IsActiveInfo
		{
			get { return FZ_IsActiveInfo; }
		}

		RefCityTown ILocation.CityTown
		{
			get { return null; }
		}

		RefCountry ILocation.Country => Countries.Cast<ILocation>().Union(UNLOCOs.Cast<ILocation>()).SameOrDefault(x => x.Country);

		RefCountryStates ILocation.State
		{
			get { return null; }
		}

		[MaxLength(100)]
		ZString ILocationBiz.StateDescription
		{
			get { return ZString.Empty; }
		}

		ZPropertyInfo ILocationBiz.StateDescriptionInfo
		{
			get { return GetZPropertyInfo("StateDescription"); }
		}

		RefUNLOCO ILocation.UNLOCO =>
			UNLOCOs.Count > 0 && Countries.Count == 0
				? UNLOCOs.Cast<RefUNLOCO>().SameOrDefault(BusinessObjectEqualityComparer<RefUNLOCO>.PKOnlyComparer)
				: null;

		IATACityCode ILocation.IATACityCode =>
			UNLOCOs.Count > 0 && Countries.Count == 0
				? IATACityCode.GetValidOrDefault(Factory, UNLOCOs.Cast<RefUNLOCO>().SameOrDefault(x => x.RL_IATARegionCode))
				: null;

		RefZoneHeader[] ILocation.Zones => Factory.GetCachedValue("ZonesFor" + FZ_Code, GetZonesIncludingCountriesAndUNLOCOsZones);

		RefZoneHeader[] GetZonesIncludingCountriesAndUNLOCOsZones()
		{
			var query = new ZDBOnlyQuery(typeof(RefZoneHeader));
			query.AddFilterAndZSQLParameterCollection(@"
				FZ_PK IN
				(
					SELECT ZonePK FROM GetCommonRelatedZonesOfAZone(@CurrentZone)
				)", new ZSqlParameterCollection(ZSqlParameter.New("@CurrentZone", PK, RefZoneHeaderSchema.PK)));
			var commonRelatedZonesOfZones = Factory.Load<RefZoneHeader>(query);
			var allZones = commonRelatedZonesOfZones.Append(this);

			return allZones.ToArray();
		}

		bool ILocationReference.IsLocalInRelationTo(ZString code)
		{
			ILocation locationToCheck = null;

			if (code.Length == 5)
			{
				locationToCheck = new RefUNLOCO.Loader(Factory).Load(code);
			}
			else if (code.Length == 2)
			{
				locationToCheck = RefCountry.LoadFromCountryCode(Factory, code);
			}

			return locationToCheck != null && this.CompletelyCovers(locationToCheck);
		}

		#endregion

		#region ITemplateCopyable Members

		public IBusiness TemplateCopy()
		{
			return (RefZoneHeader)Clone();
		}

		#endregion

		#region ICanDelete

		public override bool CanDelete => FZ_ZoneType != ZoneTypeCodeDescriptionPair.Tax.Code && FZ_ZoneType != ZoneTypeCodeDescriptionPair.WiseRatesOcean.Code;

		public override MultilingualString ReasonForNotAbleToDelete => ResString.GetMultilingualString("07c3f3f7-6a0a-49ec-98cd-898633709936", "Cannot delete zone of {0} type.", FZ_ZoneType);

		#endregion

		#region IRatingZone

		ZString IRatingZone.ZoneCode => FZ_Code;

		ZGuid IRatingZone.RelatedOrgPK => FZ_OH_RelatedParty;

		/// <summary>
		/// This function is utilized by the user interface to retrieve a warning message.
		/// The warning is triggered when locations attached in the editing RefZoneHeader are also found to be used in other zones.
		/// </summary>
		[SuppressMessage("CargoWiseOne", "CW1107:Db.Connection.Command - Use the BusinessObjectFactory rather than hitting the DB directly.", Justification = "Select with CTE and groupby should be faster")]
		public string GetOverlappingZonesMessage()
		{
			var sqlQuery = GetOverlappingZonesQuery();
			var locationGroups = new Dictionary<string, HashSet<string>>();
			using (var command = Db.Connection.Command(sqlQuery))
			{
				command.AddParameter("@checkingZoneHeaderPK", SqlDbType.UniqueIdentifier, PK.ToGuid());
				command.AddParameter("@zoneType", SqlDbType.VarChar, FZ_ZoneType.ToString());
				command.AddParameter("@zoneMode", SqlDbType.VarChar, FZ_ZoneMode.ToString());
				if (!FZ_OH_RelatedParty.IsEmpty)
				{
					command.AddParameter("@zoneRelatedParty", SqlDbType.UniqueIdentifier, FZ_OH_RelatedParty.ToGuid());
				}

				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						var location = (string)reader["RL_Code"];
						var zone = (string)reader["FZ_Code"];
						if (!locationGroups.TryGetValue(location, out var zones))
						{
							zones = new HashSet<string>();
							locationGroups.Add(location, zones);
						}

						zones.Add(zone);
					}
				}
			}

			var messageBuilder = new StringBuilder();
			bool isContractZone = FZ_ZoneType == RefZoneHeaderLookups.ZoneTypeCodes.Contract;

			foreach (var locationGroup in locationGroups)
			{
				string newLine;

				if (isContractZone)
				{
					newLine = Res.GetString(
						"9fae9d48-d1c5-7c9e-4a7b-68d20af95313",
						"'{0}' is included in overlapping Contract International Zones: '{1}'.",
						locationGroup.Key,
						string.Join("', '", locationGroup.Value));
				}
				else
				{
					newLine = Res.GetString(
						"8554e0da-ce3c-4219-bdaa-e9619916a954",
						"'{0}' is included in overlapping Rating International Zones: '{1}'.",
						locationGroup.Key,
						string.Join("', '", locationGroup.Value));
				}

				messageBuilder.AppendLine(newLine);

				// Too much info doesn't help. Let's limit the message length.
				if (messageBuilder.Length >= MAGIC_MESSAGE_LENGTH_CUT)
				{
					messageBuilder.AppendLine("...");
					break;
				}
			}

			if (locationGroups.Count > 0)
			{
				if (isContractZone)
				{
					messageBuilder.Append(Res.GetString("43983316-b652-19a7-4bf1-eee6f684a9ee", "It may result in Allocation Routes with different but overlapping International Zones as Load / Discharge Ports being loaded into the ‘Contract & Allocation Routes Search Form’ when launched from relevant jobs."));
				}
				else
				{
					messageBuilder.Append(Res.GetString("2a61496e-8ceb-4e39-8721-0c5c3496fe30", "It may result in overlapping Rates or Charges to be loaded to the relevant Job when Autorating."));
				}
			}

			return messageBuilder.ToString();
		}

		string GetOverlappingZonesQuery()
		{
			// table names
			var refUNLOCO = $"dbo.{RefUNLOCOSchema.Constants.TableName}";
			var refUNLOCOTableCode = RefUNLOCOSchema.Constants.Prefix;
			var zonePivot = $"dbo.{RefZonePivotSchema.Constants.TableName}";
			var zoneHeader = $"dbo.{RefZoneHeaderSchema.Constants.TableName}";

			// column names
			var rl_PK = RefUNLOCOSchema.Constants.PK;
			var rl_IsActive = RefUNLOCOSchema.Constants.RL_IsActive;
			var rl_Code = RefUNLOCOSchema.Constants.RL_Code;
			var fz_PK = RefZoneHeaderSchema.Constants.PK;
			var fz_Code = RefZoneHeaderSchema.Constants.FZ_Code;
			var fz_OH_RelatedParty = RefZoneHeaderSchema.Constants.FZ_OH_RelatedParty;
			var fz_ZoneType = RefZoneHeaderSchema.Constants.FZ_ZoneType;
			var fz_ZoneMode = RefZoneHeaderSchema.Constants.FZ_ZoneMode;
			var fz_IsActive = RefZoneHeaderSchema.Constants.FZ_IsActive;
			var f2_ParentTableCode = RefZonePivotSchema.Constants.F2_ParentTableCode;
			var f2_ParentID = RefZonePivotSchema.Constants.F2_ParentID;
			var f2_FZ = RefZonePivotSchema.Constants.F2_FZ;

			var relatedPartyComparision = FZ_OH_RelatedParty.IsEmpty
				? $"zoneHeader.{fz_OH_RelatedParty} IS NULL"
				: $"zoneHeader.{fz_OH_RelatedParty} = @zoneRelatedParty";

			return @$"
WITH CTE AS (
	SELECT 
		refUNLOCO.{rl_PK},
		zoneHeader.{fz_OH_RelatedParty},
		zoneHeader.{fz_ZoneType},
		zoneHeader.{fz_ZoneMode},
		COUNT(refUNLOCO.{rl_PK}) as CountDistinct
	FROM (
		SELECT * FROM {refUNLOCO} WHERE {rl_PK} IN (
			SELECT {f2_ParentID}
			FROM {zonePivot}
			WHERE {f2_ParentTableCode} = '{refUNLOCOTableCode}'	AND {f2_FZ} = @checkingZoneHeaderPK
		)
	) AS refUNLOCO -- Only locations being used in checkingZoneHeader
	JOIN {zonePivot} zonePivot
		ON refUNLOCO.{rl_PK} = zonePivot.{f2_ParentID}
		AND zonePivot.{f2_ParentTableCode} = '{refUNLOCOTableCode}'
	JOIN {zoneHeader} zoneHeader
		ON zonePivot.{f2_FZ} = zoneHeader.{fz_PK}
	WHERE refUNLOCO.{rl_IsActive} = 1
		AND zoneHeader.{fz_IsActive} = 1
		AND zoneHeader.{fz_ZoneType} = @zoneType
		AND zoneHeader.{fz_ZoneMode} = @zoneMode
		AND {relatedPartyComparision}
	GROUP BY
		refUNLOCO.{rl_PK},
		zoneHeader.{fz_OH_RelatedParty},
		zoneHeader.{fz_ZoneType},
		zoneHeader.{fz_ZoneMode}
		HAVING COUNT(refUNLOCO.{rl_PK}) > 1
) -- Only select locations that appear multiple times in different zones having the same zone type, zone mode, and related party 
SELECT refUNLOCO.{rl_Code}, zoneHeader.{fz_Code}
FROM {refUNLOCO} refUNLOCO
JOIN {zonePivot} zonePivot
	ON refUNLOCO.{rl_PK} = zonePivot.{f2_ParentID}
	AND zonePivot.{f2_ParentTableCode} = '{refUNLOCOTableCode}'
JOIN {zoneHeader} zoneHeader
	ON zonePivot.{f2_FZ} = zoneHeader.{fz_PK}
WHERE refUNLOCO.{rl_IsActive} = 1
	AND zoneHeader.{fz_IsActive} = 1
	AND zoneHeader.{fz_ZoneType} = @zoneType
	AND zoneHeader.{fz_ZoneMode} = @zoneMode
	AND {relatedPartyComparision}
	AND refUNLOCO.{rl_PK} IN (SELECT {rl_PK} FROM CTE);";
		}

		public const int MAGIC_MESSAGE_LENGTH_CUT = 2000;

		#endregion

		#region Testing
#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			if (FZ_ZoneType.IsEmpty)
			{
				FZ_ZoneType = "ALL";
			}
			if (FZ_ZoneMode.IsEmpty)
			{
				FZ_ZoneMode = "ALL";
			}
			base.FillWithValidTestDataCore(kind, propertyPath);
		}

#endif
		#endregion
	}
}
