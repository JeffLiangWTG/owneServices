using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Application;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.GUI
{
	public static class AddressSourceType
	{
		public const string CusCAeMHHouseCode = "CCH";
		public const string CusDecHouseBillCode = "CDH";
		public const string JobOrderHeaderCode = "ORD";
		public const string JobComInvoiceLineCode = "INV";
		public const string RateOneOffShipmentCode = "RSH";
		public const string WhsDocketCode = "WRC";
		static readonly object syncLock = new object();

		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		static CodeDescriptionPairList addressSourceList;

		public static CodeDescriptionPairList AddressSourcePairList
		{
			get
			{
				if (addressSourceList == null)
				{
					lock (syncLock)
					{
						if (addressSourceList == null)
						{
							addressSourceList = new CodeDescriptionPairList();

							var codeDescriptionPairList = ObjectFactory.Get<IWorkflowDescriptorList>().Cast<CodeDescriptionPair>();
							foreach (var codeDescriptionPair in codeDescriptionPairList)
							{
								if (workflowDescriptorCode.Contains(codeDescriptionPair.Code))
								{
									addressSourceList.Add(codeDescriptionPair);
								}
							}
						}
					}
				}

				return addressSourceList;
			}
		}

		internal static string[] GetAddressSourceNameFromCode(string code)
		{
			var parentType = Array.Empty<string>();

			if (addressSourceCodeAndName.ContainsKey(code))
			{
				parentType = addressSourceCodeAndName[code];
			}

			return parentType;
		}

		static readonly List<string> workflowDescriptorCode = new List<string>
		{
			WorkflowDescriptors.JobConsolWorkflowDescriptorCode,
			WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode,
			WorkflowDescriptors.DtbBookingConsolidationWorkflowDescriptorCode,
			WorkflowDescriptors.DtbBookingWorkflowDescriptorCode,
			WorkflowDescriptors.WhsOrderWorkflowDescriptorCode
		};
		static readonly Dictionary<string, string[]> addressSourceCodeAndName = new Dictionary<string, string[]>
		{
			{ WorkflowDescriptors.AccPayableOrderHeaderCode, new[] { AccPayableOrderHeaderSchema.Constants.TableName } },
			{ WorkflowDescriptors.CusInBondHeaderWorkflowDescriptorCode, new[] { CusInBondBillSchema.Constants.TableName } },
			{ WorkflowDescriptors.CusISFHeaderWorkflowDescriptorCode, new[] { CusISFHeaderSchema.Constants.TableName } },
			{ WorkflowDescriptors.JobDeclarationWorkflowDescriptorCode, new[] { JobDeclarationSchema.Constants.TableName } },
			{ JobInvoicingConsumerTypes.LocalCartage.Code, new[] { JobCartageSchema.Constants.TableName } },
			{ WorkflowDescriptors.JobConsolWorkflowDescriptorCode, new[] { JobConsolSchema.Constants.TableName } },
			{ WorkflowDescriptors.BillOfLadingWorkflowDescriptorCode, new[] { JPAFRBillsSchema.Constants.TableName } },
			{ WorkflowDescriptors.JPAFRWorkflowDescriptorCode, new[] { JPAFRHeaderSchema.Constants.TableName } },
			{ WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode, new[] { JobShipmentSchema.Constants.TableName } },
			{ WorkflowDescriptors.CartageLegWorkflowDescriptorCode, new[] { JobContainerLegsSchema.Constants.TableName } },
			{ WorkflowDescriptors.DtbBookingConsolidationWorkflowDescriptorCode, new[] { DtbBookingConsolidationSchema.Constants.TableName } },
			{ WorkflowDescriptors.DtbBookingWorkflowDescriptorCode, new[] { DtbBookingSchema.Constants.TableName, DtbBookingInstructionSchema.Constants.TableName } },
			{ WorkflowDescriptors.QuotationWorkflowDescriptorCode, new[] { RatingHeaderSchema.Constants.TableName } },
			{ CusCAeMHHouseCode, new[] { CusCAeMHHouseSchema.Constants.TableName } },
			{ CusDecHouseBillCode, new[] { CusDecHouseBillSchema.Constants.TableName } },
			{ JobOrderHeaderCode, new[] { JobOrderHeaderSchema.Constants.TableName } },
			{ JobComInvoiceLineCode, new[] { JobComInvoiceLineSchema.Constants.TableName } },
			{ RateOneOffShipmentCode, new[] { RateOneOffShipmentSchema.Constants.TableName } },
			{ WhsDocketCode, new[] { WhsDocketSchema.Constants.TableName } }
		};
	}
}
