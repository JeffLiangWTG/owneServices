using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Packing.Business
{
	public class PkgPalletTransactionLookups : AutoPkgPalletTransactionLookups
	{
		public PkgPalletTransactionLookups(AutoPkgPalletTransaction parent)
			: base(parent)
		{
		}

		#region Parent

		new PkgPalletTransaction Parent
		{
			get { return (PkgPalletTransaction)base.Parent; }
		}

		#endregion

		#region PalletTypes

		public CodeDescriptionPairList PalletTypes
		{
			get { return PackingRegistry.Instance.PalletTypes.Value.Types.GetCodeDescriptionPairList(); }
		}

		#endregion

		#region TransactionTypes

		public CodeDescriptionPairList TransactionTypes
		{
			get { return new PalletTransactionTypeList(); }
		}

		#endregion

		#region TransferTypes

		public CodeDescriptionPairList TransferTypes
		{
			get
			{
				return Factory.GetCachedValue("PkgPalletTransactionLookups|TransferTypes|" + Parent.KTR_TransferType,
					() => GetTransferTypes(Parent.KTR_TransferType));
			}
		}

		static CodeDescriptionPairList GetTransferTypes(string transferType)
		{
			CodeDescriptionPairList result = null;
			if (transferType == PalletTransactionTypeList.Codes.Transfer)
			{
				result = new PalletTransferTypeList();
			}
			else
			{
				result = new PalletExchangeTypeList();
			}
			return result;
		}

		#endregion

		#region ActionTypes

		public CodeDescriptionPairList ActionTypes
		{
			get
			{
				var actionTypes = new ActionTypeList();
				if (Parent.IsTransfer && Parent.KTR_TransferType == PalletTransferTypeList.Codes.Direct)
				{
					actionTypes.RemoveCode(ActionTypeList.Codes.PickUp);
					actionTypes.RemoveCode(ActionTypeList.Codes.Deliver);
				}
				else
				{
					actionTypes.RemoveCode(ActionTypeList.Codes.Direct);
				}
				return actionTypes;
			}
		}

		#endregion

		#region StatusList

		public CodeDescriptionPairList StatusList
		{
			get { return new PalletTransactionStatusList(); }
		}

		#endregion

		#region Parties

		public OrganisationsFindBoxCollection Parties
		{
			get { return new OrganisationsFindBoxCollection(Factory); }
		}

		#endregion

		#region ParentList

		public CodeDescriptionPairList ParentList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				if (Parent.PossibleParents != null)
				{
					foreach (var palletParent in Parent.PossibleParents)
					{
						var description = palletParent.GetJobDescription(Parent.Context != null ? Parent.Context.GetType() : null);
						if (string.IsNullOrEmpty(description))
						{
							description = palletParent.GetJobDescription(null);
						}

						result.AddPair(palletParent.PK, description, ZString.Empty);
					}
				}

				return result;
			}
		}

		#endregion
	}
}
