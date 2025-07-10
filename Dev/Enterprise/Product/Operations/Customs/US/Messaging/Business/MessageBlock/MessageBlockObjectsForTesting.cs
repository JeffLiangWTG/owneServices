#if DEBUG
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;

namespace Enterprise.Customs.US.Messaging.Business.Testing
{
	[InputBlock("Z¿ºA")]
	[OutputBlock("Z¿ºA")]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.DummyForTesting1, CBPEDIInterchange.ApplicationCodeForTesting)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.DummyForTesting2, CBPEDIInterchange.ApplicationCodeForTesting)]
	class ZZZA : MessageBlock, IControlMessageBlockA
	{
		public ZZZA()
			: base("Z¿ºA")
		{
		}

		[MessageBlockDate(5, "C", "MMddyy")]
		public ZDate DateA;

		[MessageBlockDecimal(10, 11, "C", 0)]
		public ZDecimal DecimalA;

		[MessageBlockInt(5, 21, "C")]
		public ZInt IntA;

		[MessageBlockShort(5, 26, "C")]
		public ZShort ShortA;

		[MessageBlockString(40, 31, "M")]
		public ZString StringA;

		#region IControlMessageBlockA Members

		public ZString ApplicationIdentifier
		{
			get { return StringA; }
			set { StringA = value; }
		}

		public ZString FilerID
		{
			get { return filerID ?? new ZString("DUMMYFILERID"); }
			set { filerID = value; }
		}
		ZString? filerID;

		#endregion
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.DummyForTesting1, CBPEDIInterchange.ApplicationCodeForTesting)]
	class ZZZB : ZZZBBase
	{
		public override ZString ApplicationIdentifier
		{
			get { return ApplicationIdentifierCodeList.DummyForTesting1; }
			set { }
		}
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.DummyForTesting2, CBPEDIInterchange.ApplicationCodeForTesting)]
	class ZZZB2 : ZZZBBase
	{
		public override ZString ApplicationIdentifier
		{
			get { return ApplicationIdentifierCodeList.DummyForTesting2; }
			set { }
		}
	}

	[InputBlock("Z¿ºB")]
	[OutputBlock("Z¿ºB")]
	abstract class ZZZBBase : MessageBlock, IControlMessageBlockB
	{
		protected ZZZBBase()
			: base("Z¿ºB")
		{
		}

		[MessageBlockDate(5, "C", "MMddyy")]
		public ZDate DateB;

		[MessageBlockDecimal(10, 11, "C", 0)]
		public ZDecimal DecimalB;

		[MessageBlockInt(5, 21, "C")]
		public ZInt IntB;

		[MessageBlockShort(5, 26, "C")]
		public ZShort ShortB;

		[MessageBlockString(40, 31, "M")]
		public ZString StringB;

		[MessageBlockString(9, 71, "M")]
		public ZString StringB2;

		#region IControlMessageBlockB Members

		public abstract ZString ApplicationIdentifier { get; set; }

		#endregion
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.DummyForTesting1, CBPEDIInterchange.ApplicationCodeForTesting)]
	class ZZZC : ZZZCBase
	{
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.DummyForTesting2, CBPEDIInterchange.ApplicationCodeForTesting)]
	class ZZZC2 : ZZZCBase
	{
	}

	[InputBlock("Z¿ºC")]
	[OutputBlock("Z¿ºC")]
	abstract class ZZZCBase : MessageBlock
	{
		protected ZZZCBase()
			: base("Z¿ºC")
		{
		}

		[MessageBlockDate(5, "C", "MMddyy")]
		public ZDate DateC;

		[MessageBlockDecimal(10, 11, "C", 0)]
		public ZDecimal DecimalC;

		[MessageBlockInt(5, 21, "C")]
		public ZInt IntC;

		[MessageBlockShort(5, 26, "C")]
		public ZShort ShortC;

		[MessageBlockString(40, 31, "M")]
		public ZString StringC;
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.DummyForTesting2, CBPEDIInterchange.ApplicationCodeForTesting)]
	class ZZZD : ZZZDBase
	{
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.DummyForTesting1, CBPEDIInterchange.ApplicationCodeForTesting)]
	class ZZZDForAll : ZZZDBase
	{
	}

	[InputBlock("Z¿ºD")]
	[OutputBlock("Z¿ºD")]
	abstract class ZZZDBase : MessageBlock, IStatusesAndErrors
	{
		protected ZZZDBase()
			: base("Z¿ºD")
		{
		}

		[MessageBlockDate(5, "C", "MMddyy")]
		public ZDate DateD;

		[MessageBlockDecimal(10, 11, "C", 0)]
		public ZDecimal DecimalD;

		[MessageBlockInt(5, 21, "C")]
		public ZInt IntD;

		[MessageBlockShort(5, 26, "C")]
		public ZShort ShortD;

		[MessageBlockString(40, 31, "M")]
		public ZString StringD;

		#region IStatusesAndErrors Members

		ZString IStatusesAndErrors.LineNumber
		{
			get { return IntD.ToString(); }
		}

		ZString IStatusesAndErrors.NarrativeMessage
		{
			get { return StringD; }
		}

		ZString IStatusesAndErrors.Code
		{
			get { return ShortD.ToString(); }
		}

		ZString IStatusesAndErrors.ReferenceNumber
		{
			get { return DateD.ToShortDateString(); }
		}

		#endregion
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.DummyForTesting1, CBPEDIInterchange.ApplicationCodeForTesting)]
	class ZZZY : ZZZYBase
	{
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.DummyForTesting2, CBPEDIInterchange.ApplicationCodeForTesting)]
	class ZZZY2 : ZZZYBase
	{
	}

	[InputBlock("Z¿ºY")]
	[OutputBlock("Z¿ºY")]
	abstract class ZZZYBase : MessageBlock, IControlMessageBlockY
	{
		protected ZZZYBase()
			: base("Z¿ºY")
		{
		}

		[MessageBlockDate(5, "C", "MMddyy")]
		public ZDate DateY;

		[MessageBlockDecimal(10, 11, "C", 0)]
		public ZDecimal DecimalY;

		[MessageBlockInt(5, 21, "C")]
		public ZInt IntY;

		[MessageBlockShort(5, 26, "C")]
		public ZShort ShortY;

		[MessageBlockString(40, 31, "M")]
		public ZString StringY;
	}

	[InputBlock("Z¿ºZ")]
	[OutputBlock("Z¿ºZ")]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.DummyForTesting1, CBPEDIInterchange.ApplicationCodeForTesting)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.DummyForTesting2, CBPEDIInterchange.ApplicationCodeForTesting)]
	class ZZZZ : MessageBlock, IControlMessageBlockZ
	{
		public ZZZZ()
			: base("Z¿ºZ")
		{
		}

		[MessageBlockDate(5, "C", "MMddyy")]
		public ZDate DateZ;

		[MessageBlockDecimal(10, 11, "C", 0)]
		public ZDecimal DecimalZ;

		[MessageBlockInt(5, 21, "C")]
		public ZInt IntZ;

		[MessageBlockShort(5, 26, "C")]
		public ZShort ShortZ;

		[MessageBlockString(40, 31, "M")]
		public ZString StringZ;
	}
}
#endif
