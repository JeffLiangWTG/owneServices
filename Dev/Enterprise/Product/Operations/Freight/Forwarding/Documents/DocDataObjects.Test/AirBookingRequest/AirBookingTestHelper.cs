using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Forwarding.Documents.Testing
{
	static class AirBookingTestHelper
	{
		public static bool SetAirlineTermsAndConditions(this BusinessObjectFactory factory, string airlinePrefix, string termsAndConditions)
		{
			if (factory == null)
			{
				return false;
			}

			var refAirline = RefAirline.LoadFromAirlinePrefix(factory, airlinePrefix);

			if (refAirline == null)
			{
				refAirline = factory.NewWithValidTestData<RefAirline>();
				refAirline.RM_AccountingCode = airlinePrefix;
			}

			var termsAndConditionsNotes = refAirline.Notes.FindByDescription(PredefinedNoteTypes.Instance.TermsAndConditions.Description);

			if (termsAndConditionsNotes.Length == 0)
			{
				refAirline.Notes.AddNew(false, PredefinedNoteTypes.Instance.TermsAndConditions.Description, termsAndConditions);
			}
			else
			{
				termsAndConditionsNotes[0].ST_NoteText = termsAndConditions;
			}

			return true;
		}

		public static IDisposable TempEnableCarrierConfiguration() => FreightDataRegistry.Instance.EnableAirBookingCarrierConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
		public static IDisposable TempDisableCarrierConfiguration() => FreightDataRegistry.Instance.EnableAirBookingCarrierConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

		public static IDisposable TempSetSupportedCarriers()
		{
			const string supportedCarriersJson =
@"[
    {
        ""prefix"": ""057"",
        ""code"": ""AF"",
        ""name"": ""Air France"",
        ""options"": {
            ""uldBooking"": ""NotSupported"",
            ""looseBooking"": ""Required"",
            ""allotmentBooking"": ""NotSupported"",
            ""commodityCode"": ""NotSupported"",
            ""requiredTermsAgreement"": false
        }
    },
    {
        ""prefix"": ""020"",
        ""code"": ""LH"",
        ""name"": ""Lufthansa"",
        ""options"": {
            ""uldBooking"": ""NotSupported"",
            ""looseBooking"": ""NotSupported"",
            ""allotmentBooking"": ""NotSupported"",
            ""commodityCode"": ""NotSupported"",
            ""requiredTermsAgreement"": true
        }
    },
    {
        ""prefix"": ""074"",
        ""code"": ""KL"",
        ""name"": ""KLM Royal Dutch Airlines"",
        ""options"": {
            ""uldBooking"": ""NotSupported"",
            ""looseBooking"": ""Required"",
            ""allotmentBooking"": ""NotSupported"",
            ""commodityCode"": ""NotSupported"",
            ""requiredTermsAgreement"": false
        },
    },
    {
        ""prefix"": ""618"",
        ""code"": ""SQ"",
        ""name"": ""Singapore Airline"",
        ""options"": {
            ""uldBooking"": ""Optional"",
            ""looseBooking"": ""Optional"",
            ""allotmentBooking"": ""Optional"",
            ""commodityCode"": ""Required"",
            ""requiredTermsAgreement"": false
        },
        ""commodities"": [],
        ""products"": []
    },
    {
        ""prefix"": ""607"",
        ""code"": ""EY"",
        ""name"": ""Etihad Airways"",
        ""options"": {
            ""uldBooking"": ""Optional"",
            ""looseBooking"": ""Optional"",
            ""allotmentBooking"": ""Optional"",
            ""commodityCode"": ""Optional"",
            ""requiredTermsAgreement"": false
        },
        ""commodities"": [
            {
                ""code"": ""GENERAL"",
                ""description"": ""GENERAL CARGO"",
                ""specialHandlingCodes"": ""GEN""
            },
            {
                ""Code"": ""ACCESRY"",
                ""Description"": ""ACCESSORIES"",
                ""SpecialHandlingCodes"": ""GEN, HVY, HUM""
            },
            {
                ""Code"": ""ACE"",
                ""Description"": ""AIRCRAFT ENGINES"",
                ""SpecialHandlingCodes"": ""GEN""
            },
            {
                ""Code"": ""ACPTS"",
                ""Description"": ""AIRCRAFT PARTS"",
                ""SpecialHandlingCodes"": ""GEN""
            },
            {
                ""Code"": ""ADHE"",
                ""Description"": ""ADHESIVE"",
                ""SpecialHandlingCodes"": ""GEN""
            },
            {
                ""Code"": ""AERO"",
                ""Description"": ""AEROSOL"",
                ""SpecialHandlingCodes"": ""DGR""
            },
            {
                ""code"": ""OTH"",
                ""description"": ""OTHER""
            }
        ],
	    ""products"": [
	        {
	            ""Code"": ""AOG"",
	            ""Description"": ""Emirates AOG""
	        },
	        {
	            ""Code"": ""AWA"",
	            ""Description"": ""SkyWheels Premium Door-to-Door""
	        },
	        {
	            ""Code"": ""ACE"",
	            ""Description"": ""AIRCRAFT ENGINES""
	        }
	    ]
    }
]";
			var settings = new EBookingCarrierConfiguration
			{
				LastUpdatedTime = ZDateTime.Now,
				LastResponse = supportedCarriersJson
			};

			return FreightDataRegistry.Instance.EBookingCarrierConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, settings);
		}
	}
}
