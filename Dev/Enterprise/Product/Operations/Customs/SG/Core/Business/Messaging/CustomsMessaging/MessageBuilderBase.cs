namespace Enterprise.Customs.SG.Business.CustomsMessaging
{
	using Enterprise.Customs.SG.V4.Business;
	using Enterprise.Edifact;
	using Enterprise.Edifact.Auto;

	public abstract class MessageBuilderBase<T> : ICusMessage where T : SegmentGroup, new()
	{
		public MessageBuilderBase(ISGCUSDEC sgCusdec)
		{
			this.sgCusdec = sgCusdec;
			innerMessage = GenerateInternalMessage();
		}

		T GenerateInternalMessage()
		{
			var result = new T();
			GenerateHeaderSection(result);
			GenerateDetailSection(result);
			GenerateSummarySection(result);
			return result;
		}

		#region Edifact Message

		readonly T innerMessage;

		public T EdifactMessage { get { return innerMessage; } }

		public string MessageText
		{
			get { return innerMessage.ToString(new UNOASGCharacterSet()); }
		}

		public int MessageSegmentCount
		{
			get { return innerMessage.CountIncludingUNT; }
		}

		public abstract string MessageType { get; }
		public abstract string MessageSubType { get; }

		#endregion

		#region Implementation

		protected abstract string UnhMessageReleaseNumber { get; }
		protected abstract string UnhAssociationAssignedCode { get; }
		protected abstract string UnhControllingAgency { get; }
		protected abstract string UnhMessageVersionNumber { get; }

		protected abstract void GenerateHeaderSection(T cusdecEdifactMsg);
		protected abstract void GenerateDetailSection(T cusdecEdifactMsg);
		protected abstract void GenerateSummarySection(T cusdecEdifactMsg);

		#region CusdecConstants

		protected const string UnsDetail = "D";
		protected const string UnsSummary = "S";

		#endregion

		protected ISGCUSDEC sgCusdec;

		#endregion
	}
}
