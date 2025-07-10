using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.ManifestBase;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.ACEManifest.Business
{
	public class AsycudaTransferBillValidation : ASYCUDA.Business.AsycudaTransferBillValidation
	{
		public AsycudaTransferBillValidation(AutoAsycudaTransferBill parent) : base(parent)
		{
		}

		new AsycudaTransferBill Parent => (AsycudaTransferBill)base.Parent;

		public override void ValidateAll()
		{
			base.ValidateAll();
			using (((ISingleElementListInternal)Parent).SuspendListChanged())
			{
				ValidateInBondNumber();
			}
		}

		public void ValidateInBondNumber()
		{
			ValidateCalculatedProperty(Parent.InBondNumberInfo);
		}

		protected virtual void CheckInBondNumber()
		{
			var parent = Parent;
			if (parent.IsActive)
			{
				US.Business.InBondNumberAvailabilityChecker.Check(parent.InBondNumberInfo, parent.HeaderBranch, false);
			}
		}

		protected override void CheckATB_BillNumber()
		{
			base.CheckATB_BillNumber();

			var parent = Parent;
			if (parent.IsActive)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.ATB_BillNumberInfo);
				ListValidation.WarnIfInvalidCode(parent.ATB_BillNumberInfo, parent.Lookups.ManifestBillsList,
					(IMultilingualString)ResString.GetMultilingualString("106A20CC-645E-43DE-BEB9-862D15F80AF3", "The Bill Number entered does not match any Bills on this Manifest"));

				var otherActiveTransfers = GetActiveTransferBillsFromArrival();
				if (otherActiveTransfers.Any(x => x.ATB_BillNumber.EqualsIgnoringCase(parent.ATB_BillNumber)))
				{
					parent.ATB_BillNumberInfo.AddError(ResString.GetMultilingualString("4B07E88E-CED2-40C2-A7DE-E8A075066ABF", "There can only be one active Transfer per Bill on the same flight."));
				}
				else if ((parent.ATB_BillOfLadingType == AsycudaBill.ChildBolCode && otherActiveTransfers.Any(x => !x.ATB_BillNumber.IsEmpty))
					|| otherActiveTransfers.Any(x => x.ATB_BillOfLadingType == AsycudaBill.ChildBolCode))
				{
					parent.ATB_BillNumberInfo.AddError(ResString.GetMultilingualString("C56B9848-FDF3-4070-823E-F2501C253CD0", "A Master Bill cannot be transferred with a House Bill on the same flight."));
				}
			}
		}

		IEnumerable<AsycudaTransferBill> GetActiveTransferBillsFromArrival()
		{
			var result = new List<AsycudaTransferBill>();

			var parent = Parent;
			var arrival = parent.TransferHeader.ArrivalHeader;

			foreach (var transferHeader in arrival.TransferHeaders)
			{
				var activeTransfers = transferHeader.TransferBills.Cast<AsycudaTransferBill>().Where(x => x != parent && x.IsActive);
				result.AddRange(activeTransfers);
			}

			return result;
		}
	}
}
