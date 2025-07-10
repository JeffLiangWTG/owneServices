using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.Integration.TransportBooking;
using Enterprise.Packing.Business.Testing;
using Enterprise.TransportBookings.Shared.Testing;
using Enterprise.TransportCommon.Shared;

namespace Enterprise.TransportBookings.Business.Testing
{
	public class TransportBookingTestData : ITransportBookingTestData
	{
		public TransportBookingTestData(BusinessObjectFactory factory)
		{
			Factory = factory;
		}

		readonly BusinessObjectFactory Factory;

		public void CreateTransportBookingTemplates()
		{
			// Templates don't currently contain Default Container Mode per instruction, but it will be added later, so create the templates now and use them

			// FCL

			if (importFCLTemplate == null)
			{
				importFCLTemplate = Helper.CreateTransportBookingTemplate("IFCL", "FCL Import", Constants.CartageDirection.Import, RatingFreightModes.Codes.Containerised);
				Helper.AddInstructionToTemplate(importFCLTemplate, OrganisationTypesList.Codes.CTO, InstructionTypes.Codes.PickUp, true);
				Helper.AddInstructionToTemplate(importFCLTemplate, OrganisationTypesList.Codes.CNE, InstructionTypes.Codes.Multi, true);
				Helper.AddInstructionToTemplate(importFCLTemplate, OrganisationTypesList.Codes.CYD, InstructionTypes.Codes.Delivery);
			}

			if (exportFCLTemplate == null)
			{
				exportFCLTemplate = Helper.CreateTransportBookingTemplate("EFCL", "FCL Export", Constants.CartageDirection.Export, RatingFreightModes.Codes.Containerised);
				Helper.AddInstructionToTemplate(exportFCLTemplate, OrganisationTypesList.Codes.CYD, InstructionTypes.Codes.PickUp);
				Helper.AddInstructionToTemplate(exportFCLTemplate, OrganisationTypesList.Codes.CNR, InstructionTypes.Codes.Multi, true);
				Helper.AddInstructionToTemplate(exportFCLTemplate, OrganisationTypesList.Codes.CTO, InstructionTypes.Codes.Delivery, true);
			}

			if (domesticFCLTemplate == null)
			{
				domesticFCLTemplate = Helper.CreateTransportBookingTemplate("DFCL", "FCL Domestic", Constants.CartageDirection.Local, RatingFreightModes.Codes.Containerised);
				Helper.AddInstructionToTemplate(domesticFCLTemplate, OrganisationTypesList.Codes.CYD, InstructionTypes.Codes.PickUp);
				Helper.AddInstructionToTemplate(domesticFCLTemplate, OrganisationTypesList.Codes.CNR, InstructionTypes.Codes.Multi, true);
				Helper.AddInstructionToTemplate(domesticFCLTemplate, OrganisationTypesList.Codes.CFS, InstructionTypes.Codes.Delivery, true);
			}

			// LCL

			if (importLCLTemplate == null)
			{
				importLCLTemplate = Helper.CreateTransportBookingTemplate("ILCL", "LCL Import", Constants.CartageDirection.Import, RatingFreightModes.Codes.Loose);
				Helper.AddInstructionToTemplate(importLCLTemplate, OrganisationTypesList.Codes.CTO, InstructionTypes.Codes.PickUp, false, true);
				Helper.AddInstructionToTemplate(importLCLTemplate, OrganisationTypesList.Codes.CNE, InstructionTypes.Codes.Delivery, false, true);
			}

			if (exportLCLTemplate == null)
			{
				exportLCLTemplate = Helper.CreateTransportBookingTemplate("ELCL", "LCL Export", Constants.CartageDirection.Export, RatingFreightModes.Codes.Loose);
				Helper.AddInstructionToTemplate(exportLCLTemplate, OrganisationTypesList.Codes.CNR, InstructionTypes.Codes.PickUp, false, true);
				Helper.AddInstructionToTemplate(exportLCLTemplate, OrganisationTypesList.Codes.CTO, InstructionTypes.Codes.Delivery, false, true);
			}

			if (domesticLCLTemplate == null)
			{
				domesticLCLTemplate = Helper.CreateTransportBookingTemplate("DLCL", "LCL Domestic", Constants.CartageDirection.Local, RatingFreightModes.Codes.Loose);
				Helper.AddInstructionToTemplate(domesticLCLTemplate, OrganisationTypesList.Codes.CNR, InstructionTypes.Codes.PickUp, false, true);
				Helper.AddInstructionToTemplate(domesticLCLTemplate, OrganisationTypesList.Codes.CFS, InstructionTypes.Codes.Delivery, false, true);
			}

			// Mixed

			if (importMixedTemplate == null)
			{
				importMixedTemplate = Helper.CreateTransportBookingTemplate("IMIX", "Mixed Import", Constants.CartageDirection.Import);
				Helper.AddInstructionToTemplate(importMixedTemplate, OrganisationTypesList.Codes.CTO, InstructionTypes.Codes.PickUp, true);
				Helper.AddInstructionToTemplate(importMixedTemplate, OrganisationTypesList.Codes.CFS, InstructionTypes.Codes.Multi, true, true);
				Helper.AddInstructionToTemplate(importMixedTemplate, OrganisationTypesList.Codes.CYD, InstructionTypes.Codes.Delivery, true);
				Helper.AddInstructionToTemplate(importMixedTemplate, OrganisationTypesList.Codes.CNE, InstructionTypes.Codes.Delivery, false, true);
			}

			if (exportMixedTemplate == null)
			{
				exportMixedTemplate = Helper.CreateTransportBookingTemplate("EMIX", "Mixed Export", Constants.CartageDirection.Export);
				Helper.AddInstructionToTemplate(exportMixedTemplate, OrganisationTypesList.Codes.CYD, InstructionTypes.Codes.PickUp, true);
				Helper.AddInstructionToTemplate(exportMixedTemplate, OrganisationTypesList.Codes.CNR, InstructionTypes.Codes.PickUp, false, true);
				Helper.AddInstructionToTemplate(exportMixedTemplate, OrganisationTypesList.Codes.CFS, InstructionTypes.Codes.Multi, true, true);
				Helper.AddInstructionToTemplate(exportMixedTemplate, OrganisationTypesList.Codes.CTO, InstructionTypes.Codes.Delivery, true);
			}

			if (domesticMixedTemplate == null)
			{
				domesticMixedTemplate = Helper.CreateTransportBookingTemplate("DMIX", "Mixed Domestic", Constants.CartageDirection.Local);
				Helper.AddInstructionToTemplate(domesticMixedTemplate, OrganisationTypesList.Codes.CNR, InstructionTypes.Codes.PickUp, true, true);
				Helper.AddInstructionToTemplate(domesticMixedTemplate, OrganisationTypesList.Codes.CFS, InstructionTypes.Codes.Multi);
				Helper.AddInstructionToTemplate(domesticMixedTemplate, OrganisationTypesList.Codes.CNE, InstructionTypes.Codes.Delivery, true, true);
			}
		}

