using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input;

namespace Enterprise.Customs.US.Business.PGARecapPrinting
{
	class PGOrganisationData : IDataSerialiser
	{
		internal PGOrganisationData(BusinessObjectFactory factory)
		{
			this.factory = Argument.NotNull(factory, "factory");
		}
		readonly BusinessObjectFactory factory;

		public AEPAPG19 PG19;
		public IEnumerable<AEPAPG55> PG19PG55s
		{
			get { return pg19PG55s; }
		}
		List<AEPAPG55> pg19PG55s;
		public IEnumerable<AEPAPG60> PG19PG60s
		{
			get { return pg19PG60s; }
		}
		List<AEPAPG60> pg19PG60s;

		public AEPAPG20 PG20;
		public IEnumerable<AEPAPG60> PG20PG60s
		{
			get { return pg20PG60s; }
		}
		List<AEPAPG60> pg20PG60s;

		public AEPAPG21 PG21;
		public IEnumerable<AEPAPG55> PG21PG55s
		{
			get { return pg21PG55s; }
		}
		List<AEPAPG55> pg21PG55s;
		public IEnumerable<AEPAPG60> PG21PG60s
		{
			get { return pg21PG60s; }
		}
		List<AEPAPG60> pg21PG60s;

		public void AddPG60(AEPAPG60 pg60)
		{
			if (PG21 != null)
			{
				(pg21PG60s = pg21PG60s ?? new List<AEPAPG60>()).Add(pg60);
			}
			else if (PG20 != null)
			{
				(pg20PG60s = pg20PG60s ?? new List<AEPAPG60>()).Add(pg60);
			}
			else if (PG19 != null)
			{
				(pg19PG60s = pg19PG60s ?? new List<AEPAPG60>()).Add(pg60);
			}
		}

		public void AddPG55(AEPAPG55 pg55)
		{
			if (PG21 != null)
			{
				(pg21PG55s = pg21PG55s ?? new List<AEPAPG55>()).Add(pg55);
			}
			else if (PG19 != null)
			{
				(pg19PG55s = pg19PG55s ?? new List<AEPAPG55>()).Add(pg55);
			}
		}

		public void Clear()
		{
			PG19 = null;
			Clear(pg19PG55s);
			Clear(pg19PG60s);
			PG20 = null;
			Clear(pg20PG60s);
			PG21 = null;
			Clear(pg21PG55s);
			Clear(pg21PG60s);
		}

		void Clear(List<AEPAPG60> list)
		{
			if (list != null)
			{
				list.Clear();
			}
		}

		void Clear(List<AEPAPG55> list)
		{
			if (list != null)
			{
				list.Clear();
			}
		}

		EntityIdentificationCodesList EntityIdentificationCodesList
		{
			get { return factory.GetCachedValue<EntityIdentificationCodesList>(); }
		}

		EntityRoleCodeList EntityRoleCodeList
		{
			get { return factory.GetCachedValue<EntityRoleCodeList>(); }
		}

		public IEnumerable<ZString> Serialise()
		{
			var pg19 = PG19;
			if (PG19 != null)
			{
				var idType = "ID";
				if (!pg19.EntityIdentificationCode.IsEmpty)
				{
					idType = EntityIdentificationCodesList.GetDescriptionFromCode(pg19.EntityIdentificationCode);
					if (string.IsNullOrEmpty(idType))
					{
						idType = pg19.EntityIdentificationCode;
					}
				}
				yield return Serialiser.CreateLine(false, Serialiser.CreateValue((EntityRoleCodeList.GetDescriptionFromCode(PG19.EntityRoleCode) ?? pg19.EntityRoleCode) + ": ", PG19PG60s.AddAdditionalInfo(PG19.EntityName, AdditionalInformationQualifierList.Codes.EntityNameOverflow)), Serialiser.CreateValue(idType + ": ", pg19.EntityNumber));
				var addressDetail = new ZStringBuilder();
				addressDetail.AppendIfNotEmpty(PG19PG60s.AddAdditionalInfo(pg19.EntityAddress1, AdditionalInformationQualifierList.Codes.EntityAddress1Overflow));
				var pg20 = PG20;
				if (pg20 != null)
				{
					addressDetail.AppendIfNotEmpty(PG20PG60s.AddAdditionalInfo(pg20.EntityAddress2, AdditionalInformationQualifierList.Codes.EntityAddress2Overflow));
					addressDetail.AppendIfNotEmpty(PG20PG60s.AddAdditionalInfo("", AdditionalInformationQualifierList.Codes.EntityAddressLine3ForPG20));
					addressDetail.AppendIfNotEmpty(PG20PG60s.AddAdditionalInfo("", AdditionalInformationQualifierList.Codes.EntityAddressLine4ForPG20));
					addressDetail.AppendIfNotEmpty(PG20PG60s.AddAdditionalInfo("", AdditionalInformationQualifierList.Codes.EntityAddressLine5ForPG20));
					if (!pg20.EntityApartmentNumberSuiteNumber.IsEmpty)
					{
						addressDetail.Prepend(pg20.EntityApartmentNumberSuiteNumber);
					}
					addressDetail.AppendIfNotEmpty(pg20.EntityCity);
					addressDetail.AppendIfNotEmpty(pg20.EntityStateProvince);
					addressDetail.AppendIfNotEmpty(pg20.EntityZipPostalCode);
					addressDetail.AppendIfNotEmpty(pg20.EntityCountry);
				}
				yield return Serialiser.CreateLine(false, Serialiser.CreateValue(EntityPadding, addressDetail.ToStringWithDelimiterBetweenAppends(" ")));
				yield return PG19PG55s.AddAdditionalRole(ZString.Empty, EntityRoleCodeList);
				var pg21 = PG21;
				if (pg21 != null)
				{
					yield return Serialiser.CreateLine(false, Serialiser.CreateValue(EntityPadding, PG21PG60s.AddAdditionalInfo(pg21.IndividualName, AdditionalInformationQualifierList.Codes.IndividualNameOverflow)), Serialiser.CreateValue("Phone: ", PG21PG60s.AddAdditionalInfo(pg21.TelephoneNumberOfTheIndividual, AdditionalInformationQualifierList.Codes.TelephoneNumberOverflow)), Serialiser.CreateValue("Email or Fax: ", PG21PG60s.AddAdditionalInfo(pg21.EmailAddressOrFaxNumberForTheIndividual, AdditionalInformationQualifierList.Codes.EmailOverflow)));
					yield return PG21PG55s.AddAdditionalRole(pg21.IndividualQualifier == pg19.EntityRoleCode ? ZString.Empty : pg21.IndividualQualifier, EntityRoleCodeList);
				}
			}
		}

		const string EntityPadding = "        ";
	}
}
