using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ZA.Business.MessageProcessor;

namespace Enterprise.Customs.ZA.Business
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("Usage not yet developed #Victor 20160504")]
	public class CONTRLEDIMessage : SARSEDIMessage
	{
		public CONTRLEDIMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void OnFactorySaved(bool saveSucceeded)
		{
			base.OnFactorySaved(saveSucceeded);
			if (!saveSucceeded)
			{
				if (!IsInDatabase)
				{
					Delete();
				}
			}
		}

		#region Override Properties

		[BusinessObjectTestExclude]
		public override ZString EM_MessageType
		{
			get { return base.EM_MessageType; }
			set
			{
				if (value != MessageTypes.CONTRL)
				{
					throw new NotSupportedException("Invalid MessageType for a Control Message. MessageType must be CTL all the time");
				}
				base.EM_MessageType = value;
			}
		}

		public override ZString ParentMessageNumber
		{
			get
			{
				if (!parentMessageNumber.HasValue)
				{
					parentMessageNumber = CONTRLHelper?.MessageReferenceNumber ?? ZString.Empty;
				}

				return parentMessageNumber.Value;
			}
		}
		ZString? parentMessageNumber;

		#endregion

		#region New Properties

		public ZBool IsRejectionMessage
		{
			get
			{
				if (!isRejectionMessage.HasValue)
				{
					isRejectionMessage = (CONTRLHelper?.ActionCodedForMessage == Edifact.D96B.Elements.ActionCodedList.ThisLevelAndAllLowerLevelsRejected);
				}
				return isRejectionMessage.Value;
			}
		}
		ZBool? isRejectionMessage;

		public CONTRLMessageHelper CONTRLHelper
		{
			get { return contrlHelper ?? (contrlHelper = CONTRLMessageHelper.New(this)); }
		}
		CONTRLMessageHelper contrlHelper;

		#endregion

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_MessageType = MessageTypes.CONTRL;
			EM_ReceiveTransmit = Direction.Receive;
		}
	}
}
