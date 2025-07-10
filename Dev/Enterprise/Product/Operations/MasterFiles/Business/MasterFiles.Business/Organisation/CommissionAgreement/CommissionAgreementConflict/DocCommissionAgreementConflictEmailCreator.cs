using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentParsing;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.MasterFiles.Business
{
	public class DocCommissionAgreementConflictEmailCreator : DocumentWrapper
	{
		#region New

		public static DocCommissionAgreementConflictEmailCreator New(CommissionAgreementConflictEmailCreator agreementConflictEmailCreator, BusinessObjectFactory factory)
		{
			return new DocCommissionAgreementConflictEmailCreator(agreementConflictEmailCreator, factory);
		}

		#endregion

		#region Constructor

		protected DocCommissionAgreementConflictEmailCreator(CommissionAgreementConflictEmailCreator agreementConflictEmailCreator, BusinessObjectFactory factory)
			: base(agreementConflictEmailCreator, factory)
		{
		}

		#endregion

		public new CommissionAgreementConflictEmailCreator WrappedObject
		{
			get { return (CommissionAgreementConflictEmailCreator)base.WrappedObject; }
		}

		#region Document Fields

		[DocumentField("Specific Agreement ID")]
		public ZString SpecificAgreementID
		{
			get { return WrappedObject.WinnerCommissionAgreement.AgreementId; }
		}

		[DocumentField("Specific Agreement ID Hyperlink")]
		public ZString SpecificAgreementIDHyperlink
		{
			get { return WrappedObject.WinnerCommissionAgreement.AgreementId; }
		}

		[DocumentField("Specific Commission Agreement Creating User")]
		public ZString SpecificAgreementCreateUser
		{
			get { return string.Format("{0} ({1})", GlbStaff.CurrentUser.GS_FullName, GlbStaff.CurrentUser.GS_Code); }
		}

		[DocumentField("Specific Commission Agreement Items")]
		public ZString SpecificAgreementItems
		{
			get { return string.Join(System.Environment.NewLine, WrappedObject.GetConflictsDescription(1, false)); }
		}

		[DocumentField("Generic Commission Agreement ID")]
		public ZString GenericAgreementID
		{
			get { return WrappedObject.LoserCommissionAgreement.AgreementId; }
		}

		[DocumentField("Generic Commission Agreement ID Hyperlink")]
		public ZString GenericAgreementIDHyperlink
		{
			get { return WrappedObject.LoserCommissionAgreement.AgreementId; }
		}

		[DocumentField("Generic Commission Agreement Customer")]
		public ZString GenericAgreementCustomer
		{
			get
			{
				var customer = WrappedObject.LoserCommissionAgreement.Customer;
				return customer != null ? customer.OH_FullName : ZString.Empty;
			}
		}

		[DocumentField("Recipient Name")]
		public ZString RecipientName
		{
			get { return WrappedObject.RecipientName; }
		}

		#endregion
	}
}
