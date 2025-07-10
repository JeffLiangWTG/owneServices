using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Module.Testing
{
	[TestedType(typeof(DocumentTrackingModule))]
	public class DocumentTrackingModule_Test : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.DocumentTracking;
		}

		public void TestDeleteOptionNotAvailable()
		{
			using (var module = new DocumentTrackingModuleForTest())
			{
				MenuItem[] collection = module.GetNewStandardMenuItems();
				foreach (MenuItem item in collection)
				{
					if (item.Text.ToLower().Replace("&", "").IndexOf("delete") != -1)
					{
						Fail("Delete menu is available");
					}
				}
				Assert(true);
			}
		}

		public void TestNewOptionNotAvailable()
		{
			using (var module = new DocumentTrackingModuleForTest())
			{
				MenuItem[] collection = module.GetNewStandardMenuItems();
				foreach (MenuItem item in collection)
				{
					if (item.Text.ToLower().Replace("&", "").IndexOf("new") != -1)
					{
						Fail("Delete menu is available");
					}
				}
				Assert(true);
			}
		}

		[RequiresSTA]
		public void TestParentUniqueConsignRefCanBeResolved()
		{
			ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			JobRequiredDocument document = shipment.DocsAndCartage.RequiredDocuments.AddNew();
			document.EQ_DocNumber = "Thunk";
			Factory.Save();

			using (var module = new DocumentTrackingModuleForTest())
			{
				JobRequiredDocumentCollection collection = (JobRequiredDocumentCollection)module.GetNewGridCollection();
				var result = module.LoadCollection(Factory, typeof(JobRequiredDocument), new ZQuery(JobRequiredDocumentSchema.EQ_DocNumber, document.EQ_DocNumber)).Cast<JobRequiredDocument>().Single();
				AssertEquals("collection[0].EQ_Calc_ParentUniqueConsignRef", shipment.JS_UniqueConsignRef, result.EQ_Calc_ParentUniqueConsignRef);
			}
		}

		[ExpectNoExceptions]
		[RequiresSTA]
		public void TestLoadCollectionWhenSortedOnAValueFromParent()
		{
			ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.DocsAndCartage.RequiredDocuments.AddNew().EQ_DocNumber = "Thunk1";
			shipment.DocsAndCartage.RequiredDocuments.AddNew().EQ_DocNumber = "Thunk2";
			Factory.Save();

			using (var module = new DocumentTrackingModuleForTest())
			{
				IBusinessObjectCollection collection = module.GetNewGridCollection();
				collection.ApplySort(new SortInfo(JobRequiredDocument.Schema.EQ_Calc_ParentUniqueConsignRef, ListSortDirection.Ascending));
				module.PerformSearch_ForTest();
			}
		}

		public void TestSupportsWorkflow()
		{
			using (var module = ZModuleFactory.Instance.Create(ModuleIDs.DocumentTracking))
			{
				Assert(module.SupportsWorkflow);
			}
		}

		public void TestBusinessObjectReaderWithQuery()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00001234";
			JobRequiredDocument doc1 = shipment.DocsAndCartage.RequiredDocuments.AddNew();
			doc1.EQ_DocNumber = "0001";
			doc1.EQ_DocType = Core.Constants.RefDocTypes.DeliveryOrder;
			JobRequiredDocument doc2 = shipment.DocsAndCartage.RequiredDocuments.AddNew();
			doc2.EQ_DocNumber = "0002";
			doc2.EQ_DocType = Core.Constants.RefDocTypes.BookingConfirmation;
			Factory.Save();

			ZQuery query = new ZQuery(JobRequiredDocumentSchema.EQ_ParentTableCode, SQLComparisonOperator.Equal, "JP") { OrderBy = JobRequiredDocumentSchema.EQ_DocNumber.Name };
			List<JobRequiredDocument> collection = new DocumentTrackingModule.JobRequiredDocumentReader(query, typeof(ForwardingDocsAndCartage)).Cast<JobRequiredDocument>().ToList();

			AssertEquals("collection.Count", 2, collection.Count);
			AssertEquals("collection[0].ParentType", "ForwardingDocsAndCartage", collection[0].ParentType.Name);
			AssertEquals("collection[0].Parent", "S00001234", collection[0].Parent.UniqueConsignRef);
			AssertEquals("collection[1].ParentType", "ForwardingDocsAndCartage", collection[1].ParentType.Name);
			AssertEquals("collection[1].Parent", "S00001234", collection[1].Parent.UniqueConsignRef);
		}
	}

	[TestExcludeZFilterGridModulesAllHaveModuleBashers]
	class DocumentTrackingModuleForTest : DocumentTrackingModule
	{
		public new BusinessObject[] LoadCollection(BusinessObjectFactory factory, Type type, ZQuery query) => base.LoadCollection(factory, type, query).LoadedRows;

		public new IBusinessObjectCollection GetNewGridCollection() => base.GetNewGridCollection();

		public new MenuItem[] GetNewStandardMenuItems() => base.GetNewStandardMenuItems();
	}
}
