using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Packing.Business
{
	[CodeProperty(PkgPalletTransactionSchema.Constants.KTR_PaperDocketID)]
	[DescriptionProperty("Description")]
	public class PkgPalletTransaction :
		AutoPkgPalletTransaction,
		IDocManagerSupport,
		IAdditionalReferenceNumberTypeProvider,
		IDocAddresses
	{
		public PkgPalletTransaction(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			KTR_Status = PalletTransactionStatusList.Codes.Held;
			KTR_Quantity = 1;
			KTR_EffectiveDateTime = ZDateTimeOffset.Now;
			KTR_ParentTableCode = DtbBookingConfirmationSchema.Constants.Prefix;
		}

		#endregion

		#region Properties

		#region ActionType

		[List("Lookups.ActionTypes")]
		[ResourceStringData("PkgPalletTransaction|ActionType", Caption = "Action Type")]
		public ZString ActionType
		{
			get;
			set;
		}

		#endregion

		#region KTR_PaperDocketID

		[MaxLength(24)]
		[ReadOnlyMember(nameof(PaperDocketID_Readonly))]
		public override ZString KTR_PaperDocketID
		{
			get { return base.KTR_PaperDocketID; }
			set { base.KTR_PaperDocketID = value; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Code used via reflection in unit test")]
		bool PaperDocketID_Readonly
		{
			get { return !IsTransfer; }
		}

		#endregion

		[ReadOnly(true)]
		public override ZString KTR_TransactionID
		{
			get { return base.KTR_TransactionID; }
			set { base.KTR_TransactionID = value; }
		}

		protected override ZString HumanReadableNameCore
		{
			get { return Res.GetString("63b9871e-6ac4-4204-ab1e-7a2cefb52008", "Pallet Transaction {0}", KTR_PaperDocketID); }
		}

		[ResourceStringData("PkgPalletTransaction|Description", Caption = "Description")]
		public ZString Description
		{
			get { return Res.GetString("809552d2-2125-4524-9cec-d9be428cadd5", "Pallet Transaction - {0} - {1} - {2}", KTR_PalletType, KTR_EffectiveDateTime, KTR_TransactionType); }
		}

		[List("Lookups.StatusList")]
		public override ZString KTR_Status
		{
			get { return base.KTR_Status; }
			set { base.KTR_Status = value; }
		}

		[List("Lookups.PalletTypes")]
		public override ZString KTR_PalletType
		{
			get { return base.KTR_PalletType; }
			set
			{
				base.KTR_PalletType = value;
				if (PalletDefinition != null)
				{
					KTR_EquipmentCode = PalletDefinition.EquipmentCode;
				}
				SetTradingAccountNumbers();
			}
		}

		void SetTradingAccountNumbers()
		{
			if (IsTransfer)
			{
				KTR_TransferFromAccountNumber = DefaultTradingAccountNumberFromAddress(TransferFrom);
				KTR_TransferToAccountNumber = DefaultTradingAccountNumberFromAddress(TransferTo);
			}
		}

		string DefaultTradingAccountNumberFromAddress(JobDocAddress address)
		{
			string result = "";
			if (!address.E2_AddressOverride && address.Organisation != null)
			{
				result = GetTradingAccountNumber(address.Organisation);
			}
			return result;
		}

		[List("Lookups.ParentList")]
		public override ZGuid KTR_ParentID
		{
			get { return base.KTR_ParentID; }
			set
			{
				if (!value.IsEmpty)
				{
					if (PossibleParents != null)
					{
						var parent = PossibleParents.FirstOrDefault(x => x.PK == value);
						if (parent != null)
						{
							KTR_ParentTableCode = parent.TablePrefix;
						}
						else
						{
							return;
						}
					}
				}
				else
				{
					return;
				}

				base.KTR_ParentID = value;
			}
		}

		[List("Lookups.TransactionTypes")]
		public override ZString KTR_TransactionType
		{
			get { return base.KTR_TransactionType; }
			set { base.KTR_TransactionType = value; }
		}

		[List("Lookups.TransferTypes")]
		public override ZString KTR_TransferType
		{
			get { return base.KTR_TransferType; }
			set { base.KTR_TransferType = value; }
		}

		[ResourceStringData("PkgPalletTransaction|StatusDescription", Caption = "Status Desc.")]
		public ZString StatusDescription
		{
			get { return Lookups.StatusList.GetDescriptionFromCode(KTR_Status); }
		}

		[ResourceStringData("PkgPalletTransaction|PalletTypeDescription", Caption = "Pallet Type Desc.")]
		public ZString PalletTypeDescription
		{
			get { return Lookups.PalletTypes.GetDescriptionFromCode(KTR_PalletType); }
		}

		[ResourceStringData("PkgPalletTransaction|TransactionTypeDescription", Caption = "Transaction Type Desc.")]
		public ZString TransactionTypeDescription
		{
			get { return Lookups.TransactionTypes.GetDescriptionFromCode(KTR_TransactionType); }
		}

		[ResourceStringData("PkgPalletTransaction|RelatedJobCode", Caption = "Job Reference")]
		public ZString RelatedJobCode
		{
			get { return RelatedJob != null ? RelatedJob.GetJobDescription() : ZString.Empty; }
		}

		ZString GetTradingAccountNumber(OrgHeader org)
		{
			var result = ZString.Empty;
			if (org != null && PalletDefinition != null)
			{
				var providerCode = PalletDefinition.ProviderCode;
				result = org.CustomsCodes.GetCustomsRegNo(providerCode);
				if (result.IsEmpty)
				{
					result = org.CustomsCodes.GetCustomsRegNo(providerCode, ZString.Empty);
				}
			}

			return result;
		}

		[ResourceStringData("PkgPalletTransaction|PaperDocketID", Caption = "Paper Docket ID")]
		public ZString PaperDocketID
		{
			get
			{
				var refNum = AdditionalReferenceNumbers.GetFirstReferenceNumberByType(PaperDocketIDType);
				return refNum != null ? refNum.CE_EntryNum : ZString.Empty;
			}
		}

		[ReadOnlyMember(nameof(TransferFromAccountNumber_ReadOnly))]
		[ResourceStringData("PkgPalletTransaction|KTR_TransferFromAccountNumber", Caption = "Trading Account")]
		public override ZString KTR_TransferFromAccountNumber
		{
			get { return base.KTR_TransferFromAccountNumber; }
			set { base.KTR_TransferFromAccountNumber = value; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Code used via reflection in unit test")]
		bool TransferFromAccountNumber_ReadOnly
		{
			get { return !TransferFrom.E2_AddressOverride; }
		}

		[ReadOnlyMember(nameof(TransferToAccountNumber_ReadOnly))]
		[ResourceStringData("PkgPalletTransaction|KTR_TransferToAccountNumber", Caption = "Trading Account")]
		public override ZString KTR_TransferToAccountNumber
		{
			get { return base.KTR_TransferToAccountNumber; }
			set { base.KTR_TransferToAccountNumber = value; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Code used via reflection in unit test")]
		bool TransferToAccountNumber_ReadOnly
		{
			get { return !TransferTo.E2_AddressOverride; }
		}

		public bool IsTransfer
		{
			get { return KTR_TransactionType == PalletTransactionTypeList.Codes.Transfer; }
		}

		#endregion

		#region Related Objects

		PalletType[] PalletDefinitions
		{
			get { return palletDefinitions ?? (palletDefinitions = PackingRegistry.Instance.PalletTypes.Value.Types.ToArray<PalletType>()); }
		}

		PalletType[] palletDefinitions;

		public PalletType PalletDefinition
		{
			get { return PalletDefinitions.FirstOrDefault(x => x.Code == KTR_PalletType); }
		}

		#region RelatedJob

		public IPalletTransactionParent RelatedJob
		{
			get { return !KTR_ParentID.IsEmpty && !KTR_ParentTableCode.IsEmpty ? Factory.Load(KTR_ParentTableCode, KTR_ParentID) as IPalletTransactionParent : null; }
			set
			{
				if (value != null)
				{
					KTR_ParentID = value.PK;
					KTR_ParentTableCode = value.TablePrefix;

					foreach (var jobReference in value.JobReferences)
					{
						var reference = AdditionalReferenceNumbers.AddNew();
						reference.CE_EntryType = JobReferenceType;
						reference.CE_EntryNum = jobReference;
					}
					var toAddress = value.TransferTo(KTR_TransferType);
					var fromAddress = value.TransferFrom(KTR_TransferType);
					SetAddresses(TransferTo, toAddress);
					SetAddresses(TransferFrom, fromAddress);
				}
				else
				{
					KTR_ParentID = ZGuid.Empty;
					KTR_ParentTableCode = ZString.Empty;
				}
			}
		}

		void SetAddresses(JobDocAddress address, IDocAddress copyFrom)
		{
			if (address != null && copyFrom != null)
			{
				if (!address.E2_AddressOverride)
				{
					address.E2_OA_Address = copyFrom.E2_OA_Address;
				}
				else
				{
					CopyOverriddenDocAddress(copyFrom, address);
				}
			}
		}

		void CopyOverriddenDocAddress(IDocAddress copyFrom, JobDocAddress copyTo)
		{
			copyTo.E2_AddressOverride = true;
			copyTo.E2_CompanyName = copyFrom.E2_CompanyNameTruncated;
			copyTo.E2_Address1 = copyFrom.E2_Address1;
			copyTo.E2_Address2 = copyFrom.E2_Address2;
			copyTo.E2_City = copyFrom.E2_City;
			copyTo.E2_State = copyFrom.E2_State;
			copyTo.E2_Postcode = copyFrom.E2_Postcode;
			copyTo.E2_Phone = copyFrom.E2_Phone;
			copyTo.E2_Fax = copyFrom.E2_Fax;
		}

		#endregion

		[BusinessObjectTestExclude] // Allowed to be null - we are not binding to it
		public IEnumerable<IPalletTransactionParent> PossibleParents { get; set; }

		public BusinessObject Context { get; set; }

		[ChildEditable]
		public Enterprise.Integration.Customs.ICusEntryNumAdditionalReferenceCollection AdditionalReferenceNumbers
		{
			get
			{
				if (additionalReferenceNumbers == null)
				{
					var provider = ObjectFactory.New<Enterprise.Integration.Customs.ICusEntryNumAdditionalReferenceCollectionProvider>();
					additionalReferenceNumbers = provider.GetCollection(this);

					var additionalReferenceNumbersBusinessObjectCollection = additionalReferenceNumbers as BusinessObjectCollection;
					if (additionalReferenceNumbersBusinessObjectCollection != null)
					{
						additionalReferenceNumbersBusinessObjectCollection.Load();
					}

					RegisterEditableChildObject(additionalReferenceNumbers);
				}

				return additionalReferenceNumbers;
			}
		}

		Enterprise.Integration.Customs.ICusEntryNumAdditionalReferenceCollection additionalReferenceNumbers;

		#endregion

		#region Saving

		public override void Delete()
		{
			base.Delete();
			AdditionalReferenceNumbers.Delete();
		}

		public override void OnSaving()
		{
			base.OnSaving();

			if (KTR_TransactionID.IsEmpty)
			{
				KTR_TransactionID = Env.NumberFountains.PalletTransactionID.GetNextFormatted(Factory);
			}
		}

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		#endregion

		#region IDocManagerSupport Members

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get { return new DocManagerInfo(this, Constants.DocManagerCodes.PalletTransaction); }
		}

		#endregion

		#region IAdditionalReferenceNumberTypeProvider Members

		CodeDescriptionPairList IAdditionalReferenceNumberTypeProvider.GetAdditionalReferenceNumberTypeList(ZString category, ZString countryCode)
		{
			var result = new CodeDescriptionPairList();

			result.Add(new ReferenceNumberType(PaperDocketIDType, Res.GetString("4330a1b6-4865-4245-bcab-5d98f800d021", "Paper Docket ID"), true));
			result.Add(new ReferenceNumberType(JobReferenceType, Res.GetString("70d09753-f5c8-4317-b1b0-353c68f80678", "Job Reference"), false));

			return result;
		}

		const string JobReferenceType = "REF";
		const string PaperDocketIDType = "PAP";

		#endregion

		#region Addresses

		#region TransferFromAddress

		public JobDocAddress TransferFrom
		{
			get
			{
				if (transferFrom == null || transferFrom.IsDeleted)
				{
					transferFrom = CreateJobDocAddress(DocAddressType.LocalCartageExporter);
				}
				return transferFrom;
			}
		}

		JobDocAddress transferFrom;

		#endregion

		#region TransferTo

		public JobDocAddress TransferTo
		{
			get
			{
				if (transferTo == null || transferTo.IsDeleted)
				{
					transferTo = CreateJobDocAddress(DocAddressType.LocalCartageImporter);
				}
				return transferTo;
			}
		}
		JobDocAddress transferTo;

		#endregion

		JobDocAddress CreateJobDocAddress(DocAddressType addressType)
		{
			var iDocAddresses = ((IDocAddresses)this);
			var requirement = iDocAddresses.GetDocAddressRequirement(addressType);
			var address = iDocAddresses.DocAddresses.FindOrCreateWithRequirement(requirement);
			address.MakePersistentEvenIfEmpty();
			return address;
		}

		#endregion

		#region IDocAddresses

		JobDocAddressDependentCollection IDocAddresses.DocAddresses
		{
			get
			{
				if (docAddresses == null)
				{
					docAddresses = new JobDocAddressDependentCollection(this);
					docAddresses.Load();
					RegisterEditableChildObject(docAddresses);
				}

				return docAddresses;
			}
		}

		JobDocAddressDependentCollection docAddresses;

		IReadOnlyList<DocAddressType> IDocAddresses.SupportedAddressTypes
		{
			get { return new[] { DocAddressType.LocalCartageImporter, DocAddressType.LocalCartageExporter }; }
		}

		ZValidation IDocAddresses.PiggyBackedDocAddressValidation(JobDocAddress addressToValidate)
		{
			return null;
		}

		SecurityCheckpoint IDocAddresses.GetCanOverrideCheckpoint(JobDocAddress docAddress)
		{
			return Env.Security.None;
		}

		JobDocAddressRequirement IDocAddresses.GetDocAddressRequirement(DocAddressType addressType)
		{
			JobDocAddressRequirement result = null;

			if (addressType == DocAddressType.LocalCartageExporter)
			{
				result = new JobDocAddressRequirement(addressType);
			}
			else if (addressType == DocAddressType.LocalCartageImporter)
			{
				result = new JobDocAddressRequirement(addressType);
			}
			else
			{
				throw new NotImplementedException("DocAddress Type is not implemented.");
			}

			result.IsMandatory = true;

			return result;
		}

		void IDocAddresses.DocAddressChanged(JobDocAddress docAddress)
		{
			if (!docAddress.E2_AddressOverride
				&& (docAddress.E2_AddressType == DocAddressTypes.Codes.LocalCartageExporter || docAddress.E2_AddressType == DocAddressTypes.Codes.LocalCartageImporter))
			{
				SetTradingAccountNumbers();
			}
		}

		void IDocAddresses.OrgAddressBeforeChange(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OnBeforeDocAddressDeleted(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.AnyAddressFieldBeforeChange(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OrgHeaderAfterChange(JobDocAddress docAddress)
		{
		}

		bool IDocAddresses.CanDeleteAddress(JobDocAddress docAddress)
		{
			return false;
		}

		OrgHeaderCollection IDocAddresses.GetOrgHeaderList(DocAddressType addressType)
		{
			return null;
		}

		#endregion
	}
}
