using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business.Accounting.Helpers;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static System.FormattableString;

namespace Enterprise.MasterFiles.Business.Accounting.CriticalValidation
{
	#region SuppressResourceStringsCheckRegion
	// Developer only debug message should not be translated

	public static class CriticalValidationInfoExtensions
	{
		public static string GetAllPropertyValues(this BusinessObject bizo)
		{
			if (bizo.IsDeleted && !((IBusinessObjectInternals)bizo).Row.HasVersion(DataRowVersion.Original))
			{
				return "Original property values for deleted bizo are not accessible";
			}

			var result = new ZStringBuilder(bizo.GetBusinessObjectGenericInfo());
			result.Append(@"
Properties:");
			result.Append(new ZStringBuilder(bizo.ZPropertyInfoHash.Cast<ZPropertyInfo>().OrderBy(x => x.Name).Select(x => GetPropertyValueAndCode(x, bizo)).Where(x => !string.IsNullOrEmpty(x))));

			if (bizo is IAdditionalPropertyValuesProvider additionalPropertyValuesProvider)
			{
				result.Append(additionalPropertyValuesProvider.GetAdditionalProperyValues());
			}

			result.Append(bizo.GetFieldsWithChangesInfo());

			return result.ToStringWithNewLineBetweenAppends();
		}

		static string GetPropertyValueAndCode(ZPropertyInfo property, BusinessObject bo)
		{
			IZType value = null;
			if (bo.IsDeleted)
			{
				if (property.IsPersistent)
				{
					value = property.OriginalValue;
				}
				else
				{
					return string.Empty;
				}
			}
			else
			{
				try
				{
					value = property.Value;
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					value = new ZString(Invariant($"Exception: {ex.Message}"));
				}
			}

			if (value == null)
			{
				return Invariant($" {property.Name} = NULL");
			}
			else
			{
				return Invariant($" {property.Name} = {value}{GetCodeStringFromPK(value, property, bo)}");
			}
		}

		static string GetCodeStringFromPK(IZType value, ZPropertyInfo property, BusinessObject bo)
		{
			var codeString = ZString.Empty;
			var code = ZString.Empty;
			if (value is ZGuid)
			{
				var pk = (ZGuid)value;
				try
				{
					var collection = MetaData.GetListDataSource(bo, property.PropertyDescriptor) as IFindBoxListProvider;
					code = collection?.CodeFromPrimaryKey(pk);
				}
#pragma warning disable CC0004 // Catch block cannot be empty
				catch (NoCodePropertyException)
				{
				}
#pragma warning restore CC0004 // Catch block cannot be empty
				if (!code.IsEmpty)
				{
					codeString = string.Format(CultureInfo.InvariantCulture, " ({0})", code);
				}
			}
			return codeString;
		}

		public static string GetSkipDataRefreshBusInfo(this BusinessObject bizO)
		{
			string skipDataRefreshBusInfo;
			if (!ObjectFactory.Get<IDataRefreshBusUpdateActionDecider>().HasSkippedDataRefreshBusUpdate(bizO))
			{
				skipDataRefreshBusInfo = string.Empty;
			}
			else
			{
				var skipDataRefreshBusUpdateDueToAnyChangeFieldsWithChangesInfo = CriticalValidationInfoCollectorService.GetService(bizO.Factory)?.GetInfo(bizO.PK, CriticalValidationInfoCollectorServiceKeyType.DataRefreshBusUpdateSkipped);
				skipDataRefreshBusInfo = string.Format(CultureInfo.InvariantCulture, @"
Setting SkipDataRefreshBusUpdateDueToAnyChange Charge Fields With changes info = {0}", skipDataRefreshBusUpdateDueToAnyChangeFieldsWithChangesInfo);
			}
			return skipDataRefreshBusInfo;
		}

		public static string GetBusinessContextForBizObj(BusinessObject bizO)
		{
			var builder = new ZStringBuilder();

			var factoryLevelContexts = bizO.Factory.GetContexts<BusinessContext>();
			if (factoryLevelContexts.Count > 0)
			{
				builder.AppendFormat("Factory Level : ({0})", string.Join(", ", factoryLevelContexts.Select(x => x.ToString())));
			}

			var bizObjLevelContexts = bizO.GetContexts<BusinessContext>();
			if (bizObjLevelContexts.Count > 0)
			{
				builder.AppendFormat("BizObj Level : ({0})", string.Join(", ", bizObjLevelContexts.Select(x => x.ToString())));
			}
			return builder.IsEmpty ? "None" : builder.ToStringWithDelimiterBetweenAppends(",");
		}

		public static string GetContextInfo<T>(this BusinessObject bizO) where T : struct, IComparable, IConvertible
		{
			var builder = new ZStringBuilder();

			var factoryLevelContexts = getContextsInfo(bizO.Factory.GetContexts<T>());
			if (!string.IsNullOrEmpty(factoryLevelContexts))
			{
				builder.Append("Factory Level " + factoryLevelContexts);
			}

			var bizObjLevelContexts = getContextsInfo(bizO.GetContexts<T>());
			if (!string.IsNullOrEmpty(bizObjLevelContexts))
			{
				builder.Append("BizObj Level " + bizObjLevelContexts);
			}

			return builder.IsEmpty ? "None " + typeof(T).FullName : builder.ToStringWithDelimiterBetweenAppends(", ");

			string getContextsInfo(List<T> contexts)
			{
				var result = string.Empty;
				if (contexts.Any())
				{
					var contextTypeName = contexts.First().GetType().FullName;
					var contextsAsString = string.Join(", ", contexts.Select(x => x.ToString()));
					result = contextTypeName + " : " + contextsAsString;
				}
				return result;
			}
		}

		public static string GetTypesOfBizOsAroundOneRow(BusinessObject bizO)
		{
			var result = new ZStringBuilder();

			foreach (var typeName in bizO.Factory.GetBizOsForPK(bizO.PK.ToGuid()).Select(x => x.GetType().Name).OrderBy(x => x))
			{
				result.Append(typeName);
			}

			return result.ToStringWithDelimiterBetweenAppends("|");
		}

		public static string GetFieldsWithChangesInfo(this BusinessObject bizo)
		{
			StringBuilder changedFieldsWithValues = new StringBuilder();

			List<ZPropertyInfo> changedFieldsList = new List<ZPropertyInfo>();

			foreach (ZPropertyInfo property in bizo.ZPropertyInfoHash)
			{
				if (property.IsPersistent && property.HasChanges)
				{
					changedFieldsList.Add(property);
				}
			}

			if (changedFieldsList.Count > 0)
			{
				changedFieldsWithValues.Append(string.Format(CultureInfo.InvariantCulture, "{0}Fields with changes:", "\r\n\t"));
				foreach (ZPropertyInfo property in changedFieldsList.OrderBy(x => x.Name))
				{
					var propValue = (property.OriginalValue is ZDateTime) ? (object)((ZDateTime)property.OriginalValue).ToAUString() : property.OriginalValue;
					changedFieldsWithValues.Append(string.Format(CultureInfo.InvariantCulture, " {0} ({1}, {2}),", property.Name, propValue, property.Value));
				}
				changedFieldsWithValues.Replace(',', '.', changedFieldsWithValues.Length - 1, 1);
			}

			return changedFieldsWithValues.ToString();
		}

		public static IEnumerable<string> GetFieldsListWithChanges(this BusinessObject bizo)
		{
			var changedFieldsList = new List<string>();
			foreach (ZPropertyInfo property in bizo.ZPropertyInfoHash)
			{
				if (!ExcludedPropertys.Contains(property.Name) && property.IsPersistent && property.HasChanges)
				{
					changedFieldsList.Add(property.Name);
				}
			}
			return changedFieldsList.AsEnumerable();
		}

		static string[] ExcludedPropertys => new string[]
		{
			"EM_MessageNText"
		};

		public static string GetParentCollectionsInfo(this BusinessObjectCollection[] parentCollections, BusinessObject bizo)
		{
			var result = new ZStringBuilder();
			result.Append("Parent collections:");
			if (parentCollections.Length > 0)
			{
				foreach (BusinessObjectCollection collection in parentCollections)
				{
					var bizObjCollectionInternals = (IBusinessObjectCollectionInternals)collection;
					result.Append(Invariant(
$@"BusinessObjectCollection Info:
	Collection Type = {collection.GetType()}
	Element Type = {collection.TypeOfElements}
	Factory Instance = {collection.Factory._Instance}
	Hash Code = {collection.GetHashCode()}
	Contains bizo = {collection.Contains(bizo.PK)}
	Has Changes = {collection.HasChanges}
	Number of elements = {collection.Count}
	Is List Changed Suspended = {bizObjCollectionInternals.IsListChangedSuspended}
	Has Changes From Delete = {bizObjCollectionInternals.HasChangesFromDelete}
	Masters Are Deleted = {bizObjCollectionInternals.MastersAreDeleted}
	Masters Are In Database  = {bizObjCollectionInternals.MastersAreInDatabase}"));
					if (collection is IDependentBusinessObjectCollection dependentCollection)
					{
						result.Append("Master:");
						if (dependentCollection.Master != null)
						{
							result.Append(dependentCollection.Master.GetAllPropertyValues());
						}
					}
				}
			}

			return result.ToStringWithNewLineBetweenAppends();
		}

		public static string GetParentCollectionsInfo(this BusinessObject bizo) => ((IBusinessObjectInternals)bizo).ParentCollections.GetParentCollectionsInfo(bizo);

		public static string GetBusinessObjectGenericInfo(this BusinessObject bizo)
		{
			var result = new ZStringBuilder();

			result.Append(Invariant(
$@"	PK = {bizo.PK}
	Type = {bizo.GetType().Name}
	Types around row = {GetTypesOfBizOsAroundOneRow(bizo)}
	Factory Instance = {bizo.Factory._Instance}
	IsDeleted = {bizo.IsDeleted}
	IsInDb = {bizo.IsInDatabase}
	IsSavedByFactory = {bizo.IsSavedByFactory}
	HasChanges = {bizo.HasChanges}
	HasErrors = {bizo.HasErrors}
	IsDeleting = {bizo.IsDeleting}"));
			result.Append(@"
Business Contexts = " + GetBusinessContextForBizObj(bizo));
			result.Append(GetSkipDataRefreshBusInfo(bizo));

			return result.ToStringWithNewLineBetweenAppends();
		}

		public static string GetTransactionHeaderInfo(this AccTransactionHeader header)
		{
			bool showExtraInfo = header.IsInDatabase &&
					(header.AH_SystemCreateTimeUtc.IsValid || !header.AH_SystemCreateUser.IsEmpty || header.AH_SystemLastEditTimeUtc.IsValid || !header.AH_SystemLastEditUser.IsEmpty);
			var extraInfo = showExtraInfo ?
				string.Format(", System Create Time = {0}, System Create User = {1}, System Last Edit Time = {2}, System Last Edit User = {3}", header.AH_SystemCreateTimeUtc.ToAUString(), header.AH_SystemCreateUser, header.AH_SystemLastEditTimeUtc.ToAUString(), header.AH_SystemLastEditUser) :
				string.Empty;
			return string.Format(CultureInfo.InvariantCulture,
"Header: PK = {20}, Ledger = {0}, Transaction Type = {1}, Invoice Date = {2}, Post Date = {3}, Invoice Amount = {4}, GST Amount = {5}, OS Total = {6}, Exchange Rate = {7}, Currency = {8}, Outstanding Amount = {9}, Fully Paid Date =  {10}, Is Canceled = {11}, Is Aggregated = {12}, Transaction Number = {13}, Job PK = {14}, Organization = {15}, Is In DB = {16}, Is Deleted = {17}, Has Changes = {18}, Business Contexts = {21}{19}.",
				header.AH_Ledger, header.AH_TransactionType, header.AH_InvoiceDate.ToAUString(), header.AH_PostDate.ToAUString(), header.AH_InvoiceAmount.ToString(),
				header.AH_GSTAmount.ToString(), header.AH_OSTotal, header.AH_ExchangeRate, header.AH_RX_NKTransactionCurrency,
				header.AH_OutstandingAmount.ToString(), header.AH_FullyPaidDate.ToAUString(), header.IsCancelled.ToYesNoString(), header.AH_PostToGL, header.AH_TransactionNum,
				header.AH_JH, header.Header != null ? header.Header.OH_Code : ZString.Empty, header.IsInDatabase.ToYesNoString(), header.IsDeleted.ToYesNoString(), header.HasChanges.ToYesNoString(),
				extraInfo, header.PK, GetBusinessContextForBizObj(header))
				+ header.GetFieldsWithChangesInfo();
		}

		public static string GetTransactionHeaderInfo2(this AccTransactionHeader header)
		{
			return string.Format(CultureInfo.InvariantCulture,
"Header: PK = {19}, Ledger = {0}, Transaction Type = {1}, Invoice Date = {2}, Post Date = {3}, Invoice Amount = {4}, GST Amount = {5}, OS Total = {6}, Exchange Rate = {7}, Currency = {8}, Outstanding Amount = {9}, Fully Paid Date =  {10}, Is Canceled = {11}, Is Aggregated = {12}, Transaction Number = {13}, Job PK = {14}, Organization = {15}, Is In DB = {16}, Is Deleted = {18}, Has Changes = {17}, Transaction Group Identifier = {20}, Business Contexts = {21}.",
				  header.AH_Ledger, header.AH_TransactionType, header.AH_InvoiceDate.ToAUString(), header.AH_PostDate.ToAUString(), header.AH_InvoiceAmount.ToString(),
				  header.AH_GSTAmount.ToString(), header.AH_OSTotal, header.AH_ExchangeRate, header.AH_RX_NKTransactionCurrency,
				  header.AH_OutstandingAmount.ToString(), header.AH_FullyPaidDate.ToAUString(), header.IsCancelled.ToYesNoString(), header.AH_PostToGL, header.AH_TransactionNum,
				  header.AH_JH, header.Header != null ? header.Header.OH_Code : ZString.Empty, header.IsInDatabase.ToYesNoString(), header.HasChanges.ToYesNoString(), header.IsDeleted.ToYesNoString(), header.PK,
				  header.AH_TransactionBelongsToGroup, GetBusinessContextForBizObj(header))
				  + header.GetFieldsWithChangesInfo();
		}

		public static string GetTransactionHeaderOriginalInfo(this AccTransactionHeader header)
		{
			if (header.IsDeleted && !((IBusinessObjectInternals)header).Row.HasVersion(DataRowVersion.Original))
			{
				return "Original property values for deleted bizo are not accessible";
			}

			var organisation = header.Factory.Load<OrgHeader>(((ZGuid)header.AH_OHInfo.OriginalValue));
			var orgCode = organisation?.OH_Code ?? ZString.Empty;

			bool showExtraInfo = header.IsInDatabase && (header.AH_SystemCreateTimeUtcInfo.OriginalValue.IsValid || !header.AH_SystemCreateUserInfo.OriginalValue.IsEmpty ||
				header.AH_SystemLastEditTimeUtcInfo.OriginalValue.IsValid || !header.AH_SystemLastEditUserInfo.OriginalValue.IsEmpty);

			var extraInfo = showExtraInfo ?
				Invariant($@", 
System Create Time = {((ZDateTime)header.AH_SystemCreateTimeUtcInfo.OriginalValue).ToAUString()}, 
System Create User = {header.AH_SystemCreateUserInfo.OriginalValue}, 
System Last Edit Time = {((ZDateTime)header.AH_SystemLastEditTimeUtcInfo.OriginalValue).ToAUString()}, 
System Last Edit User = {header.AH_SystemLastEditUserInfo.OriginalValue}"
				) :
				".";

			return Invariant($@"Header original values: 
PK = {header.PK}, 
Ledger = {header.AH_LedgerInfo.OriginalValue}, 
Transaction Type = {header.AH_TransactionTypeInfo.OriginalValue},
Invoice Date = {((ZDateTime)header.AH_InvoiceDateInfo.OriginalValue).ToAUString()},
Post Date = {((ZDateTime)header.AH_PostDateInfo.OriginalValue).ToAUString()},
Invoice Amount = {header.AH_InvoiceAmountInfo.OriginalValue},
GST Amount = {header.AH_GSTAmountInfo.OriginalValue},
OS Total = {header.AH_OSTotalInfo.OriginalValue},
Exchange Rate = {header.AH_ExchangeRateInfo.OriginalValue},
Currency = {header.AH_RX_NKTransactionCurrencyInfo.OriginalValue},
Outstanding Amount = {header.AH_OutstandingAmountInfo.OriginalValue},
Fully Paid Date =  {((ZDateTime)header.AH_FullyPaidDateInfo.OriginalValue).ToAUString()},
Is Canceled = {((ZBool)header.AH_IsCancelledInfo.OriginalValue).ToString()},
Is Aggregated = {header.AH_PostToGLInfo.OriginalValue},
Transaction Number = {header.AH_TransactionNumInfo.OriginalValue},
Job PK = {header.AH_JHInfo.OriginalValue},
Organization = {orgCode},
Is In DB = {header.IsInDatabase.ToYesNoString()},
Is Deleted = {header.IsDeleted.ToYesNoString()}{extraInfo}");
		}

		public static string GetTransactionLineInfo(this AccTransactionLines line)
		{
			return string.Format(CultureInfo.InvariantCulture,
"Line: PK = {18}, Charge Code = {0}, GL Account = {1}, Type = {2}, OS Amount = {3}, Local Amount = {4}, GST = {5}, Tax Rate = {19}, Tax Class = {20}, Exchange Rate = {6}, Currency = {7}, Post Date = {8}, Reverse Date = {9}, Post To GL = {10}, Reverse To GL = {11}, Header PK = {12}, Job PK = {13}, Organization = {14}, Revenue Recognition Type = {22}, Is In DB = {15},{23} Is Final = {17}, Sub Accounts = {21}, Has Changes = {16}.",
				line.ChargeCode != null ? line.ChargeCode.AC_Code : ZString.Empty, line.GLHeader != null ? line.GLHeader.AG_AccountNum : ZString.Empty,
				line.AL_LineType, line.AL_OSAmount, line.AL_LineAmount, line.AL_GSTVAT, line.AL_ExchangeRate, line.AL_RX_NKTransactionCurrency, line.AL_PostDate.ToAUString(),
				line.AL_ReverseDate.ToAUString(), line.AL_PostToGL, line.AL_ReverseToGL, line.AL_AH, line.AL_JH, line.Header != null ? line.Header.OH_Code : ZString.Empty,
				line.IsInDatabase.ToYesNoString(), line.HasChanges.ToYesNoString(), ((bool)line.AL_IsFinalCharge).ToYesNoString(), line.PK, line.TaxRate == null ? ZString.Empty : line.TaxRate.AT_Code,
				line.VATClass == null ? ZString.Empty : line.VATClass.A9_Code, ObjectFactory.Get<IAccounting>()?.GetSubAccountsInfo(line), line.AL_RevRecognitionType,
				(!line.IsInDatabase || line.HasChanges) && !line.IsSavedByFactory ? " <New or has changes, but will NOT be saved in db>," : "")
				+ line.GetFieldsWithChangesInfo();
		}

		public static string GetJobRevenueRecognitionInfo(this JobHeader job)
		{
			var revenueRecognitionInfo = new ZStringBuilder(@"Revenue Recognition info:
");
			var methodInfo = job.GetType().GetProperty("RevenueRecognitionCollection", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

			if (methodInfo != null)
			{
				var bizOs = (methodInfo.GetValue(job, null) as IEnumerable)?.Cast<BusinessObject>().ToArray();

				if (bizOs != null && bizOs.Length > 0)
				{
					foreach (BusinessObject bizo in bizOs)
					{
						var pk = new ZGuid(bizo[JobChargeRevRecognitionSchema.PK]);
						var jobPK = new ZGuid(bizo[JobChargeRevRecognitionSchema.D3_JH]);
						var recognitionDate = new ZDateTime(bizo[JobChargeRevRecognitionSchema.D3_RecognitionDate]);
						var recognitionType = new ZString(bizo[JobChargeRevRecognitionSchema.D3_RecognitionType]);

						revenueRecognitionInfo = revenueRecognitionInfo.Append(string.Format(CultureInfo.InvariantCulture, "PK: {0}, D3_JH: {1}, D3_RecognitionType : {2}, D3_RecognitionDate : {3}, Is In DB = {4}, Has Changes = {5}", pk, jobPK, recognitionType, recognitionDate.ToAUString(), bizo.IsInDatabase.ToYesNoString(), bizo.HasChanges.ToYesNoString()));
					}
				}
				else
				{
					revenueRecognitionInfo.Append("No D3 record is created for this Job");
				}
			}

			return revenueRecognitionInfo.ToStringWithNewLineBetweenAppends();
		}

		public static string GetJobChargeInfo(this JobCharge charge)
		{
			var iCharge = charge as ICharge;
			return Invariant($@"Charge: PK = {charge.PK}, Type = {GetTypesOfBizOsAroundOneRow(charge)}, Charge Type = {charge.JR_ChargeType}, 
Job PK = {charge.JR_JH}, Job Number = {(charge.Job == null ? ZString.Empty : charge.Job.JH_JobNum)}, 
Charge Code = {(charge.ChargeCode == null ? ZString.Empty : charge.ChargeCode.AC_Code)}, 
Charge Code Type = {(charge.ChargeCode == null ? ZString.Empty : charge.ChargeCode.AC_ChargeType)}, 
Cost Account = {(charge.CostAccount == null ? ZString.Empty : charge.CostAccount.OH_Code)}, 
OS Cost Amount = {charge.JR_OSCostAmt}, Local Cost Amount = {charge.JR_LocalCostAmt}, OS Cost Exchange Rate = {charge.JR_OSCostExRate}, 
Cost GST is Overridden = {((bool)charge.JR_IsCostTaxAmountOverridden).ToYesNoString()}, 
OS Cost GST Amount = {charge.JR_OSCostGSTAmt}, OS Cost WHT Amount = {charge.JR_OSCostWHTAmt}, 
AP Invoice # = {charge.JR_APInvoiceNum}, AP Invoice Date = {charge.JR_APInvoiceDate.ToAUString()}, 
Supplier Cost Reference = {charge.JR_CostReference}, Payment Date = {charge.JR_PaymentDate.ToAUString()}, Payment Type = {charge.JR_PaymentType}, 
Cheque # = {charge.JR_ChequeNo}, Cheque Book = {charge.JR_AK}, Cheque Book Code = {(charge.ChequeBook == null ? ZString.Empty : charge.ChequeBook.AK_Code)}, 
Bank Account = {charge.JR_AB}, 
Is Cost Posted = {charge.IsCostPosted.ToYesNoString()}, AP Line = {charge.JR_AL_APLine}, Sell Account = {(charge.SellAccount == null ? ZString.Empty : charge.SellAccount.OH_Code)}, 
OS Sell Exchange Rate = {charge.JR_OSSellExRate}, OS Sell Amount = {charge.JR_OSSellAmt}, Local Sell Amount = {charge.JR_LocalSellAmt}, 
OS Sell GST Amount = {charge.JR_OSSellGSTAmt_Calc}, OS Sell WHT Amount = {charge.JR_OSSellWHTAmt}, 
Is Revenue Posted = {charge.IsRevenuePosted.ToYesNoString()}, AR Line = {charge.JR_AL_ARLine}, 
CFX Line = {charge.JR_AL_CFXLine}, Invoice Type = {charge.JR_InvoiceType}, 
Order Reference = {charge.JR_OrderReference}, OP Product = {charge.JR_OP_Product}, Product Quantity = {charge.JR_ProductQuantity}, 
Consol Cost = {charge.JR_E6}, Gateway Sell Header = {charge.JR_E6_GatewaySellHeader}, 
Cost Currency = {charge.JR_RX_NKCostCurrency}, Sell Currency = {charge.JR_RX_NKSellCurrency}, 
Is In DB = {charge.IsInDatabase.ToYesNoString()}, Has Changes = {charge.HasChanges.ToYesNoString()}, 
Is Saved By Factory = {charge.IsSavedByFactory.ToYesNoString()}, 
Cost GST Rate = {(charge.CostGSTRate == null ? ZString.Empty : charge.CostGSTRate.AT_Code)}, 
Sell GST Rate = {(charge.SellGSTRate == null ? ZString.Empty : charge.SellGSTRate.AT_Code)}, 
Cost Tax Class = {(charge.CostVATClass == null ? ZString.Empty : charge.CostVATClass.A9_Code)}, 
Sell Tax Class = {(charge.SellVATClass == null ? ZString.Empty : charge.SellVATClass.A9_Code)}, 
Sell Invoice Currency = {charge.JR_RX_NKSellInvoiceCurrency}, 
{(iCharge != null ? Invariant($"OS Sell Invoice Amt = {iCharge.OSSellInvoiceAmt}, Local Sell Invoice Amt = {iCharge.LocalSellInvoiceAmt}, Sell Invoice Ex Rate = {iCharge.SellInvoiceCurrencyExRate}, CFX Amt = {iCharge.CFXAmt}, ") : string.Empty)}
Cost Tax Date = {charge.JR_CostTaxDate}, 
Sell Tax Date = {charge.JR_SellTaxDate},
Cost Supply Type = {charge.JR_CostSupplyType},
Cost Tax Branch = {(charge.CostTaxBranch?.GB_Code ?? ZString.Empty)},
Business Contexts = {GetBusinessContextForBizObj(charge)}.").Replace(System.Environment.NewLine, "") + charge.GetFieldsWithChangesInfo() + GetSkipDataRefreshBusInfo(charge);
		}

		public static string GetJobInfo(this JobHeader job)
		{
			if (job == null)
			{
				return string.Empty;
			}

			IJobInvoicingPlugIn pluginData = null;
			if (job.Parent == null)
			{
				job.InitializeParentFromGenericJobWithSettingDefaults();
			}

			pluginData = job.Parent as IJobInvoicingPlugIn;
			var paymentTerm = pluginData != null ? pluginData.InvoicingSupporter.PaymentTerm.GetPaymentTermInfo(CostSell.Revenue) : null;
			var incoterm = paymentTerm != null && paymentTerm.InfoType == PaymentTermType.Incoterm ? paymentTerm.Value : string.Empty;
			var tableName = pluginData != null ? pluginData.TableName : string.Empty;
			var consumerType = pluginData != null ? pluginData.InvoicingSupporter.ConsumerType : null;
			var controllerID = consumerType != null ? consumerType.ControllerID.Name : string.Empty;
			return string.Format(CultureInfo.InvariantCulture, "Job: Job Number = {0}, PK = {1}, Parent Table Code = {2}, Parent Table Name = {3}, ControllerID = {4}, Shipment Incoterm = {5}, Local Client Code = {6}, Local Client Address = {7}, Overseas Agent Code = {8}, Overseas Agent Address = {9}, Is In DB = {10}, Has Changes = {11}."
				, job.JH_JobNum
				, job.PK
				, job.JH_ParentTableCode
				, tableName
				, controllerID
				, incoterm
				, job.LocalCharges != null ? job.LocalCharges.GetOrganizationInfo(job.JH_GC) : string.Empty
				, job.LocalCharges != null ? job.LocalChargesAddr.OA_Code : ZString.Empty
				, job.AgentCollect != null ? job.AgentCollect.GetOrganizationInfo(job.JH_GC) : string.Empty
				, job.AgentCollect != null ? job.AgentCollectAddr.OA_Code : ZString.Empty
				, job.IsInDatabase.ToYesNoString()
				, job.HasChanges.ToYesNoString())
				+ job.GetFieldsWithChangesInfo();
		}

		public static string GetOrganizationInfo(this OrgHeader org, ZGuid companyPk)
		{
			if (org == null)
			{
				return string.Empty;
			}

			return string.Format(CultureInfo.InvariantCulture, "{0} (AR: {1}, AP: {2})", org.OH_Code, org.IsDebtorForCompany(companyPk).ToYesNoString(), org.IsCreditorForCompany(companyPk).ToYesNoString()); //Suppress - Part of bigger message
		}

		public static string GetPostedCostInfo(this JobCharge jobCharge)
		{
			if (jobCharge.IsCostPosted)
			{
				return string.Format(CultureInfo.InvariantCulture,
@"Posted Cost:
Cost {0}
Cost {1}"
					, GetTransactionHeaderInfo(jobCharge.APLine.TransactionHeader)
					, GetTransactionLineInfo(jobCharge.APLine));
			}
			else
			{
				return string.Empty;
			}
		}

		public static string GetConsolCostInfo(this JobCharge jobCharge)
		{
			if (jobCharge.JR_IsApportioned)
			{
				return ((IJobConsolCost)jobCharge.ParentConsolCost).GetJobConsolCostInfo();
			}
			else
			{
				return string.Empty;
			}
		}

		public static string GetJobChargeOriginalInfo(this JobCharge charge)
		{
			if (charge.IsDeleted && !((IBusinessObjectInternals)charge).Row.HasVersion(DataRowVersion.Original))
			{
				return "Original property values for deleted bizo are not accessible";
			}

			var chargeCode = charge.Factory.Load<AccChargeCode>((ZGuid)charge.JR_ACInfo.OriginalValue);
			var costAccount = charge.Factory.Load<OrgHeader>((ZGuid)charge.JR_OH_CostAccountInfo.OriginalValue);
			var sellAccount = charge.Factory.Load<OrgHeader>((ZGuid)charge.JR_OH_SellAccountInfo.OriginalValue);
			var arLine = charge.Factory.Load<AccTransactionLines>((ZGuid)charge.JR_AL_ARLineInfo.OriginalValue);
			var apLine = charge.Factory.Load<AccTransactionLines>((ZGuid)charge.JR_AL_APLineInfo.OriginalValue);
			bool isRevenuePosted = AccTransactionLines.IsRevenueLine(arLine);
			bool isCostPosted = AccTransactionLines.IsCostLine(apLine);

			return string.Format(CultureInfo.InvariantCulture,
"Charge original values: PK = {0}, Job PK = {1}, Charge Code = {2}, Cost Account = {3}, OS Cost Amount = {4}, Local Cost Amount = {5}, OS Cost Exchange Rate = {6}, OS Cost GST Amount = {7}, OS Cost WHT Amount = {8}, AP Invoice # = {9}, AP Invoice Date = {10}, Supplier Cost Reference = {11}, Payment Date = {12}, Payment Type = {13}, Cheque # = {14}, Cheque Book = {15}, Bank Account = {16}, Is Cost Posted = {17}, AP Line = {18}, Sell Account = {19}, OS Sell Exchange Rate = {20}, OS Sell Amount = {21}, Local Sell Amount = {22}, OS Sell WHT Amount = {23}, Is Revenue Posted = {24}, AR Line = {25}, CFX Line = {26}, Invoice Type = {27}, Order Reference = {28}, OP Product = {29}, Product Quantity = {30}, Consol Cost = {31}, Gateway Sell Header = {35}, Cost Currency = {32}, Sell Currency = {33}, Is In DB = {34}.",
				charge.PK, charge.JR_JHInfo.OriginalValue, chargeCode == null ? ZString.Empty : chargeCode.AC_Code,
				costAccount == null ? ZString.Empty : costAccount.OH_Code, charge.JR_OSCostAmtInfo.OriginalValue, charge.JR_LocalCostAmtInfo.OriginalValue,
				charge.JR_OSCostExRateInfo.OriginalValue, charge.JR_OSCostGSTAmtInfo.OriginalValue, charge.JR_OSCostWHTAmtInfo.OriginalValue, charge.JR_APInvoiceNumInfo.OriginalValue,
				((ZDateTime)charge.JR_APInvoiceDateInfo.OriginalValue).ToAUString(), charge.JR_CostReferenceInfo.OriginalValue,
				((ZDateTime)charge.JR_PaymentDateInfo.OriginalValue).ToAUString(), charge.JR_PaymentTypeInfo.OriginalValue, charge.JR_ChequeNoInfo.OriginalValue, charge.JR_AKInfo.OriginalValue,
				charge.JR_ABInfo.OriginalValue, isCostPosted.ToYesNoString(),
				charge.JR_AL_APLineInfo.OriginalValue, sellAccount == null ? ZString.Empty : sellAccount.OH_Code, charge.JR_OSSellExRateInfo.OriginalValue, charge.JR_OSSellAmtInfo.OriginalValue,
				charge.JR_LocalSellAmtInfo.OriginalValue, charge.JR_OSSellWHTAmtInfo.OriginalValue, isRevenuePosted.ToYesNoString(),
				charge.JR_AL_ARLineInfo.OriginalValue, charge.JR_AL_CFXLineInfo.OriginalValue, charge.JR_InvoiceTypeInfo.OriginalValue, charge.JR_OrderReferenceInfo.OriginalValue,
				charge.JR_OP_ProductInfo.OriginalValue, charge.JR_ProductQuantityInfo.OriginalValue,
				charge.JR_E6Info.OriginalValue, charge.JR_RX_NKCostCurrencyInfo.OriginalValue, charge.JR_RX_NKSellCurrencyInfo.OriginalValue, charge.IsInDatabase.ToYesNoString(),
				charge.JR_E6_GatewaySellHeaderInfo.OriginalValue);
		}

		public static string GetJobChargeInternalFieldsInfo(this JobCharge charge)
		{
			return string.Format(CultureInfo.InvariantCulture,
				$"InternalFields: InternalJob = {charge.InternalJob?.JH_JobNum}, InternalBranch = {charge.InternalBranch?.GB_Code}, InternalDepartment = {charge.InternalDept?.GE_Code}");
		}

		#region AccTransactionMatchLink

		public static string GetMatchLinkInfo(this AccTransactionMatchLink link)
		{
			return string.Format(CultureInfo.InvariantCulture,
				"Match Link: Group Number = {0}, Amount = {1}, OS Amount = {2}, Match Date = {3}, Transaction PK = {4}, Is In DB = {5}, Has Changes = {6}.",
				link.AP_MatchGroupNum, link.AP_Amount, link.AP_OSAmount, link.AP_MatchDate.ToAUString(), link.AP_AH, link.IsInDatabase.ToYesNoString(), link.HasChanges.ToYesNoString())
				+ link.GetFieldsWithChangesInfo();
		}

		public static string GetTransactionHeaderMatchLinkInfos(this AccTransactionMatchLink[] matchLinks)
		{
			StringBuilder matchLinksInfo = new StringBuilder();

			if (matchLinks.Length > 0)
			{
				matchLinksInfo.AppendLine();
				matchLinksInfo.AppendLine();
				matchLinksInfo.Append(Res.GetString("51d6be32-6227-460a-9e69-09784d70b83e", "Related Match Links:"));
				matchLinksInfo.AppendLine();

				var matchGroupNums = matchLinks.Select(m => m.AP_MatchGroupNum).ToArray();
				var query = new ZDBOnlyQuery(typeof(AccTransactionMatchLink));
				query.AddToFilter(AccTransactionMatchLinkSchema.AP_MatchGroupNum, matchGroupNums);
				var subQuery = new ZDBOnlySubQuery(typeof(AccTransactionHeader), AccTransactionMatchLinkSchema.AP_AH);
				subQuery.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);
				query.AddSubQuery(AccTransactionMatchLinkSchema.AP_AH, AccTransactionHeaderSchema.PK, subQuery, JoinCondition.And);

				var relateGroupLinks = matchLinks[0].Factory.Load<AccTransactionMatchLink>(query).ToArray();
				var relateTransactionHeads = matchLinks[0].Factory.Load<AccTransactionHeader>(new ZQuery().AddToFilter(AccTransactionHeaderSchema.PK, relateGroupLinks.Select(g => g.AP_AH).Distinct().ToArray()));

				foreach (AccTransactionMatchLink link in matchLinks)
				{
					matchLinksInfo.AppendLine(link.GetMatchLinkInfo());

					var otherMatchLinks = relateGroupLinks.Where(m => m.PK != link.PK && m.AP_MatchGroupNum == link.AP_MatchGroupNum).ToArray();
					if (otherMatchLinks.Length > 0)
					{
						matchLinksInfo.AppendLine(Res.GetString("BFE527EA-107A-4510-9A76-6C215EFA1A99", "Other Match Links In This Group:"));
						foreach (var groupLink in otherMatchLinks)
						{
							matchLinksInfo.AppendLine(groupLink.GetMatchLinkInfo());
							matchLinksInfo.AppendLine(relateTransactionHeads.FirstOrDefault(t => t.PK == groupLink.AP_AH)?.GetTransactionHeaderInfo());
						}

						matchLinksInfo.AppendLine();
					}
				}
			}

			return matchLinksInfo.ToString();
		}

		public static string GetTransactionHeaderMatchLinkInfosAndUnmatchDeletionInfos(this AccTransactionMatchLink[] matchLinks)
		{
			StringBuilder matchLinksInfo = new StringBuilder();

			if (matchLinks?.Length > 0)
			{
				matchLinksInfo.AppendLine().AppendLine().AppendLine(FormattableString.Invariant($"Related Match Links:"));

				var matchGroupNums = matchLinks.Select(m => m.AP_MatchGroupNum).ToArray();
				var query = new ZDBOnlyQuery(typeof(AccTransactionMatchLink));
				query.AddToFilter(AccTransactionMatchLinkSchema.AP_MatchGroupNum, matchGroupNums);
				var subQuery = new ZDBOnlySubQuery(typeof(AccTransactionHeader), AccTransactionMatchLinkSchema.AP_AH);
				subQuery.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);
				query.AddSubQuery(AccTransactionMatchLinkSchema.AP_AH, AccTransactionHeaderSchema.PK, subQuery, JoinCondition.And);

				var relateGroupLinks = matchLinks[0].Factory.Load<AccTransactionMatchLink>(query).ToArray();
				var relateTransactionHeads = matchLinks[0].Factory.Load<AccTransactionHeader>(new ZQuery().AddToFilter(AccTransactionHeaderSchema.PK, relateGroupLinks.Select(g => g.AP_AH).Distinct().ToArray()));
				var collectorService = CriticalValidationInfoCollectorService.GetService(matchLinks[0].Factory);

				matchLinksInfo.AppendLine(FormattableString.Invariant($"Match Link Factory Instance Number = {matchLinks[0].Factory._Instance}.")).AppendLine("----------------------");

				foreach (AccTransactionMatchLink link in matchLinks)
				{
					matchLinksInfo.AppendLine(link.GetMatchLinkInfo());

					if (collectorService != null)
					{
						matchLinksInfo.AppendLine(collectorService.GetInfo(link.AP_AH, CriticalValidationInfoCollectorServiceKeyType.MatchLinkDeletionInfo));
					}

					var otherMatchLinks = relateGroupLinks.Where(m => m.PK != link.PK && m.AP_MatchGroupNum == link.AP_MatchGroupNum).ToArray();
					if (otherMatchLinks.Length > 0)
					{
						matchLinksInfo.AppendLine(FormattableString.Invariant($">> Other Match Links In This Group:"));
						foreach (var groupLink in otherMatchLinks)
						{
							matchLinksInfo.AppendLine(groupLink.GetMatchLinkInfo());

							if (collectorService != null)
							{
								matchLinksInfo.AppendLine(collectorService.GetInfo(groupLink.AP_AH, CriticalValidationInfoCollectorServiceKeyType.MatchLinkDeletionInfo))
											  .AppendLine();
							}

							matchLinksInfo.AppendLine(FormattableString.Invariant($"Relative Transaction Header:"))
										  .AppendLine(relateTransactionHeads.FirstOrDefault(t => t.PK == groupLink.AP_AH)?.GetTransactionHeaderInfo())
										  .AppendLine();
						}
					}

					matchLinksInfo.AppendLine(FormattableString.Invariant($">> ----- End of Other Match Links In This Group -----------------"));
				}

				matchLinksInfo.AppendLine(FormattableString.Invariant($"----- End of Related Match Links -----------------"));
			}

			return matchLinksInfo.ToString();
		}

		#endregion

		public static string ToYesNoString(this bool value)
		{
			return value ? "Yes" : "No";
		}

		public static string ToYesNoString(this ZBool value)
		{
			return value ? "Yes" : "No";
		}

		public static JobCharge LoadRelatedJobCharge(this AccTransactionLines line, bool useLocalCacheOnly = false)
		{
			if (line == null || line.AL_JH.IsEmpty)
			{
				return null;
			}

			var query = new ZQuery { FetchOnlyFromLocalCache = useLocalCacheOnly };
			query.AddToFilter(JobChargeSchema.JR_JH, line.AL_JH);

			JobCharge result = null;
			if (line.AL_LineType != TransactionLineTypes.Accrual)
			{
				result = line.Factory.LoadTop1<JobCharge>(new ZQuery(query).AddToFilter(JobChargeSchema.JR_AL_ARLine, line.PK));
			}
			if (result == null && line.AL_LineType != TransactionLineTypes.WIP)
			{
				result = line.Factory.LoadTop1<JobCharge>(new ZQuery(query).AddToFilter(JobChargeSchema.JR_AL_APLine, line.PK));
			}

			return result;
		}

		#region Extra Information for User Message

		public static MultilingualString GetConsolCostAndChargeInvoiceDetailsDifference(JobCharge charge, IJobConsolCost jobConsolCost, out bool isExistingDataWithoutChange)
		{
			var consolCost = jobConsolCost as BusinessObject;
			MultilingualString msg = (NoResString)"";
			isExistingDataWithoutChange = charge.IsInDatabase && !charge.JR_E6Info.HasChanges;

			if (charge != null && consolCost != null)
			{
				var consolTaxRate = charge.Factory.Load<AccTaxRate>((ZGuid)consolCost[JobConsolCostSchema.E6_AT_TaxRate]);
				var consolCreditor = charge.Factory.Load<OrgHeader>((ZGuid)consolCost[JobConsolCostSchema.E6_OH_Creditor.Name]);
				var consolTaxClass = charge.Factory.Load<AccInvMsg>((ZGuid)consolCost[JobConsolCostSchema.E6_A9_VATClass]);
				var consolTaxBranch = charge.Factory.Load<GlbBranch>((ZGuid)consolCost[JobConsolCostSchema.E6_GB_CostTaxBranch]);

				if (charge.JR_APInvoiceNum != (ZString)consolCost[JobConsolCostSchema.E6_InvoiceNum.Name])
				{
					isExistingDataWithoutChange = isExistingDataWithoutChange && !charge.JR_APInvoiceNumInfo.HasChanges && !consolCost.ZPropertyInfoHash[JobConsolCostSchema.E6_InvoiceNum.Name].HasChanges;
					msg = MultilingualString.Join(System.Environment.NewLine, msg, ResString.GetMultilingualString("7f241ce4-01f3-4b08-a90c-0b62bba68879", "Invoice Number:    {0}    |    {1}", charge.JR_APInvoiceNum, (ZString)consolCost[JobConsolCostSchema.E6_InvoiceNum.Name]));
				}
				if (!AreDateTimesEqualSafe(charge.JR_APInvoiceDateInfo, consolCost.ZPropertyInfoHash[JobConsolCostSchema.E6_InvoiceDate.Name]))
				{
					isExistingDataWithoutChange = isExistingDataWithoutChange && !charge.JR_APInvoiceDateInfo.HasChanges && !consolCost.ZPropertyInfoHash[JobConsolCostSchema.E6_InvoiceDate.Name].HasChanges;
					msg = MultilingualString.Join(System.Environment.NewLine, msg, ResString.GetMultilingualString("c3d7a79d-cba0-4abf-b041-f55b4aa72470", "Invoice Date:    {0}    |    {1}", charge.JR_APInvoiceDate.ToString("dd-MMM-yyyy hh:mm:ss.ffff"), ((ZDateTime)consolCost[JobConsolCostSchema.E6_InvoiceDate.Name]).ToString("dd-MMM-yyyy hh:mm:ss.ffff")));
				}
				if (!AreDateTimesEqualSafe(charge.JR_PaymentDateInfo, consolCost.ZPropertyInfoHash[JobConsolCostSchema.E6_PaymentDate.Name]))
				{
					isExistingDataWithoutChange = isExistingDataWithoutChange && !charge.JR_PaymentDateInfo.HasChanges && !consolCost.ZPropertyInfoHash[JobConsolCostSchema.E6_PaymentDate.Name].HasChanges;
					msg = MultilingualString.Join(System.Environment.NewLine, msg, ResString.GetMultilingualString("de5677a7-479c-4e2d-9e71-b89439eb1199", "Payment Date:    {0}    |    {1}", charge.JR_PaymentDate.ToString("dd-MMM-yyyy hh:mm:ss.ffff"), ((ZDateTime)consolCost[JobConsolCostSchema.E6_PaymentDate.Name]).ToString("dd-MMM-yyyy hh:mm:ss.ffff")));
				}
				if (charge.JR_OH_CostAccount != (ZGuid)consolCost[JobConsolCostSchema.E6_OH_Creditor.Name])
				{
					isExistingDataWithoutChange = isExistingDataWithoutChange && !charge.JR_OH_CostAccountInfo.HasChanges && !consolCost.ZPropertyInfoHash[JobConsolCostSchema.E6_OH_Creditor.Name].HasChanges;
					msg = MultilingualString.Join(System.Environment.NewLine, msg, ResString.GetMultilingualString("44a93b7e-1f8f-4c16-9cff-03a281caf41c", "Creditor:    {0}    |    {1}", charge.CostAccount != null ? charge.CostAccount.OH_Code : EmptyText, consolCreditor != null ? consolCreditor.OH_Code : EmptyText));
				}
				if (charge.JR_CostReference != (ZString)consolCost[JobConsolCostSchema.E6_CostReference.Name])
				{
					isExistingDataWithoutChange = isExistingDataWithoutChange && !charge.JR_CostReferenceInfo.HasChanges && !consolCost.ZPropertyInfoHash[JobConsolCostSchema.E6_CostReference.Name].HasChanges;
					msg = MultilingualString.Join(System.Environment.NewLine, msg, ResString.GetMultilingualString("33f05c32-65f2-4652-bb90-2fbded66d6b2", "Supplier Cost Reference:    {0}    |    {1}", charge.JR_CostReference, (ZString)consolCost[JobConsolCostSchema.E6_CostReference.Name]));
				}
				if (charge.JR_AT_CostGSTRate != (ZGuid)consolCost[JobConsolCostSchema.E6_AT_TaxRate])
				{
					isExistingDataWithoutChange = isExistingDataWithoutChange && !charge.JR_AT_CostGSTRateInfo.HasChanges && !consolCost.ZPropertyInfoHash[JobConsolCostSchema.E6_AT_TaxRate.Name].HasChanges;
					msg = MultilingualString.Join(System.Environment.NewLine, msg, ResString.GetMultilingualString("2921b5b7-bd73-41ad-a3aa-63c88a405ec7", "GST Rate:    {0}    |    {1}", charge.CostGSTRate != null ? charge.CostGSTRate.AT_Code : EmptyText, consolTaxRate != null ? consolTaxRate.AT_Code : EmptyText));
				}
				if ((ZDate)charge.JR_CostTaxDateInfo.Value != (ZDate)consolCost.ZPropertyInfoHash[JobConsolCostSchema.E6_TaxDate.Name].Value)
				{
					isExistingDataWithoutChange = isExistingDataWithoutChange && !charge.JR_CostTaxDateInfo.HasChanges && !consolCost.ZPropertyInfoHash[JobConsolCostSchema.E6_TaxDate.Name].HasChanges;
					msg = MultilingualString.Join(System.Environment.NewLine, msg, ResString.GetMultilingualString("0ae7cfcc-fa8f-4a9e-a531-ae85d76d9c59", "Tax Date:    {0}    |    {1}", charge.JR_CostTaxDate, ((ZDate)consolCost[JobConsolCostSchema.E6_TaxDate.Name])));
				}
				if (charge.JR_A9_CostVATClass != (ZGuid)consolCost[JobConsolCostSchema.E6_A9_VATClass])
				{
					isExistingDataWithoutChange = isExistingDataWithoutChange && !charge.JR_A9_CostVATClassInfo.HasChanges && !consolCost.ZPropertyInfoHash[JobConsolCostSchema.E6_A9_VATClass.Name].HasChanges;
					msg = MultilingualString.Join(System.Environment.NewLine, msg, ResString.GetMultilingualString("ce62ceda-65e2-40b5-a3eb-c7ae09b7f615", "Tax Class:    {0}    |    {1}", charge.CostVATClass != null ? charge.CostVATClass.A9_Code : EmptyText, consolTaxClass != null ? consolTaxClass.A9_Code : EmptyText));
				}
				if (charge.JR_CostSupplyType != (ZString)consolCost[JobConsolCostSchema.E6_SupplyType])
				{
					isExistingDataWithoutChange = isExistingDataWithoutChange && !charge.JR_CostSupplyTypeInfo.HasChanges && !consolCost.ZPropertyInfoHash[JobConsolCostSchema.E6_SupplyType.Name].HasChanges;
					msg = MultilingualString.Join(System.Environment.NewLine, msg, ResString.GetMultilingualString("FD83FDCC-7D7A-4D24-91A6-F7027F15691C", "Supply Type:    {0}    |    {1}", charge.JR_CostSupplyType, (ZString)consolCost[JobConsolCostSchema.E6_SupplyType.Name]));
				}
				if (charge.JR_GB_CostTaxBranch != (ZGuid)consolCost[JobConsolCostSchema.E6_GB_CostTaxBranch])
				{
					isExistingDataWithoutChange = isExistingDataWithoutChange && !charge.JR_GB_CostTaxBranchInfo.HasChanges && !consolCost.ZPropertyInfoHash[JobConsolCostSchema.E6_GB_CostTaxBranch.Name].HasChanges;
					msg = MultilingualString.Join(System.Environment.NewLine, msg, ResString.GetMultilingualString("01ae0298-1e25-49be-a7cc-d6f629eb50d6", "Tax Branch:    {0}    |    {1}", charge.CostTaxBranch?.GB_Code ?? EmptyText, consolTaxBranch?.GB_Code ?? EmptyText));
				}

				var isConsolCostPosted = !((ZGuid)consolCost[JobConsolCostSchema.Constants.E6_AH_APInvoice]).IsEmpty;
				if (charge.IsCostPosted != isConsolCostPosted)
				{
					isExistingDataWithoutChange = isExistingDataWithoutChange && !charge.JR_AL_APLineInfo.HasChanges && !consolCost.ZPropertyInfoHash[JobConsolCostSchema.E6_AH_APInvoice.Name].HasChanges;
					msg = MultilingualString.Join(System.Environment.NewLine, msg, ResString.GetMultilingualString("3a0eef53-cabe-4576-a9f9-7512077fa308", "Charge Cost:{0}    |    Consol Cost:{1}", charge.IsCostPosted ? "Posted" : "Not Posted", isConsolCostPosted ? "Posted" : "Not Posted"));
				}
			}

			msg = !msg.IsEmpty ? MultilingualString.Join("", (NoResString)System.Environment.NewLine, PrefixText, msg) : (NoResString)"";

			return msg;
		}

		static MultilingualString PrefixText
		{
			get
			{
				return ResString.GetMultilingualString("30142d1f-2081-4246-b1f2-d4b79bf0de06", @"Mismatched fields are listed below -->
				
Field Name:    Values in Charge    |    Values in Consol Cost
---------------------------------------------------------------------------------------------------------------------------");
			}
		}

		static MultilingualString EmptyText
		{
			get { return ResString.GetMultilingualString("d207fb5e-6e5b-4b7e-8a27-24cf462872bd", "<empty>"); }
		}
		#endregion

		internal static bool AreDateTimesEqualSafe(ZPropertyInfo dateInfo1, ZPropertyInfo dateInfo2)
		{
			var compareAsSmallDateTime = dateInfo1.IsDbColumnSmallDateTime && dateInfo2.IsDbColumnSmallDateTime;
			return GetSafeDateTimeValueForComparison(dateInfo1, compareAsSmallDateTime) == GetSafeDateTimeValueForComparison(dateInfo2, compareAsSmallDateTime);
		}

		static ZDateTime GetSafeDateTimeValueForComparison(ZPropertyInfo datePropertyInfo, bool compareAsSmallDateTime)
		{
			var value = (ZDateTime)datePropertyInfo.Value;

			if (!value.IsValid)
			{
				return ZDateTime.MinSmallDateTimeValue;
			}

			return compareAsSmallDateTime && datePropertyInfo.IsDbColumnSmallDateTime && value.IsValidSmallDateTime ? value.ToSmallDateTimeFloor() : value;
		}

		public static string ToAUString(this ZDateTime date)
		{
			var auCulture = new CultureInfo("en-AU");
			return string.Format(auCulture, "{0}", date);
		}

		public static string GetInvoiceApprovalInfo(this AccTransactionHeader header)
		{
			var query = new ZDBOnlyQuery(typeof(GenApprovalRequest));
			query.AddToFilter(GenApprovalRequestSchema.XP_ParentID, header.PK);
			query.AddToFilter(GenApprovalRequestSchema.XP_ParentTableCode, AccTransactionHeaderSchema.Constants.Prefix);

			var requests = (new BusinessObjectFactory()).Load<GenApprovalRequest>(query);
			var builder = new ZStringBuilder();

			foreach (var request in requests)
			{
				builder.Append(request.GetInvoiceApprovalInfo());
			}

			return builder.ToStringWithNewLineBetweenAppends();
		}

		public static string GetInvoiceApprovalInfo(this GenApprovalRequest approvalRequest)
		{
			return Invariant($@"Approval Request: PK = {approvalRequest.PK}, Approval Status = {approvalRequest.XP_ApprovalStatus}, Approval Type = {approvalRequest.XP_ApprovalType}, Approval Date = {approvalRequest.XP_ApprovalDate}, Reason Code = {approvalRequest.XP_ReasonCode}, Reason Description = {approvalRequest.XP_ReasonDescription}, Request ID = {approvalRequest.XP_RequestID}, Create Date = {approvalRequest.XP_SystemCreateTimeUtc}, Create User = {approvalRequest.XP_SystemCreateUser}");
		}
	}

	#endregion
}
