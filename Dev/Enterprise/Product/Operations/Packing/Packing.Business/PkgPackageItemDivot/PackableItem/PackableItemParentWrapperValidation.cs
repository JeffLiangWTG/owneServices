using System;
using CargoWise.EntityFramework;
using CargoWise.ResourceStrings.Grammar;
using CargoWise.Types;

namespace Enterprise.Packing.Business
{
	public class PackableItemParentWrapperEmptyValidation : ZValidation
	{
		public PackableItemParentWrapperEmptyValidation(PackableItemParentWrapper wrapper)
			: base(wrapper)
		{
			Parent = wrapper;
		}

		protected readonly PackableItemParentWrapper Parent;

		public override Type AutoValidationType
		{
			get { return typeof(PackableItemParentWrapperEmptyValidation); }
		}

		#region ValidateProposedPackQty

		public void ValidateProposedPackQty()
		{
			ValidateCalculatedProperty(Parent.ProposedPackQtyInfo);
		}

		protected virtual void CheckProposedPackQty()
		{
		}

		#endregion

		#region ValidateProposedRemoveQty

		public void ValidateProposedRemoveQty()
		{
			ValidateCalculatedProperty(Parent.ProposedRemoveQtyInfo);
		}

		protected virtual void CheckProposedRemoveQty()
		{
		}

		#endregion

		#region ValidateUnpackedWeight

		public void ValidateUnpackedWeight()
		{
			ValidateCalculatedProperty(Parent.UnpackedWeightInfo);
		}

		protected virtual void CheckUnpackedWeight()
		{
		}

		#endregion

		#region ValidateAll

		public override void ValidateAll()
		{
			ValidateProposedPackQty();
			ValidateProposedRemoveQty();
		}

		#endregion
	}

	public class PackableItemParentWrapperValidation : PackableItemParentWrapperEmptyValidation
	{
		public PackableItemParentWrapperValidation(PackableItemParentWrapper wrapper)
			: base(wrapper)
		{
		}

		public override Type AutoValidationType
		{
			get { return typeof(PackableItemParentWrapperValidation); }
		}

		#region ValidateProposedPackQty

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Baseline")]
		protected override void CheckProposedPackQty()
		{
			if (Parent.ProposedPackQty < 0)
			{
				Parent.ProposedPackQtyInfo.AddError(Res.GetString("354762c7-64d6-409b-b5e6-7c6f76a364a9", "Quantity to pack cannot be less than 0."));
			}
			else if (Parent.ProposedPackQty > 0 && Parent.ProposedPackQty > Parent.UnpackedQty)
			{
				Parent.ProposedPackQtyInfo.AddError(Res.GetString("549a025d-49ec-41e2-8f4d-6f824d8873ff", "Cannot Pack {0}x {1} as there {2} only {3} available.",
					Parent.ProposedPackQty.ToStringTrimZeros(), Parent.Description, Parent.UnpackedQty == 1 ? "is" : "are", Parent.UnpackedQty.ToStringTrimZeros()));
			}
			else if (Parent.ProposedPackQty > 0 && !Parent.PackageTypeToCreateDescription.IsEmpty && Parent.PackageQtyToCreate > 1 && Parent.PackageQtyToCreate > Parent.ProposedPackQty)
			{
				var packageQty = Parent.PackageQtyToCreate;
				var package = Grammar.Instance.Pluralize(Parent.PackageTypeToCreateDescription);
				var itemQty = Parent.ProposedPackQty;
				var itemDescription = Parent.Description;

				var warning = Res.GetString("8ed080eb-557d-44fe-8fde-a5c65ed702ba", "There are {0} {1} but only {2} {3}(s). {4} {1} will not contain a {3}.",
					packageQty,
					package,
					itemQty.ToStringTrimZeros(),
					itemDescription,
					new ZDecimal(packageQty - itemQty).ToZInt());

				Parent.ProposedPackQtyInfo.AddWarning(warning);
			}
		}

		#endregion

		#region ValidateProposedRemoveQty

		protected override void CheckProposedRemoveQty()
		{
			if (Parent.ProposedRemoveQty < 0)
			{
				Parent.ProposedRemoveQtyInfo.AddError(Res.GetString("3092f115-7808-47a9-84b8-8fa9b1a8ca4c", "Quantity to remove cannot be less than 0."));
			}
			else if (Parent.GroupedPackedItems != null)
			{
				var packedQty = Parent.PackedQtyFromWrapper;
				if (Parent.ProposedRemoveQty > packedQty)
				{
					ZString msg = (packedQty == 1)
							? Res.GetString("7d4b2cb3-307f-4ab9-bdd5-6e51f935f907", "There is only 1 item packed.")
							: Res.GetString("ea9ed232-b1a8-4327-be6b-db7c420afa7d", "There are only {0} items packed.", packedQty.ToStringTrimZeros());

					Parent.ProposedRemoveQtyInfo.AddError(msg);
				}
			}
		}

		#endregion

		#region ValidateUnpackedWeight

		protected override void CheckUnpackedWeight()
		{
			if (Parent.PackableItemParent.WeightPerUnit <= 0m)
			{
				Parent.UnpackedWeightInfo.AddWarning(Res.GetString("77e666bb-66fe-4df3-8dc3-4e6e4cc711c6",
					"No Weight defined. When packing this item the Package Weight should be manually entered."));
			}
			else if (Parent.PackableItemParent.WeightUQ.IsEmpty)
			{
				Parent.UnpackedWeightInfo.AddWarning(Res.GetString("83d7ed80-1551-4b06-8b77-1ded01633ca6",
					"No Weight UQ defined. When packing this item the Package Weight should be manually entered."));
			}
		}

		#endregion
	}
}
