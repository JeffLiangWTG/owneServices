using Enterprise.Core;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportBookings.Business.Testing;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.TransportBookings.Module.Testing
{
	[TestedType(typeof(DtbBookingTmplFilterBusinessObject))]
	public class DtbBookingTmplFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestCodeFilter()
		{
			var importTemplate = Helper.CreateTransportBookingTemplate("IFCL", "FCL Import", Constants.CartageDirection.Import);
			var exportTemplate = Helper.CreateTransportBookingTemplate("EXP", "FCL Export", Constants.CartageDirection.Export);
			Factory.Save();

			var filterStrip = new DtbBookingTmplFilterBusinessObject();
			var filter = ((ModuleTextFilter)filterStrip["Code"]);
			filter.Property = exportTemplate.KT_Code;
			filter.IsActive = true;

			var filteredTemplates = new DtbBookingTmplCollection(Factory, filterStrip.Filter);

			AssertCollectionNotContains(importTemplate, filteredTemplates);
			AssertCollectionContains(exportTemplate, filteredTemplates);
		}

		public void TestDescriptionFilter()
		{
			var importTemplate = Helper.CreateTransportBookingTemplate("IFCL", "FCL Import", Constants.CartageDirection.Import);
			var exportTemplate = Helper.CreateTransportBookingTemplate("EXP", "FCL Export", Constants.CartageDirection.Export);
			Factory.Save();

			var filterStrip = new DtbBookingTmplFilterBusinessObject();
			var filter = ((ModuleTextFilter)filterStrip["Description"]);
			filter.Property = exportTemplate.KT_Description;
			filter.IsActive = true;

			var filteredTemplates = new DtbBookingTmplCollection(Factory, filterStrip.Filter);

			AssertCollectionNotContains(importTemplate, filteredTemplates);
			AssertCollectionContains(exportTemplate, filteredTemplates);

			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			using (var mockRes = Res.UseMockData())
			{
				filterStrip = new DtbBookingTmplFilterBusinessObject();
				AssertType(typeof(ModuleTextFilter), filterStrip["Description"]);
				AssertEquals("Description (English)", filterStrip["Description"].MultilingualDescription);
				AssertEquals(DtbBookingTmplSchema.KT_Description, filterStrip["Description"].FilterColumn);

				AssertType(typeof(ModuleTranslatableTextFilter), filterStrip["Description_Local"]);
				AssertEquals("Description (Chinese - Simplified)", filterStrip["Description_Local"].MultilingualDescription);
				AssertEquals(DtbBookingTmplSchema.KT_Description, filterStrip["Description_Local"].FilterColumn);
			}
		}

		public void TestDirectionFilter()
		{
			var importTemplate = Helper.CreateTransportBookingTemplate("IFCL", "FCL Import", Constants.CartageDirection.Import);
			var exportTemplate = Helper.CreateTransportBookingTemplate("EXP", "FCL Export", Constants.CartageDirection.Export);
			Factory.Save();

			var filterStrip = new DtbBookingTmplFilterBusinessObject();
			var filter = ((ModuleTextFilter)filterStrip["Direction"]);
			filter.Property = exportTemplate.KT_Direction;
			filter.IsActive = true;

			var filteredTemplates = new DtbBookingTmplCollection(Factory, filterStrip.Filter);

			AssertCollectionNotContains(importTemplate, filteredTemplates);
			AssertCollectionContains(exportTemplate, filteredTemplates);
		}

		public void TestOrganisationFilter()
		{
			var importTemplate = Helper.CreateTransportBookingTemplate("IFCL", "FCL Import", Constants.CartageDirection.Import);
			var importInstruction1 = importTemplate.Instructions.AddNew();
			var importInstruction2 = importTemplate.Instructions.AddNew();
			importInstruction1.K2_OrgType = OrganisationTypesList.Codes.CTO;
			importInstruction2.K2_OrgType = OrganisationTypesList.Codes.CNE;

			var exportTemplate = Helper.CreateTransportBookingTemplate("EXP", "FCL Export", Constants.CartageDirection.Export);
			var exportInstruction1 = exportTemplate.Instructions.AddNew();
			var exportInstruction2 = exportTemplate.Instructions.AddNew();
			exportInstruction1.K2_OrgType = OrganisationTypesList.Codes.CNR;
			exportInstruction2.K2_OrgType = OrganisationTypesList.Codes.CFS;

			Factory.Save();

			var filterStrip = new DtbBookingTmplFilterBusinessObject();
			var filter = ((ModuleTextFilter)filterStrip["Organization Type"]);
			filter.Property = exportInstruction2.K2_OrgType;
			filter.IsActive = true;

			var filteredTemplates = new DtbBookingTmplCollection(Factory, filterStrip.Filter);

			AssertCollectionNotContains(importTemplate, filteredTemplates);
			AssertCollectionContains(exportTemplate, filteredTemplates);

			filter.Property = importInstruction1.K2_OrgType;
			filteredTemplates = new DtbBookingTmplCollection(Factory, filterStrip.Filter);
			AssertCollectionContains(importTemplate, filteredTemplates);
			AssertCollectionNotContains(exportTemplate, filteredTemplates);
		}

		public void TestIsSystemFilter()
		{
			var customTemplate = Helper.CreateTransportBookingTemplate("IFCL", "FCL Import", Constants.CartageDirection.Import);
			customTemplate.KT_IsSystem = false;

			var systemTemplate = Helper.CreateTransportBookingTemplate("EXP", "FCL Export", Constants.CartageDirection.Export);
			systemTemplate.KT_IsSystem = true;

			Factory.Save();

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.DtbBookingTmpl))
			{
				var filterStrip = module.FilterBusinessObject;
				var filter = ((ModuleTextFilter)filterStrip["Is System Defined"]);
				filter.Property = "Not System";
				filter.IsActive = true;

				var filteredTemplates = new DtbBookingTmplCollection(Factory, filterStrip.Filter);

				AssertCollectionContains(customTemplate, filteredTemplates);
				AssertCollectionNotContains(systemTemplate, filteredTemplates);

				filter.Property = "System";
				filteredTemplates = new DtbBookingTmplCollection(Factory, filterStrip.Filter);
				AssertCollectionNotContains(customTemplate, filteredTemplates);
				AssertCollectionContains(systemTemplate, filteredTemplates);
			}
		}

		//public void TestTextFilter()
		//{
		//}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new DtbBookingTmplFilterBusinessObject();
		}

		TransportBookingTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingTestHelper(Factory)); }
		}
		TransportBookingTestHelper helper;
	}
}
