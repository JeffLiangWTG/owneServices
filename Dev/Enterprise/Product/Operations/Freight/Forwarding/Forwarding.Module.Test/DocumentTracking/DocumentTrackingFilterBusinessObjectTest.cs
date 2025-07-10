using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Module.Testing
{
	[TestedType(typeof(DocumentTrackingFilterBusinessObject))]
	internal class DocumentTrackingFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestDocumentType()
		{
			Document1A.EQ_DocType = "TP1";
			Document1B.EQ_DocType = "TP2";

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)Strip["Document Type"];

			filter.Property = "";
			AssertMatches("empty", filter, Document1A, Document1B);

			filter.Property = "TP1";
			AssertMatches("TP1", filter, Document1A);
		}

		public void TestGetModuleFiltersWhenCustomFilterNamesClashWithReservedNames()
		{
			var customFieldName = "ETA";
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "SHP";

			var columnDef = template.GenCustomColumnDefinitions.AddNew();
			columnDef.XC_Name = customFieldName;
			columnDef.XC_Type = Enterprise.MasterFiles.Business.CustomValues.AddOnColumnDataType.Codes.Datetime;

			Factory.Save();

			var collection = new DocumentTrackingFilterBusinessObject().ModuleFilters;

			AssertNotNull(collection[customFieldName]);
		}

		public void TestShipmentSubGroup_WithRecursionSubGroup()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Singapore))
			{
				Document1A.EQ_DocType = "TP1";
				Document2A.EQ_DocType = "TP2";

				var jobRequiredDocuments = new JobRequiredDocumentCollection(Factory);

				var jobDeclaration = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Customs.IBaseJobDeclaration)));
				var stuff = Factory.NewWithValidTestData<GlbStaff>();
				stuff.GS_Code = "IGG";
				jobDeclaration[JobDeclarationSchema.JE_GS_NKCusAgent] = "IGG";
				jobDeclaration[JobDeclarationSchema.JE_JS] = Shipment1.PK;

				Factory.Save();

				var filterStripBizOSG = new DocumentTrackingFilterBusinessObject();
				var filter = (ModuleNkFilter)filterStripBizOSG["Customs Broker"];
				filter.Property = stuff.GS_Code;
				filter.IsActive = true;

				var result = Factory.Load<JobRequiredDocument>(filterStripBizOSG.Filter);

				AssertContainsExactElementsInAnyOrder("Should contain documents from Shipment1", new[] { Document1A }, result);
			}
		}

		public void TestWorkflowFiltersNotPresent()
		{
			JobShipmentFilterBusinessObject milestoneFilter = (JobShipmentFilterBusinessObject)GetNewFilterStripBusinessObject();
			AssertNull("Workflow filters should not be present", milestoneFilter["Milestone Date"]);
		}

		#region Implementation

		void AssertMatches(string message, ModuleFilter filter, params JobRequiredDocument[] documents)
		{
			var subGroup = (ModuleFilterSubGroup)filter.SubGroup;
			ZQuery query = new ZQuery();
			query.AddToFilter(JobRequiredDocumentSchema.PK, documentPKList);
			query.AddToFilter(subGroup == null ? filter.Query : subGroup.GetSubQuery(filter.Query));

			AssertContainsExactElementsInAnyOrder(message,
				(d) => d.EQ_DocNumber,
				documents,
				Factory.Load<JobRequiredDocument>(query));
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new DocumentTrackingFilterBusinessObject();
		}

		protected override List<Tuple<string, string>> GetFiltersExcludedFromSubgroupCheckForCommonTables()
		{
			return ShipmentFilterBusinessObjectTest.FiltersExcludedFromSubgroupCheckForCommonTables;
		}

		DocumentTrackingFilterBusinessObject Strip
		{
			get { return strip ?? (strip = new DocumentTrackingFilterBusinessObject()); }
		}
		DocumentTrackingFilterBusinessObject strip;

		ForwardingShipment Shipment1
		{
			get
			{
				if (shipment1 == null)
				{
					shipment1 = Factory.New<ForwardingShipment>();
					shipment1.JS_UniqueConsignRef = "Shipment1";
				}
				return shipment1;
			}
		}
		ForwardingShipment shipment1;

		ForwardingShipment Shipment2
		{
			get
			{
				if (shipment2 == null)
				{
					shipment2 = Factory.New<ForwardingShipment>();
					shipment2.JS_UniqueConsignRef = "Shipment2";
				}
				return shipment2;
			}
		}
		ForwardingShipment shipment2;

		JobRequiredDocument Document1A
		{
			get
			{
				if (document1A == null)
				{
					document1A = Shipment1.DocsAndCartage.RequiredDocuments.AddNew();
					document1A.EQ_DocType = "DOC";
					document1A.EQ_DocNumber = "Document1A";
					documentPKList.Add(document1A.PK);
				}
				return document1A;
			}
		}
		JobRequiredDocument document1A;

		JobRequiredDocument Document1B
		{
			get
			{
				if (document1B == null)
				{
					document1B = Shipment1.DocsAndCartage.RequiredDocuments.AddNew();
					document1B.EQ_DocType = "DOC";
					document1B.EQ_DocNumber = "Document1B";
					documentPKList.Add(document1B.PK);
				}
				return document1B;
			}
		}
		JobRequiredDocument document1B;

		JobRequiredDocument Document2A
		{
			get
			{
				if (document2A == null)
				{
					document2A = Shipment2.DocsAndCartage.RequiredDocuments.AddNew();
					document2A.EQ_DocType = "DOC";
					document2A.EQ_DocNumber = "Document2A";
					documentPKList.Add(document2A.PK);
				}
				return document2A;
			}
		}
		JobRequiredDocument document2A;

		readonly List<ZGuid> documentPKList = new List<ZGuid>();

		#endregion
	}
}
