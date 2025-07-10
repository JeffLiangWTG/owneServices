using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.SG.Access.Business;
using Enterprise.Customs.SG.Access.Business.UniversalDataTransfer;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using static System.FormattableString;

namespace Enterprise.Customs.SG.Access.GUI
{
	public class MultiManifestBillSender : AutoMultiManifestBillSender, ICycleDetailSupporter
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public MultiManifestBillSender(IEnumerable<AsycudaBill> bills)
			: base(new BusinessObjectFactory())
		{
			MasterDatas = bills.GroupBy(x => x.Header).Select(x => new MasterData() { HeaderPK = x.Key.PK, DisplayValue = Invariant($"{x.Key.AMA_JobReference} - {x.Key.AMA_MasterBill}"), BillDatas = x.Distinct().OrderBy(b => b.ABL_BillNumber).Select(b => new BillData() { BillPK = b.PK, DisplayValue = b.ABL_BillNumber }).ToArray() }).OrderBy(x => x.DisplayValue).ToArray();
		}

		public class MasterData
		{
			public ZString DisplayValue { get; internal set; }
			public ZGuid HeaderPK { get; internal set; }
			public IEnumerable<BillData> BillDatas { get; internal set; }
		}

		public class BillData
		{
			public ZString DisplayValue { get; internal set; }
			public ZGuid BillPK { get; internal set; }
		}

		public readonly IEnumerable<MasterData> MasterDatas;

		[List(nameof(Lookups) + "." + nameof(MultiManifestBillSenderLookups.CycleNumbers))]
		public override ZString CycleNumber { get => base.CycleNumber; set => base.CycleNumber = value; }

		public delegate void UpdateProgressDelegate(string status, int percentComplete);

		public UpdateProgressDelegate UpdateProgress;

		public void CancelSendingTheRest(object sender, EventArgs e)
		{
			cancelSending = true;
		}
		bool cancelSending;

		public ZString Send()
		{
			cancelSending = false;
			var missingManifestResult = new ZStringBuilder();
			var sentManifestResult = new ZStringBuilder();
			var errorManifestResult = new ZStringBuilder();
			var cancelManifestResult = new ZStringBuilder();
			var totalJobs = MasterDatas.Count();
			var count = 0m;
			var securityCheckPoint = Env.Security.GlobalManifestSendWithMessageErrors;
			var canSendWithError = securityCheckPoint.IsAllowed;
			foreach (var masterData in MasterDatas)
			{
				if (cancelSending)
				{
					cancelManifestResult.Append(masterData.DisplayValue);
				}
				else
				{
					count++;
					var percentage = count / totalJobs * 100m;
					var factory = new BusinessObjectFactory();
					(AsycudaManifestHeader header, ActiveBusinessObjectCollection<AsycudaBill> bills) = GetManifest(factory, masterData);
					if (header == null)
					{
						UpdateProgressInfo(Res.GetString("{AB692A57-402C-4C6A-9BAE-586CB9D29902}", "System was unable to load manifest '{0}'.", masterData.DisplayValue), percentage);
						missingManifestResult.Append(masterData.DisplayValue);
					}
					else if (IsValidToSend(canSendWithError, header, masterData.DisplayValue, percentage))
					{
						if (SendAndSave(header, bills, masterData.DisplayValue, percentage))
						{
							sentManifestResult.Append(masterData.DisplayValue);
						}
						else
						{
							errorManifestResult.Append(Res.GetString("{EE9687AC-6751-4A64-BF29-5B802702A4B6}", "The Bills chosen to send have no Packs requiring notification to Customs."));
						}
					}
					else
					{
						errorManifestResult.Append(masterData.DisplayValue);
					}
				}
			}

			var result = new ZStringBuilder();
			if (!missingManifestResult.IsEmpty)
			{
				missingManifestResult.Prepend(Res.GetString("{01A40440-D48B-44A3-A927-889DEF5D21D4}", "System was unable to load the following manifests:"));
				result.Append(missingManifestResult.ToStringWithNewLineBetweenAppends());
			}

			if (!errorManifestResult.IsEmpty)
			{
				if (!canSendWithError)
				{
					result.Append(securityCheckPoint.ErrorMessageForNotAllowed);
				}

				errorManifestResult.Prepend(Res.GetString("{10221F37-9866-4276-A247-C8B368E714D3}", "The following manifests could not be sent due to errors:"));
				result.Append(errorManifestResult.ToStringWithNewLineBetweenAppends());
			}

			if (!sentManifestResult.IsEmpty)
			{
				sentManifestResult.Prepend(Res.GetString("{0CA48BE1-634E-46E3-B9A4-5207219769A4}", "The following manifests were sent successfully:"));
				result.Append(sentManifestResult.ToStringWithNewLineBetweenAppends());
			}

			if (!cancelManifestResult.IsEmpty)
			{
				cancelManifestResult.Prepend(Res.GetString("{9EA21F71-AEA4-4592-8895-8BC45D9562E8}", "The following manifests sending were aborted:"));
				result.Append(cancelManifestResult.ToStringWithNewLineBetweenAppends());
			}

			return result.ToStringWithNewLineBetweenAppends();
		}

