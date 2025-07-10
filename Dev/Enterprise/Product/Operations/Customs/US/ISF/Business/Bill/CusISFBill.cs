using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.US.ISF;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.ISF.Business
{
	[CodeProperty(CusISFBill.Schema.BB_BillNum), DescriptionProperty("BillTypeDescriptonAndNumber")]
	[DependentBusinessObject(typeof(CusISFHeader), "Bills")]
	[SingleObjectAroundARow]
	public class CusISFBill : AutoCusISFBill,
		Integration.Customs.US.ISF.ICusISFBill,
		IShipmentReferenceID,
		IReferenceData,
		IWorkflowTriggerFieldChangeSource,
		IWorkflowTriggerEventSource
	{
		public CusISFBill(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : AutoCusISFBill.Schema
		{
			public const string BB_BillTypeDescription = "BB_BillTypeDescription";
			public const string BB_CustomsStatusDescription = "BB_CustomsStatusDescription";
		}

		#region Properties

		public ZString BillTypeDescriptonAndNumber
		{
			get { return BB_BillTypeDescription + ":" + BB_BillNum; }
		}

		[List(nameof(Lookups) + "." + nameof(CusISFBillLookups.BillTypes))]
		public override ZString BB_BillType
		{
			get { return base.BB_BillType; }
			set
			{
				ZString oldValue = BB_BillType;
				base.BB_BillType = value;
				if (!IsCopying && oldValue != BB_BillType)
				{
					UpdateRefreshHeaderData(BB_BillNum);
				}
				if (Header != null)
				{
					Header.MarkAsNeedingValidation();
				}
			}
		}

		public bool BB_BillType_ReadOnly
		{
			get { return !BB_CustomsStatus.IsEmpty; }
		}

		public override ZPropertyInfo BB_BillTypeInfo
		{
			get { return GetZPropertyInfo(Schema.BB_BillType, "Reference Type"); }
		}

		public override ZString BB_BillNum
		{
			get { return base.BB_BillNum; }
			set
			{
				ZString oldValue = BB_BillNum;
				base.BB_BillNum = value;
				UpdateRefreshHeaderData(oldValue);
			}
		}

		public bool BB_BillNum_ReadOnly
		{
			get { return !BB_CustomsStatus.IsEmpty; }
		}

		public override ZPropertyInfo BB_BillNumInfo
		{
			get { return GetZPropertyInfo(Schema.BB_BillNum, "Reference Data"); }
		}

		public ZString BB_BillTypeDescription
		{
			get { return Lookups.BillTypes.GetDescriptionFromCode(BB_BillType); }
		}

		public ZPropertyInfo BB_BillTypeDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.BB_BillTypeDescription, "Reference Type Description"); }
		}

		[ReadOnly(true)]
		[List(nameof(Lookups) + "." + nameof(CusISFBillLookups.DispositionCodeList))]
		public override ZString BB_CustomsStatus
		{
			get { return base.BB_CustomsStatus; }
			set
			{
				ZString oldValue = BB_CustomsStatus;
				base.BB_CustomsStatus = value;
				if (!IsCopying && oldValue != BB_CustomsStatus)
				{
					LogManager.AddALogIfNecessary(oldValue, BB_CustomsStatus);
				}
			}
		}

		public ZString BB_CustomsStatusDescription
		{
			get { return Lookups.DispositionCodeList.GetDescriptionFromCode(BB_CustomsStatus) ?? ZString.Empty; }
		}

		public ZPropertyInfo BB_CustomsStatusDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.BB_CustomsStatusDescription); }
		}

		[ReadOnly(true)]
		public override ZDateTime BB_MatchDate
		{
			get { return base.BB_MatchDate; }
			set
			{
				base.BB_MatchDate = value;
				if (BB_FirstMatchedDate.IsEmpty && BB_MatchDate.IsValid)
				{
					BB_FirstMatchedDate = BB_MatchDate;
				}
			}
		}

		[ReadOnly(true)]
		public override ZDateTime BB_FirstMatchedDate
		{
			get { return base.BB_FirstMatchedDate; }
			set { base.BB_FirstMatchedDate = value; }
		}

		#endregion

		internal StatusLogManager LogManager
		{
			get { return fLogManager ?? (fLogManager = new StatusLogManager(Logs)); }
		}
		StatusLogManager fLogManager;

		#region Flags

		public bool IsCBPEntryNumber
		{
			get { return BB_BillType == BillTypeList.Codes.USCBPEntryNumber; }
		}

		public bool IsHouseBillOfLading
		{
			get { return BB_BillType == BillTypeList.Codes.HouseBillOfLading; }
		}

		public bool IsMasterBillOfLading
		{
			get { return BB_BillType == BillTypeList.Codes.MasterBillOfLading; }
		}

		public bool IsLowestBill
		{
			get { return IsOceanBillOfLading || IsHouseBillOfLading; }
		}

		public bool IsOceanBillOfLading
		{
			get { return BB_BillType == BillTypeList.Codes.OceanBillOfLading; }
		}

		public bool IsSuretyCode
		{
			get { return BB_BillType == BillTypeList.Codes.SuretyCode; }
		}

		public bool IsCarnetIssuingCountryCodeAndCarnetNumber
		{
			get { return BB_BillType == BillTypeList.Codes.CarnetIssuingCountryCodeAndCarnetNumber; }
		}

		public bool IsBondReferenceNumber
		{
			get { return BB_BillType == BillTypeList.Codes.BondReferenceNumber; }
		}

		public bool IsFullNameOfISFImporter
		{
			get { return BB_BillType == BillTypeList.Codes.FullNameOfISFImporter; }
		}

		#endregion

		public CusISFHeader Header
		{
			get { return Factory.Load<CusISFHeader>(BB_BF); }
		}

		[ReadOnly(true)]
		[List(nameof(Lookups) + "." + nameof(CusISFBillLookups.Declarations))]
		[ResourceStringData("Enterprise.Customs.US.ISF.Business.CusISFBill|DeclarationJobNumber", Caption = "Declaration Job Number")]
		public ZString DeclarationJobNumber
		{
			get { return ZString.Join(",", JobDeclarationNumbers.ToArray()); }
		}

		public List<ZString> JobDeclarationNumbers
		{
			get
			{
				if (fJobDeclarationNumbers == null)
				{
					fJobDeclarationNumbers = GetJobDeclarationNumbers();
				}
				return fJobDeclarationNumbers;
			}
		}

		List<ZString> fJobDeclarationNumbers;

		List<ZString> GetJobDeclarationNumbers()
		{
			const int SCAClength = 4;
			const string BillIssuerSCACStr = "UI_NKBillIssuerSCAC=";
			var declarations = new List<ZString>();

			if (IsOceanBillOfLading || IsHouseBillOfLading)
			{
				if (BB_BillNum.Length < 5)
				{
					return declarations;
				}

				var billIssuerSCACStr = BillIssuerSCACStr + BB_BillNum.Left(SCAClength);

				var billQuery = new ZDBOnlySubQuery(typeof(Bill), CusDecHouseBillSchema.CU_JE, JobDeclarationSchema.PK);
				billQuery.AddToFilter(CusDecHouseBillSchema.CU_BillNum, SQLComparisonOperator.Equal, BB_BillNum.Substring(SCAClength));
				billQuery.AddToFilter(CusDecHouseBillSchema.CU_AddInfo, SQLComparisonOperator.Contains, billIssuerSCACStr.Trim());
				if (IsOceanBillOfLading)
				{
					billQuery.AddToFilter(CusDecHouseBillSchema.CU_BillType, SQLComparisonOperator.Equal, Customs.Business.BillTypeList.Codes.MasterBill);
				}
				else
				{
					billQuery.AddToFilter(CusDecHouseBillSchema.CU_BillType, SQLComparisonOperator.Equal, Customs.Business.BillTypeList.Codes.HouseBill);
				}

				var declarationQuery = new ZDBOnlyQuery(typeof(JobDeclaration));
				if (Header.BF_SystemCreateTimeUtc.IsValid)
				{
					var fromDate = Header.BF_SystemCreateTimeUtc.AddMonths(-6);
					var toDate = Header.BF_SystemCreateTimeUtc.AddMonths(6);
					declarationQuery.AddToFilter(JobDeclarationSchema.JE_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThan, fromDate);
					declarationQuery.AddToFilter(JobDeclarationSchema.JE_SystemCreateTimeUtc, SQLComparisonOperator.LessThan, toDate);
				}
				else
				{
					declarationQuery.AddToFilter(JobDeclarationSchema.JE_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThan, ZDateTime.UtcToday.AddMonths(-6));
				}
				declarationQuery.AddToFilter(JobDeclarationSchema.JE_MessageType, JobMessageTypeList.Codes.Import);
				declarationQuery.AddToFilter(JobDeclarationSchema.JE_TransportMode, Core.Constants.TransportModes.Sea);
				declarationQuery.AddSubQuery(billQuery, JoinCondition.And);

				declarations = Factory.Load<JobDeclaration>(declarationQuery).Select(jobDeclaration => jobDeclaration.JE_DeclarationReference).Distinct().ToList();
			}
			return declarations;
		}

		#region Logging

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		#endregion

		public override void OnSaving()
		{
			base.OnSaving();
			if (BB_BillNum.IsEmpty)
			{
				Delete();
			}
		}

		public override void Delete()
		{
			CusISFHeader header = this.Header;
			base.Delete();
			if (header != null)
			{
				header.RefreshBinding();
			}
		}

		#region Implementation

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		void UpdateRefreshHeaderData(ZString oldValue)
		{
			if (!IsCopying)
			{
				CusISFHeader header = this.Header;
				if (header != null)
				{
					ZPropertyInfo info = null;
					switch (BB_BillType)
					{
						case BillTypeList.Codes.HouseBillOfLading:
							info = header.BF_HouseBillInfo;
							break;
						case BillTypeList.Codes.OceanBillOfLading:
							info = header.BF_OceanBillInfo;
							break;
						case BillTypeList.Codes.SuretyCode:
							info = header.BF_SuretyCodeInfo;
							break;
						case BillTypeList.Codes.USCBPEntryNumber:
							info = header.BF_EntryNumberInfo;
							break;
						case BillTypeList.Codes.MasterBillOfLading:
							info = header.BF_MasterBillInfo;
							break;
					}

					if (info != null)
					{
						if (oldValue != BB_BillNum)
						{
							info.RefreshBinding(oldValue);
						}
						else
						{
							info.RefreshBinding();
						}
						header.RefreshBinding();
					}
				}
			}
		}
		#endregion

		#region IShipmentReferenceID Members

		ZString IShipmentReferenceID.CodeQualifier
		{
			get { return ShipmentReferenceIDCodeQualifier; }
		}

		ZString ShipmentReferenceIDCodeQualifier
		{
			get
			{
				if (shipmentReferenceIDCodeQualifierCached == null)
				{
					shipmentReferenceIDCodeQualifierCached = new CachedProperty<ZString>(Factory, delegate
					{
						switch (BB_BillType)
						{
							case BillTypeList.Codes.OceanBillOfLading:
								return ShipmentReferenceIdentifierTypeList.Codes.OceanBillOfLading;
							case BillTypeList.Codes.HouseBillOfLading:
								return ShipmentReferenceIdentifierTypeList.Codes.HouseBillOfLading;
							default:
								return ZString.Empty;
						}
					});
				}
				return shipmentReferenceIDCodeQualifierCached.Value;
			}
		}
		CachedProperty<ZString> shipmentReferenceIDCodeQualifierCached;

		ZString IShipmentReferenceID.ShipmentReferenceIdentifier
		{
			get { return ShipmentReferenceIDCodeQualifier.IsEmpty ? ZString.Empty : BB_BillNum; }
		}

		#endregion

		#region IReferenceData Members

		ZString IReferenceData.CodeQualifier
		{
			get { return ReferenceDataCodeQualifier; }
		}

		ZString ReferenceDataCodeQualifier
		{
			get
			{
				if (referenceDataCodeQualifierCached == null)
				{
					referenceDataCodeQualifierCached = new CachedProperty<ZString>(Factory, delegate
					{
						switch (BB_BillType)
						{
							case BillTypeList.Codes.MasterBillOfLading:
								return ReferenceDataCodeList.Codes.MasterBillOfLading;
							case BillTypeList.Codes.SuretyCode:
								return ReferenceDataCodeList.Codes.SuretyCode;
							case BillTypeList.Codes.USCBPEntryNumber:
								return ReferenceDataCodeList.Codes.USCBPEntryNumber;
							case BillTypeList.Codes.CarnetIssuingCountryCodeAndCarnetNumber:
								return ReferenceDataCodeList.Codes.CarnetIssuingCountryCodeAndCarnetNumber;
							case BillTypeList.Codes.BondReferenceNumber:
								return ReferenceDataCodeList.Codes.BondReferenceNumber;
							case BillTypeList.Codes.UserDefinedReferenceNumber:
								return ReferenceDataCodeList.Codes.UserDefinedReferenceNumber;
							case BillTypeList.Codes.FullNameOfISFImporter:
								return ReferenceDataCodeList.Codes.FullNameOfISFImporter;
							default:
								return ZString.Empty;
						}
					});
				}
				return referenceDataCodeQualifierCached.Value;
			}
		}
		CachedProperty<ZString> referenceDataCodeQualifierCached;

		ZString IReferenceData.ReferenceData
		{
			get
			{
				var result = ZString.Empty;
				if (!ReferenceDataCodeQualifier.IsEmpty)
				{
					result = ReferenceDataCodeQualifier == ReferenceDataCodeList.Codes.FullNameOfISFImporter ? ACEOceanManifestIllegalCharacters.ReplaceIllegalCharacters(BB_BillNum) : BB_BillNum;
				}
				return result;
			}
		}

		#endregion

		#region IWorkflowTriggerFieldChangeSource Members

		IReadOnlyList<IWorkflowProvider> IWorkflowTriggerFieldChangeSource.ParentWorkflowProviders
		{
			get { return new IWorkflowProvider[] { Header }; }
		}

		#endregion

		#region IWorkflowTriggerEventSource Members

		IGlbCompany IWorkflowTriggerEventSource.JobHeaderCompany
		{
			get { return Header.Branch.Company; }
		}

		IReadOnlyList<IWorkflowProviderCore> IWorkflowTriggerEventSource.ParentWorkflowProviders
		{
			get
			{
				var result = new List<IWorkflowProviderCore>();
				var header = Header;
				if (header != null)
				{
					result.Add(header);
				}
				return result;
			}
		}

		#endregion
	}
}
