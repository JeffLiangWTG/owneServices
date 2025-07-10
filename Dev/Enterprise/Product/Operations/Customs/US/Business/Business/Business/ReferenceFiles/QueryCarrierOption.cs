using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input;

namespace Enterprise.Customs.US.Business
{
	public class QueryCarrierOption : NonPersistentBusinessObject, IObsoleteValidation
	{
		public QueryCarrierOption(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public void SendQuery()
		{
			ERFF106 erff106 = new ERFF106();
			ReplaceInvalidCharactersAndSetValues(erff106);
			new ReferenceFileRequester().RequestRefFileSimple(erff106);
		}

		void ReplaceInvalidCharactersAndSetValues(ERFF106 erff106)
		{
			var code = US_CarrierCode;
			var name = US_CarrierName;
			erff106.CarrierCode = StringChecker.ReplaceSpecialCharactersWithSpaces(code);
			erff106.CarrierName = StringChecker.ReplaceSpecialCharactersWithSpaces(name);
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateUS_CarrierCode();
			ValidateUS_CarrierName();
		}

		#region US_CarrierCode

		[CargoWise.ComponentModel.MaxLength(4)]
		public ZString US_CarrierCode
		{
			get { return carrierCode; }
			set
			{
				value = value.ConvertToWesternEuropeanCharacters();
				SetNonPersistentPropertyValue(US_CarrierCodeInfo, ref carrierCode, value);
				ValidateUS_CarrierCode();
			}
		}
		ZString carrierCode;

		const int US_CarrierCode_MinLengh = 2;

		public ZPropertyInfo US_CarrierCodeInfo
		{
			get { return GetZPropertyInfo(nameof(US_CarrierCode)); }
		}

		public void ValidateUS_CarrierCode()
		{
			 if (!IsValidationSuspended)
			{
				US_CarrierCodeInfo.ClearAllNotifications();

				if (!US_CarrierCode.IsEmpty)
				{
					if (US_CarrierCode.Length < US_CarrierCode_MinLengh || US_CarrierCode.Length > US_CarrierCodeInfo.MaxLength)
					{
						US_CarrierCodeInfo.AddError(string.Format("Carrier Code should be between {0} and {1} Alpha-Numerics.", US_CarrierCode_MinLengh, US_CarrierCodeInfo.MaxLength));
					}
					if (!StringChecker.IsLettersAndNumbersAndSpaces(US_CarrierCode))
					{
						US_CarrierCodeInfo.AddError(StringChecker.ShouldBeLettersAndNumbersSpaceOnly);
					}
				}
			}
		}

		#endregion

		#region US_CarrierName

		[CargoWise.ComponentModel.MaxLength(35)]
		public ZString US_CarrierName
		{
			get { return carrierName; }
			set
			{
				value = value.ConvertToWesternEuropeanCharacters();
				SetNonPersistentPropertyValue(US_CarrierNameInfo, ref carrierName, value);
				ValidateUS_CarrierName();
			}
		}
		ZString carrierName;

		public ZPropertyInfo US_CarrierNameInfo
		{
			get { return GetZPropertyInfo(nameof(US_CarrierName)); }
		}

		public void ValidateUS_CarrierName()
		{
			if (!IsValidationSuspended)
			{
				US_CarrierNameInfo.ClearAllNotifications();
				if (!US_CarrierName.IsEmpty)
				{
					if (US_CarrierName.Length > US_CarrierNameInfo.MaxLength)
					{
						US_CarrierNameInfo.AddError(string.Format("Carrier Code should be less than {0} Alpha-Numerics.", US_CarrierNameInfo.MaxLength));
					}
					if (!StringChecker.IsLettersAndNumbersAndSpaces(US_CarrierName))
					{
						US_CarrierNameInfo.AddMessageError(StringChecker.ShouldBeLettersAndNumbersSpaceOnly);
					}
				}
			}
		}

		#endregion
	}
}