		public IEnumerable<IDtbBooking> CreateTransportBookings()
		{
			var result = new List<IDtbBooking>();
			CreateTransportBookingTemplates();

			var dummy = Factory.New<DummyWithDtbBooking>();
			dummy.Z0_Description = "DUM456";
			var consolidation = Helper.CreateConsolidation(dummy);
			var packageJob = Helper.CreatePackageJob(consolidation);
			var container = PackingHelper.CreatePackage(packageJob, 1, Constants.PkgUnit.Container);
			var pallets10 = PackingHelper.CreatePackage(container, 10, Constants.PkgUnit.Pallet);
			container.KP_PackageID = "CONT1";

			if (importFCLTransportBooking == null)
			{
				importFCLTransportBooking = Helper.CreateBooking(consolidation);
				importFCLTransportBooking.KM_KT_NKBookingTemplate = ImportFCLTemplate.KT_Code;
				Helper.CreateAndAssignPackageDivots(importFCLTransportBooking, container);
				result.Add(importFCLTransportBooking);
			}

			if (exportFCLTransportBooking == null)
			{
				exportFCLTransportBooking = Helper.CreateBooking();
				exportFCLTransportBooking.KM_KT_NKBookingTemplate = ExportFCLTemplate.KT_Code;
				Helper.CreateAndAssignPackageDivots(exportFCLTransportBooking, container);
				result.Add(exportFCLTransportBooking);
			}

			if (domesticFCLTransportBooking == null)
			{
				domesticFCLTransportBooking = Helper.CreateBooking();
				domesticFCLTransportBooking.KM_KT_NKBookingTemplate = DomesticFCLTemplate.KT_Code;
				Helper.CreateAndAssignPackageDivots(domesticFCLTransportBooking, container);
				result.Add(domesticFCLTransportBooking);
			}

			// LCL

			if (importLCLTransportBooking == null)
			{
				importLCLTransportBooking = Helper.CreateBooking();
				importLCLTransportBooking.KM_KT_NKBookingTemplate = ImportLCLTemplate.KT_Code;
				Helper.CreateAndAssignPackageDivots(importLCLTransportBooking, pallets10, 10);
				result.Add(importLCLTransportBooking);
			}

			if (exportLCLTransportBooking == null)
			{
				exportLCLTransportBooking = Helper.CreateBooking();
				exportLCLTransportBooking.KM_KT_NKBookingTemplate = ExportLCLTemplate.KT_Code;
				Helper.CreateAndAssignPackageDivots(exportLCLTransportBooking, pallets10, 10);
				result.Add(exportLCLTransportBooking);
			}

			if (domesticLCLTransportBooking == null)
			{
				domesticLCLTransportBooking = Helper.CreateBooking();
				domesticLCLTransportBooking.KM_KT_NKBookingTemplate = DomesticLCLTemplate.KT_Code;
				Helper.CreateAndAssignPackageDivots(domesticLCLTransportBooking, pallets10, 10);
				result.Add(domesticLCLTransportBooking);
			}

			// Mixed

			if (importMixedTransportBooking == null)
			{
				importMixedTransportBooking = Helper.CreateBooking();
				importMixedTransportBooking.KM_KT_NKBookingTemplate = ImportMixedTemplate.KT_Code;
				Helper.CreateAndAssignPackageDivots(importMixedTransportBooking, container);
				Helper.CreateAndAssignPackageDivots(importMixedTransportBooking, pallets10, 10);
				result.Add(importMixedTransportBooking);
			}

			if (exportMixedTransportBooking == null)
			{
				exportMixedTransportBooking = Helper.CreateBooking();
				exportMixedTransportBooking.KM_KT_NKBookingTemplate = ExportMixedTemplate.KT_Code;
				Helper.CreateAndAssignPackageDivots(exportMixedTransportBooking, container);
				Helper.CreateAndAssignPackageDivots(exportMixedTransportBooking, pallets10, 10);
				result.Add(exportMixedTransportBooking);
			}

			if (domesticMixedTransportBooking == null)
			{
				domesticMixedTransportBooking = Helper.CreateBooking();
				domesticMixedTransportBooking.KM_KT_NKBookingTemplate = DomesticMixedTemplate.KT_Code;
				Helper.CreateAndAssignPackageDivots(domesticMixedTransportBooking, container);
				Helper.CreateAndAssignPackageDivots(domesticMixedTransportBooking, pallets10, 10);
				result.Add(domesticMixedTransportBooking);
			}

			return result;
		}

