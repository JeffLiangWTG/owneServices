using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(StaffSecurityModuleFilter))]
	internal class StaffSecurityModuleFilterTest : ModuleGuidsFilterTest
	{
		public void TestSerialization()
		{
			CheckpointLookupKey lookupKey = new CheckpointLookupKey("ABC", Guid.NewGuid());
			Filter.SecurityFilterContainer.LookupKey = lookupKey;

			byte[] bytes;
			using (MemoryStream stream = new MemoryStream())
			using (XmlTextWriter writer = new XmlTextWriter(stream, Encoding.UTF8))
			{
				writer.WriteStartDocument();
				writer.WriteStartElement("StaffSecurityModuleFilter");
				((IXmlSerializable)Filter).WriteXml(writer);
				writer.WriteEndElement();
				writer.WriteEndDocument();
				writer.Flush();
				bytes = stream.ToArray();
			}

			StaffSecurityModuleFilter newFilter = (StaffSecurityModuleFilter)GetNewBusinessObject();
			using (MemoryStream stream = new MemoryStream(bytes))
			using (XmlTextReader reader = new XmlTextReader(stream))
			{
				reader.ReadToFollowing("Property1");
				((IXmlSerializable)newFilter).ReadXml(reader);
			}

			AssertEquals("SecurityFilterContainer.LookupKey", lookupKey, newFilter.SecurityFilterContainer.LookupKey);

			XmlDocument document = new XmlDocument();
			using (MemoryStream stream = new MemoryStream(bytes))
			{
				document.Load(stream);
			}
			document.DocumentElement.RemoveChild(document.DocumentElement.SelectSingleNode("ItemGuid"));
			bytes = Encoding.UTF8.GetBytes(document.OuterXml);

			using (MemoryStream stream = new MemoryStream(bytes))
			using (XmlTextReader reader = new XmlTextReader(stream))
			{
				reader.ReadToFollowing("Property1");
				((IXmlSerializable)newFilter).ReadXml(reader);
			}

			AssertEquals("SecurityFilterContainer.LookupKey", new CheckpointLookupKey("ABC", Guid.Empty), newFilter.SecurityFilterContainer.LookupKey);
		}

		#region Properties

		public void TestBranchProperty1()
		{
			Filter.Branch = ZGuid.NewZGuid();
			AssertEquals(Filter.Branch, Filter.Property1);
		}

		public void TestDepartmentProperty2()
		{
			Filter.Department = ZGuid.NewZGuid();
			AssertEquals(Filter.Department, Filter.Property2);
		}

		#endregion

		#region Empty

		public void TestIsEmpty()
		{
			AssertEquals("IsEmpty", true, Filter.IsEmpty);

			Filter.SecurityFilterContainer.LookupKey = new CheckpointLookupKey("x");
			AssertEquals("IsEmpty", false, Filter.IsEmpty);

			Filter.SecurityFilterContainer.LookupKey = CheckpointLookupKey.Empty;
			Filter.Branch = ZGuid.NewZGuid();
			AssertEquals("IsEmpty", false, Filter.IsEmpty);

			Filter.Branch = ZGuid.Empty;
			AssertEquals("IsEmpty", true, Filter.IsEmpty);
		}

		#endregion

		#region Clear

		public void TestClear()
		{
			Filter.SecurityFilterContainer.LookupKey = new CheckpointLookupKey("x");
			Filter.Clear();
			AssertEquals("SecurityFilterContainer.LookupKey", CheckpointLookupKey.Empty, Filter.SecurityFilterContainer.LookupKey);
		}

		#endregion

		#region Query

		public virtual void TestGetQuery()
		{
			AssertEquals("", Filter.Query.LiteralTextADO);
			Filter.SecurityFilterContainer.LookupKey = new CheckpointLookupKey("ABC");
			AssertEquals("GS_IsResource = 0", Filter.Query.LiteralTextADO);
		}

		#endregion

		#region Implementation

		new StaffSecurityModuleFilter Filter
		{
			get { return (StaffSecurityModuleFilter)base.Filter; }
		}

		protected override ModuleGuidsFilter GetNewModuleFilter()
		{
			return new StaffSecurityModuleFilter("moo", Branches, Departments);
		}

		protected override string[] GetPropertiesExcludedFromCacheInvalidationTest(ModuleGuidsFilter filter)
		{
			var securityFilter = (StaffSecurityModuleFilter)filter;
			return base.GetPropertiesExcludedFromCacheInvalidationTest(filter).Concat(new[] { nameof(securityFilter.SecurityFilterContainer) }).ToArray();
		}

		#region Branches

		public GlbBranchCollection Branches
		{
			get
			{
				if (fBranches == null)
				{
					fBranches = new GlbBranchCollection(Factory);
				}

				return fBranches;
			}
		}

		GlbBranchCollection fBranches;

		#endregion

		#region Departments

		public GlbDepartmentCollection Departments
		{
			get
			{
				if (fDepartments == null)
				{
					fDepartments = new GlbDepartmentCollection(Factory);
				}

				return fDepartments;
			}
		}

		GlbDepartmentCollection fDepartments;

		#endregion

		#endregion
	}
}
