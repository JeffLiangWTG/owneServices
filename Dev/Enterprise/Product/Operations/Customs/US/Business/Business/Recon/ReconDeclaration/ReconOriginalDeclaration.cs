using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public class ReconOriginalDeclaration : NonPersistentBusinessObject, IAddInfoManager, IObsoleteValidation
	{
		public ReconOriginalDeclaration(BusinessObjectFactory factory, IColumnIndexer row, IColumnIndexer entryRow)
			: base(factory) // don't pass row to base as we don't want it to registered in the factory
		{
			this.row = Argument.NotNull(row, "row");
			this.entryRow = Argument.NotNull(entryRow, "entryRow");
		}
		readonly IColumnIndexer row;
		readonly IColumnIndexer entryRow;

		public override bool IsInDatabase
		{
			get { return true; }
		}

		public ZGuid EntrySummaryPK
		{
			get { return entryRow.GetValue(CusEntryHeaderSchema.PK); }
		}

		public ZString EntrySummaryStatus
		{
			get { return entryRow.GetValue(CusEntryHeaderSchema.CH_Status); }
		}

		public ZString JE_DeclarationReference
		{
			get { return row.GetValue(JobDeclarationSchema.JE_DeclarationReference); }
		}

		public ZString US_BondProducerAccNo
		{
			get { return AddInfo.US_BondProducerAccNo; }
		}

		public ZString US_EntryType
		{
			get { return AddInfo.US_EntryType; }
		}

		public ZString US_OtherReconIndicator
		{
			get { return AddInfo.US_OtherReconIndicator; }
		}

		public ZBool US_NAFTAReconIndicator
		{
			get { return AddInfo.US_NAFTAReconIndicator; }
		}

		public ZString US_SuretyCode
		{
			get { return AddInfo.US_SuretyCode; }
		}

		public ZString ImporterOfRecordNumber
		{
			get { return OrgHeaderWrapper.GetCustomsCode(ImporterOfRecord, new ZString[] { OrgCusCode.USACodeTypes.EmployerIdentificationNumber, OrgCusCode.USACodeTypes.SocialSecurityNumber, OrgCusCode.USACodeTypes.CBPAssignedNumber }); }
		}

		public ZBool US_PriorDisclosure => AddInfo.US_PriorDisclosure;
		public ZBool US_NAFTAClaimStat => AddInfo.US_NAFTAClaimStat;
		public ZBool US_ProtestStat => AddInfo.US_ProtestStat;

		OrgHeader ImporterOfRecord
		{
			get
			{
				var orgAddress = Factory.Load<OrgAddress>(JE_OA_DeclarantAddress);
				return orgAddress == null ? null : orgAddress.Header;
			}
		}

		public ZGuid JE_OA_DeclarantAddress
		{
			get { return row.GetValue(JobDeclarationSchema.JE_OA_DeclarantAddress); }
		}

		ZString JE_AddInfo
		{
			get { return row.GetValue(JobDeclarationSchema.JE_AddInfo); }
		}

		ZPropertyInfo JE_AddInfoInfo
		{
			get { return GetZPropertyInfo(nameof(JE_AddInfo)); }
		}

		AddInfoReconOriginalDeclaration AddInfo
		{
			get
			{
				if (addInfo == null)
				{
					addInfo = new AddInfoReconOriginalDeclaration(JE_AddInfoInfo);
				}
				return addInfo;
			}
		}
		AddInfoReconOriginalDeclaration addInfo;

		#region IAddInfoManager Members

		IAddInfo IAddInfoManager.AddInfo
		{
			get { return AddInfo; }
		}

		#endregion
	}
}
