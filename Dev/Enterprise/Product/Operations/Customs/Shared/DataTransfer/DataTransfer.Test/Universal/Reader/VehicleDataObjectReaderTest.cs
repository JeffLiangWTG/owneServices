using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer.Universal.AddInfoExtensions;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.ZArchitecture.Schema;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.DataTransfer.Universal.Testing
{
	partial class JobDeclarationDataObjectReaderTest
	{
		public void TestVehicleFieldMappings()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
			{
				var declaration = (BaseJobDeclaration)Factory.BOFactory.New<Integration.Customs.ES.IJobDeclaration>();
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				var vehicle1 = CreateVehicle("Model1", "Brand1", "VIN1", "RegNum1");
				var vehicle2 = CreateVehicle("Model2", "Brand2", "VIN2", "RegNum2");
				var vehicle3 = CreateVehicle("Model3", "Brand3", "VIN3", "RegNum3");
				var invoiceLineDataObject = new UniversalCustoms.CommercialInvoiceLine()
				{
					VehicleCollection = new List<UniversalCustoms.Vehicle>(new[] { vehicle1, vehicle2, vehicle3 })
				};
				var vehicles = new VehicleCollectionDataObjectReader(logger, new UniversalDataObjectReaderHelper(Factory, Core.Constants.CountryCodes.Spain, Core.Constants.CountryCodes.Spain))
									.ReadIntoDataRows(invoiceLine.PK, invoiceLine.TablePrefix, invoiceLine.IsInDatabase, invoiceLineDataObject);

				AssertNotNull(vehicles);

				CombineAssertions(delegate
				{
					AssertEquals("Vehicles", 3, vehicles.Length);

					AssertEquals(invoiceLine.PK, vehicles[0].GetValue(CusVehicleSchema.CVH_ParentID));
					AssertEquals(invoiceLine.TablePrefix, vehicles[0].GetValue(CusVehicleSchema.CVH_ParentTableCode));

					AssertEquals("Model1", vehicles[0].GetValue(CusVehicleSchema.CVH_ModelName));
					AssertEquals("Brand1", vehicles[0].GetValue(CusVehicleSchema.CVH_BrandName));
					AssertEquals("VIN1", vehicles[0].GetValue(CusVehicleSchema.CVH_VehicleIdentificationNumber));
					AssertEquals("RegNum1", vehicles[0].GetValue(CusVehicleSchema.CVH_RegistrationNumber));
				});
			}
		}

		internal static UniversalCustoms.Vehicle CreateVehicle(string model, string brand, string vin, string registrationNumber)
		{
			return new UniversalCustoms.Vehicle()
			{
				Model = model,
				Brand = brand,
				VIN = vin,
				RegistrationNumber = registrationNumber,
			};
		}
	}
}
