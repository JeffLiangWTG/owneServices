using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Integration.Customs.Shared;

namespace Enterprise.MasterFiles.Business
{
	public class OrgInvoiceTypeHelper
	{
		public OrgInvoiceTypeHelper(IOrgInvoiceType parent)
		{
			this.parent = parent;
		}

		protected readonly IOrgInvoiceType parent;

		#region Look Ups
		public virtual CodeDescriptionPairList JobTypeList
		{
			get
			{
				if (fJobTypes == null)
				{
					fJobTypes = JobInvoicingConsumerTypes.NewOnlyJobInvoicingTypes();
					fJobTypes.Remove(JobInvoicingConsumerTypes.ForwardingConsol);
					fJobTypes.Remove(JobInvoicingConsumerTypes.TransportBooking);

					if (ObjectFactory.Get<IAccounting>().IsMiscInvoiceInPeriodicInvoiceEnabled(GlbCompany.CurrentCompany.PK.ToGuid()))
					{
						fJobTypes.Add(new CodeDescriptionPair(InvoiceTypeModuleList.Codes.MSC, InvoiceTypeModuleList.Descriptions.MSC));
					}

					if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode != Core.Constants.CountryCodes.UnitedStates)
					{
						fJobTypes.RemoveCode(JobInvoicingConsumerTypes.ImporterSecurityFiling);
					}

					fJobTypes.Sort();
					fJobTypes.Insert(0, OrgInvoiceRollupOrGroupLookups.JobType_List.All);
				}
				return fJobTypes;
			}
		}

		JobInvoicingConsumerTypes fJobTypes;

		public virtual CodeDescriptionPairList TransportModeList
		{
			get
			{
				var transportModeList = new CodeDescriptionPairList();
				if (parent.JobType == JobInvoicingConsumerTypes.Brokerage.Code)
				{
					var transportModeProvider = System.Activator.CreateInstance(ObjectFactory.GetType<IDeclarationTransportModeCodeDescriptionPairProvider>()) as IDeclarationTransportModeCodeDescriptionPairProvider;
					if (transportModeProvider != null)
					{
						transportModeList.AddRange(((ICodeDescriptionPairListProvider)transportModeProvider).CodeDescriptionPairList);
						transportModeList.Sort();
						transportModeList.Insert(0, new CodeDescriptionPair(OrgConstants.ModesForGroupOrSubTotal.Codes.All, OrgDescriptions.ModesForGroupOrSubTotal.All));
					}
				}
				else
				{
					transportModeList = new CodeDescriptionPairList(OLookUpEditType.TransportType);
					transportModeList.Sort();
					transportModeList.Insert(0, new CodeDescriptionPair(OrgConstants.ModesForGroupOrSubTotal.Codes.All, OrgDescriptions.ModesForGroupOrSubTotal.All));
				}
				return transportModeList;
			}
		}

		public CodeDescriptionPairList ServiceDirectionList
		{
			get
			{
				CodeDescriptionPairList list = new CodeDescriptionPairList();
				if (parent.JobType == JobInvoicingConsumerTypes.Brokerage.Code)
				{
					var messageTypeProvider = System.Activator.CreateInstance(ObjectFactory.GetType<IDeclarationMessageTypeCodeDescriptionPairProvider>()) as IDeclarationMessageTypeCodeDescriptionPairProvider;
					if (messageTypeProvider != null)
					{
						list.AddRange(((ICodeDescriptionPairListProvider)messageTypeProvider).CodeDescriptionPairList);
						list.Sort();
						list.Insert(0, new CodeDescriptionPair(OrgConstants.ModesForGroupOrSubTotal.Codes.All, OrgDescriptions.ModesForGroupOrSubTotal.All));
					}
				}
				else
				{
					list.AddPair(OrgConstants.ServiceDirection.Code.All, OrgDescriptions.ServiceDirection.All);
					list.AddPair(OrgConstants.ServiceDirection.Code.Export, OrgDescriptions.ServiceDirection.Export);
					list.AddPair(OrgConstants.ServiceDirection.Code.Import, OrgDescriptions.ServiceDirection.Import);
					list.AddPair(OrgConstants.ServiceDirection.Code.Domestic, OrgDescriptions.ServiceDirection.Domestic);
					list.AddPair(OrgConstants.ServiceDirection.Code.CrossTrade, OrgDescriptions.ServiceDirection.CrossTrade);
					list.Sort();
				}
				return list;
			}
		}

