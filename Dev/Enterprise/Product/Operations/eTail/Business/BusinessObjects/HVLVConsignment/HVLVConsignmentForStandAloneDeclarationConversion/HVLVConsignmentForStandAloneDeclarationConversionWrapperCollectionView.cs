using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.eTail.Business
{
	public class HVLVConsignmentForStandAloneDeclarationConversionWrapperCollectionView : NonPersistentBusinessObjectCollectionView<HVLVConsignmentForStandAloneDeclarationConversionWrapper>
	{
		public HVLVConsignmentForStandAloneDeclarationConversionWrapperCollectionView(NonPersistentBusinessObjectCollection<HVLVConsignmentForStandAloneDeclarationConversionWrapper> consignmentWrappers) : base(consignmentWrappers)
		{
			foreach (HVLVConsignmentForStandAloneDeclarationConversionWrapper consignmentWrapper in consignmentWrappers)
			{
				Add(consignmentWrapper);
			}
		}

		protected override bool AllowNewCore => false;

		protected override HVLVConsignmentForStandAloneDeclarationConversionWrapper CreateNonPersistentBusinessObject()
		{
			return new HVLVConsignmentForStandAloneDeclarationConversionWrapper();
		}

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			var result = false;
			if (element is HVLVConsignmentForStandAloneDeclarationConversionWrapper)
			{
				result = CollectionToFilter.Any(consignment => consignment.PK == element.PK);
			}

			return result;
		}
	}
}
