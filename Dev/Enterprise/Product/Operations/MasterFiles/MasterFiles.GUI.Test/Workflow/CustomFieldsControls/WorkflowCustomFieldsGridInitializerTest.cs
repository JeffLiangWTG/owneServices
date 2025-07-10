using System;
using System.Data;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class WorkflowCustomFieldsGridInitializerTest : TestCaseWithFactory
	{
		public void TestHookCollectionHooksProxiedProperties()
		{
			var collection = new DummyBusinessObjectCollection(Factory);
			var dummy = Factory.New<DummyWithCustomBizo>();
			dummy.InitRelatedDummyWithTasks();
			dummy.RelatedDummyWithTasksClient = Client1PK;
			collection.Add(dummy);

			Factory.Save();

			using (ZGrid grid = new ZGrid())
			{
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo { ColumnName = "_abc", Caption = "Abc", GroupName = new ResourceStringData(), IsVisible = false });

				using (WorkflowCustomFieldsGridInitializer.AddWorkflowCustomFieldsColumns(grid, collection, true))
				{
					AssertEquals(2, grid.ColumnStyles.Count);
					AssertEqualColumn("C31", "__C31__prop__ZString", "Workflow Custom Fields", (ZGridColumnInfo)grid.ColumnStyles[0]);
					AssertEqualColumn("Abc", "_abc", null, (ZGridColumnInfo)grid.ColumnStyles[1]);

					dummy.RelatedDummyWithTasksClient = Client2PK;

					AssertEquals(3, grid.ColumnStyles.Count);
					AssertEqualColumn("C31", "__C31__prop__ZString", "Workflow Custom Fields", (ZGridColumnInfo)grid.ColumnStyles[0]);
					AssertEqualColumn("C41", "__C41__prop__ZString", "Workflow Custom Fields", (ZGridColumnInfo)grid.ColumnStyles[1]);
					AssertEqualColumn("Abc", "_abc", null, (ZGridColumnInfo)grid.ColumnStyles[2]);
				}
			}
		}

		public void TestAddWorkflowCustomFieldsColumns()
		{
			DummyBusinessObjectCollection dummies = new DummyBusinessObjectCollection(Factory);
			DummyWithCustomBizo dummy = Factory.New<DummyWithCustomBizo>();
			dummy.InitRelatedDummyWithTasks();
			dummy.SubType1 = "T2";
			dummies.Add(dummy);

			Factory.Save();

			using (ZGrid grid = new ZGrid())
			{
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo { ColumnName = "_abc", Caption = "Abc", GroupName = new ResourceStringData() });

				using (WorkflowCustomFieldsGridInitializer.AddWorkflowCustomFieldsColumns(grid, dummies))
				{
					AssertEquals("Precondition", 3, grid.ColumnStyles.Count);

					AssertEqualColumn("Abc", "_abc", null, (ZGridColumnInfo)grid.ColumnStyles[0]);
					AssertEquals(typeof(ZTextBoxColumnStyleInfo), grid.ColumnStyles[0].GetType());

					AssertEqualColumn("C21", "__C21__prop__ZDateTime", "Workflow Custom Fields", (ZGridColumnInfo)grid.ColumnStyles[1]);
					AssertEquals(typeof(ZDateEditColumnStyleInfo), grid.ColumnStyles[1].GetType());

					AssertEqualColumn("C22", "__C22__prop__ZBool", "Workflow Custom Fields", (ZGridColumnInfo)grid.ColumnStyles[2]);
					AssertEquals(typeof(ZCheckBoxColumnStyleInfo), grid.ColumnStyles[2].GetType());

					dummy.SubType1 = "T1";

					AssertEquals("Precondition", 5, grid.ColumnStyles.Count);

					AssertEqualColumn("C11", "__C11__prop__ZString", "Workflow Custom Fields", (ZGridColumnInfo)grid.ColumnStyles[1]);
					AssertEquals(typeof(ZTextBoxColumnStyleInfo), grid.ColumnStyles[1].GetType());
					grid.Columns.Add((ZTextBoxColumnStyleInfo)grid.ColumnStyles[1]);
					AssertEquals(AutoGenCustomAddOnValue.Schema.XV_DataMaxLength, ((DataGridTextBox)((ZTextBoxColumnStyle)grid.Columns["__C11__prop__ZString"].ColumnStyle).EditControl).MaxLength);

					AssertEqualColumn("C12", "__C12__prop__ZInt", "Workflow Custom Fields", (ZGridColumnInfo)grid.ColumnStyles[2]);
					AssertEquals(typeof(ZCalcEditColumnStyleInfo), grid.ColumnStyles[2].GetType());
				}
			}
		}

		void AssertEqualColumn(string caption, string columnName, string groupNameCaption, ZGridColumnInfo columnStyle)
		{
			AssertEquals(caption, columnStyle.Caption);
			AssertEquals(columnName, columnStyle.ColumnName);
			AssertEquals(groupNameCaption, columnStyle.GroupName.Caption);
		}

		public void TestGetCustomBusinessObject()
		{
			AssertNull(CustomBusinessObjectExtensions.GetCustomBusinessObject(null));
			AssertNull(CustomBusinessObjectExtensions.GetCustomBusinessObject(Factory.New<DummyBusinessObject>()));

			DummyWithCustomBizo dummy2 = Factory.New<DummyWithCustomBizo>();
			AssertEquals(0, ((IBusiness)dummy2).Children.Length);

			AssertNull(CustomBusinessObjectExtensions.GetCustomBusinessObject(dummy2));
			CustomBusinessObject customBusinessObject = CustomBusinessObjectExtensions.GetCustomBusinessObject(dummy2, true);
			AssertNotNull(customBusinessObject);
			AssertEquals(1, ((IBusiness)dummy2).Children.Length);

			Assert("Should return previously found object", object.ReferenceEquals(customBusinessObject, CustomBusinessObjectExtensions.GetCustomBusinessObject(dummy2)));
			AssertEquals(1, ((IBusiness)dummy2).Children.Length);

			AssertSame(customBusinessObject, CustomBusinessObjectExtensions.GetCustomBusinessObject(customBusinessObject));

			dummy2.UnRegisterEditableChildObject(customBusinessObject);
			AssertEquals(0, ((IBusiness)dummy2).Children.Length);

			AssertNull(CustomBusinessObjectExtensions.GetCustomBusinessObject(dummy2));
			AssertNotNull(CustomBusinessObjectExtensions.GetCustomBusinessObject(dummy2, true));
			AssertEquals(1, ((IBusiness)dummy2).Children.Length);

			Assert("Should return new object", !object.ReferenceEquals(customBusinessObject, CustomBusinessObjectExtensions.GetCustomBusinessObject(dummy2)));
			AssertEquals(1, ((IBusiness)dummy2).Children.Length);
		}

		#region Implemantation

		protected override void SetUp()
		{
			base.SetUp();

			AssertNotNull(DummyWorkflowDescriptor.Instance);
			PrepareTemplates();
		}

		void PrepareTemplates()
		{
			ProcessTaskTemplate template1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template1.P0_ProcessType = "DUM";
			template1.P0_SubType1 = "T1";

			GenCustomColumnDefinition def11 = template1.GenCustomColumnDefinitions.AddNew();
			def11.XC_Name = "C11";
			def11.XC_Type = AddOnColumnDataType.Codes.String;

			GenCustomColumnDefinition def12 = template1.GenCustomColumnDefinitions.AddNew();
			def12.XC_Name = "C12";
			def12.XC_Type = AddOnColumnDataType.Codes.Integer;

			ProcessTaskTemplate template2 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template2.P0_ProcessType = "DUM";
			template2.P0_SubType1 = "T2";

			GenCustomColumnDefinition def21 = template2.GenCustomColumnDefinitions.AddNew();
			def21.XC_Name = "C21";
			def21.XC_Type = AddOnColumnDataType.Codes.Datetime;

			GenCustomColumnDefinition def22 = template2.GenCustomColumnDefinitions.AddNew();
			def22.XC_Name = "C22";
			def22.XC_Type = AddOnColumnDataType.Codes.Boolean;

			ProcessTaskTemplate template3 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template3.P0_ProcessType = "DUM";
			template3.P0_OH_Client = Client1PK;

			GenCustomColumnDefinition def31 = template3.GenCustomColumnDefinitions.AddNew();
			def31.XC_Name = "C31";
			def31.XC_Type = AddOnColumnDataType.Codes.String;

			ProcessTaskTemplate template4 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template4.P0_ProcessType = "DUM";
			template4.P0_OH_Client = Client2PK;

			GenCustomColumnDefinition def41 = template4.GenCustomColumnDefinitions.AddNew();
			def41.XC_Name = "C41";
			def41.XC_Type = AddOnColumnDataType.Codes.String;

			Factory.Save();
		}

		ZGuid Client1PK
		{
			get
			{
				if (client1PK == ZGuid.Empty)
				{
					var client = Factory.New<OrgHeader>();
					client.OH_Code = "CLIENT1";
					client1PK = client.PK;
				}
				return client1PK;
			}
		}
		ZGuid client1PK;

		ZGuid Client2PK
		{
			get
			{
				if (client2PK == ZGuid.Empty)
				{
					var client = Factory.New<OrgHeader>();
					client.OH_Code = "CLIENT2";
					client2PK = client.PK;
				}
				return client2PK;
			}
		}
		ZGuid client2PK;

		[UserDefinedValues]
		public class DummyWithCustomBizo : DummyWithWorkflow, ICustomFieldProvider
		{
			public DummyWithCustomBizo(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			#region ICustomFieldProvider Members

			CustomBusinessObject ICustomFieldProvider.GetCustomBusinessObject(bool shouldRefresh)
			{
				return new CustomBusinessObject(Factory, this, new UserDefinedPropertyCollection(this) { GetProcessTaskTemplate(this) });
			}

			static IProcessTaskTemplateMatches GetProcessTaskTemplate(BusinessObject businessObject)
			{
				IProcessTaskTemplateLoader loader = (IProcessTaskTemplateLoader)Activator.CreateInstance(ObjectFactory.GetType<IProcessTaskTemplateLoader>(), businessObject.Factory);
				return loader.FindMatches((IWorkflowProviderCore)businessObject);
			}

			#endregion
		}

		#endregion
	}
}
