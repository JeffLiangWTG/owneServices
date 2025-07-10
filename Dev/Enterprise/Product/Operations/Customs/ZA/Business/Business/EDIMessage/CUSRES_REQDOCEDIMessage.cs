using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.ZA.Business.MessageProcessor;

namespace Enterprise.Customs.ZA.Business
{
	class CUSRES_REQDOCEDIMessage : SARSEDIMessage
	{
		public CUSRES_REQDOCEDIMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region New Properties

		public CUSRESMessageHelper CUSRESHelper
		{
			get { return cusresHelper ?? (cusresHelper = CUSRESMessageHelper.New(this)); }
		}
		CUSRESMessageHelper cusresHelper;

		#endregion

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_MessageType = MessageTypes.CUSRES_REQDOC;
		}
	}
}
