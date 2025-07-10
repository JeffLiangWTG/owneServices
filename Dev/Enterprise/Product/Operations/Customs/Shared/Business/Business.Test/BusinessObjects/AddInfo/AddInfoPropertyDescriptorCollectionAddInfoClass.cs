using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business.Testing
{
	sealed class AddInfoPropertyDescriptorCollectionAddInfoClass : DummyBusinessObject
	{
		public AddInfoPropertyDescriptorCollectionAddInfoClass(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
		{
		}

		[List(nameof(Lookups) + "." + nameof(AddInfoPropertyDescriptorCollectionAddInfoClassLookups.List))]
		[MaxLength(5)]
		public override ZString Z0_Code { get; set; }

		public AddInfoPropertyDescriptorCollectionAddInfoClassLookups Lookups => lookups ??= new AddInfoPropertyDescriptorCollectionAddInfoClassLookups(this);
		AddInfoPropertyDescriptorCollectionAddInfoClassLookups lookups;
	}

	sealed class AddInfoPropertyDescriptorCollectionAddInfoClassLookups : ZLookups
	{
		public AddInfoPropertyDescriptorCollectionAddInfoClassLookups(BusinessObject parent) : base(parent)
		{
		}

		public CodeDescriptionPairList List => new CodeDescriptionPairList();
	}
}
