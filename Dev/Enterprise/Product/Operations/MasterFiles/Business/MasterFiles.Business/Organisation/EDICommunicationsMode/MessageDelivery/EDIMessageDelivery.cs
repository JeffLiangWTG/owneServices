using System;
using System.Collections.Generic;
using System.Globalization;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business.MessageDelivery
{
	public sealed class EDIMessageDelivery
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
		public static class ReplacementConstants
		{
			public const string DateTime = "(*DateTime*)";
			public const string JobNumber = "(*JobNumber*)";
			public const string Organisation = "(*Organisation*)";
			public const string Type = "(*Type*)";
		}

		public CommunicationsModeSubstitutor.ExtraDataSubstitutionDelegate ExtraDataSubstitution;

		public EDIMessageDelivery()
			: this(ZString.Empty)
		{
		}

		public EDIMessageDelivery(ZString jobNumber)
		{
			this.JobNumber = jobNumber;
		}
		public readonly ZString JobNumber;

		public IDeliveryResult[] Deliver(DeliveryContext context, IEnumerable<IEDICommunicationsMode> modes, IDeliveryStreamWrapper stream)
		{
			var deliveryStatus = new List<IDeliveryResult>();
			foreach (var mode in modes)
			{
				deliveryStatus.Add(DeliverIndividualMode(context, mode, stream));
			}

			if (stream.Content != null)
			{
				stream.Content.Position = stream.Content.Length;
			}
			return deliveryStatus.ToArray();
		}

		public IDeliveryResult Deliver(DeliveryContext context, IEDICommunicationsMode mode, IDeliveryStreamWrapper stream)
		{
			var result = DeliverIndividualMode(context, mode, stream);
			if (stream.Content != null)
			{
				stream.Content.Position = stream.Content.Length;
			}

			return result;
		}

		public IDeliveryResult DeliverBatch(DeliveryContext context, IEDICommunicationsMode mode, IDeliveryStreamWrapper[] batchStreams)
		{
			IDeliveryResult isDelivered;
			var delivery = Delivery.GetInstance(mode);

			var batchDelivery = delivery as IBatchDelivery;
			if (batchDelivery != null)
			{
				var data = SubstituteModeTemplatesWithRealData(mode);
				isDelivered = ProcessDeliveryResult(mode, batchDelivery.DeliverBatch(context, data, batchStreams));
			}
			else
			{
				if (batchStreams.Length != 1)
				{
					throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Batch delivery is not supported for {0} mode", mode.EK_CommunicationsTransport));
				}
				isDelivered = Deliver(context, mode, batchStreams[0]);
			}

			foreach (var batchStream in batchStreams)
			{
				if (batchStream.Content != null)
				{
					batchStream.Content.Position = batchStream.Content.Length;
				}
			}

			return isDelivered;
		}

		#region Implementation

		IDeliveryResult DeliverIndividualMode(DeliveryContext context, IEDICommunicationsMode mode, IDeliveryStreamWrapper stream)
		{
			var delivery = Delivery.GetInstance(mode);
			var data = SubstituteModeTemplatesWithRealData(mode);

			return ProcessDeliveryResult(mode, delivery.Deliver(context, data, stream));
		}

		IEDICommunicationsMode SubstituteModeTemplatesWithRealData(IEDICommunicationsMode mode)
		{
			IEDICommunicationsMode data = new CommunicationsModeSubstitutor
			{
				JobNumber = JobNumber,
				ExtraDataSubstitution = (property, value) => ExtraDataSubstitution != null ? ExtraDataSubstitution(property, value) : value
			}.Substitute(mode);

			return data;
		}

		IDeliveryResult ProcessDeliveryResult(IEDICommunicationsMode mode, IDeliveryResult result)
		{
			if (!result.Succeeded)
			{
				mode.EK_LastFailed = result.FailTime;
			}

			return result;
		}

		#endregion
	}
}
