//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoItemPackagingAddInfoValidation
//
//    This class should be used for overriding validation in AutoItemPackagingAddInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.NZ.Business.Declaration
{
	using CargoWise.EntityFramework;
	using Enterprise.Core;

	public class ItemPackagingAddInfoValidation : AutoItemPackagingAddInfoValidation
	{
		public ItemPackagingAddInfoValidation(AutoItemPackagingAddInfo parent) : base(parent)
		{
		}

		protected override void CheckNZ_NumberOfPackages()
		{
			base.CheckNZ_NumberOfPackages();
			if (InvoiceLineRequiresPackingLine)
			{
				MandatoryValidation.WarnIfNotEntered(Parent.NZ_NumberOfPackagesInfo);
				var itemPackagingLine = (ItemPackaging)Parent.Parent;
				var invLine = (JobComInvoiceLine)itemPackagingLine.Parent;
				if (itemPackagingLine.NZ_PackageUQ == PackageTypeConverter.GetCustomsTSWPackagingType(Parent.Factory, Constants.PkgUnit.Package))
				{
					if (invLine.JI_InvoiceUQ != Constants.PkgUnit.Package && invLine.JI_InvoiceQuantity.Round(0) == itemPackagingLine.NZ_NumberOfPackages)
					{
						Parent.NZ_NumberOfPackagesInfo.AddWarning("Packaging details cannot be accurately defaulted. Please ensure Package Qty/UQ and Volume are valid");
					}
				}
			}
		}

		protected override void CheckNZ_PackageUQ()
		{
			base.CheckNZ_PackageUQ();
			if (InvoiceLineRequiresPackingLine)
			{
				if (Parent.NZ_PackageUQ.IsEmpty)
				{
					Parent.NZ_PackageUQInfo.AddWarning(PackageUQRequired);
				}
				else
				{
					ListValidation.WarnIfInvalidCode(Parent.NZ_PackageUQInfo, Parent.Lookups.PackageUQList);
				}
			}
		}
		public const string PackageUQRequired = "Must be transmitted to state the Packaging unit of quantity.";

		bool InvoiceLineRequiresPackingLine
		{
			get
			{
				var itemPackagingLine = (ItemPackaging)Parent.Parent;
				var invLine = (JobComInvoiceLine)itemPackagingLine.Parent;
				return invLine != null && invLine.RequiresPackingLine && !invLine.IsDeleted;
			}
		}
	}
}