		[SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "No need to be translated")]
		public CodeDescriptionPairList ServiceLevelList
		{
			get
			{
				var serviceLevelList = new CodeDescriptionPairList();
				var collection = new ActiveServiceLevelCollection(parent.Factory ?? new BusinessObjectFactory());
				collection.ForEach(x => serviceLevelList.AddPair(x.RS_Code, x.RS_Description));
				serviceLevelList.Sort();
				serviceLevelList.Insert(0, new CodeDescriptionPair("", "ALL"));
				return serviceLevelList;
			}
		}

		#endregion

		#region Validations
		public virtual void ValidateJobType()
		{
			CheckEnteredAndValidCode(parent.JobTypeInfo, parent.JobTypeList);
			CheckDuplicatedLines(parent.JobTypeInfo);
		}

		public virtual void ValidateServiceDirection()
		{
			CheckEnteredAndValidCode(parent.ServiceDirectionInfo, parent.ServiceDirectionList);
			CheckDuplicatedLines(parent.ServiceDirectionInfo);
		}

		public virtual void ValidateTransportMode()
		{
			CheckEnteredAndValidCode(parent.TransportModeInfo, parent.TransportModeList);
			CheckDuplicatedLines(parent.TransportModeInfo);
		}

		public virtual void ValidateServiceLevel()
		{
			ListValidation.ErrorIfInvalidCode(parent.ServiceLevelInfo, parent.ServiceLevelList);
			CheckDuplicatedLines(parent.ServiceLevelInfo);
		}

		#endregion

		public virtual string DuplicateRowErrorMessage
		{
			get { return Res.GetString("5a8d6629-a530-4fe4-bc4b-80d3d3030973", "Can NOT have more than one Line with the same Job Type, Direction, Mode and Service Level"); }
		}

		protected void CheckEnteredAndValidCode(ZPropertyInfo propertyInfo, ICodeDescriptionPairList list)
		{
			MandatoryValidation.CheckEntered(propertyInfo);
			ListValidation.ErrorIfInvalidCode(propertyInfo, list);
		}

		protected virtual void CheckDuplicatedLines(ZPropertyInfo propertyInfo)
		{
			if (parent.ParentCollection != null && !parent.JobType.IsEmpty && !parent.ServiceDirection.IsEmpty && !parent.TransportMode.IsEmpty)
			{
				if ((from IOrgInvoiceType item in parent.ParentCollection where item != parent select (item.JobType == parent.JobType && item.ServiceDirection == parent.ServiceDirection && item.TransportMode == parent.TransportMode && item.ServiceLevel == parent.ServiceLevel)).Any(x => x))
				{
					propertyInfo.AddError(parent.DuplicateRowErrorMessage);
				}
			}
		}

		public virtual void OnSettingJobType(ZString jobType)
		{
			if (!(jobType == JobInvoicingConsumerTypes.Shipment.Code ||
				jobType == JobInvoicingConsumerTypes.Brokerage.Code ||
				jobType == OrgInvoiceRollupOrGroupLookups.JobType_List.ShipmentAndBrokerage.Code))
			{
				parent.ServiceDirection = OrgConstants.ServiceDirection.Code.All;
				parent.TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.All;
				parent.ServiceLevel = ZString.Empty;
			}

			parent.ServiceDirectionInfo.RefreshBinding();
			parent.TransportModeInfo.RefreshBinding();
			parent.ServiceLevelInfo?.RefreshBinding();
		}

		public bool ServiceDirection_ReadOnly
		{
			get { return !IsShipmentBrokerageOrBoth; }
		}

		public bool TransportMode_ReadOnly
		{
			get { return !IsShipmentBrokerageOrBoth; }
		}

		bool IsShipmentBrokerageOrBoth
		{
			get { return OrgInvoiceRollupOrGroupLookups.IsJobTypeShipmentBrokerageOrBoth(parent.JobType); }
		}

		public bool ServiceLevel_ReadOnly => !CanSupportServiceLevel();

		bool CanSupportServiceLevel()
		{
			return parent.JobType == JobInvoicingConsumerTypes.AgencyBookingCode
				|| parent.JobType == JobInvoicingConsumerTypes.AgencyBillOfLadingCode
				|| parent.JobType == JobInvoicingConsumerTypes.ShipmentCode
				|| parent.JobType == JobInvoicingConsumerTypes.CFSShipmentCode
				|| parent.JobType == JobInvoicingConsumerTypes.BrokerageCode
				|| parent.JobType == JobInvoicingConsumerTypes.TransportConsignmentCode
				|| parent.JobType == JobInvoicingConsumerTypes.QuotedBookingCode
				|| parent.JobType == JobInvoicingConsumerTypes.LocalCartageCode
				|| parent.JobType == JobInvoicingConsumerTypes.WarehouseInwardsCode
				|| parent.JobType == JobInvoicingConsumerTypes.WarehouseOutwardsCode;
		}
	}

	public class InvoiceRollupOrGroupHelper : OrgInvoiceTypeHelper
	{
		public InvoiceRollupOrGroupHelper(IInvoiceRollupOrGroup parent)
			: base(parent)
		{
		}

		protected IInvoiceRollupOrGroup Parent
		{
			get { return base.parent as IInvoiceRollupOrGroup; }
		}

		#region Properties

		public override CodeDescriptionPairList TransportModeList
		{
			get
			{
				CodeDescriptionPairList list = new CodeDescriptionPairList();
				list.AddPair(OrgConstants.ModesForGroupOrSubTotal.Codes.All, OrgDescriptions.ModesForGroupOrSubTotal.All);
				list.AddPair(OrgConstants.ModesForGroupOrSubTotal.Codes.Air, OrgDescriptions.ModesForGroupOrSubTotal.Air);
				list.AddPair(OrgConstants.ModesForGroupOrSubTotal.Codes.Sea, OrgDescriptions.ModesForGroupOrSubTotal.Sea);
				list.AddPair(OrgConstants.ModesForGroupOrSubTotal.Codes.FCL, OrgDescriptions.ModesForGroupOrSubTotal.FCL);
				list.AddPair(OrgConstants.ModesForGroupOrSubTotal.Codes.LCL, OrgDescriptions.ModesForGroupOrSubTotal.LCL);
				list.AddPair(OrgConstants.ModesForGroupOrSubTotal.Codes.Rail, OrgDescriptions.ModesForGroupOrSubTotal.Rail);
				list.AddPair(OrgConstants.ModesForGroupOrSubTotal.Codes.Road, OrgDescriptions.ModesForGroupOrSubTotal.Road);
				list.AddPair(OrgConstants.ModesForGroupOrSubTotal.Codes.Courier, OrgDescriptions.ModesForGroupOrSubTotal.Courier);
				list.Sort();

				return list;
			}
		}

		public override CodeDescriptionPairList JobTypeList
		{
			get { return OrgInvoiceRollupOrGroupLookups.JobTypeFullList; }
		}

		public bool InvoicePostingStyle_ReadOnly
		{
			get { return Parent.JobType == OrgInvoiceRollupOrGroupLookups.JobType_List.NonJobRelated.Code; }
		}

		public bool InvoiceLineDisplayOption_ReadOnly
		{
			get { return Parent.JobType == OrgInvoiceRollupOrGroupLookups.JobType_List.NonJobRelated.Code; }
		}

		public override void OnSettingJobType(ZString jobType)
		{
			base.OnSettingJobType(jobType);
			if (Parent.JobType == OrgInvoiceRollupOrGroupLookups.JobType_List.NonJobRelated.Code)
			{
				Parent.InvoiceLineDisplayOption = InvoiceDescriptionOptionsList.Codes.None;
				Parent.InvoicePostingStyle = ZString.Empty;
			}

			Parent.InvoiceLineDisplayOptionInfo.RefreshBinding();
			Parent.InvoicePostingStyleInfo.RefreshBinding();
		}

		public bool GroupOrSubtotalStyle_ReadOnly
		{
			get
			{
				bool result = false;
				if (Parent.GroupOrSubTotal != "")
				{
					result = IsGroupOrSubTotalDoNotEqualAnyValueFromList;
				}
				return result;
			}
		}

		public override string DuplicateRowErrorMessage
		{
			get { return Res.GetString("3f8340e6-0a96-4d28-a428-5d1a1ab9997b", "Can NOT have more than one Invoice Group or Sub Total Charge with the same Job Type, Direction and Mode"); }
		}

		protected virtual bool IsGroupOrSubTotalDoNotEqualAnyValueFromList
		{
			get
			{
				var groupOrSubTotal = Parent.GroupOrSubTotal;
				return groupOrSubTotal != OrgConstants.GroupOrSubTotalCharges.Code.RollUp &&
					groupOrSubTotal != OrgConstants.GroupOrSubTotalCharges.Code.RollUpEntireConsol &&
					groupOrSubTotal != OrgConstants.GroupOrSubTotalCharges.Code.SubTotal &&
					groupOrSubTotal != OrgConstants.GroupOrSubTotalCharges.Code.SubTotalAndSequence &&
					groupOrSubTotal != OrgConstants.GroupOrSubTotalCharges.Code.RollupAndSequence &&
					!IsGroupOrSubTotalOnlyForCLCAndNOG(groupOrSubTotal);
			}
		}

		public bool IsGroupOrSubTotalOnlyForCLCAndNOG(ZString groupOrSubTotal)
		{
			return groupOrSubTotal == OrgConstants.GroupOrSubTotalCharges.Code.Alphabetical ||
					Parent.GroupOrSubTotal == OrgConstants.GroupOrSubTotalCharges.Code.Sequence ||
					Parent.GroupOrSubTotal == OrgConstants.GroupOrSubTotalCharges.Code.User;
		}

		#endregion

		#region Lookups

		public CodeDescriptionPairList InvoicePostingOptionsList
		{
			get
			{
				return new InvoicePostingOptionsList();
			}
		}

		public CodeDescriptionPairList InvoiceLineDisplayOptionsList
		{
			get
			{
				return new InvoiceDescriptionOptionsList();
			}
		}

		public CodeDescriptionPairList GroupOrSubTotalList
		{
			get
			{
				CodeDescriptionPairList list = new CodeDescriptionPairList();
				list.AddPair(OrgConstants.GroupOrSubTotalCharges.Code.Alphabetical, OrgDescriptions.GroupOrSubTotalCharges.Alphabetical);
				list.AddPair(OrgConstants.GroupOrSubTotalCharges.Code.Sequence, OrgDescriptions.GroupOrSubTotalCharges.Sequence);
				list.AddPair(OrgConstants.GroupOrSubTotalCharges.Code.SubTotalAndSequence, OrgDescriptions.GroupOrSubTotalCharges.SubTotalAndSequence);
				list.AddPair(OrgConstants.GroupOrSubTotalCharges.Code.RollUp, OrgDescriptions.GroupOrSubTotalCharges.RollUp);
				list.AddPair(OrgConstants.GroupOrSubTotalCharges.Code.SubTotal, OrgDescriptions.GroupOrSubTotalCharges.SubTotal);
				list.AddPair(OrgConstants.GroupOrSubTotalCharges.Code.User, OrgDescriptions.GroupOrSubTotalCharges.User);
				list.AddPair(OrgConstants.GroupOrSubTotalCharges.Code.RollupAndSequence, OrgDescriptions.GroupOrSubTotalCharges.RollupAndSequence);

				if (Parent.JobType == JobInvoicingConsumerTypes.ForwardingConsol.Code || Parent.JobType == JobInvoicingConsumerTypes.GatewayConsol.Code)
				{
					list.AddPair(OrgConstants.GroupOrSubTotalCharges.Code.RollUpEntireConsol, OrgDescriptions.GroupOrSubTotalCharges.RollUpEntireConsol);
				}
				list.Sort();

				return list;
			}
		}

		public CodeDescriptionPairList GroupOrSubTotalStyleList
		{
			get
			{
				var result = new CodeDescriptionPairList();

				if (IsGroupOrSubTotalOnlyForCLCAndNOG(Parent.GroupOrSubTotal))
				{
					result.AddPair(OrgConstants.InvoiceLineGroupings.Code.None, OrgDescriptions.InvoiceLineGroupings.None);
					result.AddPair(OrgConstants.InvoiceLineGroupings.Code.CLC, OrgDescriptions.InvoiceLineGroupings.CLC);
				}
				else
				{
					result = OrgInvoiceRollupOrGroupLookups.GetGroupOrSubTotalStyleList(Parent.JobType);
					ProcessForDisallowedInvoiceLineGroupingsForGroupOrSubTotal(result);
				}

				return result;
			}
		}

		void ProcessForDisallowedInvoiceLineGroupingsForGroupOrSubTotal(CodeDescriptionPairList list)
		{
			var groupOrSubTotal = Parent.GroupOrSubTotal;
			if (!groupOrSubTotal.IsEmpty && disallowedInvoiceLineGroupingsForGroupOrSubTotal.ContainsKey(groupOrSubTotal) && list != null && list.Count > 0)
			{
				var disallowedInvoiceLineGroupings = disallowedInvoiceLineGroupingsForGroupOrSubTotal[groupOrSubTotal];

				if (disallowedInvoiceLineGroupings != null && disallowedInvoiceLineGroupings.Length > 0)
				{
					disallowedInvoiceLineGroupings.ToList().ForEach(style => list.RemoveCode(style));
				}
			}
		}

		readonly Dictionary<ZString, ZString[]> disallowedInvoiceLineGroupingsForGroupOrSubTotal = new Dictionary<ZString, ZString[]>()
		{
			{ OrgConstants.GroupOrSubTotalCharges.Code.RollupAndSequence, new ZString[] { OrgConstants.InvoiceLineGroupings.Code.None } }
		};

		public RefCurrencyCollection InvoicePostingCurrencies
		{
			get { return new RefCurrencyCollection(parent.Factory ?? new BusinessObjectFactory()); }
		}

		#endregion

		#region Validation

		public void ValidateInvoicePostingStyle()
		{
			if (Parent.JobType != OrgInvoiceRollupOrGroupLookups.JobType_List.NonJobRelated.Code)
			{
				CheckEnteredAndValidCode(Parent.InvoicePostingStyleInfo, Parent.InvoicePostingOptionsList);
			}
		}

		public void ValidateInvoiceLineDisplayOption()
		{
			CheckEnteredAndValidCode(Parent.InvoiceLineDisplayOptionInfo, Parent.InvoiceLineDisplayOptionsList);
		}

		public void ValidateGroupOrSubtotalStyle()
		{
			CheckEnteredAndValidCode(Parent.GroupOrSubtotalStyleInfo, Parent.GroupOrSubTotalStyleList);
		}

		public void ValidateGroupOrSubTotal()
		{
			CheckEnteredAndValidCode(Parent.GroupOrSubTotalInfo, Parent.GroupOrSubTotalList);
		}

		public void ValidateInvoicePostingCurrency()
		{
			ListValidation.ErrorIfInvalidCode(Parent.InvoicePostingCurrencyInfo);
		}

		public override void ValidateServiceLevel()
		{
			/*
			 * No need to vilidate service level.
			 * Because we do not use it for for InvoiceRollupOrGroupHelper yet.
			*/
		}

		#endregion

		protected override void CheckDuplicatedLines(ZPropertyInfo propertyInfo)
		{
			if (parent.ParentCollection != null && !parent.JobType.IsEmpty && !parent.ServiceDirection.IsEmpty && !parent.TransportMode.IsEmpty)
			{
				foreach (IInvoiceRollupOrGroup rollupOrGroup in parent.ParentCollection)
				{
					if (parent.PK != rollupOrGroup.PK && parent.JobType == rollupOrGroup.JobType &&
						parent.ServiceDirection == rollupOrGroup.ServiceDirection && parent.TransportMode == rollupOrGroup.TransportMode)
					{
						propertyInfo.AddError(parent.DuplicateRowErrorMessage);
						break;
					}
				}
			}
		}
	}
}
