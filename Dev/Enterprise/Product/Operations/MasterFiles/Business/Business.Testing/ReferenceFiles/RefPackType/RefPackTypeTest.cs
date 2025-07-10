using System;
using System.Data;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Modules.DocumentScanning;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RefPackType))]
	sealed class RefPackTypeTest : EnterpriseBusinessObjectTestCase
	{
		#region TestDocManagerInfo

		public void TestDocManagerInfo()
		{
			RefPackType packType = Factory.New<RefPackType>();

			AssertEquals("DocManagerInfo's DocManagerCode should be PAT.", "PAT", ((IDocManagerSupport)packType).DocManagerInfo.DocManagerCode);
			AssertEquals("DocManagerInfo's BusinessEntity should be our RefPackType.", packType, ((IDocManagerSupport)packType).DocManagerInfo.BusinessEntity);
		}

		#endregion

		#region TestDocManagerAssemblyDataProviderAttribute

		public void TestDocManagerAssemblyDataProviderAttribute()
		{
			Assembly assembly = typeof(RefPackType).Assembly;

			var attributes = assembly.GetCustomAttributes(false).ToList<Attribute>();

			var assemblyDataProviderAttribs = (from attribute in attributes
											   where attribute.GetType() == typeof(AssemblyDataProviderAttribute)
											   select attribute).ToList<AssemblyDataProviderAttribute>();

			Assert("AssemblyDataProvider attribute for RefPackType is MIA", assemblyDataProviderAttribs.Exists((x) => x.DocManagerCode == Core.Constants.DocManagerCodes.PackType));
		}

		#endregion

		#region TestDocManagerPackTypeData

		public void TestDocManagerPackTypeData()
		{
			Assembly assembly = typeof(RefPackType).Assembly;

			var types = assembly.GetTypes();

			var assemblyDataTypes = (from type in types
									 orderby type.FullName
									 where type.IsSubclassOf(typeof(AssemblyData))
									 select type).ToList<Type>();

			Assert("AssemblyData class for RefPackType is MIA",
				assemblyDataTypes.Exists((x) =>
				{
					AssemblyData instance = Activator.CreateInstance(x) as AssemblyData;

					return instance != null && instance.BusinessObjectType == typeof(RefPackType);
				}));
		}

		#endregion

		#region TestUpdateEDICodeMappingWhenCodeChanges

		public void TestUpdateEDICodeMappingWhenCodeChanges()
		{
			RefPackType packType = Factory.NewWithValidTestData<RefPackType>();
			packType.F3_Code = "OLD";
			OrgHeader orgHeader = Factory.LoadTop1<OrgHeader>(new ZQuery());
			OrgPatternMatchOverride ov = orgHeader.CreatePatternMatchOverrideForTest();
			ov.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.PackageType;
			ov.OO_ForeignCode = "ABC";
			ov.OO_LocalCode = packType.F3_Code;
			Factory.Save();

			AssertEquals("ABC", OrgPatternMatchOverride.GetMappingForOrganisationByLocalCode(orgHeader.PK, Constants.OrgPatternMatchOverrideRelationships.PackageType, packType.F3_Code, Factory));

			packType.F3_Code = "NEW";
			Factory.Save();
			AssertEquals("ABC", OrgPatternMatchOverride.GetMappingForOrganisationByLocalCode(orgHeader.PK, Constants.OrgPatternMatchOverrideRelationships.PackageType, packType.F3_Code, Factory));
		}

		#endregion

		#region TestCanDelete

		public void TestCanDelete()
		{
			var packType = Factory.New<RefPackType>();
			packType.F3_Code = "UNT";
			AssertEquals(true, packType.CanDelete);

			packType.F3_Code = RefPackTypeCollection.ReservedContainerType;
			AssertEquals(false, packType.CanDelete);
		}

		#endregion

		#region TestReadOnlypackTypeUNT

		public void TestReadOnly()
		{
			var packTypeCNT = Factory.New<RefPackType>();
			packTypeCNT.F3_Code = RefPackTypeCollection.ReservedContainerType;
			AssertEquals(true, packTypeCNT.ReadOnly);

			var packTypeUNT = Factory.New<RefPackType>();
			packTypeUNT.F3_Code = "UNT";
			AssertEquals(false, packTypeUNT.ReadOnly);
		}

		#endregion

		public void TestHumanReadableName()
		{
			var packType = Factory.NewWithValidTestData<RefPackType>();
			packType.F3_Code = "GLD";
			packType.F3_Description = "Gold";

			AssertEquals("Package Type - GLD - Gold", packType.HumanReadableName);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public override void TestBizObjectFields()
		{
			base.TestBizObjectFields();
		}
	}
}
