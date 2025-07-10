using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class USHFCHeader : AutoUSHFCHeader, ICusAddInfoTypeSupporter, IPGADataCorrection, ICusDispositionParent, ICanDelete, IHFCHeader
	{
		public USHFCHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : AutoUSHFCHeader.Schema
		{
			public const string US_TrackingStatusDesc = "US_TrackingStatusDesc";
		}

		[ReadOnly(true)]
		[ResourceStringData("Enterprise.Customs.US.Business.USHFCHeader|US_LineNo", Caption = "Line No.")]
		public override ZInt US_LineNo
		{
			get { return base.US_LineNo; }
			set
			{
				var oldValue = US_LineNo;
				if (oldValue != value)
				{
					try
					{
						suspendTrackingStatusChange = true;
						base.US_LineNo = value;
					}
					finally
					{
						suspendTrackingStatusChange = false;
					}
				}
			}
		}
		bool suspendTrackingStatusChange;

		[ResourceStringData("Enterprise.Customs.US.Business.USHFCHeader|US_ASHRAENumber", Caption = "ASHRAE Number")]
		public override ZString US_ASHRAENumber
		{
			get => base.US_ASHRAENumber;
			set
			{
				var oldValue = US_ASHRAENumber;
				base.US_ASHRAENumber = value;
				if (US_ASHRAENumber != oldValue && !US_ASHRAENumber.IsEmpty)
				{
					USHFCDetails.RemoveAndDeleteAll();
				}
			}
		}

		[ResourceStringData("Enterprise.Customs.US.Business.USHFCHeader|US_NetWeight", Caption = "Net Weight(kg)")]
		public override ZDecimal US_NetWeight
		{
			get => base.US_NetWeight;
			set => base.US_NetWeight = value;
		}

		[ResourceStringData("Enterprise.Customs.US.Business.USHFCHeader|US_CertifyingIndividual", Caption = "Certifying Individual")]
		public override ZString US_CertifyingIndividual
		{
			get => base.US_CertifyingIndividual;
			set => base.US_CertifyingIndividual = value;
		}

		[ResourceStringData("Enterprise.Customs.US.Business.USHFCHeader|US_HFCImageSent", Caption = "Elec. Image Submitted")]
		public override ZBool US_HFCImageSent
		{
			get => base.US_HFCImageSent;
			set => base.US_HFCImageSent = value;
		}

		public ZString US_TrackingStatusDesc
		{
			get { return Factory.GetCachedValue<PGATrackingStatusList>().GetDescriptionFromCode(US_TrackingStatus); }
		}

		public ZPropertyInfo US_TrackingStatusDescInfo
		{
			get { return GetZPropertyInfo(Schema.US_TrackingStatusDesc); }
		}

		[ChildEditable(true)]
		[UniversalCopyCollectionEntity(AutoCusAddInfo.Schema.TableName, AutoCusAddInfo.Schema.B7_ParentID)]
		public USHFCDetailCollection USHFCDetails
		{
			get
			{
				if (ushfcDetails == null)
				{
					ushfcDetails = new USHFCDetailCollection(this);
					ushfcDetails.Load();
					RegisterEditableChildObject(ushfcDetails);
				}
				return ushfcDetails;
			}
		}
		USHFCDetailCollection ushfcDetails;

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new Customs.Business.FetchStrategies.CusAddInfoTypeSupporterFetchStrategy(this);
		}

		IDictionary<ZString, Type> ICusAddInfoTypeSupporter.GetCusAddInfoTypes()
		{
			var result = new Dictionary<ZString, Type>();
			result.Add(CusAddInfoTypeAttribute.Codes.USHFCDetail, typeof(USHFCDetail));
			return result;
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			UpdateAddInfoProperties();

			var result = (USHFCHeader)base.CloneInternal(args);
			result.US_HFCImageSent = ZBool.False;

			foreach (USHFCDetail detail in USHFCDetails)
			{
				result.USHFCDetails.Add((USHFCDetail)detail.Clone(new BusinessObjectCloneArgs(args?.AlternativeFactoryToInstantiateCloneIn ?? Factory, Array.Empty<string>(), typeof(USHFCDetail), false)));
			}
			return result;
		}

		public void UpdateAddInfoProperties()
		{
			if (AddInfo != null && HasChanges)
			{
				AddInfo.UpdateRelatedPropertyInfo();
			}
		}

		public override void Delete()
		{
			PGADataCorrection.UnRegisterTrackerIfNeeded();
			USHFCDetails.RemoveAndDeleteAll();
			base.Delete();
		}

		public override void OnLoaded()
		{
			base.OnLoaded();
			PGADataCorrection.RegisterTrackerIfNeeded();
			SetReadOnlyIncludingChildren(PGADataCorrection.IsPGALineReadOnly());
		}

		#region Related

		public JobComInvoiceLine InvoiceLine => Factory.Load<JobComInvoiceLine>(B7_ParentID);

		public JobDeclaration Declaration => InvoiceLine?.Declaration;

		USHFCHeaderAddInfo AddInfo
		{
			get
			{
				if (fAddInfo == null)
				{
					fAddInfo = new USHFCHeaderAddInfo(B7_AddInfoDataInfo);
					RegisterEditableChildObject(fAddInfo);
					RegisterListChangedCalledRefreshBinding(fAddInfo);
				}
				return fAddInfo;
			}
		}
		USHFCHeaderAddInfo fAddInfo;

		#endregion

		#region IPGADataCorrection

		IPGADataCorrection PGADataCorrection
		{
			get { return this; }
		}

		bool IPGADataCorrection.SettingStatusInProgress
		{
			get { return settingPGATrackingStatusInProgress; }
			set { settingPGATrackingStatusInProgress = value; }
		}
		bool settingPGATrackingStatusInProgress;

		bool IPGADataCorrection.SuspendTrackingStatusChange
		{
			get { return suspendTrackingStatusChange || Data.IsSettingAddInfoPropertyInProgress; }
		}

		JobComInvoiceLine IPGADataCorrection.InvoiceLine
		{
			get { return InvoiceLine; }
		}

		string[] IPGADataCorrection.GetIndicatorFields()
		{
			return new[] { JobComInvoiceLine.Schema.US_HFCInd };
		}

		string[] IPGADataCorrection.GetDislaimReasonFields()
		{
			return new[] { JobComInvoiceLine.Schema.US_HFCDisclaimReason };
		}

		string[] IPGADataCorrection.GetRelatedInvoiceLineFields()
		{
			return new[] { JobComInvoiceLine.Schema.JI_OA_ConsigneeAddress };
		}

		string[] IPGADataCorrection.GetRelatedInvoiceFields()
		{
			return new[] { JobComInvoiceHeader.Schema.JZ_OA_ConsigneeAddress };
		}

		string[] IPGADataCorrection.GetRelatedContainerFields()
		{
			return new[] { CusContainer.Schema.CO_ContainerNumber, CusContainer.Schema.CO_RC };
		}

		string[] IPGADataCorrection.GetRelatedDeclarationFields()
		{
			return new[] { JobDeclaration.Schema.JE_OA_ConsigneeAddress };
		}

		ZPropertyInfo IPGADataCorrection.TrackingStatusInfo
		{
			get { return US_TrackingStatusInfo; }
		}

		#endregion

		#region IPGALineStatus

		ZString IPGALineStatus.PGALineStatusAgencyCode
		{
			get { return ACEGovernmentAgenciesCodeList.Codes.EPA; }
		}

		ZInt IPGALineStatus.PGALineNumber
		{
			get { return US_LineNo; }
		}

		CusDispositionCollection IPGALineStatus.PGALineCusDispositions
		{
			get
			{
				if (fCusDisposition == null)
				{
					fCusDisposition = new CusDispositionCollection(this);
					fCusDisposition.Load();
				}
				return fCusDisposition;
			}
		}
		CusDispositionCollection fCusDisposition;

		public ZString Status
		{
			get { return this.GetStatus(); }
		}

		public ZString StatusDesc
		{
			get { return ((ICusDispositionParent)this).GetStatusDescription(Status); }
		}

		public ZDateTime StatusDate
		{
			get { return this.GetStatusDate(); }
		}

		ZString ICusDispositionParent.Type
		{
			get { return CusDispositionTypeCodeList.Codes.USPGALineStatus; }
		}

		ZString ICusDispositionParent.ParentTableCode
		{
			get { return CusAddInfoSchema.Constants.Prefix; }
		}

		BusinessObject ICusDispositionParent.CollectionMaster
		{
			get { return this; }
		}

		ZString ICusDispositionParent.GetStatusDescription(ZString status)
		{
			return PGADispositionProviderExtensionMethods.GetDescriptionFromZZRefCusCodeList(Factory, status, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineStatus);
		}

		#endregion

		#region ICanDelete

		bool ICanDelete.CanDelete
		{
			get { return PGADataCorrection.PGALinesCanBeDeleted(); }
		}

		MultilingualString ICanDelete.ReasonForNotAbleToDelete
		{
			get { return PGADataChangeTracker.ReasonForNotAbleToDelete; }
		}

		#endregion

		#region FetchStrategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new Strategy(this);
		}

		class Strategy : EnterpriseBusinessObjectFetchStrategy
		{
			public Strategy(USHFCHeader businessObject)
				: base(businessObject)
			{
			}

			protected override void FetchForLoadChildEditableObjectsCore()
			{
				base.FetchForLoadChildEditableObjectsCore();
				Factory.AddFetchHint(typeof(CusDisposition), new ZQuery(CusDispositionSchema.CDI_Type, CusDispositionTypeCodeList.Codes.USPGALineStatus), new ZQuery(CusDispositionSchema.CDI_ParentID, BusinessObject.PK));
			}
		}

		#endregion

		#region IHFCHeader
		ZInt IHFCHeader.LineNo
		{
			get { return US_LineNo; }
			set { US_LineNo = value; }
		}

		ZBool IHFCHeader.ElectronicImageSubmitted => US_HFCImageSent;

		ZString IHFCHeader.ASHRAENumber => US_ASHRAENumber;

		ZString IHFCHeader.CertifyingIndividual => US_CertifyingIndividual;

		IPGAContactDetails IHFCHeader.Importer => Declaration?.IORWrapper;

		IPGAContactDetails IHFCHeader.Consignee => OrgHeaderWrapper.New(InvoiceLine?.ConsigneeAddress);

		ICustomsBrokerDetails IHFCHeader.CustomsBroker => InvoiceLine;

		ZDecimal IHFCHeader.NetWeight => US_NetWeight;

		IEnumerable<IContainerDetail> IHFCHeader.CusContainers
		{
			get
			{
				if (InvoiceLine is JobComInvoiceLine invoiceLine)
				{
					foreach (CusContainerInvoiceLinePivot pivot in invoiceLine.ContainersPivot)
					{
						var container = pivot.Container;
						if (container != null)
						{
							yield return container;
						}
					}
				}
			}
		}

		IEnumerable<IHFCDetail> IHFCHeader.HFCDetails => USHFCDetails.OfType<IHFCDetail>();

		#endregion

	}
}
