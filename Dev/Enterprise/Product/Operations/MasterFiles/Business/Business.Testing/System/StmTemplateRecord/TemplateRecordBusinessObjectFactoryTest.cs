using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class TemplateRecordBusinessObjectFactoryTest : TransactionedTestCase
	{
		public void TestTemplateRecordBusinessObjectFactory()
		{
			var factory = new TemplateRecordBusinessObjectFactory();
			Assert(!factory.RefreshEnabled);
			AssertNotNull(factory.TemplateRecordFactory);
			Assert(factory.ChildFactories.Contains(factory.TemplateRecordFactory));
		}

		public void TestCreateNewFactory()
		{
			var factory = new TemplateRecordBusinessObjectFactory();
			AssertEquals(typeof(BusinessObjectFactory), factory.CreateNewFactory().GetType());
		}

		public void TestSaveInTransactionCore()
		{
			var factory = new TemplateRecordBusinessObjectFactory();
			var dummy1 = factory.New<DummyBusinessObject>();
			var dummy2 = factory.TemplateRecordFactory.New<DummyBusinessObject>();

			var dummyTemplate = factory.New<DummyTemplateRecordProvider>();
			factory.TemplateRecordProvider = dummyTemplate;
			var templateRecord = factory.TemplateRecordFactory.New<StmTemplateRecord>();
			((ITemplateRecordProvider)dummyTemplate).TemplateRecord = templateRecord;
			((ITemplateRecordProvider)dummyTemplate).IsTemplateRecord = true;
			dummyTemplate.Z0_Description = "ABC";

			factory.Save();

			var otherFactory = new BusinessObjectFactory { RefreshEnabled = false };

			AssertNull("dummy1 should not be saved to db", otherFactory.Load<DummyBusinessObject>(dummy1.PK));
			AssertNotNull("dummy2 should be saved to db", otherFactory.Load<DummyBusinessObject>(dummy2.PK));

			AssertNull("dummyTemplate should not be saved to db", otherFactory.Load<DummyTemplateRecordProvider>(dummyTemplate.PK));
			AssertNotNull("templateRecord should be saved to db", otherFactory.Load<StmTemplateRecord>(templateRecord.PK));
			AssertEquals("ABC", templateRecord.STR_Data);
		}

		public void TestValidationIsSuspendedIfRegistrySaysSo()
		{
			DataRegistry.Instance.RawRegistry.TemplateRecordValidation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, RawDataRegistry.TemplateRecordValidationCodes.StandardValidation);
			Assert(!new TemplateRecordBusinessObjectFactory().IsValidationSuspended);

			DataRegistry.Instance.RawRegistry.TemplateRecordValidation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, RawDataRegistry.TemplateRecordValidationCodes.NoValidation);
			Assert(new TemplateRecordBusinessObjectFactory().IsValidationSuspended);

			DataRegistry.Instance.RawRegistry.TemplateRecordValidation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, RawDataRegistry.TemplateRecordValidationCodes.IgnoreAndSave);
			Assert(!new TemplateRecordBusinessObjectFactory().IsValidationSuspended);
		}
	}

	#region DummyTemplateRecordProvider

	public class DummyTemplateRecordProvider : DummyBusinessObject, ITemplateRecordProvider
	{
		public DummyTemplateRecordProvider(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

		void ITemplateRecordProvider.SaveToTemplateRecord()
		{
			TemplateRecord.STR_ModuleID = "Dummy";
			TemplateRecord.STR_Data = Z0_Description;
		}

		void ITemplateRecordProvider.LoadFromTemplateRecord(ITemplateRecord templateRecord)
		{
			Z0_Description = TemplateRecord.STR_Data.SubstringSafe(0, Z0_DescriptionInfo.MaxLength);
			((ITemplateRecordProvider)this).IsTemplateRecord = true;
		}

		bool ITemplateRecordProvider.IsTemplateRecord { get; set; }

		ITemplateRecord ITemplateRecordProvider.TemplateRecord
		{
			get => TemplateRecord;
			set => TemplateRecord = (StmTemplateRecord)value;
		}

		public StmTemplateRecord TemplateRecord { get; private set; }

		BusinessObject ITemplateRecordProvider.InstantiateFromTemplateRecord(BusinessObjectFactory factory, Type elementType, ITemplateRecord templateRecord) { throw new NotImplementedException(); }
	}

	#endregion
}
