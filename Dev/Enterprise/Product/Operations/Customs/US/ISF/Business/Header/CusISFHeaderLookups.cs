using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.ISF.Business
{
	public class CusISFHeaderLookups : AutoCusISFHeaderLookups
	{
		public CusISFHeaderLookups(AutoCusISFHeader parent)
			: base(parent)
		{
		}

		protected new CusISFHeader Parent
		{
			get { return (CusISFHeader)base.Parent; }
		}

		public override GlbBranchCollection Branches
		{
			get { return new GlbBranchNotCurrentCompanyRelatedCollection(Factory); }
		}

		#region Organisations

		public ActiveOrgCusCodeCollection ImporterCodes
		{
			get { return new ActiveOrgCusCodeCollection(Factory); }
		}

		public ActiveOrgCusCodeCollection ConsigeeCodes
		{
			get { return new ActiveOrgCusCodeCollection(Factory); }
		}

		public ActiveOrgCusCodeCollection BondNumberOrHolderCodes
		{
			get
			{
				var result = new ActiveOrgCusCodeCollection(Factory);
				var query = new ZQuery(OrgCusCodeSchema.OK_CodeType, new[] { CodeTypeList.Codes.IRS, CodeTypeList.Codes.CBPAssignedNumber, CodeTypeList.Codes.SocialSecurity });
				result.AdditionalFilter = query;
				result.AddNotificationWhenAdditionalFilterNotMetOverride = (errors, bizObj) =>
					{
						var cusCode = (OrgCusCode)bizObj;
						if (cusCode.OK_CodeType != CodeTypeList.Codes.IRS && cusCode.OK_CodeType != CodeTypeList.Codes.CBPAssignedNumber
							&& cusCode.OK_CodeType != CodeTypeList.Codes.SocialSecurity)
						{
							errors.Add(BondNumberOrHolderTypeRestriction);
						}
					};
				return result;
			}
		}
		internal ZString BondNumberOrHolderTypeRestriction
		{
			get { return Res.GetString("F4A1B9FF-1EA9-4A80-8935-BAB12C4FDADF", "Bond number or holder must be EIN/CBN/SSN code."); }
		}

		public OrgHeaderCollection Organisations
		{
			get { return new OrgHeaderCollection(Factory); }
		}

		public override OrgHeaderCollection Importers
		{
			get { return new ConsigneeCollection(Factory); }
		}

		public ConsignorCollection Consignors
		{
			get { return new ConsignorCollection(Factory); }
		}

		#endregion

		#region Importer Code Types

		public ImporterCodeTypeList ImporterCodeTypes
		{
			get
			{
				if (Parent.IsISF10Entry)
				{
					return Factory.GetCachedValue("ImporterCodeTypeListWithoutSCAC", delegate
					{
						ImporterCodeTypeList result = new ImporterCodeTypeList();
						result.RemoveCode(ImporterCodeTypeList.Codes.SCAC);
						return result;
					});
				}
				else
				{
					return Factory.GetCachedValue<ImporterCodeTypeList>();
				}
			}
		}

		#endregion

		#region Consignee Code Types

		public ConsigneeCodeTypeList ConsigneeCodeTypes
		{
			get { return Factory.GetCachedValue<ConsigneeCodeTypeList>(); }
		}

		#endregion

		#region Shipment Types

		public ShipmentTypeList ShipmentTypes
		{
			get { return Factory.GetCachedValue<ShipmentTypeList>(); }
		}

		#endregion

		#region Entry Types

		public SubmissionTypeList EntryTypes
		{
			get
			{
				return Factory.GetCachedValue("EntryTypeListWithout5and6", delegate
				{
					SubmissionTypeList result = new SubmissionTypeList();
					if (!Parent.BF_SystemCreateTimeUtc.IsValid || Parent.BF_SystemCreateTimeUtc >= new ZDateTime(2016, 3, 1))
					{
						result.RemoveCode(SubmissionTypeList.Codes.LateISF10);
						result.RemoveCode(SubmissionTypeList.Codes.LateISF5);
					}
					return result;
				});
			}
		}

		#endregion

		#region Transport Modes

		public TransportModeCodes TransportModes
		{
			get { return Factory.GetCachedValue<TransportModeCodes>(); }
		}

		#endregion

		#region Message Status List

		public MessageStatusList MessageStatusList
		{
			get { return Factory.GetCachedValue<MessageStatusList>(); }
		}

		#endregion

		#region Countries

		public RefCountryCollection Countries
		{
			get { return new RefCountryCollection(Factory); }
		}

		#endregion

		public USCarrierCombinedCollection USCarrierList
		{
			get
			{
				List<ZString> list = new List<ZString>(TransportModes.Count + 1);
				foreach (CodeDescriptionPair pair in TransportModes)
				{
					list.Add(pair.Code);
				}
				list.Add("00");
				return new USCarrierCombinedCollection(Factory, new ZQuery(USCarrierCombinedSchema.UI_ModeOfTransportation, list.ToArray()));
			}
		}

		public NumberOfHarmonizedDigitsList NumberOfHarmonizedDigitsToReportList
		{
			get { return Factory.GetCachedValue<NumberOfHarmonizedDigitsList>(); }
		}

		public MergeStyleList MergeStyleList
		{
			get { return Factory.GetCachedValue<MergeStyleList>(); }
		}

		public YesNoDefaultList YesNoDefaultList
		{
			get { return Factory.GetCachedValue<YesNoDefaultList>(); }
		}

		public GlbStaffCollection Staff
		{
			get { return new GlbStaffCollection(Factory); }
		}

		public ISFBondActivityCodeList BondActivityCodeList
		{
			get
			{
				return Factory.GetCachedValue("ISFBondActivityCodeListWithout99", delegate
				{
					ISFBondActivityCodeList result = new ISFBondActivityCodeList();
					result.RemoveCode(ISFBondActivityCodeList.Codes.ISFBond99);
					return result;
				});
			}
		}

		public ImporterBondTypeList BondTypeList
		{
			get { return Factory.GetCachedValue<ImporterBondTypeList>(); }
		}

		public ActionReasonCodeList ActionReasonCodeList
		{
			get { return Factory.GetCachedValue<ActionReasonCodeList>(); }
		}

		public CodeDescriptionPairList WeightUQList
		{
			get { return Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight); }
		}

		public ShipmentSubTypeList ShipmentSubTypeList
		{
			get { return Factory.GetCachedValue<ShipmentSubTypeList>(); }
		}

		public CodeDescriptionPairList PackingingUnitList
		{
			get { return ShippingOrPackingingUnitList.GetWithPieceType(Factory); }
		}
	}
}
