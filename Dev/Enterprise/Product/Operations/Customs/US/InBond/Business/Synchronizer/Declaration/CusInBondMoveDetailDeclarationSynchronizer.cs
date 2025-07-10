using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.US.InBond.Business
{
	class CusInBondMoveDetailDeclarationSynchronizer : BusinessObjectSynchroniser
	{
		internal CusInBondMoveDetailDeclarationSynchronizer(CusInBondMoveDetail destination, US.Business.Bill source, CusInBondBill destinationParent)
			: base(destination, source)
		{
			this.destinationParent = Argument.NotNull(destinationParent, "destinationParent");
		}

		readonly CusInBondBill destinationParent;

		protected new CusInBondMoveDetail Destination
		{
			get { return (CusInBondMoveDetail)base.Destination; }
		}

		protected US.Business.Bill Bill
		{
			get { return (US.Business.Bill)base.Source; }
		}

		protected override void HookSynchronisers()
		{
			base.HookSynchronisers();

			Synchronisers.Add(new FieldSynchroniser(Destination.B9_B0Info, GetBillPK, GetBillPKRelatedInfos));
			Synchronisers.Add(new CusInBondContainerCollectionDeclarationSynchroniser(Bill, Destination));
		}

		#region Bill PK

		IZType GetBillPK()
		{
			return destinationParent.PK;
		}

		IEnumerable<ZPropertyInfo> GetBillPKRelatedInfos()
		{
			yield return Destination.B9_B0Info;
		}

		#endregion
	}
}
