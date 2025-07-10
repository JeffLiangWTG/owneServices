using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.ZArchitecture.Business;
using IContainer = Enterprise.DocumentVisualizer.DocDataObjects.IContainer;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.CN
{
	sealed class Booking : DocDataObject, IBooking
	{
		public Booking(string identifier, IStmALogProvider logProvider = null)
			: base(identifier)
		{
			this.logProvider = logProvider;
		}

		readonly IStmALogProvider logProvider;

		#region Send

		[IgnoreChanges]
		public ZBool Send
		{
			get => send;
			set
			{
				if (SetNonPersistentPropertyValue(SendInfo, ref send, value))
				{
					Validate(SendInfo);
				}
			}
		}

		ZBool send;

		public ZPropertyInfo SendInfo => GetZPropertyInfo(nameof(Send));

		#endregion

		#region BookingNumber

		public ZString BookingNumber
		{
			get => bookingNumber;
			set
			{
				if (SetNonPersistentPropertyValue(BookingNumberInfo, ref bookingNumber, value))
				{
					Validate(BookingNumberInfo);
				}
			}
		}

		ZString bookingNumber;

		public ZPropertyInfo BookingNumberInfo => GetZPropertyInfo(nameof(BookingNumber));

		#endregion

		#region MessageStatus

		public MessageStatus MessageStatus => messageStatus ?? (messageStatus = new MessageStatus(BookingNumber, logProvider));
		MessageStatus messageStatus;

		#endregion

		#region Containers
		public IReadOnlyCollection<BookingContainer> Containers
		{
			get => containers;
			set => containers = SetChildCollection(containers, value);
		}

		IReadOnlyCollection<BookingContainer> containers;

		IReadOnlyCollection<IContainer> IBooking.Containers => Containers;

		#endregion
	}
}
