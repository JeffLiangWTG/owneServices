using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Module
{
	public class FTZAdmissionNumberFilter : ModuleTextFilter
	{
		public FTZAdmissionNumberFilter(ZString description)
			: base(description, DummyQuery)
		{
		}

		public new FTZAdmissionNumberFilterValidation Validation => (FTZAdmissionNumberFilterValidation)base.Validation;

		[CargoWise.ComponentModel.MaxLength(9)]
		public ZString ZoneID
		{
			get => zoneID;
			set
			{
				SetNonPersistentPropertyValue(ZoneIDInfo, ref zoneID, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateZoneID();
				}
				ZoneIDInfo.RefreshBinding();
			}
		}
		ZString zoneID;

		public ZPropertyInfo ZoneIDInfo => GetZPropertyInfo(nameof(ZoneID));

		[CargoWise.ComponentModel.MaxLength(2)]
		public ZString Year
		{
			get => year;
			set
			{
				SetNonPersistentPropertyValue(YearInfo, ref year, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateYear();
				}
				YearInfo.RefreshBinding();
			}
		}
		ZString year;

		public ZPropertyInfo YearInfo => GetZPropertyInfo(nameof(Year));

		[CargoWise.ComponentModel.MaxLength(8)]
		public ZString ControlNumber
		{
			get => controlNumber;
			set
			{
				SetNonPersistentPropertyValue(ControlNumberInfo, ref controlNumber, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateControlNumber();
				}
				ControlNumberInfo.RefreshBinding();
			}
		}
		ZString controlNumber;

		public ZPropertyInfo ControlNumberInfo => GetZPropertyInfo(nameof(ControlNumber));

		protected override ZQuery GetQuery()
		{
			var query = new ZQuery();

			if (!ZoneID.IsEmpty || !Year.IsEmpty || !ControlNumber.IsEmpty)
			{
				query.AddToFilter(GetFTZControlNumberFilter());
			}
			return query;
		}

		protected override void ClearCore()
		{
			base.ClearCore();
			ZoneID = ZString.Empty;
			Year = ZString.Empty;
			ControlNumber = ZString.Empty;
		}

		protected override ModuleFilterValidation GetNewValidation() => new FTZAdmissionNumberFilterValidation(this);

		ZQuery GetFTZControlNumberFilter()
		{
			var result = new ZDBOnlyQuery(typeof(JobDeclaration));
			result.AddToFilter(JobDeclarationSchema.JE_MessageType, JobMessageTypeList.Codes.FTZ);

			var controlNumberQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
			controlNumberQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.UnitedStates.FTZ);
			controlNumberQuery.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Core.Constants.CountryCodes.UnitedStates);

			if (!ZoneID.IsEmpty && !Year.IsEmpty && !ControlNumber.IsEmpty)
			{
				var formattedValue = ZoneID + FTZAdmissionNumberRetriever.FTZAdmissionNumberSeparator + Year + FTZAdmissionNumberRetriever.FTZAdmissionNumberSeparator + ControlNumber;
				controlNumberQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, SQLComparisonOperator.StartsWith, formattedValue);
			}
			else if (!ZoneID.IsEmpty && Year.IsEmpty && ControlNumber.IsEmpty)
			{
				controlNumberQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, SQLComparisonOperator.StartsWith, ZoneID);
			}
			else if (!ZoneID.IsEmpty && !Year.IsEmpty && ControlNumber.IsEmpty)
			{
				controlNumberQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, SQLComparisonOperator.StartsWith, ZoneID + FTZAdmissionNumberRetriever.FTZAdmissionNumberSeparator + Year);
			}
			else if (ZoneID.IsEmpty && !Year.IsEmpty && ControlNumber.IsEmpty)
			{
				controlNumberQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, SQLComparisonOperator.Contains, FTZAdmissionNumberRetriever.FTZAdmissionNumberSeparator + Year + FTZAdmissionNumberRetriever.FTZAdmissionNumberSeparator);
			}
			else if (ZoneID.IsEmpty && Year.IsEmpty && !ControlNumber.IsEmpty)
			{
				controlNumberQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, SQLComparisonOperator.EndsWith, ControlNumber);
			}
			else if (ZoneID.IsEmpty && !Year.IsEmpty && !ControlNumber.IsEmpty)
			{
				controlNumberQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, SQLComparisonOperator.Contains, FTZAdmissionNumberRetriever.FTZAdmissionNumberSeparator + Year + FTZAdmissionNumberRetriever.FTZAdmissionNumberSeparator + ControlNumber);
			}
			else if (!ZoneID.IsEmpty && Year.IsEmpty && !ControlNumber.IsEmpty)
			{
				controlNumberQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, SQLComparisonOperator.StartsWith, ZoneID);
				controlNumberQuery.AddToFilter(JoinCondition.And, CusEntryNumSchema.CE_EntryNum, SQLComparisonOperator.EndsWith, FTZAdmissionNumberRetriever.FTZAdmissionNumberSeparator + ControlNumber);
			}

			result.AddSubQuery(controlNumberQuery, JoinCondition.And);
			return result;
		}

		static ZQuery DummyQuery(SQLComparisonOperator comparisonOperator, ZString value) => new ZQuery();
	}
}
