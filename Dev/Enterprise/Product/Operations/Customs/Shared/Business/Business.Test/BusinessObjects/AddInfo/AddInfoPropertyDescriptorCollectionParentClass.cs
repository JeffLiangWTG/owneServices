using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.Testing
{
	[PropertyDescriptorCollection(typeof(AddInfoPropertyDescriptorCollection))]
	class AddInfoPropertyDescriptorCollectionParentClass : DummyBusinessObject
	{
		public AddInfoPropertyDescriptorCollectionParentClass(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override ZString Z0_Code
		{
			get { return AddInfo.Z0_Code; }
			set { AddInfo.Z0_Code = value; }
		}

		public override ZPropertyInfo Z0_CodeInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(Z0_Code), x => AddInfo.Z0_CodeInfo); }
		}

		[List(nameof(Lookups) + "." + nameof(AddInfoPropertyDescriptorCollectionParentClassLookups.List))]
		public override ZString Z0_Description
		{
			get { return AddInfo.Z0_Description; }
			set { AddInfo.Z0_Description = value; }
		}

		public override ZPropertyInfo Z0_DescriptionInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(Z0_Description), x => AddInfo.Z0_DescriptionInfo); }
		}

		[MaxLength(5)]
		public override ZString Z0_Xml
		{
			get { return AddInfo.Z0_Xml; }
			set { AddInfo.Z0_Xml = value; }
		}

		public override ZPropertyInfo Z0_XmlInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(Z0_Xml), x => AddInfo.Z0_XmlInfo); }
		}

		public AddInfoPropertyDescriptorCollectionAddInfoClass AddInfo => Factory.New<AddInfoPropertyDescriptorCollectionAddInfoClass>();

		public static System.Type AddInfoType => typeof(AddInfoPropertyDescriptorCollectionAddInfoClass);

		public static CargoWise.Schema.ITableSchema AddInfoSchema => DummyBizoSchema.Instance;

		public AddInfoPropertyDescriptorCollectionParentClassLookups Lookups => lookups ??= new AddInfoPropertyDescriptorCollectionParentClassLookups(this);
		public AddInfoPropertyDescriptorCollectionParentClassLookups lookups;
	}

	class AddInfoPropertyDescriptorCollectionParentClassWithOldTablePrefix : AddInfoPropertyDescriptorCollectionParentClass
	{
		public AddInfoPropertyDescriptorCollectionParentClassWithOldTablePrefix(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public ZString XC_Code
		{
			get { return AddInfo.Z0_Code; }
			set { AddInfo.Z0_Code = value; }
		}

		public ZPropertyInfo XC_CodeInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(Z0_Code), x => AddInfo.Z0_CodeInfo); }
		}
		public static string OldTablePrefix => "XC";
	}

	class AddInfoPropertyDescriptorCollectionParentClassWithSameOldTablePrefix : AddInfoPropertyDescriptorCollectionParentClass
	{
		public AddInfoPropertyDescriptorCollectionParentClassWithSameOldTablePrefix(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static string OldTablePrefix => DummyBizoSchema.Constants.Prefix;
	}

	sealed class AddInfoPropertyDescriptorCollectionParentClassLookups : ZLookups
	{
		public AddInfoPropertyDescriptorCollectionParentClassLookups(BusinessObject parent) : base(parent)
		{
		}

		public CodeDescriptionPairList List => new CodeDescriptionPairList();
	}
}
