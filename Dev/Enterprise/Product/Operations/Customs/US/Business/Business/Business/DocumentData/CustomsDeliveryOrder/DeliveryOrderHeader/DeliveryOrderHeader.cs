using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.US.Business
{
	public class DeliveryOrderHeader : AutoDeliveryOrderHeader, ICusAddInfoTypeSupporter, ICusCodeDataTypeSupporter
	{
		public DeliveryOrderHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : AutoDeliveryOrderHeader.Schema
		{
			public const string US_DeliveryInstructions = "US_DeliveryInstructions";
		}

		public static class Constants
		{
			public const string CusCodeDataBill = "BIL";
		}

		#region US_DeliveryInstructions

		[CargoWise.ComponentModel.MaxLength(StmNote.Schema.ST_NoteTextMaxLength)]
		public ZString US_DeliveryInstructions
		{
			get
			{
				StmNote note = DeliveryInstructionsNote;
				return (note != null) ? note.ST_NoteText : ZString.Empty;
			}
			set
			{
				StmNote newDeliveryInstructions = DeliveryInstructionsNote;

				if (value.IsEmpty)
				{
					if (newDeliveryInstructions != null)
					{
						newDeliveryInstructions.Delete();
						newDeliveryInstructions = null;
					}
				}
				else
				{
					if (newDeliveryInstructions == null)
					{
						newDeliveryInstructions = Notes.AddNew();
						newDeliveryInstructions.ST_ParentID = PK;
						newDeliveryInstructions.ST_Table = TableName;
						newDeliveryInstructions.ST_Description = PredefinedNoteTypes.Instance.DeliveryInstructionsNote.Description;
					}

					CheckMaximumLength(US_DeliveryInstructionsInfo, value);
					newDeliveryInstructions.ST_NoteDataAsText = value;
				}
				US_DeliveryInstructionsInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo US_DeliveryInstructionsInfo
		{
			get { return GetZPropertyInfo(Schema.US_DeliveryInstructions); }
		}

		protected StmNote DeliveryInstructionsNote
		{
			get
			{
				StmNote[] deliveryInstructionsNotes = Notes.FindByDescription(PredefinedNoteTypes.Instance.DeliveryInstructionsNote.Description);
				return deliveryInstructionsNotes.Length > 0 ? deliveryInstructionsNotes[0] : null;
			}
		}

		#endregion

		[BusinessObjectTestExclude]
		public override ZString B7_ParentTableCode
		{
			get { return base.B7_ParentTableCode; }
			set
			{
				if (!value.IsEmpty && value != JobDeclarationSchema.Constants.Prefix)
				{
					throw new NotSupportedException("Setting DeliveryOrderHeader.B7_ParentTableCode is not supported.");
				}
				base.B7_ParentTableCode = value;
			}
		}

		public override ZGuid US_OH_Shipper
		{
			get { return GetEffectiveValueToReturn(base.US_OH_Shipper, JobDeclaration.Schema.JE_OH_Importer, Schema.US_OH_Shipper); }
			set { base.US_OH_Shipper = GetEffectiveValueToSet(value, JobDeclaration.Schema.JE_OH_Importer); }
		}

		public JobDeclaration Declaration
		{
			get { return Parent; }
		}

		public ZString FormattedEntryNumber
		{
			get
			{
				ZString result = ZString.Empty;
				if (Declaration is JobDeclaration declaration && !declaration.ImportEntryNumber.IsEmpty)
				{
					result = declaration.US_EntryFilerCode + "-" + declaration.ImportEntryNumber.SubstringSafe(0, 7) + "-" + declaration.ImportEntryNumber.SubstringSafe(7, 1);
				}
				return result;
			}
		}

		public ZString ImportingCarrier
		{
			get
			{
				ZString result = ZString.Empty;
				if (Declaration is JobDeclaration declaration)
				{
					if (declaration.IsAir)
					{
						result = declaration.JE_VoyageFlightNo + " " + declaration.JE_MasterBillIssuerSCAC;
					}
					else if (declaration.IsSea)
					{
						result = declaration.JE_VesselName;
					}
					else
					{
						result = declaration.JE_VoyageFlightNo;
					}
				}
				return result;
			}
		}

		public JobDocAddress ForDeliveryToAddress
		{
			get
			{
				if (forDeliveryToAddress == null ||
					forDeliveryToAddress.IsDeleted ||
					forDeliveryToAddress.E2_AddressType != DocAddressTypes.Codes.DropOffAddress ||
					forDeliveryToAddress.E2_AddressSequence != 0 ||
					forDeliveryToAddress.E2_ParentID != PK ||
					forDeliveryToAddress.E2_ParentTableCode != TablePrefix)
				{
					forDeliveryToAddress = LoadOrCreateForDeliveryToAddress();
					RegisterEditableChildObject(forDeliveryToAddress);
				}
				return forDeliveryToAddress;
			}
		}
		JobDocAddress forDeliveryToAddress;

		JobDocAddress LoadOrCreateForDeliveryToAddress()
		{
			JobDocAddress result = null;
			ZQuery query = new ZQuery(JobDocAddressSchema.E2_ParentID, PK);
			query.AddToFilter(JobDocAddressSchema.E2_ParentTableCode, TablePrefix);
			query.AddToFilter(JobDocAddressSchema.E2_AddressType, DocAddressTypes.Codes.DropOffAddress);
			query.AddToFilter(JobDocAddressSchema.E2_AddressSequence, 0);
			query.FetchOnlyFromLocalCache = !IsInDatabase;
			result = Factory.LoadTop1<JobDocAddress>(query);
			if (result == null)
			{
				result = Factory.New<JobDocAddress>();
				result.E2_ParentTableCode = TablePrefix;
				result.E2_ParentID = PK;
				result.DocAddressType = DocAddressType.DropOffAddress;
			}
			result.DefaultAddressType = AddressType.DLV;
			return result;
		}

		[ChildEditable(true)]
		public DeliveryOrderLineCollection DeliveryOrderLines
		{
			get
			{
				if (fDeliveryOrderLines == null)
				{
					fDeliveryOrderLines = new DeliveryOrderLineCollection(this);
					fDeliveryOrderLines.Load();
					RegisterEditableChildObject(fDeliveryOrderLines);
				}
				return fDeliveryOrderLines;
			}
		}
		DeliveryOrderLineCollection fDeliveryOrderLines;

		[ChildEditable(true)]
		public DeliveryOrderContainerCollection DeliveryOrderContainers
		{
			get
			{
				if (fDeliveryOrderContainers == null)
				{
					fDeliveryOrderContainers = new DeliveryOrderContainerCollection(this);
					fDeliveryOrderContainers.Load();
					RegisterEditableChildObject(fDeliveryOrderContainers);
				}
				return fDeliveryOrderContainers;
			}
		}
		DeliveryOrderContainerCollection fDeliveryOrderContainers;

		[ChildEditable(true)]
		public DeliveryOrderHazmatCollection DeliveryOrderHazmats
		{
			get
			{
				if (fDeliveryOrderHazmats == null)
				{
					fDeliveryOrderHazmats = new DeliveryOrderHazmatCollection(this);
					fDeliveryOrderHazmats.Load();
					RegisterEditableChildObject(fDeliveryOrderHazmats);
				}
				return fDeliveryOrderHazmats;
			}
		}
		DeliveryOrderHazmatCollection fDeliveryOrderHazmats;

		[ChildEditable(true)]
		public DeliveryOrderBillCollection DeliveryOrderBills
		{
			get
			{
				if (fDeliveryOrderBills == null)
				{
					fDeliveryOrderBills = new DeliveryOrderBillCollection(this);
					RegisterEditableChildObject(fDeliveryOrderBills);
				}
				return fDeliveryOrderBills;
			}
		}
		DeliveryOrderBillCollection fDeliveryOrderBills;

		public new JobDeclaration Parent
		{
			get
			{
				if (B7_Type != CusAddInfoTypeAttribute.Codes.USDeliveryOrderHeader)
				{
					fParent = null;
				}
				else if ((fParent == null) || (fParent.PK != B7_ParentID))
				{
					fParent = Factory.Load<JobDeclaration>(B7_ParentID);
				}
				return fParent;
			}
		}
		JobDeclaration fParent;

		public override void Delete()
		{
			DeliveryOrderLines.DeleteAll();
			DeliveryOrderContainers.DeleteAll();
			DeliveryOrderHazmats.DeleteAll();
			DeliveryOrderBills.DeleteAll();
			base.Delete();
		}

		public override ZString US_PrepaidCollect
		{
			get { return base.US_PrepaidCollect; }
			set
			{
				ZString oldValue = US_PrepaidCollect;
				base.US_PrepaidCollect = value;
				if (!IsCopying && oldValue != US_PrepaidCollect && US_PrepaidCollect != DeliveryOrderPrepaidCollectTypeList.Codes.ThirdParty)
				{
					US_OH_BillToParty = ZGuid.Empty;
				}
			}
		}

		protected bool US_OH_BillToParty_ReadOnly
		{
			get { return US_PrepaidCollect != DeliveryOrderPrepaidCollectTypeList.Codes.ThirdParty; }
		}

		IAddressDetails BranchIAddresDetails
		{
			get
			{
				var branch = Declaration?.Branch;
				IAddressDetails result = branch;
				if (branch?.OrgProxy?.MainAddress is OrgAddress orgProxyAddress)
				{
					result = orgProxyAddress;
				}
				return result;
			}
		}

		public ZString BranchName
		{
			get
			{
				var branch = Declaration?.Branch;
				if (branch != null && branch.OrgProxy != null)
				{
					if (branch?.OrgProxy?.MainAddress is OrgAddress address)
					{
						return address.EffectiveCompanyName;
					}
				}
				return BranchIAddresDetails.CompanyName;
			}
		}

		public ZString BranchAddress1
		{
			get { return BranchIAddresDetails.AddressLine1; }
		}

		public ZString BranchAddress2
		{
			get { return BranchIAddresDetails.AddressLine2; }
		}

		public ZString BranchCity
		{
			get { return BranchIAddresDetails.City; }
		}

		public ZString BranchState
		{
			get { return BranchIAddresDetails.State; }
		}

		public ZString BranchPostCode
		{
			get { return BranchIAddresDetails.PostCode; }
		}

		public ZString BranchCountryCode
		{
			get { return BranchIAddresDetails.Country; }
		}

		public ZString Disclaimer
		{
			get
			{
				JobDeclaration declaration = Parent;
				Guid companyPK = declaration == null ? GlbCompany.CurrentCompany.PK.ToGuid() : declaration.RegistryCompanyPK;
				return USCustomsDataRegistry.Instance.CustomsDeliveryOrderDisclaimer.GetValueWithoutFallback(companyPK, Guid.Empty, Guid.Empty);
			}
		}

		#region ICusAddInfoTypeSupporter Members

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new Customs.Business.FetchStrategies.CusAddInfoTypeSupporterFetchStrategy(this);
			yield return new Customs.Business.FetchStrategies.CusCodeDataTypeSupporterFetchStrategy(this);
		}

		IDictionary<ZString, Type> ICusAddInfoTypeSupporter.GetCusAddInfoTypes()
		{
			var result = new Dictionary<ZString, Type>();
			result.Add(CusAddInfoTypeAttribute.Codes.USDeliveryOrderContainer, typeof(DeliveryOrderContainer));
			result.Add(CusAddInfoTypeAttribute.Codes.USDeliveryOrderHazmat, typeof(DeliveryOrderHazmat));
			result.Add(CusAddInfoTypeAttribute.Codes.USDeliveryOrderLine, typeof(DeliveryOrderLine));
			return result;
		}

		#endregion

		#region Implementation

		T GetEffectiveValueToReturn<T>(T baseValue, string fieldNameInJobDeclaration, string deliveryFieldName)
		where T : IZType
		{
			T result = baseValue;
			if (result.IsEmpty)
			{
				IZType effectiveValue = GetEffectiveValue(deliveryFieldName);
				if (effectiveValue != null)
				{
					result = (T)effectiveValue;
				}
				else
				{
					var dec = Declaration;
					if (dec != null)
					{
						result = (T)dec[fieldNameInJobDeclaration];
					}
				}
			}

			return result;
		}

		T GetEffectiveValueToSet<T>(T valuePassed, string fieldNameInJobDeclaration) where T : IZType
		{
			T result = valuePassed;
			var dec = Declaration;
			if (dec != null && dec[fieldNameInJobDeclaration].Equals(valuePassed))
			{
				result = (T)valuePassed.Default;
			}

			return result;
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new Strategy(this);
		}

		class Strategy : EnterpriseBusinessObjectFetchStrategy
		{
			public Strategy(DeliveryOrderHeader header)
				: base(header)
			{
			}

			protected override void FetchForLoadChildEditableObjectsCore()
			{
				base.FetchForLoadChildEditableObjectsCore();
				Factory.AddFetchHint(typeof(JobDocAddress), BusinessObject.PK);
			}
		}

		protected override NoteTypeCollection NoteTypesCore
		{
			get
			{
				NoteTypeCollection noteTypes = base.NoteTypesCore;
				noteTypes.Add(PredefinedNoteTypes.Instance.DeliveryInstructionsNote);
				return noteTypes;
			}
		}

		protected override bool IsDataEmpty
		{
			get { return base.IsDataEmpty && DeliveryOrderLines.Count == 0 && DeliveryOrderContainers.Count == 0 && DeliveryOrderHazmats.Count == 0 && DeliveryOrderBills.Count == 0; }
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			base.B7_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			B7_Type = CusAddInfoTypeAttribute.Codes.USDeliveryOrderHeader;
		}

		#endregion

		#region Suspend Effective Value

		internal IDisposable SuspendEffectiveValue(string fieldName, IZType decValue)
		{
			EffectiveValueSuspender holder;
			if (!EffectiveValueSuspenders.TryGetValue(fieldName, out holder))
			{
				holder = new EffectiveValueSuspender(this, fieldName) { DeclarationValue = decValue };
				EffectiveValueSuspenders.Add(fieldName, holder);
			}
			return holder;
		}

		IZType GetEffectiveValue(string fieldName)
		{
			EffectiveValueSuspender holder;
			return EffectiveValueSuspenders.TryGetValue(fieldName, out holder) ? holder.DeclarationValue : null;
		}

		Dictionary<string, EffectiveValueSuspender> EffectiveValueSuspenders
		{
			get { return effectiveValueSuspenders ?? (effectiveValueSuspenders = new Dictionary<string, EffectiveValueSuspender>()); }
		}
		Dictionary<string, EffectiveValueSuspender> effectiveValueSuspenders;

		class EffectiveValueSuspender : IDisposable
		{
			public EffectiveValueSuspender(DeliveryOrderHeader header, string fieldName)
			{
				this.header = header;
				this.fieldName = fieldName;
			}

			public IZType DeclarationValue { get; set; }

			readonly DeliveryOrderHeader header;
			readonly string fieldName;

			#region IDisposable Members

			public void Dispose()
			{
				header.EffectiveValueSuspenders.Remove(fieldName);
			}

			#endregion
		}
		#endregion

		#region ICusCodeDataTypeSupporter Members

		IDictionary<ZString, Type> ICusCodeDataTypeSupporter.GetCusCodeDataTypes()
		{
			var result = new Dictionary<ZString, Type>();
			result.Add(DeliveryOrderHeader.Constants.CusCodeDataBill, typeof(DeliveryOrderBill));
			return result;
		}

		#endregion
	}
}
