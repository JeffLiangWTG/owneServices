//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoOrgCollectionNoteValidation
//
//    This class should be used for overriding validation in AutoOrgCollectionNoteValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class OrgCollectionNoteValidation : AutoOrgCollectionNoteValidation
	{
		public OrgCollectionNoteValidation(AutoOrgCollectionNote parent)
			: base(parent)
		{
		}

		protected override void CheckPN_Status()
		{
			base.CheckPN_Status();
			MandatoryValidation.CheckEntered(Parent.PN_StatusInfo, Res.GetString("a823225c-1e0d-424d-a309-c67776ce065e", "Call Status"));
			ListValidation.ErrorIfInvalidCode(Parent.PN_StatusInfo);
		}

		protected override void CheckPN_CallDisposition()
		{
			base.CheckPN_CallDisposition();

			if (Parent.PN_Status != CollectionNoteStatusList.Codes.Open)
			{
				MandatoryValidation.CheckEntered(Parent.PN_CallDispositionInfo, Res.GetString("d481c17e-4fe2-4a2c-96a5-812b96657c6a", "Disposition"));
			}

			if (!Parent.PN_CallDisposition.IsEmpty)
			{
				ListValidation.ErrorIfInvalidCode(Parent.PN_CallDispositionInfo);
			}
		}

		protected override void CheckPN_OC()
		{
			base.CheckPN_OC();
			MandatoryValidation.CheckEntered(Parent.PN_OCInfo, Res.GetString("e508ef01-673a-4f86-abba-97551e0fbf7c", "Contact"));
			ListValidation.ErrorIfInvalidPK(Parent.PN_OCInfo);
		}

		protected override void CheckPN_CallDetailNote()
		{
			base.CheckPN_CallDetailNote();
			MandatoryValidation.CheckEntered(Parent.PN_CallDetailNoteInfo, Res.GetString("a4cf1391-9743-4bde-9597-e5dc7c2105de", "Internal Call Note"));
		}
	}
}
