using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class AssemblyDataTest : TestCaseWithFactory
	{
		public void TestGetCollection_ShouldGetCorrectCollectionType()
		{
			var assemblyData = new XWingAssemblyData();
			var collection = assemblyData.GetBusinessObjectCollection(Factory, new AssemblyDataParams { CompanyCode = RebelAlliance.GC_Code });
			AssertEquals(typeof(XWingCollection), collection.GetType());
			AssertEquals(null, assemblyData.GetBusinessObjectCollection(Factory, new AssemblyDataParams { CompanyCode = "" }));
		}

		public void TestGetCollectionForNullCollectionType_ShouldReturnNull()
		{
			var assemblyData = new YWingAssemblyData();
			AssertNull(assemblyData.GetBusinessObjectCollection(Factory, new AssemblyDataParams { CompanyCode = RebelAlliance.GC_Code }));
			AssertNull(assemblyData.GetBusinessObjectCollection(Factory, new AssemblyDataParams { CompanyCode = "" }));
		}

		public void TestGetCollectionForTypeWithoutCompanyConstructor_ShouldReturnNull()
		{
			var assemblyData = new CommandShipAssemblyData();
			AssertEquals(null, assemblyData.GetBusinessObjectCollection(Factory, new AssemblyDataParams { CompanyCode = RebelAlliance.GC_Code }));
			AssertEquals(null, assemblyData.GetBusinessObjectCollection(Factory, new AssemblyDataParams { CompanyCode = "" }));
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			RebelAlliance = Factory.New<GlbCompany>();
			RebelAlliance.GC_Code = "RAL";
		}

		GlbCompany RebelAlliance { get; set; }

		class XWing : BusinessObject
		{
			public XWing()
				: base(null)
			{
			}

			public override CargoWise.Schema.SchemaGuidColumn PKSchemaColumn
			{
				get { throw new NotImplementedException(); }
			}
		}

		class XWingCollection : BusinessObjectCollection<XWing>
		{
			public XWingCollection(BusinessObjectFactory factory, GlbCompany company)
				: base(factory)
			{
			}
		}

		class XWingAssemblyData : AssemblyData
		{
			public override Type BusinessObjectType
			{
				get { return null; }
			}

			public override string ReferenceType
			{
				get { return "XWG"; }
			}

			protected override Type CollectionType
			{
				get { return typeof(XWingCollection); }
			}
		}

		class YWingAssemblyData : AssemblyData
		{
			public override Type BusinessObjectType
			{
				get { return null; }
			}

			public override string ReferenceType
			{
				get { return "YWG"; }
			}

			protected override Type CollectionType
			{
				get { return null; }
			}
		}

		static class CommandShipCollection
		{
		}

		class CommandShipAssemblyData : AssemblyData
		{
			public override Type BusinessObjectType
			{
				get { throw new NotImplementedException(); }
			}

			public override string ReferenceType
			{
				get { throw new NotImplementedException(); }
			}

			protected override Type CollectionType
			{
				get { return typeof(CommandShipCollection); }
			}
		}

		#endregion
	}
}
