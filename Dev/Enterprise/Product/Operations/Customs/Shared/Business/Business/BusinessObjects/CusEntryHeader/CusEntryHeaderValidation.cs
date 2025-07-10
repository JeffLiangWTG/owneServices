//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusEntryHeaderValidation
//
//    This class should be used for overriding validation in AutoCusEntryHeaderValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Business
{
	public class CusEntryHeaderValidation : AutoCusEntryHeaderValidation
	{
		public CusEntryHeaderValidation(AutoCusEntryHeader parent) : base(parent)
		{
		}

		protected new CusEntryHeader Parent
		{
			get { return (CusEntryHeader)base.Parent; }
		}

		public void ValidatePackagesCount()
		{
			ValidateCalculatedProperty(Parent.PackagesCountInfo);
		}

		protected virtual void CheckPackagesCount()
		{
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidatePackagesCount();
			ValidateEntryNumber();
			ValidateCH_CustomsMessageRemarks();
		}

		protected override void CheckCH_BGMReference()
		{
			base.CheckCH_BGMReference();
			if (Parent.IsActive)
			{
				if (Parent.HasNonAmendableChanges)
				{
					Parent.CH_BGMReferenceInfo.AddMessageError(MessageToAddForNonAmendableChanges);
				}
			}
			else
			{
				CheckDeactivatedEntry();
			}
		}

		protected virtual void CheckDeactivatedEntry()
		{
			if (!Parent.HasBeenWithdrawn &&
				(Parent.HasBeenLodgedAtCustoms || Parent.IsWaitingForResponse))
			{
				Parent.CH_BGMReferenceInfo.AddError(DeactivatedEntryWithActiveCustomsTransaction);
			}
		}

		protected virtual string MessageToAddForNonAmendableChanges
		{
			get { return HasNonAmendableChanges; }
		}

		public static string HasNonAmendableChanges
		{
			get { return Res.GetString("34996898-e49b-4529-bccd-b28da421660a", "The changes have just made the entry non-amendable and it will be rejected by Customs. You should withdraw the current entry first WITHOUT changes you have just made and then re-lodge a new entry with the change."); }
		}
		public static string DeactivatedEntryWithActiveCustomsTransaction
		{
			get { return Res.GetString("b0de1419-333e-4b6b-b2ca-4c457940fc57", "The changes you are trying to make cannot be made to this declaration, as an entry has already been lodged with the current details. If you actually mean to make these changes, you will need to withdraw the current entry first."); }
		}

		public void ValidateEntryNumber()
		{
			ValidateCalculatedProperty(Parent.EntryNumberInfo);
		}

		protected virtual void CheckEntryNumber()
		{
		}

		public void ValidateCH_CustomsMessageRemarks()
		{
			ValidateCalculatedProperty(Parent.CH_CustomsMessageRemarksInfo);
		}

		protected virtual void CheckCH_CustomsMessageRemarks()
		{
		}
	}
}
