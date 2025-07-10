using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.DataTransfer.Universal.Testing
{
	partial class JobDeclarationDataObjectReaderTest
	{
		public void TestVehicleDeletionAndAddition()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				var declaration = (BaseJobDeclaration)Factory.BOFactory.New<Integration.Customs.GB.IJobDeclaration>();
				declaration.JE_MasterBill = "MYMASTER";
				declaration.JE_SystemCreateTimeUtc = ZDateTime.Now.AddMonths(-1);
				var invoiceHeader = declaration.Invoices.AddNew();
				var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
				var originalVehicle = Factory.New<CusVehicle>();
				originalVehicle.CVH_ParentID = invoiceLine.PK;
				originalVehicle.CVH_BrandName = "OriginalBrand";
				originalVehicle.CVH_ModelName = "OriginalModel";
				originalVehicle.CVH_VehicleIdentificationNumber = "OriginalVIN";
				originalVehicle.CVH_RegistrationNumber = "OriginalRegNumber";

				var vehicleDataObject = JobDeclarationDataObjectReaderTest.CreateVehicle("NewModel", "NewBrand", "newVIN", "newRegNum");
				var universalInvoiceLineData = new UniversalCustoms.CommercialInvoiceLine();
				universalInvoiceLineData.VehicleCollection = new List<UniversalCustoms.Vehicle>() { vehicleDataObject };
				var reader = new VehicleCollectionDataObjectReader(logger, new UniversalDataObjectReaderHelper(Factory, Core.Constants.CountryCodes.Spain, Core.Constants.CountryCodes.Spain));
				var vehicles = reader.ReadIntoDataRows(invoiceLine.PK, invoiceLine.TablePrefix, invoiceLine.IsInDatabase, universalInvoiceLineData);

				AssertNotNull(vehicles);

				CombineAssertions(delegate
				{
					AssertEquals(true, originalVehicle.IsDeleted);
					AssertEquals(1, vehicles.Length);
					var reloadedVehicle = vehicles[0];
					AssertEquals("NewModel", reloadedVehicle.GetValue(CusVehicleSchema.CVH_ModelName));
					AssertEquals("NewBrand", reloadedVehicle.GetValue(CusVehicleSchema.CVH_BrandName));
					AssertEquals("newVIN", reloadedVehicle.GetValue(CusVehicleSchema.CVH_VehicleIdentificationNumber));
					AssertEquals("newRegNum", reloadedVehicle.GetValue(CusVehicleSchema.CVH_RegistrationNumber));
				});
			}
		}
	}
}
