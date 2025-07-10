using System.Collections.Generic;
using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CustomsBusinessObjectCloneStrategyTest : TestCaseWithFactory
	{
		public void TestClone()
		{
			MasterFiles.Business.GlbCompany.CurrentCompany.SetCountry("AU");
			BaseJobDeclaration declarationToClone = (BaseJobDeclaration)Factory.New<Integration.Customs.AU.IJobDeclaration>();
			declarationToClone.JE_AddInfo = "XXX";
			declarationToClone.JE_MasterBill = "MasterBill";
			declarationToClone.JE_HouseBill = "HouseBill";
			declarationToClone.JE_GB = ZGuid.NewZGuid();

			BaseJobDeclaration clonedDeclaration2 = (BaseJobDeclaration)new CustomsBusinessObjectCloneStrategy(declarationToClone, CloneType.TemplateCopy).Clone();
			AssertEquals("copied as an AU dec", ObjectFactory.GetType<Integration.Customs.AU.IJobDeclaration>(), clonedDeclaration2.GetType());
			AssertEquals("JE_AddInfo is copied unchanged", "XXX", clonedDeclaration2.JE_AddInfo);
			AssertEquals("JE_MasterBill is not copied", "", clonedDeclaration2.JE_MasterBill);
			AssertEquals("JE_HouseBill is not copied", "", clonedDeclaration2.JE_HouseBill);
			AssertEquals("JE_GB is set to a current branch", declarationToClone.JE_GB, clonedDeclaration2.JE_GB);

			MasterFiles.Business.GlbCompany.CurrentCompany.SetCountry("NZ");
			BaseJobDeclaration clonedDeclaration = (BaseJobDeclaration)new CustomsBusinessObjectCloneStrategy(declarationToClone, CloneType.CountryToCountryCopy).Clone();
			AssertEquals("copied as a BaseJobDeclaration, not as AU dec", ObjectFactory.GetType<Integration.Customs.NZ.IJobDeclaration>(), clonedDeclaration.GetType());
			AssertNotContains("JE_AddInfo content is not copied", "XXX", clonedDeclaration.JE_AddInfo);
			AssertEquals("JE_MasterBill is kept", "MasterBill", clonedDeclaration.JE_MasterBill);
			AssertEquals("JE_HouseBill is kept", "HouseBill", clonedDeclaration.JE_HouseBill);
			AssertEquals("JE_GB is set to a current branch", MasterFiles.Business.GlbBranch.CurrentBranch.PK, clonedDeclaration.JE_GB);
		}

		public void TestCopyGenAddOnColumnCollectionWhenParentIsCopied()
		{
			var declaration = Factory.New<JobDeclarationWithSystemDefinedValuesAttribute>();
			TestAddInfo addInfo = new TestAddInfo(declaration.JE_AddInfoInfo);

			addInfo.ColumnsForFastSearchExposed = new SchemaColumn[] { TestAddInfoSchema.UZ_Date, TestAddInfoSchema.UZ_String };

			addInfo.UZ_Boolean = true;
			addInfo.UZ_Date = ZDateTime.BrettsBirthday;

			Factory.Save();
			Assert("PreCondition:JZ_AddInfo serialised", !declaration.JE_AddInfo.IsEmpty);

			BaseJobDeclaration declarationCopied = (BaseJobDeclaration)new JobDeclarationDeepCloneStrategy(declaration, CloneType.TemplateCopy).Clone();

			AssertEquals("Copied declaration should have GenAddOnColumns for template copy", ZDateTime.BrettsBirthday, declarationCopied.GetSystemDefinedValue<ZDateTime>("UZ_Date"));

			BaseJobDeclaration countryToCountryCopy = (BaseJobDeclaration)new JobDeclarationDeepCloneStrategy(declaration, CloneType.CountryToCountryCopy).Clone();

			AssertEquals("Copied declaration should not have GenAddOnColumns for CountryToCountry copy", ZDateTime.Empty, countryToCountryCopy.GetSystemDefinedValue<ZDateTime>("UZ_Date"));
		}

		public void TestNotCopyGenAddOnColumnIfExclude()
		{
			var declaration = Factory.New<JobDeclarationWithSystemDefinedValuesAttribute>();
			TestAddInfo addInfo = new TestAddInfo(declaration.JE_AddInfoInfo);

			addInfo.ColumnsForFastSearchExposed = new SchemaColumn[] { TestAddInfoSchema.UZ_Date, TestAddInfoSchema.UZ_String };

			addInfo.UZ_Date = ZDateTime.BrettsBirthday;
			addInfo.UZ_String = "test";

			Factory.Save();
			Assert("PreCondition:JZ_AddInfo serialised", !declaration.JE_AddInfo.IsEmpty);

			var declarationCopied = (BaseJobDeclaration)new CustomsBusinessObjectCloneStrategyWithExcludeCopyValue(declaration, CloneType.TemplateCopy).Clone();
			AssertEquals("Copied declaration should not have GenAddOnColumns if exclude", ZDateTime.Empty, declarationCopied.GetSystemDefinedValue<ZDateTime>("UZ_Date"));
			AssertEquals("Copied declaration should have GenAddOnColumns for template copy", "test", declarationCopied.GetSystemDefinedValue<ZString>("UZ_String"));
		}

		[SystemDefinedValues]
		class JobDeclarationWithSystemDefinedValuesAttribute : BaseJobDeclaration, IAddInfoManager
		{
			public JobDeclarationWithSystemDefinedValuesAttribute(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			#region IAddInfoManager Members

			IAddInfo IAddInfoManager.AddInfo
			{
				get { return null; }
			}

			#endregion
		}

		class CustomsBusinessObjectCloneStrategyWithExcludeCopyValue : CustomsBusinessObjectCloneStrategy
		{
			public CustomsBusinessObjectCloneStrategyWithExcludeCopyValue(BaseJobDeclaration declarationToCopy, CloneType cloneType)
				: base(declarationToCopy, cloneType)
			{
			}

			protected override HashSet<string> ExcludeCopySystemDefinedValues => new HashSet<string> { "UZ_Date" };
		}
	}
}
