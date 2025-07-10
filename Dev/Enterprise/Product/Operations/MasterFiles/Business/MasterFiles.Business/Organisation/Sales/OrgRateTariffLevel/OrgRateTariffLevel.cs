using System;
using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	[ProvideMetaDataProperty("ReadOnlySecurity", MetaDataTypes.ReadOnly)]
	public class OrgRateTariffLevel : AutoOrgRateTariffLevel
	{
		public OrgRateTariffLevel(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Properties

		#region P7_TariffType
		[List("Lookups.TariffTypes")]
		public override ZString P7_TariffType
		{
			get
			{
				return base.P7_TariffType;
			}
			set
			{
				if (base.P7_TariffType != value)
				{
					base.P7_TariffType = value;
					if (!Lookups.Modes.ContainsCode(P7_Mode))
					{
						P7_Mode = ALL;
						P7_ModeInfo.RefreshBinding();
					}
				}
			}
		}

		internal const string ALL = "ALL";

		#endregion

		#region TariffDescription

		[MaxLength(64)]
		public ZString TariffDescription
		{
			get
			{
				OrgCompanyData companyData = Parent;
				if (companyData != null)
				{
					return CachedOrgCodeLists.CompanyTariffTypes_List(Factory, P7_GC).GetDescriptionFromCode(P7_TariffType);
				}

				return ZString.Empty;
			}
		}

		public ZPropertyInfo TariffDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(TariffDescription)); }
		}

		OrgCodeLists CachedOrgCodeLists
		{
			get { return cachedOrgCodeLists ?? (cachedOrgCodeLists = new OrgCodeLists()); }
		}

		OrgCodeLists cachedOrgCodeLists;

		#endregion

		#region TariffLevelAsString

		[BusinessObjectTestExclude]
		[List("Lookups.TariffLevels")]
		[MaxLength(3)]
		public ZString TariffLevelAsString
		{
			get { return fTariffLevelAsString.IsEmpty ? (ZString)P7_TariffLevel.ToString() : fTariffLevelAsString; }
			set
			{
				CheckMaximumLength(TariffLevelAsStringInfo, value);
				fTariffLevelAsString = value;
				Validation.ValidateTariffLevelAsString();

				try
				{
					P7_TariffLevel = Byte.Parse(value);
					fTariffLevelAsString = ZString.Empty;
				}
				catch (OverflowException)
				{
					P7_TariffLevel = 0;
				}
				catch (FormatException)
				{
					P7_TariffLevel = 0;
				}
				TariffLevelAsStringInfo.RefreshBinding();
			}
		}

		ZString fTariffLevelAsString;

		public ZPropertyInfo TariffLevelAsStringInfo
		{
			get { return GetZPropertyInfo(nameof(TariffLevelAsString)); }
		}

		#endregion

		#region P7_Mode

		[List("Lookups+Modes")]
		public override ZString P7_Mode
		{
			get { return base.P7_Mode; }
			set { base.P7_Mode = value; }
		}

		#endregion

		#region P7_Direction

		[List("Lookups+Directions")]
		public override ZString P7_Direction
		{
			get { return base.P7_Direction; }
			set { base.P7_Direction = value; }
		}

		#endregion

		#region P7_StartDate

		public override ZDate P7_StartDate
		{
			get { return base.P7_StartDate; }
			set { base.P7_StartDate = value; }
		}

		#endregion

		#region P7_ExpiryDate

		public override ZDate P7_ExpiryDate
		{
			get { return base.P7_ExpiryDate; }
			set { base.P7_ExpiryDate = value; }
		}

		#endregion

		#region IsExpired

		public bool IsExpired
		{
			get { return P7_ExpiryDate < ZDate.Today; }
		}

		#endregion

		[Flags]
		public enum Directions
		{
			ALL = 1, IMP = 2, EXP = 4
		}

		#endregion

		#region Parent

		public OrgCompanyData Parent
		{
			get { return Header == null ? null : Header.CompanyData; }
		}

		#endregion

		/// <summary>
		/// Checks if this OrgRateTariffLevel has default values for TariffType, Mode, Direction, Start Date and Expiry Date
		/// </summary>
		internal bool IsDefault
		{
			get { return P7_TariffType == DefaultTariffType && P7_Mode == ALL && P7_Direction == nameof(Directions.ALL) && P7_StartDate.IsDefault && P7_ExpiryDate.IsDefault; }
		}

		static string GetTariffString(ZString type, ZString mode, ZString direction, ZByte level, ZBool group, ZDate startDate, ZDate expiryDate)
		{
			var groupStr = group ? (NoResString)"True" : (NoResString)"False";
			var blankStr = (NoResString)"Blank";
			var startDateStr = startDate.IsEmpty ? blankStr : startDate.ToString();
			var expiryDateStr = expiryDate.IsEmpty ? blankStr : expiryDate.ToString();

			return $"{type}, {mode}, {direction}, {level}, {groupStr}, {startDateStr}, {expiryDateStr}";
		}

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			if (IsInDatabase && (P7_TariffTypeInfo.HasChanges || P7_ModeInfo.HasChanges || P7_DirectionInfo.HasChanges
				|| P7_ApplyGroupRateInfo.HasChanges || P7_StartDateInfo.HasChanges || P7_ExpiryDateInfo.HasChanges || P7_TariffLevelInfo.HasChanges))
			{
				var prevTariffStr = GetTariffString(
					(ZString)P7_TariffTypeInfo.OriginalValue,
					(ZString)P7_ModeInfo.OriginalValue,
					(ZString)P7_DirectionInfo.OriginalValue,
					(ZByte)P7_TariffLevelInfo.OriginalValue,
					(ZBool)P7_ApplyGroupRateInfo.OriginalValue,
					(ZDate)P7_StartDateInfo.OriginalValue,
					(ZDate)P7_ExpiryDateInfo.OriginalValue);

				var curTariffStr = GetTariffString(P7_TariffType, P7_Mode, P7_Direction, P7_TariffLevel, P7_ApplyGroupRate, P7_StartDate, P7_ExpiryDate);
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				Header.Logs.AddNew(AutoEvents.EditedARecord, $"Changed Organisation {Header.OH_Code} Company Tariff and Group Rate Usage config for '{prevTariffStr}' to '{curTariffStr}'.");
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			}
			else if (!IsInDatabase)
			{
				var tariffStr = GetTariffString(P7_TariffType, P7_Mode, P7_Direction, P7_TariffLevel, P7_ApplyGroupRate, P7_StartDate, P7_ExpiryDate);
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				Header.Logs.AddNew(AutoEvents.AddedARecordToTheSystem, $"Added Organisation {Header.OH_Code} Company Tariff and Group Rate Usage config for '{tariffStr}'.");
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			}
		}

		protected override void BeforeSuccessfulDelete()
		{
			if (IsInDatabase)
			{
				var tariffStr = GetTariffString(P7_TariffType, P7_Mode, P7_Direction, P7_TariffLevel, P7_ApplyGroupRate, P7_StartDate, P7_ExpiryDate);
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				Header.Logs.AddNew(AutoEvents.DeletedARecordInTheSystem, $"Deleted Organisation {Header.OH_Code} Company Tariff and Group Rate Usage config for '{tariffStr}'.");
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			}

			base.BeforeSuccessfulDelete();
		}

		public const string DefaultTariffType = "DEF";

		#region IReadOnlySecurity Members

		protected bool GetReadOnlySecurity(PropertyDescriptor property)
		{
			return Header == null || !Header.SecurityProvider.HasModifyDetailsRatingAndTariffsSecurity || MetaData.GetReadOnlyExcludingMethodProvider(this, property);
		}

		public static Directions GetOrgRateTariffLevelDirection(Business.Directions jobDirection)
		{
			switch (jobDirection)
			{
				case Business.Directions.Import:
					return OrgRateTariffLevel.Directions.IMP;
				case Business.Directions.Export:
					return OrgRateTariffLevel.Directions.EXP;
				default:
					return OrgRateTariffLevel.Directions.ALL;
			}
		}

		#endregion
	}
}
