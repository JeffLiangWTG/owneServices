using CargoWise.Types;
using Enterprise.Messaging.Business.AWB;

namespace Enterprise.Freight.Forwarding.AWB.Messaging
{
	/**
	 * <summary>
	 * Cargo-IMP stands for Cargo Interchange Message Protocol.
	 * It is a set of air cargo messaging standards developed by members of the ATA and IATA.
	 * </summary>
	 */
	public abstract class CargoIMP
	{
		public static class MessageTypes
		{
			public const string FWB = "FWB";
			public const string FHL = "FHL";
		}

		protected CargoIMP()
		{
		}

		#region Elements

		protected ElementList Elements
		{
			get { return fElements ?? (fElements = new ElementList()); }
		}
		ElementList fElements;

		public sealed override string ToString()
		{
			if (!isConstructed)
			{
				AddStandardMessageIdentification();
				ConstructMessage();
				isConstructed = true;
			}
			return Elements.ToString();
		}

		bool isConstructed;

		#endregion

		#region Message ID

		void AddStandardMessageIdentification()
		{
			var messageIDLine = new ElementList();

			messageIDLine.AddLineIdentifier(StandardMessageIdentifier);
			messageIDLine.AddSlant();
			messageIDLine.AddValue(new Format(3, 0, CharType.Numeric), MessageTypeVersionNumber);
			messageIDLine.AddCRLF();

			Elements.AddHeader(messageIDLine);
		}

		public abstract ZString StandardMessageIdentifier { get; }
		protected abstract ZString MessageTypeVersionNumber { get; }
		protected abstract void ConstructMessage();

		public const int MessageMaxLength = 1600;
		public const int LineMaxLength = 69;

		#endregion
	}
}
