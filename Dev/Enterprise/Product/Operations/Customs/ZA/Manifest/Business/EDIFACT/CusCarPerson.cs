using System.Globalization;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Universal.Messaging.CUSCAR;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ZA.Manifest.Business.EDIFACT
{
	public class CusCarPerson : ICusCarPerson
	{
		public CusCarPerson(CusPerson person, PartyType party)
		{
			cusPerson = person;
			PartyType = party;
		}
		readonly CusPerson cusPerson;

		#region ICusCarPerson

		public PartyType PartyType { get; }

		public ZString Surname => (" " + cusPerson.PersonFullName).Split(' ').Last();

		public ZString FullName => cusPerson.PersonFullName;

		public ZString PassportNumber => cusPerson.PersonPassport;

		public ZString TravelDocumentType => cusPerson.TravelDocumentTypeInZA;

		public ZString DrivingLicenceNumber => cusPerson.PersonIdentificationNumber;

		public ZString AdditionalInformationOne
		{
			get
			{
				var passPortExpiry = cusPerson.PersonPassportExpiry;
				var dateOfBirth = cusPerson.PersonBirthDate.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
				return SmushFieldsTogether(new Field(passPortExpiry.ToString("yyyyMMdd", CultureInfo.InvariantCulture), 8),
											new Field(dateOfBirth, 8));
			}
		}

		public ZString AdditionalInformationTwo
		{
			get
			{
				var gender = cusPerson.PersonGender;
				var occupation = cusPerson.OccupationInZA;
				var reasonForMovement = cusPerson.ReasonForMovementInZA;
				var travellerType = cusPerson.TravellerTypeInZA;

				var residence = RefCountry.LoadFromCountryCode(cusPerson.Factory, cusPerson.PersonCountry)?.RN_IsoAlpha3Code;
				var nationality = RefCountry.LoadFromCountryCode(cusPerson.Factory, cusPerson.PersonNationality)?.RN_IsoAlpha3Code;
				var passportCountry = RefCountry.LoadFromCountryCode(cusPerson.Factory, cusPerson.PersonPassportPlaceOfIssue)?.RN_IsoAlpha3Code;

				return SmushFieldsTogether(new Field(gender, 1),
					new Field(occupation, 1),
					new Field(reasonForMovement, 1),
					new Field(travellerType, 3),
					new Field(residence, 3),
					new Field(nationality, 3),
					new Field(passportCountry, 3));
			}
		}

		public ZString AdditionalInformationTwoFor16A
		{
			get
			{
				var gender = cusPerson.PersonGender;
				var occupation = cusPerson.OccupationInZA;
				var reasonForMovement = cusPerson.ReasonForMovementInZA;
				var travellerType = cusPerson.TravellerTypeInZA;

				var glbPerson = cusPerson.Person;
				var residence = glbPerson?.PER_RN_NKCountry;
				var nationality = glbPerson?.PER_RN_NKNationalityCodeISO;
				var passportCountry = glbPerson?.PER_PassportPlaceOfIssue;

				return SmushFieldsTogether(new Field(gender, 1),
											new Field(occupation, 1),
											new Field(reasonForMovement, 1),
											new Field(travellerType, 3),
											new Field(residence, 2),
											new Field(nationality, 2),
											new Field(passportCountry, 2));
			}
		}

		#endregion

		static ZString SmushFieldsTogether(params Field[] data)
		{
			var sb = new ZStringBuilder();
			foreach (var field in data)
			{
				var formatted = field.Value.Left(field.Length).PadRight(field.Length, ' ');
				sb.Append(formatted);
			}
			return sb.ToString();
		}

		class Field
		{
			public readonly ZString Value;
			public readonly ZInt Length;

			public Field(string value, int l)
			{
				Value = value;
				Length = l;
			}
		}
	}
}
