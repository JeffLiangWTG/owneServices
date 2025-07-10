using System;
using System.Collections;
using System.Data;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Moq;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesConstants;

namespace Enterprise.MasterFiles.Business.Accounting.JobConfigurationHelpers.Testing
{
	sealed class SupplyTypeConfigurationBizoImplementationHelperTest : TestCaseWithFactory
	{
		public void TestGetListContainsSupplyTypes()
		{
			var supplyTypeConfigurationMock = new Mock<ISupplyTypeConfiguration>();
			var helper = GetSupplyTypeConfigurationHelper(supplyTypeConfigurationMock.Object);

			AssertContainsExactElementsInAnyOrder(new string[] { "LOC", "LOX", "LOA", "INT", "INX", "INA", "DSB" }, helper.GetLookupList().GetAllCodes());
		}

		public void TestReadOnlyStatusForSupplyType()
		{
			var supplyTypeConfigurationMock = new Mock<ISupplyTypeConfiguration>();
			var supplyTypeConfigurationHelper = GetSupplyTypeConfigurationHelper(supplyTypeConfigurationMock.Object);
			Assert(!supplyTypeConfigurationHelper.GetReadOnlyStatus());
		}

		public void TestValidate_CheckEnteredAndInvalidCodeOnSupplyTypeCode()
		{
			var supplyTypeConfigurationMock = Factory.New<DummyBizoWithSupplyTypeConfiguration>();
			var supplyTypeConfigurationHelper = GetSupplyTypeConfigurationHelper(supplyTypeConfigurationMock);
			var supplyTypeList = new CodeDescriptionPairList();
			supplyTypeList.AddPair("VALID1", "Desc1");
			supplyTypeList.AddPair("VALID2", "Desc2");
			supplyTypeConfigurationMock.SupplyType_List = supplyTypeList;

			supplyTypeConfigurationMock.SupplyType = ZString.Empty;
			supplyTypeConfigurationHelper.Validate();
			AssertHasErrorContaining(supplyTypeConfigurationMock.SupplyTypeInfo, "Please enter");

			supplyTypeConfigurationMock.SupplyType = "INVALID";
			supplyTypeConfigurationHelper.Validate();
			AssertHasErrorContaining(supplyTypeConfigurationMock.SupplyTypeInfo, "Enter a valid");

			supplyTypeConfigurationMock.SupplyType = "VALID1";
			supplyTypeConfigurationHelper.Validate();
			AssertNoErrors(supplyTypeConfigurationMock.SupplyTypeInfo);

			supplyTypeConfigurationMock.SupplyType = "VALID2";
			supplyTypeConfigurationHelper.Validate();
			AssertNoErrors(supplyTypeConfigurationMock.SupplyTypeInfo);
		}

		public void TestValidate_WarningOnInactiveSupplyTypeCode()
		{
			var supplyTypeConfigurationMock = Factory.New<DummyBizoWithSupplyTypeConfiguration>();
			var supplyTypeConfigurationHelper = GetSupplyTypeConfigurationHelper(supplyTypeConfigurationMock);
			var supplyTypeList = AccountingMasterFilesRegistry.Instance.SupplyTypeClassificationCodesList.Value;

			((CodeDescriptionBool)supplyTypeList.FindByCode(SupplyTypeClassificationCodes.INX)).Bool = false;
			((CodeDescriptionBool)supplyTypeList.FindByCode(SupplyTypeClassificationCodes.LOC)).Bool = true;

			AccountingMasterFilesRegistry.Instance.SupplyTypeClassificationCodesList.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, supplyTypeList);
			supplyTypeConfigurationMock.SupplyType_List = supplyTypeList;

			supplyTypeConfigurationMock.SupplyType = ZString.Empty;
			supplyTypeConfigurationHelper.Validate();
			AssertNoWarnings(supplyTypeConfigurationMock.SupplyTypeInfo);

			supplyTypeConfigurationMock.SupplyType = "AAA";
			supplyTypeConfigurationHelper.Validate();
			Assert("Precondition:", supplyTypeConfigurationMock.SupplyTypeInfo.HasErrors());
			AssertNoWarnings(supplyTypeConfigurationMock.SupplyTypeInfo);

			supplyTypeConfigurationMock.SupplyType = SupplyTypeClassificationCodes.INX;
			Assert("Precondition:", !supplyTypeList.GetBoolFromCode(supplyTypeConfigurationMock.SupplyType));
			supplyTypeConfigurationHelper.Validate();
			Assert("Precondition:", !supplyTypeConfigurationMock.SupplyTypeInfo.HasErrors());
			AssertHasWarningContaining(supplyTypeConfigurationMock.SupplyTypeInfo, "This Supply Type Code is inactive. This rule will not be applied.");

			supplyTypeConfigurationMock.SupplyType = SupplyTypeClassificationCodes.LOC;
			Assert("Precondition:", supplyTypeList.GetBoolFromCode(supplyTypeConfigurationMock.SupplyType));
			supplyTypeConfigurationHelper.Validate();
			AssertNoWarnings(supplyTypeConfigurationMock.SupplyTypeInfo);
		}

		IJobConfigurationHelper GetSupplyTypeConfigurationHelper(ISupplyTypeConfiguration supplyTypeConfig) => ObjectFactory.Get<IJobConfigurationHelperFactory>().GetSupplyTypeHelper(supplyTypeConfig);

		class DummyBizoWithSupplyTypeConfiguration : DummyBusinessObject, ISupplyTypeConfiguration, IObsoleteValidation
		{
			public DummyBizoWithSupplyTypeConfiguration(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}
			ZString ISupplyTypeConfiguration.SupplyTypeCode => SupplyType;

			ZPropertyInfo ISupplyTypeConfiguration.SupplyTypeCodeInfo => SupplyTypeInfo;

			public ZString SupplyType { get => Z0_Description; set => Z0_Description = value; }

			public ZPropertyInfo SupplyTypeInfo { get => Z0_DescriptionInfo; }

			[List("SupplyType_List")]
			public override ZString Z0_Description { get => base.Z0_Description; set => base.Z0_Description = value; }

			public IList SupplyType_List { get; set; }
		}
	}
}
