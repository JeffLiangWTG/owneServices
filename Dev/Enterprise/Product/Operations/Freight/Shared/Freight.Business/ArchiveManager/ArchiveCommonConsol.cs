using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ArchiveManager.Integration;

namespace Enterprise.Freight.Business.ArchiveManager
{
	internal class ArchiveCommonConsol : IArchiveableBusinessObject
	{
		public ArchiveCommonConsol(CommonConsol consol)
		{
			this.consol = consol;
		}

		readonly CommonConsol consol;

		#region IArchiveableBusinessObject Members

		IEnumerable<ArchiveReferenceKey> IArchiveableBusinessObject.AdditionalKeys
		{
			get
			{
				if (!consol.JK_MasterBillNum.IsEmpty)
				{
					yield return new ArchiveReferenceKey(ArchiveReferenceKey.CommonTypes.Masterbill, consol.JK_MasterBillNum);
				}

				if (!consol.JK_BookingReference.IsEmpty)
				{
					yield return new ArchiveReferenceKey(FreightArchiveKeyTypes.BookingRef, consol.JK_BookingReference);
				}

				if (!consol.JK_AgentsReference.IsEmpty)
				{
					yield return new ArchiveReferenceKey(FreightArchiveKeyTypes.AgentRef, consol.JK_AgentsReference);
				}
			}
		}

		IEnumerable<ArchiveDocumentDescriptor> IArchiveableBusinessObject.ArchiveDocuments
		{
			get { return null; }
		}

		BusinessObject IArchiveableBusinessObject.ArchiveableBusinessObject
		{
			get { return consol; }
		}

		ArchiveReferenceKey IArchiveableBusinessObject.NaturalKey
		{
			get { return new ArchiveReferenceKey(FreightArchiveKeyTypes.Consol, consol.JK_UniqueConsignRef); }
		}

		Guid IArchiveableBusinessObject.BranchPK
		{
			get { return Guid.Empty; }
		}

		#endregion
	}
}
