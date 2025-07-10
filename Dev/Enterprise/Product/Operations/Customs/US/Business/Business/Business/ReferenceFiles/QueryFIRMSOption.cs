using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input;

namespace Enterprise.Customs.US.Business
{
	public class QueryFIRMSOption : NonPersistentBusinessObject, IObsoleteValidation
	{
		public QueryFIRMSOption(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public MQEDIMessage SendQuery()
		{
			ERFF111 erff111 = new ERFF111();
			ReplaceInvalidCharactersAndSetValues(erff111);
			erff111.DistrictCode = US_DistrictCode;
			if (US_FIRMSCode.IsEmpty && US_FacilityName.IsEmpty)
			{
				erff111.BeginDate = ReferenceFileRequester.FIRMSBeginDate;
			}
			return new ReferenceFileRequester().RequestRefFileSimple(erff111);
		}

		void ReplaceInvalidCharactersAndSetValues(ERFF111 erff111)
		{
			var code = US_FIRMSCode;
			var name = US_FacilityName;
			erff111.FIRMSCode = StringChecker.ReplaceSpecialCharactersWithSpaces(code);
			erff111.NameOfFacility = StringChecker.ReplaceAsValidForClassX(name);
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateUS_FIRMSCode();
			ValidateUS_FacilityName();
			ValidateUS_DistrictCode();
		}

		#region US_FIRMSCode

		[CargoWise.ComponentModel.MaxLength(4)]
		public ZString US_FIRMSCode
		{
			get { return firmsCode; }
			set
			{
				SetNonPersistentPropertyValue(US_FIRMSCodeInfo, ref firmsCode, value);
				ValidateUS_FIRMSCode();
			}
		}
		ZString firmsCode;

		public ZPropertyInfo US_FIRMSCodeInfo
		{
			get { return GetZPropertyInfo(nameof(US_FIRMSCode)); }
		}

		public void ValidateUS_FIRMSCode()
		{
			if (!IsValidationSuspended)
			{
				US_FIRMSCodeInfo.ClearAllNotifications();

				if (!US_FIRMSCode.IsEmpty)
				{
					if (US_FIRMSCode.Length != US_FIRMSCodeInfo.MaxLength)
					{
						US_FIRMSCodeInfo.AddError(string.Format("FIRMS Code should be {0} Alpha-Numerics and spaces.", US_FIRMSCodeInfo.MaxLength));
					}
					if (!StringChecker.IsLettersAndNumbersAndSpaces(US_FIRMSCode))
					{
						US_FIRMSCodeInfo.AddMessageError(StringChecker.ShouldBeLettersAndNumbersSpaceOnly);
					}
				}
			}
		}

		#endregion

		#region US_FacilityName

		[CargoWise.ComponentModel.MaxLength(35)]
		public ZString US_FacilityName
		{
			get { return facilityName; }
			set
			{
				SetNonPersistentPropertyValue(US_FacilityNameInfo, ref facilityName, value);
				ValidateUS_FacilityName();
			}
		}
		ZString facilityName;

		public ZPropertyInfo US_FacilityNameInfo
		{
			get { return GetZPropertyInfo(nameof(US_FacilityName)); }
		}

		public void ValidateUS_FacilityName()
		{
			if (!IsValidationSuspended)
			{
				US_FacilityNameInfo.ClearAllNotifications();
				if (!US_FacilityName.IsEmpty)
				{
					if (!US_FIRMSCode.IsEmpty)
					{
						US_FacilityNameInfo.AddError("You can specify only one of 'FIRMS Code' and 'Facility Name'.");
					}
					if (US_FacilityName.Length > US_FacilityNameInfo.MaxLength)
					{
						US_FacilityNameInfo.AddError(string.Format("FIRMS Code should be {0} Alpha-Numerics and spaces.", US_FacilityNameInfo.MaxLength));
					}
					if (!StringChecker.IsValidCharactersForClassX(US_FacilityName))
					{
						US_FacilityNameInfo.AddMessageError(string.Format("Field contains invalid characters. Supported characters are numbers, letters, white spaces and {0}", StringChecker.SpecialCharactersForClassX));
					}
				}
			}
		}

		#endregion

		#region US_DistrictCode

		[CargoWise.ComponentModel.MaxLength(2)]
		public ZString US_DistrictCode
		{
			get { return districtCode; }
			set
			{
				SetNonPersistentPropertyValue(US_DistrictCodeInfo, ref districtCode, value);
				ValidateUS_DistrictCode();
			}
		}
		ZString districtCode;

		public ZPropertyInfo US_DistrictCodeInfo
		{
			get { return GetZPropertyInfo(nameof(US_DistrictCode)); }
		}

		public void ValidateUS_DistrictCode()
		{
			if (!IsValidationSuspended)
			{
				US_DistrictCodeInfo.ClearAllNotifications();
				if (US_DistrictCode.IsEmpty)
				{
					if (!US_FacilityName.IsEmpty)
					{
						US_DistrictCodeInfo.AddError("District Code is required when a 'Facility Name' has been entered.");
					}
				}
				else
				{
					if (!Regex.IsMatch(US_DistrictCode, @"^[0-9]{2}$"))
					{
						US_DistrictCodeInfo.AddError("District Code should be 2 Numerics.");
					}
				}
			}
		}

		#endregion
	}
}
