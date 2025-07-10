using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.ClusterKey;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	[ProvideMetaDataProperty("ShouldPropertiesBeReadOnly", MetaDataTypes.ReadOnly)]
	public class CusLiquidation : AutoCusLiquidation, IClusterKeyWorker
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new class Loader : AutoCusLiquidation.Loader
		{
			public Loader(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public CusLiquidation LoadTop1WithActualLiquidationDate(ZString entryFilerCode, ZString entryNumber)
			{
				ZQuery query = new ZQuery(CusLiquidationSchema.B8_EntryFilerCode, entryFilerCode);
				query.AddToFilter(CusLiquidationSchema.B8_EntryNumber, entryNumber);
				query.AddToFilter(CusLiquidationSchema.B8_LiquidationDate, SQLComparisonOperator.NotEqual, DBNull.Value);
				return Factory.LoadTop1<CusLiquidation>(query);
			}

			protected override Type GetTypeOfBusinessObjectToLoad()
			{
				return typeof(CusLiquidation);
			}
		}

		#region Fetch Strategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new Strategy(this);
		}

		class Strategy : EnterpriseBusinessObjectFetchStrategy
		{
			public Strategy(CusLiquidation liquidation)
				: base(liquidation)
			{
			}

			CusLiquidation Liquidation
			{
				get { return (CusLiquidation)BusinessObject; }
			}

			protected override void FetchForViewCore(TableColumn[] columns)
			{
				base.FetchForViewCore(columns);

				foreach (TableColumn tableColumn in columns)
				{
					switch (tableColumn.ColumnName)
					{
						case CusLiquidation.Schema.B8_BrokerReferenceNo:
							Factory.AddFetchHint(JobDeclarationSchema.JE_DeclarationReference, Liquidation.B8_BrokerReferenceNo);
							break;

						case CusLiquidation.Schema.B8_JE:
							Factory.AddFetchHint(JobDeclarationSchema.PK, Liquidation.B8_JE);
							break;
					}
				}
			}
		}

		#endregion

		public CusLiquidation(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Related Objects

		public JobDeclaration Declaration
		{
			get { return declaration ?? (declaration = Factory.Load<JobDeclaration>(B8_JE)); }
		}
		JobDeclaration declaration;

		public MQEDIMessage Message
		{
			get { return message ?? (message = Factory.LoadTop1<MQEDIMessage>(new ZQuery(EDIMessageSchema.EM_LinkUniqueID, PK))); }
		}
		MQEDIMessage message;

		#endregion

		#region Properties

		[List(nameof(Lookups) + "." + nameof(CusLiquidationLookups.JobDeclarationList))]
		public override ZString B8_BrokerReferenceNo
		{
			get { return base.B8_BrokerReferenceNo; }
			set { base.B8_BrokerReferenceNo = value; }
		}

		[List(nameof(Lookups) + "." + nameof(CusLiquidationLookups.ExtensionSuspensionCodeList))]
		public override ZString B8_ExtensionSuspensionCode
		{
			get { return base.B8_ExtensionSuspensionCode; }
			set { base.B8_ExtensionSuspensionCode = value; }
		}

		[List(nameof(Lookups) + "." + nameof(CusLiquidationLookups.ExtensionSuspensionCodeList))]
		public override ZString B8_ExtensionSuspensionCode2
		{
			get { return base.B8_ExtensionSuspensionCode2; }
			set { base.B8_ExtensionSuspensionCode2 = value; }
		}

		[List(nameof(Lookups) + "." + nameof(CusLiquidationLookups.ExtensionSuspensionCodeList))]
		public override ZString B8_ExtensionSuspensionCode3
		{
			get { return base.B8_ExtensionSuspensionCode3; }
			set { base.B8_ExtensionSuspensionCode3 = value; }
		}

		[List(nameof(Lookups) + "." + nameof(CusLiquidationLookups.ExtensionSuspensionCodeList))]
		public override ZString B8_ExtensionSuspensionCode4
		{
			get { return base.B8_ExtensionSuspensionCode4; }
			set { base.B8_ExtensionSuspensionCode4 = value; }
		}

		public ZString ExtensionSuspensionCodeDescription
		{
			get { return Lookups.ExtensionSuspensionCodeList.GetDescriptionFromCode(B8_ExtensionSuspensionCode); }
		}

		[List(nameof(Lookups) + "." + nameof(CusLiquidationLookups.ChangeLiquidationReasonCodeList))]
		public override ZString B8_ChangeLiquidationReasonCode
		{
			get { return base.B8_ChangeLiquidationReasonCode; }
			set { base.B8_ChangeLiquidationReasonCode = value; }
		}

		[List(nameof(Lookups) + "." + nameof(CusLiquidationLookups.ChangeLiquidationReasonCodeList))]
		public override ZString B8_ChangeLiquidationReasonCode2
		{
			get { return base.B8_ChangeLiquidationReasonCode2; }
			set { base.B8_ChangeLiquidationReasonCode2 = value; }
		}

		[List(nameof(Lookups) + "." + nameof(CusLiquidationLookups.ChangeLiquidationReasonCodeList))]
		public override ZString B8_ChangeLiquidationReasonCode3
		{
			get { return base.B8_ChangeLiquidationReasonCode3; }
			set { base.B8_ChangeLiquidationReasonCode3 = value; }
		}

		[List(nameof(Lookups) + "." + nameof(CusLiquidationLookups.ChangeLiquidationReasonCodeList))]
		public override ZString B8_ChangeLiquidationReasonCode4
		{
			get { return base.B8_ChangeLiquidationReasonCode4; }
			set { base.B8_ChangeLiquidationReasonCode4 = value; }
		}

		public ZString ChangeLiquidationReasonCodeDescription
		{
			get { return Lookups.ChangeLiquidationReasonCodeList.GetDescriptionFromCode(B8_ChangeLiquidationReasonCode); }
		}

		[List(nameof(Lookups) + "." + nameof(CusLiquidationLookups.LiquidationTypeCodeList))]
		public override ZString B8_LiquidationType
		{
			get { return base.B8_LiquidationType; }
			set { base.B8_LiquidationType = value; }
		}

		[List(nameof(Lookups) + "." + nameof(CusLiquidationLookups.EntryTypeList))]
		public override ZString B8_EntryType
		{
			get { return base.B8_EntryType; }
			set { base.B8_EntryType = value; }
		}

		public ZString EntryTypeDescription
		{
			get { return Lookups.EntryTypeList.GetDescriptionFromCode(B8_EntryType); }
		}

		public ZString LiquidationTypeDescription
		{
			get { return Lookups.LiquidationTypeCodeList.GetDescriptionFromCode(B8_LiquidationType); }
		}

		public ZString JobNumber
		{
			get { return Declaration != null ? Declaration.JE_DeclarationReference : ZString.Empty; }
		}

		public ZString ImporterName
		{
			get { return ImporterOfRecord != null ? ImporterOfRecord.OH_FullNameTruncated : ZString.Empty; }
		}

		public ZString ImporterOfRecordNumberForDisplay
		{
			get
			{
				if (SocialSecurityNumberValidator.IsValidSSN(B8_ImportOfRecordNo) && !Env.Security.OrgDetailsViewPersonalInformation.IsAllowed)
				{
					return SocialSecurityNumberValidator.SSNWithMask;
				}
				return B8_ImportOfRecordNo;
			}
		}

		OrgHeader ImporterOfRecord
		{
			get
			{
				if (importer == null && !B8_ImportOfRecordNo.IsEmpty)
				{
					var organizations = new OrganizationLoader(Factory).LoadAllOrganisationByCusCode(B8_ImportOfRecordNo).ToArray();

					if (organizations.Length > 1)
					{
						if (Declaration != null && Declaration.IOR != null && organizations.Contains(Declaration.IOR))
						{
							importer = Declaration.IOR;
						}
					}
					else if (organizations.Length > 0)
					{
						importer = organizations[0];
					}
				}
				return importer;
			}
		}
		OrgHeader importer;

		#endregion

		#region ReadOnly

		protected bool GetShouldPropertiesBeReadOnly(PropertyDescriptor property)
		{
			return true;
		}

		#endregion

		#region These are not persistent properties. Only used by message processors
		internal ZInt NegativeInterestIndicator
		{
			get;
			set;
		}

		internal ZString DistrictPortOfEntry
		{
			get;
			set;
		}
		#endregion

		protected override ZString HumanReadableNameCore => Res.GetString("F0A42D24-106A-4E40-AED7-E9F1D156A492", "Liquidation Notice {0}", B8_EntryNumber);

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			B8_GC = GlbCompany.CurrentCompany.PK;
			B8_SystemCreateDate = ZDateTime.UtcNow;
		}

		#region IClusterKeyWorker

		ZPropertyInfoInt IClusterKeyEntity.ClusterKeyPty => (ZPropertyInfoInt)B8_ClusterKeyInfo;
		Type IClusterKeyWorker.ParentBizObjType => typeof(JobDeclaration);
		ZPropertyInfoGuid IClusterKeyWorker.FkToParentPty => (ZPropertyInfoGuid)B8_JEInfo;
		IEnumerable<ClusterKeyChildInfo> IClusterKeyWorker.ClusterKeyChildList => null;

		#endregion
	}
}
