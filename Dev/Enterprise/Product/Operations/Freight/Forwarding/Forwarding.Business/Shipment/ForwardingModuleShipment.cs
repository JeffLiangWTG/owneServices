using System;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.DocumentEngineCore.DocumentSupporter.DocumentSupporterHelper;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ForwardingModuleShipment : ForwardingShipment, ITopLevelBusinessEntityForDocSup
	{
		#region Schema

		public new class Schema : ForwardingShipment.Schema
		{
			public const string EntryNumberStatus = "EntryNumberStatus";
			public const string JS_GenericOrderNumbers = "JS_GenericOrderNumbers";
			public const string JS_JH_Branch = "JS_JH_Branch";
			public const string JS_JH_Dept = "JS_JH_Dept";
			public const string JS_Calc_ImportManifestStatus = "JS_Calc_ImportManifestStatus";
			public const string JS_Calc_PossibleOversize = "JS_Calc_PossibleOversize";
		}

		#endregion

		public ForwardingModuleShipment(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region JS_GenericOrderNumber

		public ZString JS_GenericOrderNumbers
		{
			get
			{
				var sortedOrders = LegacyOrders;
				var orders = new ZStringBuilder();

				foreach (IAttachedOrder order in sortedOrders.Take(3))
				{
					orders.Append(order.JobNo);
				}
				var result = orders.ToStringWithDelimiterBetweenAppends(", ");

				return (sortedOrders.Count > 3) ? result + "..." : result;
			}
		}

		public ZPropertyInfo JS_GenericOrderNumbersInfo
		{
			get { return GetZPropertyInfo(Schema.JS_GenericOrderNumbers); }
		}

		#endregion

		#region Customs Broker

		public ZString CustomsBroker
		{
			get
			{
				if (fCustomsBroker == null)
				{
					fCustomsBroker = GetCustomsBroker();
				}
				return fCustomsBroker;
			}
		}
		string fCustomsBroker;

		public ZPropertyInfo CustomsBrokerInfo
		{
			get { return GetZPropertyInfo(nameof(CustomsBroker), "Customs Broker"); }
		}

		ZString GetCustomsBroker()
		{
			var result = new ZStringBuilder();

			foreach (Enterprise.Integration.Customs.IBaseJobDeclaration declaration in Declarations)
			{
				result.Append((ZString)declaration[JobDeclarationSchema.JE_GS_NKCusAgent.Name]);
				if (declaration.CompanyPK == GlbCompany.CurrentCompany.PK)
				{
					return (ZString)declaration[JobDeclarationSchema.JE_GS_NKCusAgent.Name];
				}
			}

			return result.ToStringWithDelimiterBetweenAppends(",");
		}

		#endregion

		#region Customs Entry Numbers

		[MaxLength(CusEntryNumber.Schema.CE_EntryNumMaxLength)]
		public ZString EntryNum
		{
			get
			{
				if (fEntryNum == null)
				{
					fEntryNum = GetEntryNum();
				}

				return fEntryNum;
			}
		}
		string fEntryNum;

		public ZPropertyInfo EntryNumInfo
		{
			get { return GetZPropertyInfo(nameof(EntryNum)); }
		}

		[MaxLength(CusEntryNumber.Schema.CE_EntryStatusMaxLength)]
		public ZString EntryNumberStatus
		{
			get
			{
				if (fEntryNumberStatus == null)
				{
					fEntryNumberStatus = GetEntryNumStatus();
				}
				return fEntryNumberStatus;
			}
		}
		string fEntryNumberStatus;

		public ZPropertyInfo EntryNumberStatusInfo
		{
			get { return GetZPropertyInfo(nameof(EntryNumberStatus)); }
		}

		[MaxLength(4)]
		public ZString EntryNumberType
		{
			get
			{
				if (fEntryNumberType == null)
				{
					fEntryNumberType = GetEntryNumberType();
				}
				return fEntryNumberType;
			}
		}
		string fEntryNumberType;

		public ZPropertyInfo EntryNumberTypeInfo
		{
			get { return GetZPropertyInfo(nameof(EntryNumberType)); }
		}

		string GetEntryNumberType()
		{
			return ShipmentDomainService.GetInstance(Factory).ModuleShipmentCollection.GetEntryType(this);
		}

		string GetEntryNumStatus()
		{
			return ShipmentDomainService.GetInstance(Factory).ModuleShipmentCollection.GetEntryStatus(this);
		}

		string GetEntryNum()
		{
			return ShipmentDomainService.GetInstance(Factory).ModuleShipmentCollection.GetEntryNum(this);
		}

		public BusinessObject CusEntryNumBizO
		{
			get
			{
				if (!EntryNumberChecked && fCusEntryNum == null)
				{
					ZQuery shipmentFilter = new ZQuery(CusEntryNumSchema.CE_ParentID, PK);

					ZDBOnlySubQuery declarationQuery = new ZDBOnlySubQuery(typeof(Enterprise.Integration.Customs.IBaseJobDeclaration), JobDeclarationSchema.PK);
					declarationQuery.AddToFilter(new ZQuery(JobDeclarationSchema.JE_JS, PK));
					ZDBOnlyQuery entryNumQuery = new ZDBOnlyQuery(typeof(CusEntryNumber));
					entryNumQuery.AddSubQuery(CusEntryNumSchema.CE_ParentID, declarationQuery, JoinCondition.And);
					entryNumQuery.AddToFilter(shipmentFilter, JoinCondition.Or);

					entryNumQuery.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

					fCusEntryNum = Factory.LoadTop1(typeof(CusEntryNumber), entryNumQuery);

					EntryNumberChecked = true;
				}
				return fCusEntryNum;
			}
		}
		BusinessObject fCusEntryNum;
		bool EntryNumberChecked;

		#endregion

		#region JS_JH_Branch

		public ZString JS_JH_Branch
		{
			get { return ShipmentJobHeader != null ? ShipmentJobHeader.Branch.GB_Code : ZString.Empty; }
		}

		public ZPropertyInfo JS_JH_BranchInfo
		{
			get { return GetZPropertyInfo(Schema.JS_JH_Branch); }
		}

		#endregion

		#region JS_JH_Dept

		public ZString JS_JH_Dept
		{
			get { return ShipmentJobHeader != null ? ShipmentJobHeader.Department.GE_Code : ZString.Empty; }
		}

		public ZPropertyInfo JS_JH_DeptInfo
		{
			get { return GetZPropertyInfo(Schema.JS_JH_Dept); }
		}

		#endregion

		#region Calc Fields for Import Manifest Status

		public const string EntryTypeForImportManifestStatus = "IMS";
		public const string WaitingStatus = "WAIT";
		public static string WaitingDescription
		{
			get { return Res.GetString("b0fc22a5-432d-490c-a144-06b551d52d1c", "Waiting for response"); }
		}
		public const string ImpededStatus = "IPD";
		public static string ImpededDescription
		{
			get { return Res.GetString("d7ceafff-b304-4876-8981-83dc1b8e8b0c", "Impeded"); }
		}
		public const string ClearStatus = "CLD";
		public static string ClearDescription
		{
			get { return Res.GetString("286fb1d8-ebea-4d3c-872b-f07da964e743", "Cleared"); }
		}
		public const string ErrorStatus = "ERR";
		public static string ErrorDescription
		{
			get { return Res.GetString("d82bad6c-2e68-42a9-a1c1-3f716bfc5e3c", "Has errors."); }
		}
		public const string RejectedStatus = "REJ";
		public static string RejectedDescription
		{
			get { return Res.GetString("c37bb413-d6f9-497e-ac06-2e41c6673c98", "Rejected."); }
		}
		public const string AcknowledgedStatus = "ACK";
		public static string AcknowledgedDescription
		{
			get { return Res.GetString("1cd99784-6466-4bdd-b34e-eacec530cbb6", "Acknowledged."); }
		}
		public const string InProgressStatus = "PRG";
		public static string InProgressDescription
		{
			get { return Res.GetString("17a404e0-93dd-4e4c-8d6e-091e3488928b", "In progress."); }
		}

		protected ZString AirCargoSpecialStatus
		{
			get
			{
				ZString result = ZString.Empty;
				if (HouseBill != null)
				{
					if (HouseBill.GetType() == ObjectFactory.GetType<Enterprise.Integration.Customs.AU.ICusHAWB>() && (ZBool)HouseBill[ZArchitecture.Schema.CusHAWBSchema.CS_IsResponsePending.Name])
					{
						result = WaitingStatus + ":" + ImpededDescription;
					}
				}
				return result;
			}
		}

		protected ZString SeaCargoSpecialStatus
		{
			get
			{
				ZString result = ZString.Empty;
				if (HouseBill != null)
				{
					if (HouseBill.GetType() == ObjectFactory.GetType<Enterprise.Integration.Customs.AU.ICusSCAHouse>())
					{
						ZString seaCargoInternalStatus = (ZString)HouseBill[CusSCAHouseSchema.CA_ShipmentStatus.Name];
						if (seaCargoInternalStatus == AcknowledgedStatus)
						{
							result = AcknowledgedDescription;
						}
						else if (seaCargoInternalStatus == ErrorStatus)
						{
							result = ErrorDescription;
						}
						else if (seaCargoInternalStatus == RejectedStatus)
						{
							result = RejectedDescription;
						}
						else if (seaCargoInternalStatus == InProgressStatus)
						{
							result = InProgressDescription;
						}
						else
						{
							if (seaCargoInternalStatus != "")
							{
								result = Res.GetString("85d676eb-3f2c-442c-b56b-078214e830d8", "U: {0}", seaCargoInternalStatus);
							}
							else
							{
								result = Res.GetString("f7a6d3bb-3133-4968-a0a2-e0ca5f567151", "Not Registered.");
							}
						}
					}
				}
				return result;
			}
		}

		public ZString JS_Calc_ImportManifestStatus
		{
			get
			{
				ZString result = "";
				if (IsAir)
				{
					result = AirCargoSpecialStatus;
				}
				else if (IsSea)
				{
					result = SeaCargoSpecialStatus;
				}

				if (result.IsEmpty)
				{
					if (ImportManifestStatus != null)
					{
						string entryNum = (ZString)ImportManifestStatus[ZArchitecture.Schema.CusEntryNumSchema.CE_EntryNum.Name];
						string entryStatus = (ZString)ImportManifestStatus[ZArchitecture.Schema.CusEntryNumSchema.CE_EntryStatus.Name];
						if (entryStatus == ImpededStatus)
						{
							result = entryNum + ":" + ImpededDescription;
						}
						else if (entryStatus == ClearStatus)
						{
							result = entryNum + ":" + ClearDescription;
						}
					}
					else
					{
						result = Res.GetString("37ed41df-4f18-436d-8cb7-992bec42da5b", "NOT:Not sent");
					}
				}
				return result;
			}
		}

		public ZPropertyInfo JS_Calc_ImportManifestStatusInfo
		{
			get { return GetZPropertyInfo(Schema.JS_Calc_ImportManifestStatus); }
		}

		protected BusinessObject HouseBill
		{
			get
			{
				BusinessObject result = null;
				if (IsAir)
				{
					result = (BusinessObject)Factory.LoadTop1<Enterprise.Integration.Customs.Shared.ICusHAWB>(new ZQuery(ZArchitecture.Schema.CusHAWBSchema.CS_JS, PK));
				}
				else
				{
					result = (BusinessObject)Factory.LoadTop1<Enterprise.Integration.Customs.Shared.IBaseCusSCAHouse>(new ZQuery(ZArchitecture.Schema.CusSCAHouseSchema.CA_JS, PK));
				}
				return result;
			}
		}

		protected BusinessObject ImportManifestStatus
		{
			get { return Factory.LoadTop1(typeof(CusEntryNumber), ImportManifestStatusFilter); }
		}

		protected ZQuery ImportManifestStatusFilter
		{
			get
			{
				ZQuery result = new ZQuery(ZArchitecture.Schema.CusEntryNumSchema.CE_ParentID, PK);
				result.AddToFilter(JoinCondition.And, ZArchitecture.Schema.CusEntryNumSchema.CE_ParentTable, SQLComparisonOperator.Equal, CommonShipment.Schema.TableName);
				result.AddToFilter(JoinCondition.And, ZArchitecture.Schema.CusEntryNumSchema.CE_RN_NKCountryCode, SQLComparisonOperator.Equal, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				result.AddToFilter(JoinCondition.And, ZArchitecture.Schema.CusEntryNumSchema.CE_EntryType, SQLComparisonOperator.Equal, EntryTypeForImportManifestStatus);
				return result;
			}
		}

		#endregion

		#region Implementation

		Type ITopLevelBusinessEntityForDocSup.TopLevelBusinessEntity => typeof(ForwardingShipment);

		#endregion
	}
}
