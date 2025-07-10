using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportBookings.Business.Testing
{
	public class DtbBookingFetchStrategyTest : TestCaseWithFactory
	{
		public void TestFetchForView()
		{
			var viewFactory = new BusinessObjectFactory();
			var bookings = viewFactory.Load<DtbBooking>(new ZQuery(DtbBookingSchema.PK, CreateTransportBookings()));
			int beforeFetchForView = viewFactory.DatabaseLoadCount;
			foreach (var booking in bookings)
			{
				booking.FetchStrategy.FetchForView(TableColumnsAddedInFetchForView);
			}

			foreach (var booking in bookings)
			{
				var poke = booking.LocalClient;
				var pokeBookingAddress = booking.Address.E2_City;
				var pokeConfirmation = booking.FirstPickup.ReqFrom;
			}

			var expetedDbHits = new Dictionary<string, int>();
			expetedDbHits.Add(DtbBookingSchema.Constants.TableName, 1);
			expetedDbHits.Add(DtbBookingConsolidationSchema.Constants.TableName, 2);
			expetedDbHits.Add(JobHeaderSchema.Constants.TableName, 1);
			expetedDbHits.Add(OrgAddressSchema.Constants.TableName, 2);
			expetedDbHits.Add(JobDocAddressSchema.Constants.TableName, 1);
			expetedDbHits.Add(DtbBookingInstructionSchema.Constants.TableName, 1);
			expetedDbHits.Add(DtbBookingConfirmationSchema.Constants.TableName, 1);

			AssertDbHits(expetedDbHits, viewFactory);
		}

		TableColumn[] TableColumnsAddedInFetchForView
		{
			get
			{
				return new TableColumn[]
				{
					new TableColumn(WhsDocketContainerSchema.Constants.TableName, "LocalClient"),
					new TableColumn(JobDocAddressSchema.Constants.TableName, "Address+E2_City"),
					new TableColumn(DtbBookingConfirmationSchema.Constants.TableName, "FirstPickup+FirstPickupConfirmation+KK_RequiredFrom"),
					new TableColumn(PkgPackageSchema.Constants.TableName,  "FirstPickup+PackageQty")
				};
			}
		}

		List<ZGuid> CreateTransportBookings()
		{
			var result = new List<ZGuid>();

			var importTemplate = Helper.CreateTransportBookingTemplate("IFCL", "FCL Import", Constants.CartageDirection.Import);
			Helper.AddInstructionToTemplate(importTemplate, OrganisationTypesList.Codes.CTO, InstructionTypes.Codes.PickUp);
			Helper.AddInstructionToTemplate(importTemplate, OrganisationTypesList.Codes.CNE, InstructionTypes.Codes.Delivery);
			Helper.AddInstructionToTemplate(importTemplate, OrganisationTypesList.Codes.CYD, InstructionTypes.Codes.Delivery);

			var exportTemplate = Helper.CreateTransportBookingTemplate("EFCL", "FCL Export", Constants.CartageDirection.Import);
			Helper.AddInstructionToTemplate(exportTemplate, OrganisationTypesList.Codes.CYD, InstructionTypes.Codes.PickUp);
			Helper.AddInstructionToTemplate(exportTemplate, OrganisationTypesList.Codes.CNR, InstructionTypes.Codes.PickUp);
			Helper.AddInstructionToTemplate(exportTemplate, OrganisationTypesList.Codes.CTO, InstructionTypes.Codes.Delivery);

			for (int i = 0; i < 10; i++)
			{
				var transportCo = Factory.NewWithValidTestData<OrgHeader>();
				var cto = Factory.NewWithValidTestData<OrgHeader>();
				var cne = Factory.NewWithValidTestData<OrgHeader>();
				var cyd = Factory.NewWithValidTestData<OrgHeader>();

				var consolidation = Helper.CreateConsolidation();
				var booking = consolidation.Bookings.AddNew();
				booking.Address.E2_OA_Address = transportCo.MainAddress.PK;
				booking.KM_KT_NKBookingTemplate = importTemplate.KT_Code;

				var ctoInstruction = booking.Instructions[0];
				var cneInstruction = booking.Instructions[1];
				var cydInstruction = booking.Instructions[2];

				ctoInstruction.Address.E2_OA_Address = cto.MainAddress.PK;
				cneInstruction.Address.E2_OA_Address = cne.MainAddress.PK;
				cydInstruction.Address.E2_OA_Address = cyd.MainAddress.PK;

				var requiredFrom = ZDateTime.Now;
				var ctoConfirmation1 = ctoInstruction.Confirmations.AddNew();
				ctoConfirmation1.KK_RequiredFrom = requiredFrom;
				var ctoConfirmation2 = ctoInstruction.Confirmations.AddNew();
				ctoConfirmation1.KK_RequiredFrom = requiredFrom;
				var cneConfirmation1 = cneInstruction.Confirmations.AddNew();
				ctoConfirmation1.KK_RequiredFrom = requiredFrom;
				var cneConfirmation2 = cneInstruction.Confirmations.AddNew();
				ctoConfirmation1.KK_RequiredFrom = requiredFrom;
				var cydConfirmation1 = cydInstruction.Confirmations.AddNew();
				ctoConfirmation1.KK_RequiredFrom = requiredFrom;
				var cydConfirmation2 = cydInstruction.Confirmations.AddNew();
				ctoConfirmation1.KK_RequiredFrom = requiredFrom;

				// Add Packages and Divots
				var package1 = consolidation.PackageJob.Packages.AddNew();
				package1.KP_PackageQty = 10;
				var package2 = consolidation.PackageJob.Packages.AddNew();
				package2.KP_PackageQty = 20;

				var ctoDivot1 = ctoInstruction.PackageDivots.AddNew();
				ctoDivot1.KD_KP_Package = package1.PK;
				var ctoDivot2 = ctoInstruction.PackageDivots.AddNew();
				ctoDivot2.KD_KP_Package = package2.PK;

				ctoConfirmation1.ParentID_InstructionOrPackageDivot = ctoDivot1.PK;
				ctoConfirmation2.ParentID_InstructionOrPackageDivot = ctoDivot2.PK;

				// Set Local Client
				var client = Helper.CreateOrganisation("Org" + i);
				var job = new JobHeader.Loader(booking).TryCreate();
				job.JH_OA_LocalChargesAddr = client.MainAddress.PK;

				result.Add(booking.PK);
			}

			Factory.Save();

			return result;
		}

		TransportBookingTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingTestHelper(Factory)); }
		}
		TransportBookingTestHelper helper;
	}
}
