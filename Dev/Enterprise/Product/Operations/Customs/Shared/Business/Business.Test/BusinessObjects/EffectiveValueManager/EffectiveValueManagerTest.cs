using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.Testing
{
	class EffectiveValueManagerTest : TestCaseWithFactory
	{
		public void TestClearValueIfSame()
		{
			var listToClear = new List<TestJobComInvoiceLineForClear>();
			var line1 = Factory.New<TestJobComInvoiceLineForClear>();
			line1.JI_Description = "NeedClear";
			var line2 = Factory.New<TestJobComInvoiceLineForClear>();
			line2.JI_Description = "NotNeedClear";
			var line3 = Factory.New<TestJobComInvoiceLineForClear>();
			line3.JI_Description = "NeedClear";
			listToClear.Add(line1);
			listToClear.Add(line2);
			listToClear.Add(line3);

			EffectiveValueManager.ClearValueIfSame(new ZString("NeedClear"), TestJobComInvoiceLineForClear.Schema.JI_Description, listToClear, p => p.JI_InvoiceQuantity = 10m);

			CombineAssertions(() =>
			{
				AssertEquals("Should clear JI_Description", ZString.Empty, listToClear[0].JI_Description);
				AssertEquals("Should populate JI_InvoiceQuantity", 10m, listToClear[0].JI_InvoiceQuantity);

				AssertEquals("Should not clear JI_Description", "NotNeedClear", listToClear[1].JI_Description);
				AssertEquals("Should not populate JI_InvoiceQuantity", 0m, listToClear[1].JI_InvoiceQuantity);

				AssertEquals("Should clear JI_Description", ZString.Empty, listToClear[2].JI_Description);
				AssertEquals("Should populate JI_InvoiceQuantity", 10m, listToClear[2].JI_InvoiceQuantity);
			});
		}

		public void TestGetEffectiveValue()
		{
			AssertNull("Get value without suspender will return null", effectiveValueManager.GetEffectiveValue(JobDeclarationSchema.Constants.JE_MessageType));

			using (effectiveValueManager.SuspendEffectiveValue(JobDeclarationSchema.Constants.JE_MessageType, new ZString("A")))
			{
				AssertEquals("Get effective value successfully after using suspender", "A", effectiveValueManager.GetEffectiveValue(JobDeclarationSchema.Constants.JE_MessageType));
			}
		}

		public void TestGetEffectiveValueToReturn()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			ZString expZString = JobMessageTypeList.Codes.Export;
			ZString mscZString = JobMessageTypeList.Codes.MiscellaneousCustoms;
			ZString impZString = JobMessageTypeList.Codes.Import;

			AssertEquals("Get non-empty value will return itself", expZString, effectiveValueManager.GetEffectiveValueToReturn(expZString, JobDeclarationSchema.Constants.JE_MessageType, GetParentValue));

			using (effectiveValueManager.SuspendEffectiveValue(JobDeclarationSchema.Constants.JE_MessageType, mscZString))
			{
				AssertEquals("Get empty value will return effective value in Suspenders if possible", mscZString, effectiveValueManager.GetEffectiveValueToReturn(ZString.Empty, JobDeclarationSchema.Constants.JE_MessageType, GetParentValue));
			}

			AssertEquals("Get empty value will return parent value when value in Suspenders removed", impZString, effectiveValueManager.GetEffectiveValueToReturn(ZString.Empty, JobDeclarationSchema.Constants.JE_MessageType, GetParentValue));

			ZString GetParentValue() => "IMP";
		}

		public void TestGetEffectiveValueToSet()
		{
			var aZString = new ZString("A");
			var bZString = new ZString("B");
			AssertEquals("Get same value with parent will return default value", ZString.Empty, effectiveValueManager.GetEffectiveValueToSet(aZString, aZString));
			AssertEquals("Get different value from parent will return itself", aZString, effectiveValueManager.GetEffectiveValueToSet(aZString, bZString));
		}

		protected override void SetUp()
		{
			base.SetUp();
			effectiveValueManager = new EffectiveValueManager();
		}
		EffectiveValueManager effectiveValueManager;

		class TestJobComInvoiceLineForClear : BaseJobComInvoiceLine, IEffectiveValueManagerSupporter
		{
			public TestJobComInvoiceLineForClear(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public EffectiveValueManager EffectiveValueManager => effectiveValueManager ?? (effectiveValueManager = new EffectiveValueManager());
			EffectiveValueManager effectiveValueManager;
		}
	}
}
