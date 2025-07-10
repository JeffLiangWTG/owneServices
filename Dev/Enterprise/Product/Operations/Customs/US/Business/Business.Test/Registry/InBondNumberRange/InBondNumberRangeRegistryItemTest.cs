using System;
using CargoWise.Application;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.DataRegistry.Business.Testing
{
	[TestedType(typeof(InBondNumberRangeRegistryItem))]
	sealed class InBondNumberRangeRegistryItemTest : StronglyTypedRegistryItemTestCaseWithFactory<InBondNumberRange>
	{
		protected override StronglyTypedRegistryItem<InBondNumberRange, InBondNumberRange> GetNewRegistryItem()
		{
			return new InBondNumberRangeRegistryItem("", null, null, null);
		}

		protected override InBondNumberRange ValidValue
		{
			get
			{
				var range = new InBondNumberRange();
				range.BranchPK = Env.CurrentBranch.PK;
				range.StartNumber = 1;
				range.LastNumber = 999999;
				range.RunOutWarningLimitNumber = 300;
				return range;
			}
		}

		public void TestSetNumberFountain()
		{
			var company = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IGlbCompany)));
			company[GlbCompanySchema.GC_RN_NKCountryCode] = Core.Constants.CountryCodes.UnitedStates;
			var branch = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IGlbBranch)));
			branch[GlbBranchSchema.GB_GC] = company.PK;
			Factory.Save();

			var branchPK = branch.PK.ToGuid();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branchPK, Env.CurrentDepartment.PK))
			{
				var fountain = Env.NumberFountains.USInBondNumberFountain(branchPK);
				fountain.SetNext(Factory, 201);

				var nextNumber = fountain.PeekPreliminary(Factory);
				AssertEquals(201, nextNumber);

				var newNumberRange = new InBondNumberRange();
				newNumberRange.StartNumber = 101;
				newNumberRange.LastNumber = 200;

				var registry = USCustomsDataRegistry.Instance.CompanyOrBranchInBondNumberRange;
				registry.SetValue(Guid.Empty, branchPK, Guid.Empty, newNumberRange);

				nextNumber = fountain.PeekPreliminary(Factory);
				AssertEquals(101, nextNumber);

				var companyPK = company.PK.ToGuid();
				fountain = Env.NumberFountains.USInBondNumberFountain(companyPK);
				fountain.SetNext(Factory, 201);

				nextNumber = fountain.PeekPreliminary(Factory);
				AssertEquals(201, nextNumber);

				newNumberRange = new InBondNumberRange();
				newNumberRange.StartNumber = 101;
				newNumberRange.LastNumber = 200;

				registry = USCustomsDataRegistry.Instance.CompanyOrBranchInBondNumberRange;
				registry.SetValue(companyPK, Guid.Empty, Guid.Empty, newNumberRange);

				nextNumber = fountain.PeekPreliminary(Factory);
				AssertEquals(101, nextNumber);
			}
		}
	}
}
