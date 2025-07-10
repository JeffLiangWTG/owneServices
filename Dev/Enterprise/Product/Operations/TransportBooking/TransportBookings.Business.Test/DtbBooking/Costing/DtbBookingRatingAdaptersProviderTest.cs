using System.Linq;
using Enterprise.Core;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.TransportCommon.Shared;

namespace Enterprise.TransportBookings.Business.Testing
{
	public class DtbBookingRatingAdaptersProviderTest : DtbBookingTestCaseWithFactory
	{
		public void TestRatingPairsWithNoRatableItems()
		{
			var cnr = Helper.CreateOrganisation("CNR");
			var cne = Helper.CreateOrganisation("CNE");

			var booking = Helper.CreateBooking();
			booking.KM_RatingFreightMode = RatingFreightModes.Codes.Both;
			var cnrInstruction = booking.Instructions.AddNew(InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CNR);
			var cneInstruction = booking.Instructions.AddNew(InstructionTypes.Codes.Delivery, OrganisationTypesList.Codes.CNE);
			cnrInstruction.KN_IsLooseRateable = false;
			cnrInstruction.KN_IsContainerRateable = false;
			cneInstruction.KN_IsLooseRateable = false;
			cneInstruction.KN_IsContainerRateable = false;
			cnrInstruction.Address.OrganisationPK = cnr.PK;
			cneInstruction.Address.OrganisationPK = cne.PK;

			var provider = new DtbBookingRatingAdaptersProvider(booking);

			var adapters = provider.GetAdapters(null, AutoRateOptions.AutorateRevenue);

			AssertEquals("Empty List Check", true, adapters.Count == 0);
		}

		public void TestGetAdapters()
		{
			var cnr = Helper.CreateOrganisation("CNR");
			var cne = Helper.CreateOrganisation("CNE");

			var booking = Helper.CreateBooking();
			booking.KM_RatingFreightMode = RatingFreightModes.Codes.Loose;
			var cnrInstruction = booking.Instructions.AddNew(InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CNR);
			var cneInstruction = booking.Instructions.AddNew(InstructionTypes.Codes.Delivery, OrganisationTypesList.Codes.CNE);
			var pallet = CreatePackage(booking.AssignedPackages, 2, Constants.PkgUnit.Pallet, 20m, Constants.Weight.Kilograms, 0.15m, Constants.Volume.CubicMetres);
			CreateInstructionPkgDivots(cnrInstruction, pallet);
			CreateInstructionPkgDivots(cneInstruction, pallet);
			cnrInstruction.KN_IsLooseRateable = true;
			cneInstruction.KN_IsLooseRateable = true;
			cnrInstruction.Address.OrganisationPK = cnr.PK;
			cneInstruction.Address.OrganisationPK = cne.PK;

			var provider = new DtbBookingRatingAdaptersProvider(booking);
			var adapters = provider.GetAdapters(null, AutoRateOptions.AutorateRevenue);
			AssertEquals(1, adapters.Count);
			AssertEquals(cnr.PK, adapters.First().DebtorOrgs[Registry.Business.RatingDebtorOrgTypes.CNR].PK);
			AssertEquals(cne.PK, adapters.First().DebtorOrgs[Registry.Business.RatingDebtorOrgTypes.CNE].PK);
		}

