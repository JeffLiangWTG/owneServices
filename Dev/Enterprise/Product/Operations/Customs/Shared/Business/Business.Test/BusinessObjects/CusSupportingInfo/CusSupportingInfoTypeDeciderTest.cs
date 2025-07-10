using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusSupportingInfoTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForBinding()
		{
			var typeDecider = new CusSupportingInfoTypeDecider();
			AssertNull(typeDecider.GetTypeForBinding());
		}

		public void TestGetTypeForNew()
		{
			var typeDecider = new CusSupportingInfoTypeDecider();
			AssertNull(typeDecider.GetTypeForNew());
		}

		public void TestGetTypeForLoad()
		{
			var ukCompany = Factory.New<GlbCompany>();
			ukCompany.GC_Code = "U!@";
			ukCompany.GC_Name = "UK COMPANY";
			ukCompany.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			ukCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedKingdom;
			var ukBranch = ukCompany.Branches.AddNew();
			ukBranch.GB_Code = "U!@";
			ukBranch.GB_BranchName = "UK BRANCH";
			ukBranch.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			var ukDeclaration = Factory.New<Integration.Customs.GB.IJobDeclaration>();
			ukDeclaration.JE_GB = ukBranch.PK;
			var supporter = (Integration.Customs.ICusSupportingInfoTypeSupporter)ukDeclaration;
			Type supportingDocumentType = supporter.GetCusSupportingInfoTypes()[Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument];
			var cusSupportingInfo = (CusSupportingInfo)Factory.New(supportingDocumentType);
			cusSupportingInfo.CSI_ParentID = ukDeclaration.PK;
			cusSupportingInfo.CSI_ParentTableCode = JobDeclarationSchema.Constants.Prefix;

			var noneUKDeclaration = Factory.New<BaseJobDeclaration>();

			var typeDecider = new CusSupportingInfoTypeDecider();
			var message = string.Format("{0} does not support CSI_Type '{1}'", noneUKDeclaration.GetType().FullName, Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument);
			AssertNull(typeDecider.GetTypeForLoad(Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument, JobDeclarationSchema.Constants.Prefix, noneUKDeclaration.PK, Factory));
			AssertEquals(message, ErrorReporter.LastKeyReported);
			ErrorReporter.Clear();
			AssertEquals(supportingDocumentType, typeDecider.GetTypeForLoad(Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument, JobDeclarationSchema.Constants.Prefix, ukDeclaration.PK, Factory));
			AssertEquals(true, string.IsNullOrEmpty(ErrorReporter.LastKeyReported));
			AssertEquals(supportingDocumentType, typeDecider.GetTypeForLoad(cusSupportingInfo, Factory));
			AssertEquals(true, string.IsNullOrEmpty(ErrorReporter.LastKeyReported));
			AssertEquals(supportingDocumentType, typeDecider.GetTypeForLoad(((IBusinessObjectInternals)cusSupportingInfo).Row, Factory));
			AssertEquals(true, string.IsNullOrEmpty(ErrorReporter.LastKeyReported));

			cusSupportingInfo.CSI_Type = "DEC";
			message = string.Format("{0} does not support CSI_Type 'DEC'", ukDeclaration.GetType().FullName);
			AssertNull(typeDecider.GetTypeForLoad("DEC", JobDeclarationSchema.Constants.Prefix, ukDeclaration.PK, Factory));
			AssertEquals(message, ErrorReporter.LastKeyReported);
			ErrorReporter.Clear();
			AssertNull(typeDecider.GetTypeForLoad(cusSupportingInfo, Factory));
			AssertEquals(message, ErrorReporter.LastKeyReported);
			ErrorReporter.Clear();
			AssertNull(typeDecider.GetTypeForLoad(((IBusinessObjectInternals)cusSupportingInfo).Row, Factory));
			AssertEquals(message, ErrorReporter.LastKeyReported);
			ErrorReporter.Clear();
		}

		class TestTypeDecider : ApplicationSpecificTypeDecider
		{
			protected override IEnumerable<ApplicationSpecificType> ApplicationSpecificTypesCore
			{
				get
				{
					yield return new ApplicationSpecificType("DEF", () => typeof(CusSupportingInfo));
				}
			}
		}

		class TestCusSupportingInfo : CusSupportingInfo
		{
			public TestCusSupportingInfo(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public new readonly static TestTypeDecider TypeDecider = new TestTypeDecider();
		}

		class TestJobDeclaration : BaseJobDeclaration, Integration.Customs.ICusSupportingInfoTypeSupporter
		{
			public TestJobDeclaration(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public IDictionary<ZString, Type> GetCusSupportingInfoTypes()
			{
				return new Dictionary<ZString, Type> { { "ABC", typeof(TestCusSupportingInfo) } };
			}

			public IEnumerable<IBusinessObjectFetchStrategy> GetFetchStrategies()
			{
				return null;
			}
		}

		public void TestGetTypeForLoadWithCodeTypeDecider()
		{
			var header = Factory.New<TestJobDeclaration>();

			var newFactory = new BusinessObjectFactory();

			var row1 = Factory.New<CusSupportingInfo>();
			row1.CSI_ParentID = header.PK;
			row1.CSI_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			row1.CSI_Type = "ABC";
			row1.CSI_Code = "DEF";

			var row2 = Factory.New<CusSupportingInfo>();
			row2.CSI_ParentID = header.PK;
			row2.CSI_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			row2.CSI_Type = "ABC";
			row2.CSI_Code = "XYZ";

			Factory.Save();

			_ = newFactory.Load<TestJobDeclaration>(header.PK);
			var loaded1 = newFactory.Load<CusSupportingInfo>(row1.PK);
			AssertNotNull(loaded1);
			AssertEquals(typeof(CusSupportingInfo), loaded1.GetType());

			var loaded2 = newFactory.Load<CusSupportingInfo>(row2.PK);
			AssertNotNull(loaded2);
			AssertEquals(typeof(TestCusSupportingInfo), loaded2.GetType());
		}
	}
}
