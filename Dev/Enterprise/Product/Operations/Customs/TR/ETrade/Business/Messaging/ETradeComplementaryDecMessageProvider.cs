using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.TR.Messaging;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TR.ETrade.Business
{
	public class ETradeComplementaryDecMessageProvider : IETradeComplementaryDec
	{
		public ETradeComplementaryDecMessageProvider(AsycudaManifestHeader header)
		{
			Header = Argument.NotNull(header, nameof(header));
		}
		protected readonly AsycudaManifestHeader Header;

		BusinessObject IMessageSender.Parent => Header;
		IBusinessObjectCollection IMessageSender.Messages => Header.Messages;
		ZString IMessageSender.JobReference => Header.AMA_JobReference;
		IEnumerable<IBillComplementary> IETradeComplementaryDec.Bills => SetBills(Header);
		ZString IETradeComplementaryDec.DeclarationOwnerRepresentativeNameAndTitle => GlbCompany.CurrentCompany.GC_Name;
		ZString IETradeComplementaryDec.DeclarationOwnerRepresentativeTaxNo => GlbCompany.CurrentCompany.GC_BusinessRegNo;
		ZString IETradeComplementaryDec.RegistrationNo => Header.RegistrationNumber;
		IEnumerable<IBillComplementary> SetBills(AsycudaManifestHeader header)
		{
			foreach (var item in header.Bills)
			{
				yield return new BillComplementaryProvider(item);
			}
		}
	}

	public class BillComplementaryProvider : IBillComplementary
	{
		public BillComplementaryProvider(AsycudaBill bill)
		{
			Bill = Argument.NotNull(bill, nameof(bill));
		}
		protected readonly AsycudaBill Bill;
		ZString IBillComplementary.BillNo => Bill.ABL_BillNumber;
		ZString IBillComplementary.ConsigneeTaxIDNo => Bill.SupplementaryDeclarationRegNoIdNo;
		ZDateTime IBillComplementary.DeliveryDate => Bill.SupplementaryDeclarationDeliveryDate;
	}
}