		public void TestAutoRatingFreightMode()
		{
			var cnr = Helper.CreateOrganisation("CNR");
			var cne = Helper.CreateOrganisation("CNE");
			var booking = Helper.CreateBooking();

			booking.KM_RatingFreightMode = RatingFreightModes.Codes.Loose;
			booking.KM_TransportMode = Constants.TransportModes.Road;

			var cnrInstruction = booking.Instructions.AddNew(InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CNR);
			var cneInstruction = booking.Instructions.AddNew(InstructionTypes.Codes.Delivery, OrganisationTypesList.Codes.CNE);
			var pallet = CreatePackage(booking.AssignedPackages, 2, Constants.PkgUnit.Pallet, 20m, Constants.Weight.Kilograms, 0.15m, Constants.Volume.CubicMetres);
			var container = CreatePackage(booking.AssignedPackages, 2, Constants.PkgUnit.Container, 20m, Constants.Weight.Kilograms, 0.15m, Constants.Volume.CubicMetres);

			CreateInstructionPkgDivots(cnrInstruction, pallet, container);
			CreateInstructionPkgDivots(cneInstruction, pallet, container);
			cnrInstruction.KN_IsLooseRateable = true;
			cnrInstruction.KN_IsContainerRateable = true;
			cneInstruction.KN_IsLooseRateable = true;
			cneInstruction.KN_IsContainerRateable = true;
			cnrInstruction.Address.OrganisationPK = cnr.PK;
			cneInstruction.Address.OrganisationPK = cne.PK;

			var provider = new DtbBookingRatingAdaptersProvider(booking);
			var adapters = provider.GetAdapters(null, AutoRateOptions.AutorateRevenue);

			AssertEquals("Freight Mode LSE and Transport Mode ROA should result in Adapter with LRO FreightMode", FreightMode.LRO, adapters.First().FreightMode);

			booking.KM_RatingFreightMode = RatingFreightModes.Codes.Loose;
			booking.KM_TransportMode = Constants.TransportModes.Rail;

			provider = new DtbBookingRatingAdaptersProvider(booking);
			adapters = provider.GetAdapters(null, AutoRateOptions.AutorateRevenue);

			AssertEquals("Freight Mode LSE and Transport Mode RAI should result in Adapter with LRA FreightMode", FreightMode.LRA, adapters.First().FreightMode);

			booking.KM_RatingFreightMode = RatingFreightModes.Codes.Containerised;
			booking.KM_TransportMode = Constants.TransportModes.Road;

			provider = new DtbBookingRatingAdaptersProvider(booking);
			adapters = provider.GetAdapters(null, AutoRateOptions.AutorateRevenue);

			AssertEquals("Freight Mode CNT and Transport Mode ROA should result in Adapter with FRO FreightMode", FreightMode.FRO, adapters.First().FreightMode);

			booking.KM_RatingFreightMode = RatingFreightModes.Codes.Containerised;
			booking.KM_TransportMode = Constants.TransportModes.Rail;

			provider = new DtbBookingRatingAdaptersProvider(booking);
			adapters = provider.GetAdapters(null, AutoRateOptions.AutorateRevenue);

			AssertEquals("Freight Mode CNT and Transport Mode RAI should result in Adapter with FRA FreightMode", FreightMode.FRA, adapters.First().FreightMode);
		}

		public void TestGetAdapters_MultiPickup()
		{
			var cnr1 = Helper.CreateOrganisation("CNR1");
			var cnr2 = Helper.CreateOrganisation("CNR2");
			var cnr3 = Helper.CreateOrganisation("CNR3");
			var cne = Helper.CreateOrganisation("CNE");
			var booking = Helper.CreateBooking();
			booking.KM_RatingFreightMode = RatingFreightModes.Codes.Loose;
			var cnrInstruction = booking.Instructions.AddNew(InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CNR);
			var cnrInstruction2 = booking.Instructions.AddNew(InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CNR);
			var cnrInstruction3 = booking.Instructions.AddNew(InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CNR);
			var cneInstruction = booking.Instructions.AddNew(InstructionTypes.Codes.Delivery, OrganisationTypesList.Codes.CNE);
			var pallet = CreatePackage(booking.AssignedPackages, 2, Constants.PkgUnit.Pallet, 20m, Constants.Weight.Kilograms, 0.15m, Constants.Volume.CubicMetres);
			CreateInstructionPkgDivots(cnrInstruction, pallet);
			CreateInstructionPkgDivots(cnrInstruction2, pallet);
			CreateInstructionPkgDivots(cnrInstruction3, pallet);
			CreateInstructionPkgDivots(cneInstruction, pallet);
			cnrInstruction.KN_IsLooseRateable = true;
			cnrInstruction2.KN_IsLooseRateable = true;
			cnrInstruction3.KN_IsLooseRateable = true;
			cneInstruction.KN_IsLooseRateable = true;
			cnrInstruction.Address.OrganisationPK = cnr1.PK;
			cnrInstruction2.Address.OrganisationPK = cnr2.PK;
			cnrInstruction3.Address.OrganisationPK = cnr3.PK;
			cneInstruction.Address.OrganisationPK = cne.PK;

			var provider = new DtbBookingRatingAdaptersProvider(booking);
			var adapters = provider.GetAdapters(null, AutoRateOptions.AutorateRevenue);
			AssertEquals(3, adapters.Count);
			AssertEquals(cnr1.PK, adapters.First().DebtorOrgs[Registry.Business.RatingDebtorOrgTypes.CNR].PK);
			AssertEquals(cne.PK, adapters.First().DebtorOrgs[Registry.Business.RatingDebtorOrgTypes.CNE].PK);

			AssertEquals(cnr2.PK, adapters.ElementAt(1).DebtorOrgs[Registry.Business.RatingDebtorOrgTypes.CNR].PK);
			AssertEquals(cne.PK, adapters.ElementAt(1).DebtorOrgs[Registry.Business.RatingDebtorOrgTypes.CNE].PK);

			AssertEquals(cnr3.PK, adapters.ElementAt(2).DebtorOrgs[Registry.Business.RatingDebtorOrgTypes.CNR].PK);
			AssertEquals(cne.PK, adapters.ElementAt(2).DebtorOrgs[Registry.Business.RatingDebtorOrgTypes.CNE].PK);
		}

