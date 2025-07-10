using System;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class ReconDeclarationValidation : ZValidation
	{
		public ReconDeclarationValidation(ReconDeclaration reconDeclaration)
			: base(reconDeclaration)
		{
			this.reconDeclaration = reconDeclaration;
		}

		readonly ReconDeclaration reconDeclaration;

		public override void ValidateAll()
		{
			reconDeclaration.ClearRowNotifications();
			ValidateMaximumNumberOfEntries();
		}

		void ValidateMaximumNumberOfEntries()
		{
			if (reconDeclaration.OriginalEntries.Count > 9999)
			{
				reconDeclaration.AddRowMessageError(MaximumNumberOfEntriesExceeded);
			}
		}
		public const string MaximumNumberOfEntriesExceeded = "The number of original entries have exceeded the maximum allowed; a maximum of 9,999 entries are allowed per Recon Job.";

		public override Type AutoValidationType
		{
			get { return typeof(ReconDeclarationValidation); }
		}

		public void ValidateReconEntryNumberWithEntryFilerCode()
		{
			ValidateCalculatedProperty(reconDeclaration.ReconEntryNumberWithEntryFilerCodeInfo);
		}

		protected virtual void CheckReconEntryNumberWithEntryFilerCode()
		{
			ZString bondNumber = ZString.Empty;
			foreach (ReconOriginalEntryHeader originalHeader in reconDeclaration.OriginalEntries)
			{
				if (!bondNumber.IsEmpty && bondNumber != originalHeader.DeclarationBondNo)
				{
					reconDeclaration.ReconEntryNumberWithEntryFilerCodeInfo.AddMessageError(BondNoMustBeTheSame);
					break;
				}
				bondNumber = originalHeader.DeclarationBondNo;
			}
		}
		public const string BondNoMustBeTheSame = "Bond Number for all import entries should be the same. There are import entries which have different Bond numbers. Please check Bond # columns.";
	}
}