		// FCL

		public DtbBookingTmpl ImportFCLTemplate
		{
			get { return importFCLTemplate; }
		}

		public DtbBookingTmpl ExportFCLTemplate
		{
			get { return exportFCLTemplate; }
		}

		public DtbBookingTmpl DomesticFCLTemplate
		{
			get { return domesticFCLTemplate; }
		}

		// LCL

		public DtbBookingTmpl ImportLCLTemplate
		{
			get { return importLCLTemplate; }
		}

		public DtbBookingTmpl ExportLCLTemplate
		{
			get { return exportLCLTemplate; }
		}

		public DtbBookingTmpl DomesticLCLTemplate
		{
			get { return domesticLCLTemplate; }
		}

		// Mixed

		public DtbBookingTmpl ImportMixedTemplate
		{
			get { return importMixedTemplate; }
		}

		public DtbBookingTmpl ExportMixedTemplate
		{
			get { return exportMixedTemplate; }
		}

		public DtbBookingTmpl DomesticMixedTemplate
		{
			get { return domesticMixedTemplate; }
		}

		DtbBookingTmpl importFCLTemplate;
		DtbBookingTmpl exportFCLTemplate;
		DtbBookingTmpl domesticFCLTemplate;

		DtbBookingTmpl importLCLTemplate;
		DtbBookingTmpl exportLCLTemplate;
		DtbBookingTmpl domesticLCLTemplate;

