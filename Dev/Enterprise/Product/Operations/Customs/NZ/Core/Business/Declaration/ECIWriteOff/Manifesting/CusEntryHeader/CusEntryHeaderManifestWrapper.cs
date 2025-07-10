using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.NZ.Business.Declaration.ECIWriteOff.Manifesting
{
	public class CusEntryHeaderManifestWrapper : NonPersistentBusinessObject
	{
		public CusEntryHeaderManifestWrapper(CusEntryHeader header)
		{
			this.header = Argument.NotNull(header, "Manifest Entry Header cannot be Null");
		}
		readonly CusEntryHeader header;

		JobDeclaration Declaration => header.Declaration;

		class Schema
		{
			public const string MasterBill = "MasterBill";
			public const string FlightNo = "FlightNo";
			public const string MessageType = "MessageType";
			public const string MessageTypeDescription = "MessageTypeDescription";
			public const string BarrierPort = "BarrierPort";
			public const string BarrierDate = "BarrierDate";
			public const string FormattedMasterBill = "FormattedMasterBill";
			public const string Carrier = "Carrier";
			public const string EntryNo = "EntryNo";
			public const string StatusDescription = "StatusDescription";
			public const string EDITransmitDate = "EDITransmitDate";
			public const string AmountPayable = "AmountPayable";
			public const string DeclarationCount = "DeclarationCount";
			public const string MessageMode = "MessageMode";
			public const int MessageModeMaxLength = 3;
		}

		public bool IsTSWManifest => MessageMode == JobApplicationCodeList.Codes.TSW;

		public ZString MasterBill => Declaration?.JE_MasterBill ?? ZString.Empty;

		public ZPropertyInfo MasterBillInfo => GetZPropertyInfo(Schema.MasterBill);

		public ZString FlightNo => Declaration?.JE_VoyageFlightNo ?? ZString.Empty;

		public ZPropertyInfo FlightNoInfo => GetZPropertyInfo(Schema.FlightNo);

		public ZString MessageType => Declaration?.JE_MessageType ?? ZString.Empty;

		public ZPropertyInfo MessageTypeInfo => GetZPropertyInfo(Schema.MessageType);

		public ZString MessageTypeDescription => Declaration?.MessageTypeDescription ?? ZString.Empty;

		public ZPropertyInfo MessageTypeDescriptionInfo => GetZPropertyInfo(Schema.MessageTypeDescription);

		public ZString BarrierPort => Declaration?.BarrierPort ?? ZString.Empty;

		public ZPropertyInfo BarrierPortInfo => GetZPropertyInfo(Schema.BarrierPort);

		public ZDateTime BarrierDate => Declaration?.BarrierDate ?? ZDateTime.Empty;

		public ZPropertyInfo BarrierDateInfo => GetZPropertyInfo(Schema.BarrierDate);

		public ZString FormattedMasterBill => Declaration?.FormattedMasterBill ?? ZString.Empty;

		public ZPropertyInfo FormattedMasterBillInfo => GetZPropertyInfo(Schema.FormattedMasterBill);

		public ZGuid Carrier => Declaration?.JE_OH_ShippingLine ?? ZGuid.Empty;

		public ZPropertyInfo CarrierInfo => GetZPropertyInfo(Schema.Carrier);

		public ZString EntryNo => header.EntryNumber;

		public ZPropertyInfo EntryNoInfo => GetZPropertyInfo(Schema.EntryNo);

		public ZString StatusDescription => header.Factory.GetCachedValue<LowValueManifestStatusList>().GetDescriptionFromCode(header.CH_EntryStatus);

		public ZPropertyInfo StatusDescriptionInfo => GetZPropertyInfo(Schema.StatusDescription);

		public ZDateTime EDITransmitDate => header.CH_EDITransmitDate;

		public ZPropertyInfo EDITransmitDateInfo => GetZPropertyInfo(Schema.EDITransmitDate);

		public ZDecimal AmountPayable => header.TotalAmountPayable;

		public ZPropertyInfo AmountPayableInfo => GetZPropertyInfo(Schema.AmountPayable);

		public ZInt DeclarationCount => header.Declarations.Count;

		public ZPropertyInfo DeclarationCountInfo => GetZPropertyInfo(Schema.DeclarationCount);

		[ReadOnlyMember(nameof(MessageMode_ReadOnly))]
		[BusinessObjectTestExclude()]
		public ZString MessageMode => Declaration?.JE_ApplicationCode ?? JobApplicationCodeList.Codes.TSW;

		public bool MessageMode_ReadOnly => true;

		public ZPropertyInfo MessageModeInfo => GetZPropertyInfo(Schema.MessageMode);
	}
}
