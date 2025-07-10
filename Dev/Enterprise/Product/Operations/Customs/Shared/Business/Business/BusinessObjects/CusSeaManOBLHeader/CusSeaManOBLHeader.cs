using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	[DependentBusinessObject(typeof(CusSeaManTranHead), "OceanBills")]
	public class CusSeaManOBLHeader : AutoCusSeaManOBLHeader, IStatusNeedsRecalculationProvider, ISendersMessageReferenceProvider
	{
		public CusSeaManOBLHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static CusSeaManOBLHeader LoadFromSendersReference(BusinessObjectFactory factory, ZString sendersReference)
		{
			return factory.LoadTop1<CusSeaManOBLHeader>(new ZQuery(Enterprise.ZArchitecture.Schema.CusSeaManOBLHeaderSchema.BO_SendersMessageReference, sendersReference));
		}

		public override void Delete()
		{
			Details.RemoveAndDeleteAll();
			base.Delete();
		}

		public static readonly CusSeaManOBLHeaderTypeDecider TypeDecider = new CusSeaManOBLHeaderTypeDecider();

		public CusSeaManTranHead TransportHeader
		{
			get
			{
				return Factory.Load<CusSeaManTranHead>(BO_BT);
			}
		}

		#region Fetch Strategy

		protected class CusSeaManOBLHeaderFetchStrategy : EnterpriseBusinessObjectFetchStrategy
		{
			public CusSeaManOBLHeaderFetchStrategy(CusSeaManOBLHeader header)
				: base(header)
			{
				this.header = header;
			}

			protected override void FetchForLoadChildEditableObjectsCore()
			{
				base.FetchForLoadChildEditableObjectsCore();
				Factory.AddFetchHint(EDIMessageSchema.EM_LinkUniqueID, header.PK);
				Factory.AddFetchHint(CusSeaManOBLDetailSchema.BD_BO, header.PK);
			}

			protected override void FetchForValidateCore()
			{
				base.FetchForValidateCore();
				Factory.AddFetchHint(typeof(OrgHeader), header.BO_OH_Consignee);
				Factory.AddFetchHint(typeof(OrgHeader), header.BO_OH_Consignor);

				Factory.AddFetchHint(typeof(OrgAddress), OrgAddressSchema.OA_OH, header.BO_OH_Consignee);
				Factory.AddFetchHint(typeof(OrgAddress), OrgAddressSchema.OA_OH, header.BO_OH_Consignor);

				Factory.AddFetchHint(RefUNLOCOSchema.RL_Code, header.BO_RL_NKDestinationPort);
				Factory.AddFetchHint(RefUNLOCOSchema.RL_Code, header.BO_RL_NKDischargePort);
				Factory.AddFetchHint(RefUNLOCOSchema.RL_Code, header.BO_RL_NKLoadPort);
				Factory.AddFetchHint(RefUNLOCOSchema.RL_Code, header.BO_RL_NKOriginPort);
				Factory.AddFetchHint(RefCountrySchema.RN_Code, header.BO_RN_NKConsigneeCountryCode);
				Factory.AddFetchHint(RefCountrySchema.RN_Code, header.BO_RN_NKConsignorCountryCode);
				Factory.AddFetchHint(RefCountrySchema.RN_Code, header.BO_RN_NKGoodsCountryOfOrigin);
				Factory.AddFetchHint(RefCountrySchema.RN_Code, header.BO_RN_NKNotifyCountryCode);
			}
			readonly CusSeaManOBLHeader header;
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new CusSeaManOBLHeaderFetchStrategy(this);
		}

		#endregion

		#region Business Object Overrides

		public override void OnSaving()
		{
			base.OnSaving();
			((ISendersMessageReferenceProvider)this).PopulateSendersReferenceIfNeeded();
		}

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);
			if (!saveSucceeded && !IsInDatabase)
			{
				BO_SendersMessageReference = ZString.Empty;
			}
		}

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		protected override ZString HumanReadableNameCore
		{
			get { return Res.GetString("8ef357e4-035a-40b3-9d89-495e93327e78", "Ocean Bill {0}", BO_OceanBill).Trim(); }
		}

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		#endregion

		#region Properties

		[ChildEditable(true)]
		public CusSeaManOBLDetailCollection Details
		{
			get
			{
				if (fDetails == null)
				{
					fDetails = GetNewDetails();
					fDetails.Load();
					RegisterEditableChildObject(fDetails);
				}
				return fDetails;
			}
		}
		CusSeaManOBLDetailCollection fDetails;

		EDIMessageCollection messages;
		public EDIMessageCollection Messages
		{
			get
			{
				if (messages == null)
				{
					messages = new EDIMessageCollection(this, Factory);
					messages.Load();
					messages.IsManagedForDataRefresh = true;
				}
				return messages;
			}
		}

		#endregion

		#region Implementation

		protected virtual CusSeaManOBLDetailCollection GetNewDetails()
		{
			return new CusSeaManOBLDetailCollection(this);
		}

		#endregion

		#region IStatusNeedsRecalculationProvider Members

		public bool StatusNeedsRecalculation
		{
			get
			{
				return Messages.HasChanges;
			}
		}

		#endregion

		#region ISendersMessageReferenceProvider Members

		void ISendersMessageReferenceProvider.PopulateSendersReferenceIfNeeded()
		{
			PopulateFormattedNumberPropertyIfRequired(BO_SendersMessageReferenceInfo, Env.NumberFountains.CusSeaManOBLHeaderNumber, ignoreInDatabaseCheck: true);
		}

		ZString ISendersMessageReferenceProvider.SendersReference
		{
			get { return BO_SendersMessageReference; }
		}

		#endregion
	}
}
