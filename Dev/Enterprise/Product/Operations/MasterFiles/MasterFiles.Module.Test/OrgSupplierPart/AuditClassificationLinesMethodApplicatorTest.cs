using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Integration.Customs;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(AuditClassificationLinesMethodApplicator))]
	sealed class AuditClassificationLinesMethodApplicatorTest : OperationalActionMethodApplicatorTest
	{
		public void TestAuditClassificationLines()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.UnitedStates);
			var previousDate = ZDateTime.UtcToday.AddDays(-3);

			var product1 = Factory.NewWithValidTestData<OrgSupplierPart>();
			product1.OP_PartNum = "P1";
			var pivot1 = Factory.New(ObjectFactory.GetType<IBaseCusClassPartPivot>());
			pivot1[CusClassPartPivotSchema.CI_OP] = product1.PK;
			pivot1[CusClassPartPivotSchema.CI_LastAuditedDate] = previousDate;
			pivot1[CusClassPartPivotSchema.CI_ChildType] = "HTI";

			var product2 = Factory.NewWithValidTestData<OrgSupplierPart>();
			product2.OP_PartNum = "P2";
			var pivot2 = Factory.New(ObjectFactory.GetType<IBaseCusClassPartPivot>());
			pivot2[CusClassPartPivotSchema.CI_OP] = product2.PK;
			pivot2[CusClassPartPivotSchema.CI_LastAuditedDate] = previousDate;
			pivot2[CusClassPartPivotSchema.CI_ChildType] = "HTE";

			var product3 = Factory.NewWithValidTestData<OrgSupplierPart>();
			product3.OP_PartNum = "P3";
			var pivot3 = Factory.New(ObjectFactory.GetType<IBaseCusClassPartPivot>());
			pivot3[CusClassPartPivotSchema.CI_OP] = product3.PK;
			pivot3[CusClassPartPivotSchema.CI_LastAuditedDate] = previousDate;
			pivot3[CusClassPartPivotSchema.CI_ChildType] = "SHB";

			var product4 = Factory.NewWithValidTestData<OrgSupplierPart>();
			product4.OP_PartNum = "P4";
			var pivot4 = Factory.New(ObjectFactory.GetType<IBaseCusClassPartPivot>());
			pivot4[CusClassPartPivotSchema.CI_OP] = product4.PK;
			pivot4[CusClassPartPivotSchema.CI_LastAuditedDate] = previousDate;
			pivot4[CusClassPartPivotSchema.CI_ChildType] = "HTI";
			pivot4[CusClassPartPivotSchema.CI_RN_NKCountry] = Core.Constants.CountryCodes.Australia;

			var product5 = Factory.NewWithValidTestData<OrgSupplierPart>();
			product5.OP_PartNum = "P5";
			var pivot5 = Factory.New(ObjectFactory.GetType<IBaseCusClassPartPivot>());
			pivot5[CusClassPartPivotSchema.CI_OP] = product5.PK;
			pivot5[CusClassPartPivotSchema.CI_LastAuditedDate] = previousDate;
			pivot5[CusClassPartPivotSchema.CI_ChildType] = "HTI";
			var pivot6 = Factory.New(ObjectFactory.GetType<IBaseCusClassPartPivot>());
			pivot6[CusClassPartPivotSchema.CI_OP] = product5.PK;
			pivot6[CusClassPartPivotSchema.CI_LastAuditedDate] = previousDate;
			pivot6[CusClassPartPivotSchema.CI_ChildType] = "HTE";

			Factory.Save();

			Env.Security.CustomsSupplierPartAuditExport.IsAllowed = false;
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddOKAnswer();
			ApplyApplicator(new[] { product1, product2, product3, product4, product5 },
				@"INFO: Audit Classification Line successfully. Details: Product Code: 'P1', Classification (HTI ).
WARNING: Failed to audit Classification Line. Details: Product Code: 'P2', Classification (HTE ). Reason: You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:
Maintain -> Customs Files -> Products -> Audit Classification Line -> Export.
WARNING: Failed to audit Classification Line. Details: Product Code: 'P3', Classification (SHB ). Reason: You do not have the appropriate security rights to run this function.
If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:
Maintain -> Customs Files -> Products -> Audit Classification Line -> Export.
INFO: Audit Classification Line successfully. Details: Product Code: 'P5', Classification (HTI ).
WARNING: Failed to audit Classification Line. Details: Product Code: 'P5', Classification (HTE ). Reason: You do not have the appropriate security rights to run this function.
If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:
Maintain -> Customs Files -> Products -> Audit Classification Line -> Export.");
			Assert((ZDateTime)pivot1[CusClassPartPivotSchema.CI_LastAuditedDate] > previousDate);
			Assert("Has no security right", (ZDateTime)pivot2[CusClassPartPivotSchema.CI_LastAuditedDate] == previousDate);
			Assert("Has no security right", (ZDateTime)pivot3[CusClassPartPivotSchema.CI_LastAuditedDate] == previousDate);
			Assert("Not from the current country", (ZDateTime)pivot4[CusClassPartPivotSchema.CI_LastAuditedDate] == previousDate);
			Assert((ZDateTime)pivot5[CusClassPartPivotSchema.CI_LastAuditedDate] > previousDate);
			Assert("Has no security right", (ZDateTime)pivot6[CusClassPartPivotSchema.CI_LastAuditedDate] == previousDate);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new AuditClassificationLinesMethodApplicatorForTest("test", Factory);
		}

		class AuditClassificationLinesMethodApplicatorForTest : AuditClassificationLinesMethodApplicator
		{
			public AuditClassificationLinesMethodApplicatorForTest(string name, BusinessObjectFactory factory)
				: base(name, factory)
			{
			}

			protected override void ApplyCore(IOperationalActionSectionLog log, BusinessObject[] targets)
			{
				var orgSupplierParts = targets.OfType<OrgSupplierPart>().ToArray();
				log.SetSectionProgressMax(orgSupplierParts.Length);
				AuditClassificationLines(log, orgSupplierParts, true);
			}
		}
	}
}
