using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgHeaderProcessTask))]
	class OrgHeaderProcessTaskTest : ProcessTaskTest
	{
		public void TestProcessTask()
		{
			OrgHeader orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeaderProcessTask processTask = ((OrgHeaderProcessTasksCollection)orgHeader.WorkflowItems).AddNew();
			AssertEquals(orgHeader, processTask.Parent);
			AssertEquals(ControllerIDs.Organisation, processTask.ParentControllerID);
		}

		public void TestSecurity()
		{
			bool modifyAllowed = Env.Security.OrgDetailsModify.IsAllowed;
			try
			{
				OrgHeader orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				Factory.Save();

				OrgHeaderProcessTask processTask = ((OrgHeaderProcessTasksCollection)orgHeader.WorkflowItems).AddNew();

				AssertEquals(false, processTask.ReadOnly);
				AssertEquals(false, processTask.P9_DescriptionInfo.ReadOnly);

				Env.Security.OrgDetailsModify.IsAllowed = false;

				AssertEquals(true, processTask.ReadOnly);
				AssertEquals(true, processTask.P9_DescriptionInfo.ReadOnly);
			}
			finally
			{
				Env.Security.OrgDetailsModify.IsAllowed = modifyAllowed;
			}
		}

		public void TestGetShouldPropertiesBeReadOnlyCallsGetReadOnlySecurity()
		{
			// Arrange
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var processTask = (OrgHeaderProcessTaskForTestGetShouldPropertiesBeReadOnlyCallsGetReadOnlySecurity)((OrgHeaderProcessTasksCollection)orgHeader.WorkflowItems).AddNew(typeof(OrgHeaderProcessTaskForTestGetShouldPropertiesBeReadOnlyCallsGetReadOnlySecurity));

			// Act
			processTask.CallGetShouldPropertiesBeReadOnly();

			// Assert
			AssertEquals(true, processTask.GetReadOnlySecurityWasCalled);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.NewWithValidTestData<OrgHeader>().WorkflowItems.AddNew();
		}

		[ProvideMetaDataProperty("ReadOnlySecurity", MetaDataTypes.ReadOnly)]
		class OrgHeaderProcessTaskForTestGetShouldPropertiesBeReadOnlyCallsGetReadOnlySecurity : OrgHeaderProcessTask
		{
			public OrgHeaderProcessTaskForTestGetShouldPropertiesBeReadOnlyCallsGetReadOnlySecurity(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public void CallGetShouldPropertiesBeReadOnly()
			{
				GetShouldPropertiesBeReadOnly(TypeDescriptor.GetProperties(typeof(OrgHeaderProcessTask)).OfType<PropertyDescriptor>().First());
			}

			protected override bool GetReadOnlySecurity(PropertyDescriptor property)
			{
				GetReadOnlySecurityWasCalled = true;
				return true;
			}

			public bool GetReadOnlySecurityWasCalled { get; private set; }
		}

		#endregion
	}
}
