using System;
using System.Collections.Specialized;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(StmTemplateRecord))]
	public class StmTemplateRecordTest : EnterpriseBusinessObjectTestCase
	{
		public void TestFixXmlDataWithSingleLineFeed()
		{
			var xmlText = $@"<TestNode>{System.Environment.NewLine}123{System.Environment.NewLine}456</TestNode>";

			var templateRecord = Factory.New<StmTemplateRecord>();
			templateRecord.STR_ModuleID = "JobShipment";
			templateRecord.STR_ReferenceId = "TR00001001";
			templateRecord.STR_Data = xmlText;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var templateRecord1 = newFactory.Load<StmTemplateRecord>(templateRecord.PK);
			AssertEquals(templateRecord.STR_Data, templateRecord1.STR_Data);
		}

		public void TestPopulateIdIfNeeded()
		{
			var templateRecord = Factory.New<StmTemplateRecord>();
			Assert(templateRecord.STR_ReferenceId.IsEmpty);
			using (((IDbConnected)Factory).Connection.BeginTransactionWithManager()) // Fountain GetNext() requires a transaction
			{
				templateRecord.PopulateIdIfNeeded();
			}
			Assert(!templateRecord.STR_ReferenceId.IsEmpty);
		}

		public void TestPopulateIdIfNeededOnSaving()
		{
			var templateRecord = Factory.New<StmTemplateRecord>();
			templateRecord.STR_ModuleID = "JobShipment";
			Assert(templateRecord.STR_ReferenceId.IsEmpty);
			Factory.Save();
			Assert(!templateRecord.STR_ReferenceId.IsEmpty);
		}

		public void TestDescription()
		{
			var templateRecord = Factory.New<StmTemplateRecord>();
			templateRecord.STR_ModuleID = "JobShipment";
			templateRecord.STR_ReferenceId = "TR00001001";
			AssertEquals("JobShipment TR00001001", templateRecord.Description);
		}

		public void TestIsAutoLogged()
		{
			var templateRecord = Factory.New<StmTemplateRecord>();
			Assert(templateRecord.IsAutoAdminBusinessObjectLoggerEnabled);
		}

		public void TestSTR_TemplateName_MaxLengthOverride()
		{
			var templateRecord = Factory.New<StmTemplateRecord>();
			AssertEquals("In SQL we allow 80 but we want to restrict it to 20 for reasonable usage", 20, templateRecord.STR_TemplateNameInfo.MaxLength);
		}

		public void TestOnLoaded_SetsChildrenToReadOnlyIfCancelled()
		{
			var templateRecord = Factory.New<StmTemplateRecord>();
			var child = Factory.New<StmTemplateRecord>();

			templateRecord.RegisterEditableChildObject(child);
			templateRecord.IsCancelled = true;
			templateRecord.OnLoaded();

			Assert(templateRecord.ReadOnly);
			Assert(child.ReadOnly);
		}

		public void TestCanReactivate()
		{
			var templateName = "My New Template";
			var templateModule = "JobConsol";

			var templateRecord = Factory.New<StmTemplateRecord>();
			templateRecord.STR_TemplateName = templateName;
			templateRecord.STR_IsActive = true;
			templateRecord.STR_ModuleID = templateModule;

			var templateRecord_Inactive = Factory.New<StmTemplateRecord>();
			templateRecord_Inactive.STR_TemplateName = templateName;
			templateRecord_Inactive.STR_IsActive = false;
			templateRecord_Inactive.STR_ModuleID = templateModule;

			Factory.Save();

			AssertEquals(
				"An active template already exists with this template name. You must deactivate that template before activating this template",
				templateRecord_Inactive.CanReactivate()
			);
		}
	}

	public class TemplateNumberFountainUniqueIndexFailureHandlingTest : NumberFountainUniqueIndexFailureHandlingTest
	{
		#region Implementation

		protected override SchemaColumn ColumnThatUsesNumberFountain => StmTemplateRecordSchema.STR_ReferenceId;
		protected override INumberFountainProxy NumberFountainToTest => Env.NumberFountains.TemplateRecordID;
		protected override NameValueCollection AdditionalInsertValues
		{
			get
			{
				var result = base.AdditionalInsertValues;
				result.Add(StmTemplateRecordSchema.Constants.STR_ModuleID, string.Format("'{0}'", "JobShipment"));
				result.Add(StmTemplateRecordSchema.Constants.STR_SystemCreateTimeUtc, string.Format("'{0}'", ZDateTime.Now));
				result.Add(StmTemplateRecordSchema.Constants.STR_SystemLastEditTimeUtc, string.Format("'{0}'", ZDateTime.Now));
				result.Add(StmTemplateRecordSchema.Constants.STR_SystemCreateUser, string.Format("'{0}'", "E"));
				result.Add(StmTemplateRecordSchema.Constants.STR_SystemLastEditUser, string.Format("'{0}'", "E"));
				return result;
			}
		}

		protected override void SetExtraPropertyValuesAfterCreatingBizO(BusinessObject testBizO)
		{
			var template = testBizO as StmTemplateRecord;
			template.STR_ModuleID = "JobShipment";
			template.STR_SystemCreateTimeUtc = ZDateTime.Now;
			template.STR_SystemLastEditTimeUtc = ZDateTime.Now;
			template.STR_SystemCreateUser = "E";
			template.STR_SystemLastEditUser = "E";
		}

		protected override Type BizOTypeToTest
		{
			get { return typeof(StmTemplateRecord); }
		}

		#endregion
	}
}
