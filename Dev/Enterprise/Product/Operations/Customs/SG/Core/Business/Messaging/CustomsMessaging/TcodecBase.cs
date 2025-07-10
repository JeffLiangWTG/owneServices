namespace Enterprise.Customs.SG.Business.CustomsMessaging
{
	using Enterprise.Customs.SG.V4.Business;
	using Enterprise.Edifact.Auto;

	public abstract class TcodecBase<T> : MessageBuilderBase<T> where T : SegmentGroup, new()
	{
		public TcodecBase(ITCODEC certificateOfOriginDeclaration)
			: base(certificateOfOriginDeclaration)
		{
		}

		protected ITCODEC CertificateOfOrigin
		{
			get { return (ITCODEC)sgCusdec; }
		}

		#region Implementation

		public sealed override string MessageType
		{
			get { return CommonAccessReferenceCodeList.Codes.COODEC; }
		}

		public sealed override string MessageSubType
		{
			get { return CUSDECEDIMessage.Declaration; }
		}

		protected sealed override string UnhControllingAgency
		{
			get { return TcodecControllingAgency; }
		}

		protected sealed override string UnhMessageVersionNumber
		{
			get { return TcodecMessageVersionNumber; }
		}

		protected sealed override string UnhMessageReleaseNumber
		{
			get { return TcodecMessageReleaseNumber; }
		}

		#region TcodecConstants

		const string TcodecControllingAgency = "RT";
		const string TcodecMessageVersionNumber = "0";
		const string TcodecMessageReleaseNumber = "1";
		protected const string UnhMessageTypeIdentifier = "TCODEC";
		protected const string BgmApplicationType = "COO";

		#endregion

		#endregion
	}
}
