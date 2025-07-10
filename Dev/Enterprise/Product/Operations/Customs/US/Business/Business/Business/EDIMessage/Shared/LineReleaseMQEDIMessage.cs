using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public class LineReleaseMQEDIMessage : MQEDIMessage
	{
		public new class Schema : MQEDIMessage.Schema
		{
			public const string US_ImporterNumber = "US_ImporterNumber";
			public const string US_ReleaseDateTime = "US_ReleaseDateTime";
			public const string US_BillOfLading = "US_BillOfLading";
			public const string US_PortCode = "US_PortCode";
			public const string US_SupplierCode = "US_SupplierCode";
			public const string LinkedDeclarationReference = "LinkedDeclarationReference";
			public const int US_BillOfLadingMaxLength = 100;
			public const int US_ImporterNumberMaxLength = 35;
		}

		public LineReleaseMQEDIMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public ZString US_ImporterNumber
		{
			get => this.GetSystemDefinedValue<ZString>(Schema.US_ImporterNumber);
			set => this.SetSystemDefinedValue(Schema.US_ImporterNumber, value);
		}

		public ZDateTime US_ReleaseDateTime
		{
			get => this.GetSystemDefinedValue<ZDateTime>(Schema.US_ReleaseDateTime);
			set => this.SetSystemDefinedValue(Schema.US_ReleaseDateTime, value);
		}

		public ZString US_BillOfLading
		{
			get => this.GetSystemDefinedValue<ZString>(Schema.US_BillOfLading);
			set => this.SetSystemDefinedValue(Schema.US_BillOfLading, value);
		}

		public ZString US_PortCode
		{
			get => this.GetSystemDefinedValue<ZString>(Schema.US_PortCode);
			set => this.SetSystemDefinedValue(Schema.US_PortCode, value);
		}

		public ZString US_SupplierCode
		{
			get => this.GetSystemDefinedValue<ZString>(Schema.US_SupplierCode);
			set => this.SetSystemDefinedValue(Schema.US_SupplierCode, value);
		}

		public ZString LinkedDeclarationReference
		{
			get
			{
				if (linkedDeclarationReferenceCached == null)
				{
					linkedDeclarationReferenceCached = new CachedProperty<ZString>(Factory, delegate
					{
						var result = ZString.Empty;
						var declarationPK = EM_LinkUniqueID;
						if (!declarationPK.IsEmpty)
						{
							result = Factory.LoadFromUniqueKey<JobDeclaration>(JobDeclarationSchema.PK, declarationPK).JE_DeclarationReference;
						}
						return result;
					});
				}
				return linkedDeclarationReferenceCached.Value;
			}
		}
		CachedProperty<ZString> linkedDeclarationReferenceCached;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_MessageType = ApplicationIdentifierCodeList.Codes.LineRelease;
		}
	}
}
