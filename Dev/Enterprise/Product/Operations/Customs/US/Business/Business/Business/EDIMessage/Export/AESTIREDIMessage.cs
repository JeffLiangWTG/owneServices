using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.AES;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public class AESTIREDIMessage : MQEDIMessage, IAESMessagePrint
	{
		public AESTIREDIMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors")]
		public static class MessageTypes
		{
			public const string AESDirect = "AED";
		}

		#region Implementation

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_ApplicationCode = ApplicationCodes.USCustomsExport;
		}

		protected override void GetNumberFountainNumbersAndFillInPlaceHolders()
		{
			PopulateMessageNumber();
		}

		protected override void PopulateMessageNumber()
		{
			EM_MessageNum = GetMessageReferenceNumber();
		}

		protected override string GetMessageReferenceNumber()
		{
			return AESMessageNumberFountain.GetNext(Factory, IsTransmitMessage, Company);
		}

		protected override ZQuery GetExtraOriginalMessageFilter()
		{
			return GetCompanyAndNotAESDirectFilter();
		}

		ZQuery GetCompanyAndNotAESDirectFilter()
		{
			var company = Company ?? GlbCompany.CurrentCompany;
			var result = new ZQuery(EDIMessageSchema.EM_GB, company.Branches.GetPKs());
			result.AddToFilter(EDIMessageSchema.EM_MessageType, SQLComparisonOperator.NotEqual, MessageTypes.AESDirect);
			return result;
		}

		protected override ZQuery GetExtraResponseMessageFilter()
		{
			return GetCompanyAndNotAESDirectFilter();
		}

		#endregion

		ZString IAESMessagePrint.MessageText
		{
			get { return EM_MessageText; }
		}

		ZString IAESMessagePrint.MessageType
		{
			get { return EM_MessageType; }
		}

		ZGuid IAESMessagePrint.LinkUniqueID
		{
			get { return EM_LinkUniqueID; }
		}

		BusinessObjectFactory IAESMessagePrint.Factory
		{
			get { return Factory; }
		}
	}
}
