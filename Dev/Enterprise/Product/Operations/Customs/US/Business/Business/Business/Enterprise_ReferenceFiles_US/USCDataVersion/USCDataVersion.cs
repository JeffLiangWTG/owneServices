using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using static CargoWise.Definitions.Customs.US;

namespace Enterprise.Customs.US.Business
{
	public class USCDataVersion : AutoUSCDataVersion
	{
		public USCDataVersion(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static class Constant
		{
			public const string LastHTSAttempt = "LastHTSAttempt";
			public const string DatabaseNameIdentifier = "DB:";
			public const string RefFileRequestAffirmationOfComplianceTimeStamp = "RefFileRequestAffirmationOfComplianceTimeStamp";
			public const string RefFileRequestAntiDumpingTimeStamp = "RefFileRequestAntiDumpingTimeStamp";
			public const string RefFileRequestCarrierCodeTimeStamp = "RefFileRequestCarrierCodeTimeStamp";
			public const string RefFileRequestCountryCodeTimeStamp = "RefFileRequestCountryCodeTimeStamp";
			public const string RefFileRequestFirmsCodeTimeStamp = "RefFileRequestFirmsCodeTimeStamp";
			public const string RefFileRequestForeignPortCodeTimeStamp = "RefFileRequestForeignPortCodeTimeStamp";
			public const string RefFileRequestRegionDistrictPortCodeTimeStamp = "RefFileRequestRegionDistrictPortCodeTimeStamp";
			public const string RefFileRequestTeamAssignmentTimeStamp = "RefFileRequestTeamAssignmentTimeStamp";
			public const string RefFileRequestZipStateCodeTimeStamp = "RefFileRequestZipStateCodeTimeStamp";
		}

		public static USCDataVersion GetOrCreate(BusinessObjectFactory factory, ZString name)
		{
			USCDataVersion result = null;
			if (factory != null)
			{
				result = factory.LoadFromNaturalKey<USCDataVersion>(USCDataVersionSchema.UZ_Name, name);
				if (result == null)
				{
					result = factory.New<USCDataVersion>();
					result.UZ_Name = name;
					result.SetNoteWithDatabaseDetail("");
					result.UZ_UpdateTime = ZDateTime.MinSmallDateTimeValue;
				}
			}
			return result;
		}

		public static USCDataVersion GetLastHTSAttempt(BusinessObjectFactory factory)
		{
			USCDataVersion result = null;
			if (factory != null)
			{
				result = factory.LoadFromNaturalKey<USCDataVersion>(USCDataVersionSchema.UZ_Name, Constant.LastHTSAttempt);
				if (result == null)
				{
					result = factory.New<USCDataVersion>();
					result.UZ_Name = Constant.LastHTSAttempt;
					result.UZ_Version = 901;
					result.SetNoteWithDatabaseDetail("");
					result.UZ_UpdateTime = ZDateTime.MinSmallDateTimeValue;
				}
			}
			return result;
		}

		public override bool SupportsNotes
		{
			get { return false; }
		}

		public ZString GetDatabaseNameFromNote()
		{
			var result = ZString.Empty;
			var databaseElement = UZ_Note.Split(',').FirstOrDefault(x => x.StartsWith(Constant.DatabaseNameIdentifier));
			if (!databaseElement.IsEmpty)
			{
				result = databaseElement.SubstringSafe(3);
			}
			return result;
		}

		public ZString GetNoteWithoutDatabaseDetail()
		{
			var result = new ZStringBuilder(UZ_Note.Split(',').Where(x => !x.StartsWith(Constant.DatabaseNameIdentifier)));
			return result.ToStringWithDelimiterBetweenAppends(",");
		}

		public void SetNoteWithDatabaseDetail(ZString note)
		{
			UZ_Note = string.Format("{0},{1}{2}", note, Constant.DatabaseNameIdentifier, Db.DatabaseName);
		}

		public void SetNoteWithDatabaseDetailAndUpdateTime(ZString note)
		{
			SetNoteWithDatabaseDetail(note);
			UZ_UpdateTime = ZDateTime.UtcNow;
		}

		public void PerformUpdate(Action updateAction)
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			using (var transactionManager = connection.BeginTransactionWithManager())
			{
				var result = ReferenceFileUpdateMutex.AcquireUpdateLockForSharedDatabase(connection, LockType, Core.Constants.CountryCodes.UnitedStates);
				if (result)
				{
					ReloadDataToStopConcurrencyIssues();
					updateAction?.Invoke();
				}
			}
		}

		void ReloadDataToStopConcurrencyIssues()
		{
			if (IsInDatabase)
			{
				Reload();
			}
		}

		string LockType
		{
			get
			{
				var lockType = new Dictionary<string, string>
				{
					{ "", PK.ToStringKey() },
					{ Constant.LastHTSAttempt, nameof(ReferenceLockType.Tariff) },
					{ Constant.RefFileRequestAffirmationOfComplianceTimeStamp, nameof(ReferenceLockType.AffirmationOfCompliance) },
					{ Constant.RefFileRequestAntiDumpingTimeStamp, nameof(ReferenceLockType.AntiDumping) },
					{ Constant.RefFileRequestCarrierCodeTimeStamp, nameof(ReferenceLockType.Carrier) },
					{ Constant.RefFileRequestCountryCodeTimeStamp, nameof(ReferenceLockType.Country) },
					{ Constant.RefFileRequestFirmsCodeTimeStamp, nameof(ReferenceLockType.FIRMS) },
					{ Constant.RefFileRequestForeignPortCodeTimeStamp, nameof(ReferenceLockType.ForeignPort) },
					{ Constant.RefFileRequestRegionDistrictPortCodeTimeStamp, nameof(ReferenceLockType.RegionDistrictPort) },
					{ Constant.RefFileRequestTeamAssignmentTimeStamp, nameof(ReferenceLockType.ImportSpecialistTeam) }
				};

				var result = string.Empty;
				return lockType.TryGetValue(UZ_Name, out result) ? result : UZ_Name.ToString();
			}
		}
	}
}
