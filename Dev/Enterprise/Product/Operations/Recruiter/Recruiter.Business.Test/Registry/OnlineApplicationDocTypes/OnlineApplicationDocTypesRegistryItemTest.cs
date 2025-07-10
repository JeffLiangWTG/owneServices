using System;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(OnlineApplicationDocTypesRegistryItem))]
	sealed class OnlineApplicationDocTypesRegistryItemTest : StronglyTypedRegistryItemTestCase<OnlineApplicationDocTypeCollection>
	{
		public void TestConstructor()
		{
			OnlineApplicationDocTypesRegistryItem item = (OnlineApplicationDocTypesRegistryItem)GetNewRegistryItem();
			AssertEquals("OnlineApp", item.Name);
			AssertEquals("Recruiter", item.Category);
			AssertEquals("Online Application Document Types", item.Caption);
			AssertEquals("Please specify the document types that are allowed to be uploaded onto the Careers website by the candidates.", item.Hint);
			AssertEquals(typeof(OnlineApplicationDocTypesDataType), item.DataType.GetType());
			AssertEquals(RegistryStorageFlags.System, item.Storage);
		}

		protected override StronglyTypedRegistryItem<OnlineApplicationDocTypeCollection, OnlineApplicationDocTypeCollection> GetNewRegistryItem()
		{
			return new OnlineApplicationDocTypesRegistryItem("OnlineApp", (NoResString)"Recruiter");
		}

		protected override OnlineApplicationDocTypeCollection ValidValue
		{
			get
			{
				BusinessObjectFactory factory = new BusinessObjectFactory();

				RefDocType docType1 = factory.NewWithPrimaryKey<RefDocType>(new Guid("0A38F0A7-DCA0-4f2b-83DE-5701695EED9C"));
				docType1.RT_DocType = "AAA";
				docType1.RT_ReferenceType = Core.Constants.ReferenceTypes.HumanResourcesStaffEmployment;
				docType1.RT_Desc = "Document type AAA";

				RefDocType docType2 = factory.NewWithPrimaryKey<RefDocType>(new Guid("63C10990-43E0-4ed7-8BCA-E0A11632B805"));
				docType2.RT_DocType = "BBB";
				docType2.RT_ReferenceType = Core.Constants.ReferenceTypes.HumanResourcesStaffEmployment;
				docType2.RT_Desc = "Document type BBB";

				factory.Save();

				OnlineApplicationDocTypeCollection appDocTypes = new OnlineApplicationDocTypeCollection(new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty), factory);
				OnlineApplicationDocType appDocType1 = appDocTypes.AddNew();
				appDocType1.RT_PK = docType1.PK;
				OnlineApplicationDocType appDocType2 = appDocTypes.AddNew();
				appDocType2.RT_PK = docType2.PK;
				return appDocTypes;
			}
		}
	}
}
