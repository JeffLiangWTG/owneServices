using CargoWise.EntityFramework;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.AMS.Messaging.Business
{
	partial class AMSBillSendingActionCodeList
	{
		public static CodeDescriptionPairList GetBillActionCodeList(BusinessObjectFactory factory, ActionCode actionCode)
		{
			var key = GetActionKey(actionCode);
			return factory.GetCachedValue("AMSBillSendingActionCodeList:" + key, () =>
				{
					if (key == SubsequentInBondKey)
					{
						return GetSubsequentInBondList();
					}

					if (key == AmendingKey)
					{
						return GetAmendingList();
					}
					return new AMSBillSendingActionCodeList();
				});
		}

		static string GetActionKey(ActionCode actionCode)
		{
			var result = DefaultKey;
			switch (actionCode)
			{
				case ActionCode.SubsequentInBondOriginal:
				case ActionCode.SubsequentInBondAmendment:
				case ActionCode.SubsequentInBondDelete:
					result = SubsequentInBondKey;
					break;
				case ActionCode.AmendingAdd:
				case ActionCode.AmendingDelete:
				case ActionCode.AmendingUpdate:
					result = AmendingKey;
					break;
			}
			return result;
		}
		const string SubsequentInBondKey = "SubsequentInBond";
		const string AmendingKey = "Amending";
		const string DefaultKey = "Default";

		static CodeDescriptionPairList GetSubsequentInBondList()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(Codes.AddInBondMovement, Descriptions.AddInBondMovement);
			result.AddPair(Codes.CancelSubInBond, Descriptions.CancelSubInBond);
			return result;
		}

		static CodeDescriptionPairList GetAmendingList()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(Codes.AddBill, Descriptions.AddBill);
			result.AddPair(Codes.DeleteBill, Descriptions.DeleteBill);
			result.AddPair(Codes.ReplaceManifestQuantity, Descriptions.ReplaceManifestQuantity);
			result.AddPair(Codes.ReplaceEntireBillDeleteAndAdd, Descriptions.ReplaceEntireBillDeleteAndAdd);
			return result;
		}
	}
}