		bool SendAndSave(AsycudaManifestHeader header, ActiveBusinessObjectCollection<AsycudaBill> bills, ZString manifestInfo, decimal percentage)
		{
			UpdateProgressInfo(Res.GetString("{D39BCD6B-D27D-4BD5-B41F-500E1632E13C}", "Sending manifest '{0}'.", manifestInfo), percentage);
			var context = header.GetCurrentManifestContext();
			var billsWithPackNeedSending = new List<AsycudaBill>();
			var packsNeedSending = new List<AsycudaPack>();
			foreach (var bill in bills)
			{
				var packs = bill.Packs.OfType<AsycudaPack>().Where(x => !ASYCUDA.Business.MessageStatusCodeList.HasBeenSentCustoms(x.PackedItem.API_MessageStatus)).ToArray();
				if (packs.Length > 0)
				{
					billsWithPackNeedSending.Add(bill);
					packsNeedSending.AddRange(packs);
				}
			}

			context.ManifestSendBills = billsWithPackNeedSending;
			context.ManifestSendPacks = packsNeedSending;
			var packedItems = packsNeedSending.Select(x => (ASYCUDA.Business.IMessageParent)x.PackedItem).Where(x => x != null).ToList();
			var result = false;
			if (packedItems.Count > 0)
			{
				new SGAsycudaManifestUniversalMessagingHelper(new MenuBuilder.NullLogger(), this).SendViaEHub(header, header.AMA_ManifestType, MessageSubTypeCodes.Codes.Original, packedItems);
				result = true;
			}

			return result;
		}

		bool IsValidToSend(bool canSendWithError, AsycudaManifestHeader header, ZString manifestInfo, decimal percentage)
		{
			UpdateProgressInfo(Res.GetString("{7F9760CE-1982-4947-9968-414790946649}", "Validating manifest '{0}'.", manifestInfo), percentage);
			((IBusiness)header).RunPreSaveValidationFetch(false);
			var errorCollector = new MessageSendingValidation(header).CheckBusinessObjectLevelValidation().NotificationsAsString();
			return errorCollector.IsEmpty || (canSendWithError && Globals.Message.Show(errorCollector, Res.GetString("{D1EAA017-DADD-4B0A-8AA4-0E1C966BF448}", "Proceed with errors?"), MessageBoxButtons.YesNo, DialogResult.Yes) == DialogResult.Yes);
		}

		(AsycudaManifestHeader header, ActiveBusinessObjectCollection<AsycudaBill> bills) GetManifest(BusinessObjectFactory factory, MasterData masterData)
		{
			ActiveBusinessObjectCollection<AsycudaBill> bills = null;
			var header = factory.Load<AsycudaManifestHeader>(masterData.HeaderPK);
			if (header != null)
			{
				header.UnRegisterEditableChildObject(header.Bills);
				bills = new ActiveBusinessObjectCollection<AsycudaBill>(factory, new ZQuery(AsycudaBillSchema.PK, masterData.BillDatas.Select(x => x.BillPK)));
				header.RegisterEditableChildObject(bills);
				UpdateCycleDataAndCycleNumber(bills);
			}

			return (header, bills);
		}

		class MessageSendingValidation : Customs.Business.MessageSendingValidation
		{
			public MessageSendingValidation(AsycudaManifestHeader manifestHeader)
				: base(manifestHeader, null)
			{ }

			public new AsycudaManifestHeader TopLevelBusinessObjectForValidation => (AsycudaManifestHeader)base.TopLevelBusinessObjectForValidation;

			protected override MessageSendingNotificationCollection CheckBusinessObjectLevelValidationCore()
			{
				return CheckBusinessObjectLevelValidationCore(ErrorExistHeaderText, Res.GetString("{A3103D09-46A7-45BC-8825-B1B86943718D}", "Manifest '{0} - {1}' has the following message errors:", TopLevelBusinessObjectForValidation.AMA_JobReference, TopLevelBusinessObjectForValidation.AMA_MasterBill), MessageErrorConfirmationQuestionText);
			}
		}

		void UpdateCycleDataAndCycleNumber(ActiveBusinessObjectCollection<AsycudaBill> bills)
		{
			foreach (var bill in bills)
			{
				bill.CycleDate = CycleDate;
				bill.CycleNumber = CycleNumber;
			}
		}

		void UpdateProgressInfo(string status, decimal percentage)
		{
			if (UpdateProgress != null)
			{
				UpdateProgress(status, Math.Min(100, (int)percentage));
			}
		}

		public MultiManifestBillSenderLookups Lookups => fLookups ?? (fLookups = GetNewLookups());

		MultiManifestBillSenderLookups fLookups;

		protected virtual MultiManifestBillSenderLookups GetNewLookups()
		{
			return new MultiManifestBillSenderLookups(this);
		}

		ZBool ICycleDetailSupporter.RequiresCycleFields => true;
	}
}
