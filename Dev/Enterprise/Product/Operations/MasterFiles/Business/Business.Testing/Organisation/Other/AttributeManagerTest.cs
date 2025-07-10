using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class AttributeManagerTest : TestCaseWithFactory
	{
		void AssertCount<T>(IEnumerable<T> elements, int count)
		{
			AssertEquals(count, elements.Count());
		}

		public void TestByCount()
		{
			var attributeMan = new AttributeManager();
			AssertCount(attributeMan.GetAllAttributes(AttributeManager.AttributeModules.Order, null, null), 51);
			AssertCount(attributeMan.GetAllAttributes(AttributeManager.AttributeModules.Warehouse, null, null), 43);
			AssertCount(attributeMan.GetAllAttributes(AttributeManager.AttributeModules.WhsInventory, null, null), 43);
			AssertCount(attributeMan.GetAllAttributes(AttributeManager.AttributeModules.CommercialInvoice, null, null), 20);

			AssertCount(attributeMan.GetLineAttributes(AttributeManager.AttributeModules.Order, null, null), 22);
			AssertCount(attributeMan.GetLineAttributes(AttributeManager.AttributeModules.Warehouse, null, null), 22);
			AssertCount(attributeMan.GetLineAttributes(AttributeManager.AttributeModules.WhsInventory, null, null), 22);
			AssertCount(attributeMan.GetLineAttributes(AttributeManager.AttributeModules.CommercialInvoice, null, null), 22);

			AssertCount(attributeMan.GetPartAttributes(AttributeManager.AttributeModules.Order, null, null), 4);
			AssertCount(attributeMan.GetPartAttributes(AttributeManager.AttributeModules.Warehouse, null, null), 4);
			AssertCount(attributeMan.GetPartAttributes(AttributeManager.AttributeModules.WhsInventory, null, null), 4);
			AssertCount(attributeMan.GetPartAttributes(AttributeManager.AttributeModules.CommercialInvoice, null, null), 4);

			Globals.IsWeb = true;
			try
			{
				OrgCustomLabels op_OL_CA1 = GlbCompany.CurrentCompany.OrgProxy.CustomLabels.AddNew();
				op_OL_CA1.OT_FieldName = Constants.CustomLabels.OrderLine.CustomAttribute1;
				op_OL_CA1.OT_Caption = "OLOrgProxy'sCA1";

				OrgCustomLabels op_OH_CA1 = GlbCompany.CurrentCompany.OrgProxy.CustomLabels.AddNew();
				op_OH_CA1.OT_FieldName = Constants.CustomLabels.Order.CustomAttribute1;
				op_OH_CA1.OT_Caption = "OHOrgProxy'sCA1";

				GlbCompany.CurrentCompany.OrgProxy.MiscServ[OrgMiscServSchema.OM_IMPartAttrib1Name.Name] = "OrgProxy'sPA1";

				OrgHeader testRelatedOrg = Factory.NewWithValidTestData<OrgHeader>();
				OrgHeader testLoggedInOrg = Factory.NewWithValidTestData<OrgHeader>();
				testRelatedOrg.MiscServ.OM_IMUseSerialNumber = true;

				OrgCustomLabels loCA1 = testLoggedInOrg.CustomLabels.AddNew();
				loCA1.OT_FieldName = Constants.CustomLabels.OrderLine.CustomAttribute1;
				loCA1.OT_Caption = "LoggedInOrg'sCA1";

				OrgCustomLabels loCA2 = testLoggedInOrg.CustomLabels.AddNew();
				loCA2.OT_FieldName = Constants.CustomLabels.OrderLine.CustomAttribute2;
				loCA2.OT_Caption = "LoggedInOrg'sCA2";

				OrgCustomLabels loCA3 = testLoggedInOrg.CustomLabels.AddNew();
				loCA3.OT_FieldName = Constants.CustomLabels.OrderLine.CustomAttribute3;
				loCA3.OT_Caption = "LoggedInOrg'sCA3";

				OrgCustomLabels relCA2 = testRelatedOrg.CustomLabels.AddNew();
				relCA2.OT_FieldName = Constants.CustomLabels.OrderLine.CustomAttribute2;
				relCA2.OT_Caption = "RelatedOrg'sCA2";

				AssertCount(attributeMan.GetAllAttributes(AttributeManager.AttributeModules.Order, testRelatedOrg, testLoggedInOrg), 5);
				AssertCount(attributeMan.GetLineAttributes(AttributeManager.AttributeModules.Order, testRelatedOrg, testLoggedInOrg), 3);
				AssertCount(attributeMan.GetPartAttributes(AttributeManager.AttributeModules.Order, testRelatedOrg, testLoggedInOrg), 1);
			}
			finally
			{
				Globals.IsWeb = false;
			}
		}

		public void TestOrderPartAttributes_SerialNumber()
		{
			var attributeMan = new AttributeManager();
			var attributes = attributeMan.GetAllAttributes(AttributeManager.AttributeModules.Order, null, null);

			var serialAttribute = attributes.SingleOrDefault(a => a.Key == "Serial Number");
			AssertNotNull(serialAttribute);
			AssertEquals(JobOrderLineSchema.JO_SerialNumber, serialAttribute.Column);
		}

		public void TestCommercialInvoicePartAttributes_SerialNumber()
		{
			var attributeMan = new AttributeManager();
			var attributes = attributeMan.GetAllAttributes(AttributeManager.AttributeModules.CommercialInvoice, null, null);

			var serialAttribute = attributes.SingleOrDefault(a => a.Key == "Serial Number");
			AssertNotNull(serialAttribute);
			AssertEquals(JobComInvoiceLineSchema.JI_SerialNumber, serialAttribute.Column);
		}

		public void TestGetPartAttributes_Order()
		{
			var attributeMan = new AttributeManager();
			var attributes = attributeMan.GetPartAttributes(AttributeManager.AttributeModules.Order, null, null);

			var pa1Attribute = attributes.SingleOrDefault(a => a.Key == "Part Attribute 1");
			AssertNotNull(pa1Attribute);
			AssertEquals(JobOrderLineSchema.JO_PartAttrib1, pa1Attribute.Column);

			var pa2Attribute = attributes.SingleOrDefault(a => a.Key == "Part Attribute 2");
			AssertNotNull(pa2Attribute);
			AssertEquals(JobOrderLineSchema.JO_PartAttrib2, pa2Attribute.Column);

			var pa3Attribute = attributes.SingleOrDefault(a => a.Key == "Part Attribute 3");
			AssertNotNull(pa3Attribute);
			AssertEquals(JobOrderLineSchema.JO_PartAttrib3, pa3Attribute.Column);

			var serialAttribute = attributes.SingleOrDefault(a => a.Key == "Serial Number");
			AssertNotNull(serialAttribute);
			AssertEquals(JobOrderLineSchema.JO_SerialNumber, serialAttribute.Column);
		}

		public void TestGetPartAttributes_CommercialInvoice()
		{
			var attributeMan = new AttributeManager();
			var attributes = attributeMan.GetPartAttributes(AttributeManager.AttributeModules.CommercialInvoice, null, null);

			var pa1Attribute = attributes.SingleOrDefault(a => a.Key == "Part Attribute 1");
			AssertNotNull(pa1Attribute);
			AssertEquals(JobComInvoiceLineSchema.JI_PartAttrib1, pa1Attribute.Column);

			var pa2Attribute = attributes.SingleOrDefault(a => a.Key == "Part Attribute 2");
			AssertNotNull(pa2Attribute);
			AssertEquals(JobComInvoiceLineSchema.JI_PartAttrib2, pa2Attribute.Column);

			var pa3Attribute = attributes.SingleOrDefault(a => a.Key == "Part Attribute 3");
			AssertNotNull(pa3Attribute);
			AssertEquals(JobComInvoiceLineSchema.JI_PartAttrib3, pa3Attribute.Column);

			var serialAttribute = attributes.SingleOrDefault(a => a.Key == "Serial Number");
			AssertNotNull(serialAttribute);
			AssertEquals(JobComInvoiceLineSchema.JI_SerialNumber, serialAttribute.Column);
		}

		public void TestGetPartAttributes_Warehouse()
		{
			var attributeMan = new AttributeManager();
			var attributes = attributeMan.GetPartAttributes(AttributeManager.AttributeModules.Warehouse, null, null);

			var pa1Attribute = attributes.SingleOrDefault(a => a.Key == "Part Attribute 1");
			AssertNotNull(pa1Attribute);
			AssertEquals(WhsDocketLineSchema.WE_PartAttrib1, pa1Attribute.Column);

			var pa2Attribute = attributes.SingleOrDefault(a => a.Key == "Part Attribute 2");
			AssertNotNull(pa2Attribute);
			AssertEquals(WhsDocketLineSchema.WE_PartAttrib2, pa2Attribute.Column);

			var pa3Attribute = attributes.SingleOrDefault(a => a.Key == "Part Attribute 3");
			AssertNotNull(pa3Attribute);
			AssertEquals(WhsDocketLineSchema.WE_PartAttrib3, pa3Attribute.Column);

			var serialAttribute = attributes.SingleOrDefault(a => a.Key == "Serial Number");
			AssertNotNull(serialAttribute);
			AssertEquals(WhsDocketLineSchema.WE_SerialNumber, serialAttribute.Column);
		}

		public void TestGetPartAttributes_WhsInventory()
		{
			TestGetPartAttributes_WhsInventoryCore(enableSchemaRedesignChanges: false);
		}

		public void TestGetPartAttributes_WhsInventory_EnableSchemaRedesignChanges()
		{
			TestGetPartAttributes_WhsInventoryCore(enableSchemaRedesignChanges: true);
		}

		void TestGetPartAttributes_WhsInventoryCore(bool enableSchemaRedesignChanges)
		{
			using (WarehouseDataRegistry.Instance.EnableSchemaRedesignChanges.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableSchemaRedesignChanges))
			{
				var attributeMan = new AttributeManager();
				var attributes = attributeMan.GetPartAttributes(AttributeManager.AttributeModules.WhsInventory, null, null);

				var pa1Attribute = attributes.SingleOrDefault(a => a.Key == "Part Attribute 1");
				AssertNotNull(pa1Attribute);
				AssertEquals(WhsInventoryViewSchema.WI_PartAttrib1, pa1Attribute.Column);

				var pa2Attribute = attributes.SingleOrDefault(a => a.Key == "Part Attribute 2");
				AssertNotNull(pa2Attribute);
				AssertEquals(WhsInventoryViewSchema.WI_PartAttrib2, pa2Attribute.Column);

				var pa3Attribute = attributes.SingleOrDefault(a => a.Key == "Part Attribute 3");
				AssertNotNull(pa3Attribute);
				AssertEquals(WhsInventoryViewSchema.WI_PartAttrib3, pa3Attribute.Column);

				var serialAttribute = attributes.SingleOrDefault(a => a.Key == "Serial Number");
				AssertNotNull(serialAttribute);
				AssertEquals(enableSchemaRedesignChanges ? WhsSerialNumberSchema.WSN_SerialNumber : WhsInventoryViewSchema.WI_SerialNumber, serialAttribute.Column);
			}
		}

		public void TestGetCaption()
		{
			Globals.IsWeb = true;
			try
			{
				CheckGetCaptionBehaviour("LoggedInOrg");
			}
			finally
			{
				Globals.IsWeb = false;
			}
		}

		void CheckGetCaptionBehaviour(string ownerOrg)
		{
			var attributeMan = new AttributeManager();
			bool isCustom;

			OrgHeader testRelatedOrg = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader testLoggedInOrg = Factory.NewWithValidTestData<OrgHeader>();

			GlbCompany.CurrentCompany.OrgProxy.CustomLabels.RemoveAndDeleteAll();
			testLoggedInOrg.CustomLabels.RemoveAndDeleteAll();
			testRelatedOrg.CustomLabels.RemoveAndDeleteAll();

			AssertEquals("Caption should be Empty", ZString.Empty, attributeMan.GetDescription(Constants.CustomLabels.OrderLine.CustomAttribute1, testRelatedOrg, testLoggedInOrg, out isCustom));

			OrgCustomLabels opCA1 = GlbCompany.CurrentCompany.OrgProxy.CustomLabels.AddNew();
			opCA1.OT_FieldName = Constants.CustomLabels.OrderLine.CustomAttribute1;
			opCA1.OT_Caption = "OrgProxy'sCA1";

			GlbCompany.CurrentCompany.OrgProxy.MiscServ[OrgMiscServSchema.OM_IMPartAttrib1Name.Name] = "OrgProxy'sPA1";

			OrgCustomLabels loCA2 = testLoggedInOrg.CustomLabels.AddNew();
			loCA2.OT_FieldName = Constants.CustomLabels.OrderLine.CustomAttribute2;
			loCA2.OT_Caption = "LoggedInOrg'sCA2";

			OrgCustomLabels tbCA2 = testRelatedOrg.CustomLabels.AddNew();
			tbCA2.OT_FieldName = Constants.CustomLabels.OrderLine.CustomAttribute2;
			tbCA2.OT_Caption = "Buyer'sCA2";

			AssertEquals("OrgProxy's caption for CustomAttribute1", "OrgProxy'sCA1 - CA", attributeMan.GetDescription(Constants.CustomLabels.OrderLine.CustomAttribute1, testRelatedOrg, testLoggedInOrg, out isCustom));
			AssertEquals(string.Format("{0}'s caption for CustomAttribute2", ownerOrg), string.Format("{0}'sCA2 - CA", ownerOrg), attributeMan.GetDescription(Constants.CustomLabels.OrderLine.CustomAttribute2, testRelatedOrg, testLoggedInOrg, out isCustom));
			AssertEquals("OrgProxy's caption for PartAttribute1 was not inherited", ZString.Empty, attributeMan.GetDescription(OrgMiscServSchema.OM_IMPartAttrib1Name.Name, testRelatedOrg, testLoggedInOrg, out isCustom));

			OrgCustomLabels loCA1 = testLoggedInOrg.CustomLabels.AddNew();
			loCA1.OT_FieldName = Constants.CustomLabels.OrderLine.CustomAttribute1;
			loCA1.OT_Caption = "LoggedInOrg'sCA1";

			OrgCustomLabels tbCA1 = testRelatedOrg.CustomLabels.AddNew();
			tbCA1.OT_FieldName = Constants.CustomLabels.OrderLine.CustomAttribute1;
			tbCA1.OT_Caption = "Buyer'sCA1";

			testLoggedInOrg.MiscServ.OM_IMPartAttrib1Name = "LoggedInOrg'sPA1";
			testRelatedOrg.MiscServ.OM_IMPartAttrib1Name = "Buyer'sPA1";

			AssertEquals(string.Format("{0}'s caption for CustomAttribute1 has overriden OrgProxy`s one", ownerOrg), string.Format("{0}'sCA1 - CA", ownerOrg), attributeMan.GetDescription(Constants.CustomLabels.OrderLine.CustomAttribute1, testRelatedOrg, testLoggedInOrg, out isCustom));
			AssertEquals(string.Format("{0}'s caption for CustomAttribute2", ownerOrg), string.Format("{0}'sCA2 - CA", ownerOrg), attributeMan.GetDescription(Constants.CustomLabels.OrderLine.CustomAttribute2, testRelatedOrg, testLoggedInOrg, out isCustom));
			AssertEquals(string.Format("{0}'s caption for PartAttribute1", ownerOrg), string.Format("{0}'sPA1 - PA1", ownerOrg), attributeMan.GetDescription(OrgMiscServSchema.OM_IMPartAttrib1Name.Name, testRelatedOrg, testLoggedInOrg, out isCustom));
			AssertEquals(string.Format("{0}'s caption for SerialNumber", ownerOrg), "", attributeMan.GetDescription(OrgMiscServSchema.OM_IMUseSerialNumber.Name, testRelatedOrg, testLoggedInOrg, out isCustom));

			testRelatedOrg.MiscServ.OM_IMUseSerialNumber = true;
			AssertEquals(string.Format("{0}'s caption for SerialNumber", ownerOrg), "Serial Number", attributeMan.GetDescription(OrgMiscServSchema.OM_IMUseSerialNumber.Name, testRelatedOrg, testLoggedInOrg, out isCustom));
		}

		public void TestGetCaptionBasedOnAtributeName()
		{
			var attributeMan = new AttributeManager();
			bool isCustom;

			AssertEquals("Part Attribute 1", attributeMan.GetDescription(OrgMiscServSchema.OM_IMPartAttrib1Name.Name, null, null, out isCustom));
			AssertEquals("Part Attribute 2", attributeMan.GetDescription(OrgMiscServSchema.OM_IMPartAttrib2Name.Name, null, null, out isCustom));
			AssertEquals("Part Attribute 3", attributeMan.GetDescription(OrgMiscServSchema.OM_IMPartAttrib3Name.Name, null, null, out isCustom));
			AssertEquals("Serial Number", attributeMan.GetDescription(OrgMiscServSchema.OM_IMUseSerialNumber.Name, null, null, out isCustom));
			AssertEquals("bla bla bla", attributeMan.GetDescription("bla bla bla", null, null, out isCustom));
			foreach (CodeDescriptionPair item in new CodeDescriptionPairList(OLookUpEditType.CustomLabels))
			{
				AssertEquals(item.Description, attributeMan.GetDescription(item.Code, null, null, out isCustom));
			}
		}
	}
}
