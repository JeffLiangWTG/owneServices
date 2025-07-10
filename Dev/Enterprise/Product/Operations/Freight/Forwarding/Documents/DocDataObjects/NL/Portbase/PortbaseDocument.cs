using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.NL
{
	sealed class PortbaseDocument : DocDataObject
	{
		public PortbaseDocument(object id)
					  : base(id)
		{
		}

		#region Reference

		public ZString ReferenceNumber
		{
			get => referenceNumber;
			set
			{
				if (SetNonPersistentPropertyValue(ReferenceNumberInfo, ref referenceNumber, value))
				{
					Validate(ReferenceNumberInfo);
				}
			}
		}
		ZString referenceNumber;

		public ZPropertyInfo ReferenceNumberInfo => GetZPropertyInfo(nameof(ReferenceNumber));

		#endregion

		#region Containers

		public IReadOnlyCollection<PortbaseContainer> Containers
		{
			get => containers;
			set => containers = SetChildCollection(containers, value);
		}
		IReadOnlyCollection<PortbaseContainer> containers;

		#endregion

		#region EntryType

		public ICodeDescription EntryType
		{
			get => entryType;
			set => entryType = SetChild(entryType, value);
		}

		ICodeDescription entryType;

		#endregion

		#region Status

		public ZString Status
		{
			get => status;
			set
			{
				if (SetNonPersistentPropertyValue(StatusInfo, ref status, value))
				{
					Validate(StatusInfo);
				}
			}
		}
		ZString status;

		public ZPropertyInfo StatusInfo => GetZPropertyInfo(nameof(Status));

		#endregion

		#region IsSelectedToSend

		public ZBool IsSelectedToSend
		{
			get => isSelectedToSend;
			set
			{
				if (SetNonPersistentPropertyValue(IsSelectedToSendInfo, ref isSelectedToSend, value))
				{
					Validate(IsSelectedToSendInfo);
				}
			}
		}
		ZBool isSelectedToSend;

		public ZPropertyInfo IsSelectedToSendInfo => GetZPropertyInfo(nameof(IsSelectedToSend));

		#endregion

		#region CanWithdraw

		public ZBool CanWithdraw
		{
			get => canWithdraw;
			set
			{
				if (SetNonPersistentPropertyValue(CanWithdrawInfo, ref canWithdraw, value))
				{
					Validate(CanWithdrawInfo);
				}
			}
		}
		ZBool canWithdraw;

		public ZPropertyInfo CanWithdrawInfo => GetZPropertyInfo(nameof(CanWithdraw));

		#endregion
	}
}
