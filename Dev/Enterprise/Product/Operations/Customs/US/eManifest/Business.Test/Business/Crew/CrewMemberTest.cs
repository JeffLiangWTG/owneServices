using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.eManifest.Business.Testing
{
	[TestedType(typeof(CrewMember))]
	sealed class CrewTest : EnterpriseBusinessObjectTestCase
	{
		public void TestGetTopBusinessObject()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("US"))
			{
				var declaration = Factory.New<JobDeclaration>();
				var crew = Factory.NewWithValidTestData<CrewMember>();
				crew.Trip.BH_ParentID = declaration.PK;
				crew.Trip.BH_ParentTableCode = declaration.TablePrefix;
				AssertEquals(declaration, crew.GetTopBusinessObject());
				var controllerFactory = System.Reflection.Assembly.Load("Enterprise.ZArchitecture.GUI").GetType("Enterprise.ZArchitecture.Modules.ZControllerFactory").GetField("Instance").GetValue(null);
				var controller = controllerFactory.GetType().GetMethod("GetControllerForBizo").Invoke(controllerFactory, new[] { declaration });
				AssertNotNull("We got the controller, so we got the Form", controller);
				AssertEquals("Controller type", "Enterprise.Customs.US.Module.JobDeclarationController", controller.GetType().FullName);
			}
		}

		public void TestDefaultUsAddressFromCarrier()
		{
			var trip = Factory.New<Trip>();
			var oh = Factory.NewWithValidTestData<OrgHeader>();
			var oa = oh.Addresses.MainAddress;
			var oa2 = oh.Addresses.AddNew();
			trip.BH_OH_Carrier = oh.PK;
			var crew = trip.CrewMembers.AddNew();
			AssertEquals(oa.PK, crew.USAddress.E2_OA_Address);

			crew.USAddress.E2_OA_Address = oa2.PK;
			crew.CP_BH_Header = ZGuid.Empty;
			crew.CP_BH_Header = trip.PK;
			AssertEquals(oa2.PK, crew.USAddress.E2_OA_Address);
		}

		public void TestDelete()
		{
			var crewMember = Factory.New<CrewMember>();
			var address = crewMember.USAddress;
			address.E2_AddressOverride = true;

			crewMember.Delete();

			Assert("USAddress should be deleted", address.IsDeleted);
		}

		public void TestUSAddressReadOnly()
		{
			var trip = Factory.New<Trip>();
			var crewMember = trip.CrewMembers.AddNew();
			crewMember.CP_Type = CrewTypes.Codes.CrewMember;
			var address = crewMember.USAddress;
			address.E2_AddressOverride = true;

			AssertEquals("Address should not be read only for crew member", false, crewMember.USAddress.ReadOnly);

			crewMember.OnSaving();
			AssertEquals("Address should NOT be empty for crew member", false, crewMember.USAddress.IsEmpty);

			crewMember.CP_Type = CrewTypes.Codes.Passenger;
			AssertEquals("Address should be read only for passenger", true, crewMember.USAddress.ReadOnly);

			crewMember.OnSaving();
			AssertEquals("Address should be empty for passenger", true, crewMember.USAddress.IsEmpty);

			crewMember.CP_Type = CrewTypes.Codes.ResponsibleParty;
			AssertEquals("Address should not be read only for responsible party", false, crewMember.USAddress.ReadOnly);

			address.E2_AddressOverride = true;
			crewMember.OnSaving();
			AssertEquals("Address should NOT be empty for responsible party", false, crewMember.USAddress.IsEmpty);

			crewMember.CP_Type = CrewTypes.Codes.Passenger;
			Factory.Save();

			crewMember = new BusinessObjectFactory().Load<CrewMember>(crewMember.PK);
			AssertEquals("Address should be read only for passenger when loaded", true, crewMember.USAddress.ReadOnly);
		}

		public void TestDriversLicenseAndHazmatEndorsement()
		{
			var crewMember = Factory.New<Trip>().CrewMembers.AddNew();
			crewMember.CP_Type = CrewTypes.Codes.Passenger;

			var cert1 = crewMember.Certificates.AddNew();
			cert1.XZ_Type = TravelDocumentTypes.Codes.DrivingLicenseNational;
			cert1.XZ_RefNumber = "AAB13545";

			var cert2 = crewMember.Certificates.AddNew();
			cert2.XZ_Type = TravelDocumentTypes.Codes.EnhancedDriversLicense;
			cert2.XZ_RefNumber = "ZZX13545";

			var cert3 = crewMember.Certificates.AddNew();
			cert3.XZ_Type = TravelDocumentTypes.Codes.CommercialDriversLicense;
			cert3.XZ_RefNumber = "QQW13545";

			var cert4 = crewMember.Certificates.AddNew();
			cert4.XZ_Type = TravelDocumentTypes.Codes.HazmatEndorsement;
			cert4.XZ_RefNumber = "ABC78946";

			AssertEquals("Passenger is not crew member and nobody cares if he has a drivers license", string.Empty, crewMember.DriversLicense);
			AssertEquals("Passenger is not crew member and nobody cares if he has a hazmat endorsement", string.Empty, crewMember.HazmatEndorsement);
			AssertEquals("Passenger is not crew member and nobody cares if he has a hazmat endorsement: CP_HasHazmatEndorsment", false, crewMember.CP_HasHazmatEndorsment);
			AssertEquals("Passenger is not crew member and nobody cares if he has a hazmat endorsement: CP_HasHazmatEndorsmentInfo.ReadOnly", true, crewMember.CP_HasHazmatEndorsmentInfo.ReadOnly);

			crewMember.CP_Type = CrewTypes.Codes.CrewMember;
			AssertEquals("Crew Member with commercial drivers license", "QQW13545", crewMember.DriversLicense);
			AssertEquals("Crew Member with hazmat endorsement", "ABC78946", crewMember.HazmatEndorsement);
			AssertEquals("Crew Member with hazmat endorsement: CP_HasHazmatEndorsment", true, crewMember.CP_HasHazmatEndorsment);
			AssertEquals("Crew Member with hazmat endorsement: CP_HasHazmatEndorsmentInfo.ReadOnly", true, crewMember.CP_HasHazmatEndorsmentInfo.ReadOnly);

			cert3.XZ_Type = TravelDocumentTypes.Codes.OtherTravelDocument;
			cert4.XZ_Type = TravelDocumentTypes.Codes.OtherTravelDocument;
			AssertEquals("Crew Member with enhanced driving license", "ZZX13545", crewMember.DriversLicense);
			AssertEquals("Crew Member without hazmat endorsement", "NO", crewMember.HazmatEndorsement);
			AssertEquals("Crew Member without hazmat endorsement: CP_HasHazmatEndorsment", false, crewMember.CP_HasHazmatEndorsment);
			AssertEquals("Crew Member without hazmat endorsement: CP_HasHazmatEndorsmentInfo.ReadOnly", false, crewMember.CP_HasHazmatEndorsmentInfo.ReadOnly);

			cert2.XZ_Type = TravelDocumentTypes.Codes.OtherTravelDocument;
			AssertEquals("Crew Member with national driving license", "AAB13545", crewMember.DriversLicense);

			cert1.XZ_Type = TravelDocumentTypes.Codes.OtherTravelDocument;
			AssertEquals("Crew Member without a drivers license", string.Empty, crewMember.DriversLicense);

			crewMember.CP_Type = CrewTypes.Codes.ResponsibleParty;
			cert1.XZ_Type = TravelDocumentTypes.Codes.DrivingLicenseNational;
			crewMember.CP_HasHazmatEndorsment = true;
			AssertEquals("Responsible Party with national driving license", "AAB13545", crewMember.DriversLicense);
			AssertEquals("Responsible Party with hazmat endorsement", "YES", crewMember.HazmatEndorsement);
			AssertEquals("Responsible Party with hazmat endorsement: CP_HasHazmatEndorsment", true, crewMember.CP_HasHazmatEndorsment);
			AssertEquals("Responsible Party with hazmat endorsement: CP_HasHazmatEndorsmentInfo.ReadOnly", false, crewMember.CP_HasHazmatEndorsmentInfo.ReadOnly);

			cert2.XZ_Type = TravelDocumentTypes.Codes.EnhancedDriversLicense;
			crewMember.CP_HasHazmatEndorsment = false;
			AssertEquals("Responsible Party with enhanced driving license", "ZZX13545", crewMember.DriversLicense);
			AssertEquals("Responsible Party without hazmat endorsement", "NO", crewMember.HazmatEndorsement);
			AssertEquals("Responsible Party without hazmat endorsement: CP_HasHazmatEndorsment", false, crewMember.CP_HasHazmatEndorsment);
			AssertEquals("Responsible Party without hazmat endorsement: CP_HasHazmatEndorsmentInfo.ReadOnly", false, crewMember.CP_HasHazmatEndorsmentInfo.ReadOnly);

			cert3.XZ_Type = TravelDocumentTypes.Codes.CommercialDriversLicense;
			AssertEquals("Responsible Party with commercial drivers license", "QQW13545", crewMember.DriversLicense);

			cert1.XZ_Type = TravelDocumentTypes.Codes.OtherTravelDocument;
			cert2.XZ_Type = TravelDocumentTypes.Codes.OtherTravelDocument;
			cert3.XZ_Type = TravelDocumentTypes.Codes.OtherTravelDocument;
			AssertEquals("Responsible Party without a drivers license", string.Empty, crewMember.DriversLicense);
		}

		protected override BusinessObject GetNewBusinessObject() => Factory.New<Trip>().CrewMembers.AddNew();
	}
}
