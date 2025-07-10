using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.PortMessaging.Business.Testing
{
	[TestedType(typeof(PackLinePortMessaging))]
	sealed class PackLinePortMessagingTest : EnterpriseBusinessObjectTestCase
	{
		public void TestLoad()
		{
			var packline = Factory.New<ForwardingPackLine>();
			AssertEquals(null, PackLinePortMessaging.Load(packline));

			var portMessaging = Factory.New<PackLinePortMessaging>();
			portMessaging.JLM_JL_PackLine = packline.PK;

			AssertEquals(portMessaging.PK, PackLinePortMessaging.Load(packline).PK);
		}

		public void TestLoadOrCreate()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var packline1 = shipment.OuterPackLines.AddNew();
			var packline2 = shipment.OuterPackLines.AddNew();

			var portMessaging1 = PackLinePortMessaging.LoadOrCreate(packline1);
			AssertEquals(packline1.PK, portMessaging1.JLM_JL_PackLine);

			var portMessaging11 = PackLinePortMessaging.LoadOrCreate(packline1);
			AssertEquals(portMessaging1, portMessaging11);

			var portMessaging2 = PackLinePortMessaging.LoadOrCreate(packline2);
			AssertEquals(packline2.PK, portMessaging2.JLM_JL_PackLine);

			portMessaging1.JLM_MovementReferenceNumber = "TOENABLESAVE";
			portMessaging2.JLM_MovementReferenceNumber = "TOENABLESAVE";

			Factory.Save();

			var anotherFactory = new BusinessObjectFactory();
			packline1 = anotherFactory.Load<ForwardingPackLine>(packline1.PK);
			packline2 = anotherFactory.Load<ForwardingPackLine>(packline2.PK);

			var portMessagingReloaded1 = PackLinePortMessaging.LoadOrCreate(packline1);
			AssertEquals(portMessaging1.PK, portMessagingReloaded1.PK);

			var portMessagingReloaded2 = PackLinePortMessaging.LoadOrCreate(packline2);
			AssertEquals(portMessaging2.PK, portMessagingReloaded2.PK);
		}

		public void TestIsSavedByFactory()
		{
			var portMessaging = Factory.New<PackLinePortMessaging>();
			AssertEquals("Should not be saved when there is no data", false, portMessaging.IsSavedByFactory);

			var propertyInfos = portMessaging.ZPropertyInfoHash.Cast<ZPropertyInfo>()
				.Where(property => !IPortMessagingExtensions.PropertiesToExclude.Contains(property.Name));

			foreach (var propertyInfo in propertyInfos)
			{
				var originalValue = propertyInfo.Value;
				if (propertyInfo.Value is ZString)
				{
					propertyInfo.Value = (ZString)"X";
				}
				else if (propertyInfo.Value is ZBool)
				{
					propertyInfo.Value = (ZBool)true;
				}
				else if (propertyInfo.Value is ZDateTime)
				{
					propertyInfo.Value = ZDateTime.Now;
				}

				AssertEquals(true, portMessaging.IsSavedByFactory);

				propertyInfo.Value = originalValue;
				AssertEquals(false, portMessaging.IsSavedByFactory);
			}

			var packline = Factory.NewWithValidTestData<ForwardingPackLine>();
			portMessaging.JLM_JL_PackLine = packline.PK;

			portMessaging.JLM_MovementReferenceNumber = "123";
			AssertEquals(true, portMessaging.IsSavedByFactory);

			Factory.Save();

			var anotherFactory = new BusinessObjectFactory();
			portMessaging = anotherFactory.Load<PackLinePortMessaging>(portMessaging.PK);
			portMessaging.JLM_MovementReferenceNumber = "";

			AssertEquals(true, portMessaging.IsInDatabase);
			AssertEquals(false, portMessaging.HasData());
			AssertEquals("Always saved when it's already in database", true, portMessaging.IsSavedByFactory);
		}

		public void TestAdditionalDakosyValidationIsIncluded()
		{
			var portMessaging = Factory.New<PackLinePortMessaging>();
			AssertEquals(false, portMessaging.Validation.ContainsPiggybackedValidation(typeof(PackLinePortMessagingForDakosyValidation)));

			var shipment1 = Factory.New<ForwardingShipment>();
			shipment1.JS_TransportMode = Constants.TransportModes.Air;
			var packline1 = shipment1.OuterPackLines.AddNew();
			var portMessaging1 = PackLinePortMessaging.LoadOrCreate(packline1);
			AssertEquals(false, portMessaging1.Validation.ContainsPiggybackedValidation(typeof(PackLinePortMessagingForDakosyValidation)));

			var shipment2 = Factory.New<ForwardingShipment>();
			shipment2.JS_TransportMode = Constants.TransportModes.Sea;
			var packline2 = shipment2.OuterPackLines.AddNew();
			var portMessaging2 = PackLinePortMessaging.LoadOrCreate(packline2);
			AssertEquals(false, portMessaging2.Validation.ContainsPiggybackedValidation(typeof(PackLinePortMessagingForDakosyValidation)));

			var consol = shipment2.Consols.AddNew();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			AssertEquals(false, portMessaging2.Validation.ContainsPiggybackedValidation(typeof(PackLinePortMessagingForDakosyValidation)));

			consol.JK_RL_NKLoadPort = "DEHAM";
			AssertEquals(true, portMessaging2.Validation.ContainsPiggybackedValidation(typeof(PackLinePortMessagingForDakosyValidation)));

			consol.JK_RL_NKLoadPort = "DEFRA";
			AssertEquals(false, portMessaging2.Validation.ContainsPiggybackedValidation(typeof(PackLinePortMessagingForDakosyValidation)));

			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "DEHAM";
			AssertEquals(true, portMessaging2.Validation.ContainsPiggybackedValidation(typeof(PackLinePortMessagingForDakosyValidation)));

			consol.JK_RL_NKDischargePort = "DEFRA";
			AssertEquals(false, portMessaging2.Validation.ContainsPiggybackedValidation(typeof(PackLinePortMessagingForDakosyValidation)));

			var transport = consol.Transports.AddNew();
			transport.JW_RL_NKLoadPort = "DEHAM";
			AssertEquals(true, portMessaging2.Validation.ContainsPiggybackedValidation(typeof(PackLinePortMessagingForDakosyValidation)));

			transport.JW_RL_NKLoadPort = "AUMEL";
			AssertEquals(false, portMessaging2.Validation.ContainsPiggybackedValidation(typeof(PackLinePortMessagingForDakosyValidation)));

			transport.JW_RL_NKDiscPort = "DEHAM";
			AssertEquals(true, portMessaging2.Validation.ContainsPiggybackedValidation(typeof(PackLinePortMessagingForDakosyValidation)));
		}

		public void TestPropertiesReadOnlyState()
		{
			var portMessaging = Factory.New<PackLinePortMessaging>();
			portMessaging.JLM_EntryType = EntryTypeList.Codes.AESExportDeclaration;
			Assert(portMessaging.JLM_ATBNumberInfo.ReadOnly);
			Assert(portMessaging.JLM_ExemptionReasonInfo.ReadOnly);
			Assert(portMessaging.JLM_Annex30ATypeInfo.ReadOnly);
			Assert(portMessaging.JLM_Annex30AFailureProcessInfo.ReadOnly);
			Assert(portMessaging.JLM_ExportDeclarationReferenceInfo.ReadOnly);

			portMessaging.JLM_EntryType = EntryTypeList.Codes.Message;
			Assert(!portMessaging.JLM_ATBNumberInfo.ReadOnly);
			Assert(!portMessaging.JLM_ExemptionReasonInfo.ReadOnly);
			Assert(!portMessaging.JLM_Annex30ATypeInfo.ReadOnly);
			Assert(portMessaging.JLM_Annex30AFailureProcessInfo.ReadOnly);
			Assert(portMessaging.JLM_ExportDeclarationReferenceInfo.ReadOnly);

			portMessaging.JLM_Annex30AType = Annex30ATypeList.Codes.AlreadyCompleted;
			Assert(!portMessaging.JLM_Annex30AFailureProcessInfo.ReadOnly);

			portMessaging.JLM_EntryType = EntryTypeList.Codes.ExitSummaryDeclaration;
			Assert(!portMessaging.JLM_ATBNumberInfo.ReadOnly);
			Assert(portMessaging.JLM_ExemptionReasonInfo.ReadOnly);
			Assert(portMessaging.JLM_Annex30ATypeInfo.ReadOnly);
			Assert(portMessaging.JLM_Annex30AFailureProcessInfo.ReadOnly);
			Assert(portMessaging.JLM_ExportDeclarationReferenceInfo.ReadOnly);

			portMessaging.JLM_EntryType = EntryTypeList.Codes.OtherExemptions;
			Assert(portMessaging.JLM_ATBNumberInfo.ReadOnly);
			Assert(!portMessaging.JLM_ExemptionReasonInfo.ReadOnly);
			Assert(portMessaging.JLM_Annex30ATypeInfo.ReadOnly);
			Assert(portMessaging.JLM_Annex30AFailureProcessInfo.ReadOnly);
			Assert(portMessaging.JLM_ExportDeclarationReferenceInfo.ReadOnly);

			portMessaging.JLM_EntryType = EntryTypeList.Codes.EmergencyConcept;
			Assert(portMessaging.JLM_ATBNumberInfo.ReadOnly);
			Assert(portMessaging.JLM_ExemptionReasonInfo.ReadOnly);
			Assert(portMessaging.JLM_Annex30ATypeInfo.ReadOnly);
			Assert(portMessaging.JLM_Annex30AFailureProcessInfo.ReadOnly);
			Assert(!portMessaging.JLM_ExportDeclarationReferenceInfo.ReadOnly);

			portMessaging.JLM_EntryType = EntryTypeList.Codes.ExitSummaryDeclarationWithoutMRN;
			Assert(!portMessaging.JLM_ATBNumberInfo.ReadOnly);
			Assert(portMessaging.JLM_MovementReferenceNumberInfo.ReadOnly);
			Assert(portMessaging.JLM_MovementReferenceNumberCompleteInfo.ReadOnly);
		}

		public void TestResetReadOnlyPropertiesDependingOnEntryType()
		{
			var portMessaging = Factory.New<PackLinePortMessaging>();
			portMessaging.JLM_EntryType = EntryTypeList.Codes.Message;
			portMessaging.JLM_ATBNumber = "123";
			portMessaging.JLM_ExemptionReason = "1";
			portMessaging.JLM_Annex30AType = "A";
			portMessaging.JLM_Annex30AFailureProcess = true;

			portMessaging.JLM_EntryType = EntryTypeList.Codes.AESExportDeclaration;
			CombineAssertions("Property values have been cleared", () =>
				{
					AssertEquals("", portMessaging.JLM_ATBNumber);
					AssertEquals("", portMessaging.JLM_ExemptionReason);
					AssertEquals("", portMessaging.JLM_Annex30AType);
					AssertEquals(false, portMessaging.JLM_Annex30AFailureProcess);
				});

			portMessaging.JLM_MovementReferenceNumber = "TEST";
			portMessaging.JLM_MovementReferenceNumberComplete = true;

			portMessaging.JLM_EntryType = EntryTypeList.Codes.ExitSummaryDeclarationWithoutMRN;
			CombineAssertions("Property values have been cleared", () =>
			{
				AssertEquals(string.Empty, portMessaging.JLM_MovementReferenceNumber);
				AssertEquals(false, portMessaging.JLM_MovementReferenceNumberComplete);
			});
		}

		public void TestExemptionReasonResetOnEntryTypeSet()
		{
			var portMessaging = Factory.New<PackLinePortMessaging>();
			portMessaging.JLM_ExemptionReason = "X";
			portMessaging.JLM_EntryType = EntryTypeList.Codes.Message;
			AssertEquals(ZString.Empty, portMessaging.JLM_ExemptionReason);

			portMessaging.JLM_ExemptionReason = "X";
			portMessaging.JLM_EntryType = EntryTypeList.Codes.Message;
			AssertEquals("X", portMessaging.JLM_ExemptionReason);
		}

		public void TestDGTechnicalNameProperty()
		{
			var portMessaging = Factory.New<PackLinePortMessaging>();
			var packline = Factory.NewWithValidTestData<ForwardingPackLine>();
			portMessaging.JLM_JL_PackLine = packline.PK;
			AssertEquals("DGTechnicalName should return an empty string when there is no dangerous good", ZString.Empty, portMessaging.DGTechnicalName);

			var undg = packline.UNDGs.AddNew();
			var provision = Factory.New<UNDGCommonData>();
			var subs = Factory.New<UNDGSubstance>();
			subs.DG_Code = "1001";
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			undg.LinkDefault(subs);
			undg.Substance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			provision.DC_Type = UNDGCommonDataLookups.TypeConstants.SpecialProvisions;
			provision.DC_Index = "274";
			undg.Substance.SpecialProvisions.Add(provision);
			AssertEquals("DGTechnicalName should return an empty string when there the technical name has not been set", ZString.Empty, portMessaging.DGTechnicalName);

			undg.DI_TechnicalName = "Oat";
			AssertEquals("DGTechnicalName should return the technical name correctly when it has been set", "Oat", portMessaging.DGTechnicalName);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var shipment = factory.New<ForwardingShipment>();
			var packline = shipment.OuterPackLines.AddNew();

			var portMessaging = factory.New<PackLinePortMessaging>();
			portMessaging.JLM_JL_PackLine = packline.PK;
			portMessaging.JLM_MovementReferenceNumber = "TOENABLESAVE";

			return portMessaging;
		}

		#endregion
	}
}