		public void TestGetAdapters_MultiDelivery()
		{
			var cnr = Helper.CreateOrganisation("CNR");
			var cne1 = Helper.CreateOrganisation("CNE1");
			var cne2 = Helper.CreateOrganisation("CNE2");
			var cne3 = Helper.CreateOrganisation("CNE3");

			var booking = Helper.CreateBooking();
			booking.KM_RatingFreightMode = RatingFreightModes.Codes.Loose;
			var cnrInstruction = booking.Instructions.AddNew(InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CNR);
			var cneInstruction = booking.Instructions.AddNew(InstructionTypes.Codes.Delivery, OrganisationTypesList.Codes.CNE);
			var cneInstruction2 = booking.Instructions.AddNew(InstructionTypes.Codes.Delivery, OrganisationTypesList.Codes.CNE);
			var cneInstruction3 = booking.Instructions.AddNew(InstructionTypes.Codes.Delivery, OrganisationTypesList.Codes.CNE);
			var pallet = CreatePackage(booking.AssignedPackages, 2, Constants.PkgUnit.Pallet, 20m, Constants.Weight.Kilograms, 0.15m, Constants.Volume.CubicMetres);
			CreateInstructionPkgDivots(cnrInstruction, pallet);
			CreateInstructionPkgDivots(cneInstruction, pallet);
			CreateInstructionPkgDivots(cneInstruction2, pallet);
			CreateInstructionPkgDivots(cneInstruction3, pallet);
			cnrInstruction.KN_IsLooseRateable = true;
			cneInstruction.KN_IsLooseRateable = true;
			cneInstruction2.KN_IsLooseRateable = true;
			cneInstruction3.KN_IsLooseRateable = true;
			cnrInstruction.Address.OrganisationPK = cnr.PK;
			cneInstruction.Address.OrganisationPK = cne1.PK;
			cneInstruction2.Address.OrganisationPK = cne2.PK;
			cneInstruction3.Address.OrganisationPK = cne3.PK;

			var provider = new DtbBookingRatingAdaptersProvider(booking);
			var adapters = provider.GetAdapters(null, AutoRateOptions.AutorateRevenue);
			AssertEquals(3, adapters.Count);
			AssertEquals(cnr.PK, adapters.First().DebtorOrgs[Registry.Business.RatingDebtorOrgTypes.CNR].PK);
			AssertEquals(cne1.PK, adapters.First().DebtorOrgs[Registry.Business.RatingDebtorOrgTypes.CNE].PK);

			AssertEquals(cnr.PK, adapters.ElementAt(1).DebtorOrgs[Registry.Business.RatingDebtorOrgTypes.CNR].PK);
			AssertEquals(cne2.PK, adapters.ElementAt(1).DebtorOrgs[Registry.Business.RatingDebtorOrgTypes.CNE].PK);

			AssertEquals(cnr.PK, adapters.ElementAt(2).DebtorOrgs[Registry.Business.RatingDebtorOrgTypes.CNR].PK);
			AssertEquals(cne3.PK, adapters.ElementAt(2).DebtorOrgs[Registry.Business.RatingDebtorOrgTypes.CNE].PK);
		}

		protected PkgPackage CreatePackage(PackageCollectionForBooking packageCollectionForTransport, int quantity, string quantityUQ, decimal weight, string weightUQ, decimal volume, string volumeUQ)
		{
			var result = packageCollectionForTransport.AddNew();
			result.KP_PackageQty = quantity;
			result.KP_F3_NKPackType = quantityUQ;
			result.KP_Weight = weight;
			result.KP_WeightUQ = weightUQ;
			result.KP_Volume = volume;
			result.KP_VolumeUQ = volumeUQ;

			return result;
		}

		protected void CreateInstructionPkgDivots(DtbBookingInstruction fromInstruction, params PkgPackage[] packages)
		{
			foreach (var package in packages)
			{
				var divot = fromInstruction.PackageDivots.AddNew();
				divot.KD_KP_Package = package.PK;
				divot.KD_Quantity = package.KP_PackageQty;
			}
		}
	}
}
