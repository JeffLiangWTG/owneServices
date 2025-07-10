using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ArchiveManager.Integration;
using Enterprise.Freight.Business.ArchiveManager;

namespace Enterprise.Freight.LocalCartage.Business
{
	internal class ArchiveCommonCartage : IArchiveableBusinessObject
	{
		public ArchiveCommonCartage(CommonCartage cartage)
		{
			this.cartage = cartage;
		}

		readonly CommonCartage cartage;

		IEnumerable<ArchiveReferenceKey> IArchiveableBusinessObject.AdditionalKeys
		{
			get
			{
				if (!cartage.JJ_WaybillNumber.IsEmpty)
				{
					yield return new ArchiveReferenceKey(ArchiveReferenceKey.CommonTypes.Housebill, cartage.JJ_WaybillNumber);
				}

				if (!cartage.JJ_OrderReferenceNumber.IsEmpty)
				{
					yield return new ArchiveReferenceKey(FreightArchiveKeyTypes.Order, cartage.JJ_OrderReferenceNumber);
				}

				if (cartage.CartageParent != null && cartage.CartageParent.UniqueConsignmentID.IsEmpty)
				{
					yield return new ArchiveReferenceKey(ArchiveReferenceKeyTypes.CartageParentJobNumber, cartage.CartageParent.UniqueConsignmentID);
				}
			}
		}

		IEnumerable<ArchiveDocumentDescriptor> IArchiveableBusinessObject.ArchiveDocuments
		{
			get { return null; }
		}

		BusinessObject IArchiveableBusinessObject.ArchiveableBusinessObject
		{
			get { return cartage; }
		}

		ArchiveReferenceKey IArchiveableBusinessObject.NaturalKey
		{
			get { return new ArchiveReferenceKey(ArchiveReferenceKeyTypes.CartageJobNumber, cartage.JJ_ConsignmentID); }
		}

		public static class ArchiveReferenceKeyTypes
		{
			public static readonly ReferenceKeyType CartageJobNumber = new ReferenceKeyType("CTG", ResString.GetMultilingualString("35189d54-0c66-4cdb-9f1c-a6d72e1aadbb", "Port Transport Ref #"));
			public static readonly ReferenceKeyType CartageParentJobNumber = new ReferenceKeyType("CTP", ResString.GetMultilingualString("9ed81f61-d5c9-4856-8d63-009ede9838bd", "Port Transport Parent #"));
		}

		public Guid BranchPK
		{
			get { return Guid.Empty; }
		}
	}
}
