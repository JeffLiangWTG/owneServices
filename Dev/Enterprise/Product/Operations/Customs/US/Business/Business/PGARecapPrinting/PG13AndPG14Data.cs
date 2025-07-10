using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input;

namespace Enterprise.Customs.US.Business.PGARecapPrinting
{
	class PG13AndPG14Data : IDataSerialiser
	{
		internal PG13AndPG14Data(BusinessObjectFactory factory)
		{
			this.factory = Argument.NotNull(factory, "factory");
		}
		public AEPAPG13 PG13;
		public AEPAPG14 PG14;

		public void Clear()
		{
			PG13 = null;
			PG14 = null;
		}

		public IEnumerable<ZString> Serialise()
		{
			var issuerOfLPCO = ZString.Empty;
			var location = "Location";
			var locationDetail = ZString.Empty;
			var pg13 = PG13;
			if (pg13 != null)
			{
				issuerOfLPCO = pg13.IssuerOfLPCO;
				if (!pg13.LPCOIssuerGovernmentGeographicCodeQualifier.IsEmpty)
				{
					var locationType = Serialiser.PrependDescription(pg13.LPCOIssuerGovernmentGeographicCodeQualifier, LPCOIssuerLocationTypeList, Serialiser.DelimitedType.Parenthesis);
					if (locationType == pg13.LPCOIssuerGovernmentGeographicCodeQualifier)
					{
						location += " (" + pg13.LPCOIssuerGovernmentGeographicCodeQualifier + ")";
					}
					else
					{
						location = locationType;
					}
				}
				locationDetail = Serialiser.GetLocationWithDesc(factory, pg13.LPCOIssuerGovernmentGeographicCodeQualifier, pg13.LocationCountryStateProvinceOfIssuerOfTheLPCO);
				if (!pg13.RegionalDescriptionOfLocationOfAgencyIssuingTheLPCO.IsEmpty)
				{
					if (!locationDetail.IsEmpty)
					{
						locationDetail += " - ";
					}
					locationDetail += pg13.RegionalDescriptionOfLocationOfAgencyIssuingTheLPCO;
				}
			}
			var transactionType = ZString.Empty;
			var licenceType = "LPCO Type";
			var numberorName = ZString.Empty;
			var dateType = "Date";
			var date = ZString.Empty;
			var quantity = ZString.Empty;
			var exemptionCode = ZString.Empty;
			var pg14 = PG14;
			if (pg14 != null)
			{
				transactionType = pg14.LPCOTransactionType;
				if (!pg14.LPCOType.IsEmpty)
				{
					var type = Serialiser.PrependDescription(pg14.LPCOType, LPCOTypeList, Serialiser.DelimitedType.Parenthesis);
					if (type == pg14.LPCOType)
					{
						licenceType += " (" + pg14.LPCOType + ")";
					}
					else
					{
						licenceType = type;
					}
				}
				numberorName = pg14.LPCONumberorName;
				if (!pg14.LPCODateQualifier.IsEmpty)
				{
					var dateDesc = Serialiser.PrependDescription(pg14.LPCODateQualifier, LPCODateQualifierList, Serialiser.DelimitedType.Parenthesis);
					if (dateDesc == pg14.LPCODateQualifier)
					{
						dateType += " (" + pg14.LPCODateQualifier + ")";
					}
					else
					{
						dateType = dateDesc;
					}
					if (!pg14.LPCODate.IsEmpty)
					{
						date = pg14.LPCODate.ToShortDateString();
					}
					if (!pg14.LPCOQuantity.IsEmpty)
					{
						quantity = (pg14.LPCOQuantity.ToString() + " " + pg14.LPCOUnitOfMeasure).TrimEnd();
					}
				}
				if (!pg14.ExemptionCode.IsEmpty)
				{
					exemptionCode = pg14.ExemptionCode;
				}
			}

			yield return Serialiser.CreateLine(false, Serialiser.CreateValue("Issuer of LPCO: ", issuerOfLPCO), Serialiser.CreateValue(location + ": ", locationDetail), Serialiser.CreateValue("Transaction: ", LPCOTransactionTypeList.GetDescriptionFromCode(transactionType) ?? transactionType),
				Serialiser.CreateValue(licenceType + ": ", numberorName), Serialiser.CreateValue(dateType + ": ", date), Serialiser.CreateValue("Quantity: ", quantity), Serialiser.CreateValue("Exemption: ", exemptionCode));
		}

		LPCOTransactionTypeList LPCOTransactionTypeList
		{
			get { return factory.GetCachedValue<LPCOTransactionTypeList>(); }
		}

		LPCOTypeList LPCOTypeList
		{
			get { return factory.GetCachedValue<LPCOTypeList>(); }
		}

		LPCODateQualifierList LPCODateQualifierList
		{
			get { return factory.GetCachedValue<LPCODateQualifierList>(); }
		}

		LPCOIssuerLocationTypeList LPCOIssuerLocationTypeList
		{
			get { return factory.GetCachedValue<LPCOIssuerLocationTypeList>(); }
		}

		readonly BusinessObjectFactory factory;
	}
}