		DtbBookingTmpl importMixedTemplate;
		DtbBookingTmpl exportMixedTemplate;
		DtbBookingTmpl domesticMixedTemplate;

		// FCL

		public DtbBooking ImportFCLTransportBooking
		{
			get { return importFCLTransportBooking; }
		}

		public DtbBooking ExportFCLTransportBooking
		{
			get { return exportFCLTransportBooking; }
		}

		public DtbBooking DomesticFCLTransportBooking
		{
			get { return domesticFCLTransportBooking; }
		}

		// LCL

		public DtbBooking ImportLCLTransportBooking
		{
			get { return importLCLTransportBooking; }
		}

		public DtbBooking ExportLCLTransportBooking
		{
			get { return exportLCLTransportBooking; }
		}

		public DtbBooking DomesticLCLTransportBooking
		{
			get { return domesticLCLTransportBooking; }
		}

		// Mixed

		public DtbBooking ImportMixedTransportBooking
		{
			get { return importMixedTransportBooking; }
		}

		public DtbBooking ExportMixedTransportBooking
		{
			get { return exportMixedTransportBooking; }
		}

		public DtbBooking DomesticMixedTransportBooking
		{
			get { return domesticMixedTransportBooking; }
		}

		DtbBooking importFCLTransportBooking;
		DtbBooking exportFCLTransportBooking;
		DtbBooking domesticFCLTransportBooking;

		DtbBooking importLCLTransportBooking;
		DtbBooking exportLCLTransportBooking;
		DtbBooking domesticLCLTransportBooking;

		DtbBooking importMixedTransportBooking;
		DtbBooking exportMixedTransportBooking;
		DtbBooking domesticMixedTransportBooking;

		TransportBookingTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingTestHelper(Factory)); }
		}

		PackingTestHelper PackingHelper
		{
			get { return packingHelper ?? (packingHelper = new PackingTestHelper(Factory)); }
		}

		TransportBookingTestHelper helper;
		PackingTestHelper packingHelper;

		IDtbBooking ITransportBookingTestData.ImportFCLTransportBooking
		{
			get { return ImportFCLTransportBooking; }
		}

		IDtbBooking ITransportBookingTestData.ExportFCLTransportBooking
		{
			get { return ExportFCLTransportBooking; }
		}

		IDtbBooking ITransportBookingTestData.DomesticFCLTransportBooking
		{
			get { return DomesticFCLTransportBooking; }
		}

		IDtbBooking ITransportBookingTestData.DomesticLCLTransportBooking
		{
			get { return DomesticLCLTransportBooking; }
		}

		IDtbBooking ITransportBookingTestData.ExportLCLTransportBooking
		{
			get { return ExportLCLTransportBooking; }
		}

		IDtbBooking ITransportBookingTestData.ImportMixedTransportBooking
		{
			get { return ImportMixedTransportBooking; }
		}

		IDtbBooking ITransportBookingTestData.ExportMixedTransportBooking
		{
			get { return ExportMixedTransportBooking; }
		}

		IDtbBooking ITransportBookingTestData.DomesticMixedTransportBooking
		{
			get { return DomesticMixedTransportBooking; }
		}

		IDtbBooking ITransportBookingTestData.ImportLCLTransportBooking
		{
			get { return ImportLCLTransportBooking; }
		}
	}
}
