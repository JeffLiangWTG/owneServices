using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgContainerDetentionValidationTest : BusinessObjectValidationTestCase
	{
		void TestCheckUniqueness(OrgContainerDetentionCollection collection, string[] properties, IZType[] values)
		{
			AssertEquals(properties.Length, values.Length);
			var detention1 = collection.AddNew();
			var detention2 = collection.AddNew();

			var mandatoryProperties = new string[] {
					OrgContainerDetentionSchema.Constants.PD_Direction,
					OrgContainerDetentionSchema.Constants.PD_PenaltyType
			};

			for (var inx = 0; inx < properties.Length; inx++)
			{
				if (mandatoryProperties.Contains(properties[inx]))
				{
					detention1.FindPropertyInfo(properties[inx]).Value = values[inx];
					detention2.FindPropertyInfo(properties[inx]).Value = values[inx];
				}
			}

			AssertDetentionHasUniqueErrors(detention2, true, properties);

			for (var inx = 0; inx < properties.Length; inx++)
			{
				if (!mandatoryProperties.Contains(properties[inx]))
				{
					if (detention1.FindPropertyInfo(properties[inx]).Value.Equals(values[inx]))
					{
						AssertDetentionHasUniqueErrors(detention2, true, properties);
					}
					else
					{
						detention1.FindPropertyInfo(properties[inx]).Value = values[inx];
						AssertDetentionHasUniqueErrors(detention2, false, Array.Empty<string>());
					}

					detention2.FindPropertyInfo(properties[inx]).Value = values[inx];
					AssertDetentionHasUniqueErrors(detention2, true, properties);
				}
			}
		}

		public void TestCheckUniqueness()
		{
			{
				var errorProperties = new string[] { OrgContainerDetentionSchema.Constants.PD_OH_Carrier,
					OrgContainerDetentionSchema.Constants.PD_OriginPortOrCountry,
					OrgContainerDetentionSchema.Constants.PD_DetentionPortOrCountry,
					OrgContainerDetentionSchema.Constants.PD_ContainerType,
					OrgContainerDetentionSchema.Constants.PD_OH_Client,
					OrgContainerDetentionSchema.Constants.PD_Direction };
				TestCheckUniqueness(Org.ConsignorContainerPenalties, errorProperties, new IZType[] { Carrier.PK, (ZString)"JP", (ZString)"AU", (ZString)"20F", Client.PK, (ZString)"IMP" });
			}

			{
				var errorProperties = new string[] { OrgContainerDetentionSchema.Constants.PD_OH_Carrier,
					OrgContainerDetentionSchema.Constants.PD_OriginPortOrCountry,
					OrgContainerDetentionSchema.Constants.PD_DetentionPortOrCountry,
					OrgContainerDetentionSchema.Constants.PD_ContainerType,
					OrgContainerDetentionSchema.Constants.PD_OH_Client,
					OrgContainerDetentionSchema.Constants.PD_Direction };
				TestCheckUniqueness(Org.ConsigneeContainerPenalties, errorProperties, new IZType[] { Carrier.PK, (ZString)"JP", (ZString)"AU", (ZString)"20F", Client.PK, (ZString)"IMP" });
			}

			{
				var errorProperties = new string[] { OrgContainerDetentionSchema.Constants.PD_Direction,
					OrgContainerDetentionSchema.Constants.PD_PenaltyType,
					OrgContainerDetentionSchema.Constants.PD_OH_Client,
					OrgContainerDetentionSchema.Constants.PD_OriginPortOrCountry,
					OrgContainerDetentionSchema.Constants.PD_DetentionPortOrCountry,
					OrgContainerDetentionSchema.Constants.PD_ContainerType };

				TestCheckUniqueness(Org.CarrierContainerPenalties, errorProperties, new IZType[] { (ZString)Core.Constants.ContainerDetentionDirection.Export, (ZString)Core.Constants.ContainerDetentionPenaltyType.DET,
					Client.PK, (ZString)"JP", (ZString)"AU", (ZString)"20F" });
			}

			{
				var errorProperties = new string[] { OrgContainerDetentionSchema.Constants.PD_Direction,
					OrgContainerDetentionSchema.Constants.PD_PenaltyType,
					OrgContainerDetentionSchema.Constants.PD_OH_Client,
					OrgContainerDetentionSchema.Constants.PD_OriginPortOrCountry,
					OrgContainerDetentionSchema.Constants.PD_DetentionPortOrCountry,
					OrgContainerDetentionSchema.Constants.PD_ContainerType };

				TestCheckUniqueness(Org.CarrierContainerPenalties, errorProperties, new IZType[] { (ZString)Core.Constants.ContainerDetentionDirection.Import, (ZString)Core.Constants.ContainerDetentionPenaltyType.DET,
					Client.PK, (ZString)"JP", (ZString)"AU", (ZString)"20F" });
			}

			{
				var errorProperties = new string[] { OrgContainerDetentionSchema.Constants.PD_OH_Carrier,
					OrgContainerDetentionSchema.Constants.PD_DetentionPortOrCountry,
					OrgContainerDetentionSchema.Constants.PD_ContainerType,
					OrgContainerDetentionSchema.Constants.PD_OH_CTO,
					OrgContainerDetentionSchema.Constants.PD_CreditorType };
				TestCheckUniqueness(Org.ConsignorCTOStorages, errorProperties, new IZType[] { Carrier.PK, (ZString)"AU", (ZString)"20F", CTO.PK, (ZString)"CTO" });
				TestCheckUniqueness(Org.ConsigneeCTOStorages, errorProperties, new IZType[] { Carrier.PK, (ZString)"AU", (ZString)"20F", CTO.PK, (ZString)"CTO" });
			}

			{
				var errorProperties = new string[] { OrgContainerDetentionSchema.Constants.PD_Direction,
					OrgContainerDetentionSchema.Constants.PD_PenaltyType,
					OrgContainerDetentionSchema.Constants.PD_OH_Client,
					OrgContainerDetentionSchema.Constants.PD_DetentionPortOrCountry,
					OrgContainerDetentionSchema.Constants.PD_ContainerType,
					OrgContainerDetentionSchema.Constants.PD_OH_CTO };
				TestCheckUniqueness(Org.CarrierContainerPenalties, errorProperties, new IZType[] { (ZString)Core.Constants.ContainerDetentionDirection.Export, (ZString)Core.Constants.ContainerDetentionPenaltyType.STO,
					Client.PK, (ZString)"AU", (ZString)"20F", CTO.PK });
				TestCheckUniqueness(Org.CarrierContainerPenalties, errorProperties, new IZType[] { (ZString)Core.Constants.ContainerDetentionDirection.Import, (ZString)Core.Constants.ContainerDetentionPenaltyType.STO,
					Client.PK, (ZString)"AU", (ZString)"20F", CTO.PK });
			}

			{
				var errorProperties = new string[] { OrgContainerDetentionSchema.Constants.PD_OH_Client,
					OrgContainerDetentionSchema.Constants.PD_DetentionPortOrCountry,
					OrgContainerDetentionSchema.Constants.PD_ContainerType,
					OrgContainerDetentionSchema.Constants.PD_OH_Carrier };
				TestCheckUniqueness(Org.ServiceExportCTOStorages, errorProperties, new IZType[] { Client.PK, (ZString)"AU", (ZString)"20F", Carrier.PK });
				TestCheckUniqueness(Org.ServiceImportCTOStorages, errorProperties, new IZType[] { Client.PK, (ZString)"AU", (ZString)"20F", Carrier.PK });
			}
		}

		public void TestCheckUniqueness_DifferentPages()
		{
			AssertCheckUniqueness_DetentionCollection(Org.ConsignorContainerPenalties, Org.ConsignorCTOStorages);
			AssertCheckUniqueness_DetentionCollection(Org.ConsigneeContainerPenalties, Org.ConsigneeCTOStorages);
		}

		void AssertCheckUniqueness_DetentionCollection(OrgContainerDetentionCollection containerPenaltyCollection, OrgContainerDetentionCollection otherCollection)
		{
			var errorMessage = "You can't enter more than one setting for storage free days.";
			var freeDayType = containerPenaltyCollection == Org.ConsignorContainerPenalties ? "FCL" : "CTD";
			var direction = containerPenaltyCollection == Org.ConsignorContainerPenalties ? Core.Constants.ContainerPenaltyProcessType.Export : Core.Constants.ContainerPenaltyProcessType.Import;
			var containerPenaltyDetention = containerPenaltyCollection.AddNew();
			containerPenaltyDetention.PD_CreditorType = "CAR";
			containerPenaltyDetention.PD_FreeDayType = freeDayType;
			containerPenaltyDetention.PD_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.STO;
			containerPenaltyDetention.PD_Direction = direction;
			containerPenaltyDetention.PD_OH_Client = Client.PK;
			containerPenaltyDetention.PD_DetentionPortOrCountry = "CA";
			containerPenaltyDetention.PD_ContainerType = "20F";
			AssertDetentionHasUniqueErrors(containerPenaltyDetention, false, Array.Empty<string>(), errorMessage);

			var detention1 = otherCollection.AddNew();
			detention1.PD_CreditorType = "CTO";
			detention1.PD_FreeDayType = freeDayType;
			detention1.PD_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.STO;
			detention1.PD_Direction = direction;
			detention1.PD_OH_Client = Client.PK;
			detention1.PD_DetentionPortOrCountry = "CA";
			detention1.PD_ContainerType = "20F";
			AssertDetentionHasUniqueErrors(detention1, false, Array.Empty<string>(), errorMessage);

			var detention2 = otherCollection.AddNew();
			detention2.PD_CreditorType = "CTO";
			detention2.PD_FreeDayType = freeDayType;
			detention2.PD_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.STO;
			detention2.PD_Direction = direction;
			detention2.PD_OH_Client = Client.PK;
			detention2.PD_DetentionPortOrCountry = "CA";
			detention2.PD_ContainerType = "20R";
			AssertDetentionHasUniqueErrors(detention2, false, Array.Empty<string>(), errorMessage);

			var errorProperties = new string[]
			{
				OrgContainerDetentionSchema.Constants.PD_OH_Carrier,
				OrgContainerDetentionSchema.Constants.PD_OH_CTO,
				OrgContainerDetentionSchema.Constants.PD_ContainerType,
				OrgContainerDetentionSchema.Constants.PD_CreditorType,
				OrgContainerDetentionSchema.Constants.PD_DetentionPortOrCountry
			};
			detention2.PD_ContainerType = "20F";
			AssertDetentionHasUniqueErrors(detention2, true, errorProperties, errorMessage);
		}

		public void TestCheckCarrierUniqueness_MDDDET()
		{
			AssertCheckCarrierUniqueness_MDD(Core.Constants.ContainerDetentionPenaltyType.DET, "You can't enter more than one setting for detention free days.");
		}

		public void TestCheckCarrierUniqueness_MDDSTO()
		{
			AssertCheckCarrierUniqueness_MDD(Core.Constants.ContainerDetentionPenaltyType.STO, "You can't enter more than one setting for storage free days.");
		}

		void AssertCheckCarrierUniqueness_MDD(string otherPenaltyType, string errorMessage)
		{
			var collection = Org.CarrierContainerPenalties;

			var errorProperties = new string[] { OrgContainerDetentionSchema.Constants.PD_OH_Client,
					OrgContainerDetentionSchema.Constants.PD_OriginPortOrCountry,
					OrgContainerDetentionSchema.Constants.PD_DetentionPortOrCountry,
					OrgContainerDetentionSchema.Constants.PD_ContainerType };

			var detention1 = collection.AddNew();
			var detention2 = collection.AddNew();

			detention1.PD_PenaltyType = otherPenaltyType;
			detention1.PD_Direction = Core.Constants.ContainerPenaltyProcessType.Import;
			detention1.PD_OH_Client = Client.PK;
			detention1.PD_OH_Carrier = Carrier.PK;
			detention1.PD_OriginPortOrCountry = "JP";
			detention1.PD_DetentionPortOrCountry = "AU";
			detention1.PD_ContainerType = "20F";

			AssertDetentionHasUniqueErrors(detention2, false, Array.Empty<string>(), errorMessage);

			detention2.PD_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.MDD;
			detention2.PD_Direction = Core.Constants.ContainerPenaltyProcessType.Import;
			detention2.PD_OH_Client = Client.PK;
			detention2.PD_OH_Carrier = Carrier.PK;
			detention2.PD_OriginPortOrCountry = "JP";
			detention2.PD_DetentionPortOrCountry = "AU";
			detention2.PD_ContainerType = "20F";

			AssertDetentionHasUniqueErrors(detention2, true, errorProperties, errorMessage);
		}

		#region TestCheckConsigneeUniqueness_MDDSTO

		public void TestCheckConsigneeUniqueness_MDDSTO()
		{
			AssertCheckConsigneeUniqueness_MDD(Core.Constants.ContainerDetentionPenaltyType.STO, "You can't enter more than one setting for storage free days.");
		}

		void AssertCheckConsigneeUniqueness_MDD(string otherPenaltyType, string errorMessage)
		{
			var collection = Org.ConsigneeContainerPenalties;

			var errorProperties = new string[]
			{
				OrgContainerDetentionSchema.Constants.PD_OH_Carrier,
				OrgContainerDetentionSchema.Constants.PD_OH_Client,
				OrgContainerDetentionSchema.Constants.PD_OriginPortOrCountry,
				OrgContainerDetentionSchema.Constants.PD_DetentionPortOrCountry,
				OrgContainerDetentionSchema.Constants.PD_ContainerType,
				OrgContainerDetentionSchema.Constants.PD_Direction
			};

			var detention1 = collection.AddNew();
			var detention2 = collection.AddNew();

			detention1.PD_PenaltyType = otherPenaltyType;

			if (otherPenaltyType == Core.Constants.ContainerDetentionPenaltyType.STO)
			{
				detention1.PD_CreditorType = Core.Constants.ContainerPenaltyCreditorType.Codes.Carrier;
			}

			detention1.PD_Direction = Core.Constants.ContainerPenaltyProcessType.Import;
			detention1.PD_OH_Client = Client.PK;
			detention1.PD_OH_Carrier = Carrier.PK;
			detention1.PD_OriginPortOrCountry = "JP";
			detention1.PD_DetentionPortOrCountry = "AU";
			detention1.PD_ContainerType = "20F";

			AssertDetentionHasUniqueErrors(detention1, false, Array.Empty<string>(), errorMessage);

			detention2.PD_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.MDD;
			detention2.PD_Direction = Core.Constants.ContainerPenaltyProcessType.Import;
			detention2.PD_OH_Client = Client.PK;
			detention2.PD_OH_Carrier = Carrier.PK;
			detention2.PD_OriginPortOrCountry = "JP";
			detention2.PD_DetentionPortOrCountry = "AU";
			detention2.PD_ContainerType = "20F";

			AssertDetentionHasUniqueErrors(detention1, true, errorProperties, errorMessage);
		}

		#endregion

		public void TestCheckPD_OriginPortOrCountry()
		{
			Detention.PD_OriginPortOrCountry = "";
			AssertNoErrors("Empty port or country", Detention.PD_OriginPortOrCountryInfo);
			Detention.PD_OriginPortOrCountry = "AUSYD";
			AssertNoErrors("Valid port", Detention.PD_OriginPortOrCountryInfo);
			Detention.PD_OriginPortOrCountry = "AU";
			AssertNoErrors("Valid country", Detention.PD_OriginPortOrCountryInfo);
			Detention.PD_OriginPortOrCountry = "XXXXX";
			AssertHasErrors("Invalid port", Detention.PD_OriginPortOrCountryInfo);
			Detention.PD_OriginPortOrCountry = "AUS";
			AssertHasErrors("Regions are not allowed at this time as they introduce complexity when it comes to reports", Detention.PD_OriginPortOrCountryInfo);
		}

		public void TestCheckPD_DetentionPortOrCountry()
		{
			Detention.PD_DetentionPortOrCountry = "";
			AssertNoErrors("Empty port or country", Detention.PD_DetentionPortOrCountryInfo);
			Detention.PD_DetentionPortOrCountry = "AUSYD";
			AssertNoErrors("Valid port", Detention.PD_DetentionPortOrCountryInfo);
			Detention.PD_DetentionPortOrCountry = "AU";
			AssertNoErrors("Valid country", Detention.PD_DetentionPortOrCountryInfo);
			Detention.PD_DetentionPortOrCountry = "XXXXX";
			AssertHasErrors("Invalid port", Detention.PD_DetentionPortOrCountryInfo);
			Detention.PD_DetentionPortOrCountry = "AUS";
			AssertHasErrors("Regions are not allowed at this time as they introduce complexity when it comes to reports", Detention.PD_DetentionPortOrCountryInfo);
		}

		public void TestCheckPD_ContainerType_ListValidation()
		{
			Detention.PD_ContainerType = "XXX";
			AssertHasErrors(Detention.PD_ContainerTypeInfo);

			Detention.PD_ContainerType = "20F";
			AssertNoErrors(Detention.PD_ContainerTypeInfo);
		}

		public void TestPD_CreditorType()
		{
			Detention.PD_PenaltyType = Enterprise.Core.Constants.ContainerDetentionPenaltyType.STO;

			Detention.PD_CreditorType = "XXX";
			AssertHasErrors(Detention.PD_CreditorTypeInfo);

			Detention.PD_CreditorType = "CTO";
			AssertNoErrors(Detention.PD_CreditorTypeInfo);

			Detention.PD_CreditorType = "CAR";
			AssertNoErrors(Detention.PD_CreditorTypeInfo);

			Detention.PD_CreditorType = ZString.Empty;
			AssertHasErrors(Detention.PD_CreditorTypeInfo);

			Detention.PD_PenaltyType = Enterprise.Core.Constants.ContainerDetentionPenaltyType.DET;

			Detention.PD_CreditorType = "XXX";
			AssertNoErrors(Detention.PD_CreditorTypeInfo);

			Detention.PD_CreditorType = "CTO";
			AssertNoErrors(Detention.PD_CreditorTypeInfo);

			Detention.PD_CreditorType = "CAR";
			AssertNoErrors(Detention.PD_CreditorTypeInfo);

			Detention.PD_CreditorType = ZString.Empty;
			AssertNoErrors(Detention.PD_CreditorTypeInfo);
		}

		public void TestCheckPD_FreeDayType_ListValidation()
		{
			Detention.PD_FreeDayType = "XXX";
			AssertHasErrors(Detention.PD_FreeDayTypeInfo);

			Detention.PD_FreeDayType = "FCD";
			AssertNoErrors(Detention.PD_FreeDayTypeInfo);
		}

		public void TestCheckPD_PenaltyType_ListValidation()
		{
			Detention.PD_PenaltyType = "DET";
			AssertNoErrors(Detention.PD_PenaltyTypeInfo);

			Detention.PD_PenaltyType = "STO";
			AssertNoErrors(Detention.PD_PenaltyTypeInfo);

			Detention.PD_PenaltyType = "MDD";
			AssertNoErrors(Detention.PD_PenaltyTypeInfo);

			Detention.PD_PenaltyType = "XXX";
			AssertHasErrors(Detention.PD_PenaltyTypeInfo);
		}

		public void TestCheckPD_Direction_ListValidation()
		{
			Detention.PD_Direction = ZString.Empty;
			AssertHasErrors(Detention.PD_DirectionInfo);

			Detention.PD_Direction = "XXX";
			AssertHasErrors(Detention.PD_DirectionInfo);

			Detention.PD_Direction = Core.Constants.ContainerDetentionDirection.Import;
			AssertNoErrors(Detention.PD_DirectionInfo);

			Detention.PD_Direction = Core.Constants.ContainerDetentionDirection.Export;
			AssertNoErrors(Detention.PD_DirectionInfo);
		}

		#region Implementation

		OrgContainerDetention Detention
		{
			get
			{
				if (detention == null)
				{
					detention = Factory.New<OrgContainerDetention>();
				}
				return detention;
			}
		}
		OrgContainerDetention detention;

		OrgHeader Client
		{
			get
			{
				if (client == null)
				{
					client = Factory.New<OrgHeader>();
					client.OH_IsConsignee = client.SecurityProvider.HasNewDetailsOrgTypeFlagCrr;
				}
				return client;
			}
		}
		OrgHeader client;

		OrgHeader Carrier
		{
			get
			{
				if (carrier == null)
				{
					carrier = Factory.New<OrgHeader>();
					carrier.OH_IsShippingProvider = carrier.SecurityProvider.HasNewDetailsOrgTypeFlagCrr;
				}
				return carrier;
			}
		}
		OrgHeader carrier;

		OrgHeader CTO
		{
			get
			{
				if (cto == null)
				{
					cto = Factory.New<OrgHeader>();
				}
				return cto;
			}
		}
		OrgHeader cto;

		OrgHeader Org
		{
			get
			{
				if (org == null)
				{
					org = Factory.New<OrgHeader>();
				}
				return org;
			}
		}
		OrgHeader org;

		void AssertDetentionHasUniqueErrors(OrgContainerDetention detention, bool hasErrors, string[] propertyNameList, string errorMessage = "")
		{
			var uniqueneCheckProperties = new string[] { OrgContainerDetentionSchema.Constants.PD_OH_Client ,
				OrgContainerDetentionSchema.Constants.PD_OH_Carrier,
				OrgContainerDetentionSchema.Constants.PD_OriginPortOrCountry,
				OrgContainerDetentionSchema.Constants.PD_DetentionPortOrCountry,
				OrgContainerDetentionSchema.Constants.PD_ContainerType,
				OrgContainerDetentionSchema.Constants.PD_OH_CTO };

			detention.Validation.ValidateAll();

			errorMessage = string.IsNullOrEmpty(errorMessage) ? (detention.PD_PenaltyType == Enterprise.Core.Constants.ContainerDetentionPenaltyType.STO ? "You can't enter more than one setting for storage free days." : "You can't enter more than one setting for detention free days.") : errorMessage;
			if (hasErrors)
			{
				foreach (var propertyName in propertyNameList)
				{
					AssertHasError($"If all {propertyNameList.Length} properties are not unique, there should be an error", detention.FindPropertyInfo(propertyName), errorMessage);
				}

				foreach (var propertyName in uniqueneCheckProperties)
				{
					if (!propertyNameList.Contains(propertyName))
					{
						AssertNoError(detention.FindPropertyInfo(propertyName), errorMessage);
					}
				}
			}
			else
			{
				foreach (var propertyName in uniqueneCheckProperties)
				{
					AssertNoError(detention.FindPropertyInfo(propertyName), errorMessage);
				}
			}
		}

		#endregion
	}
}
