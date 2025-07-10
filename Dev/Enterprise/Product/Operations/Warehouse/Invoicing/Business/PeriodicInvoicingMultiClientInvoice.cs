using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.ZArchitecture.Data.Mutex;

namespace Enterprise.Warehouse.Invoicing.Business
{
	public class PeriodicInvoicingMultiClientInvoice : NonPersistentBusinessObject
	{
		public PeriodicInvoicingMultiClientInvoice(string storageType)
			: base(new BusinessObjectFactory())
		{
			WarehouseAndClientWithNoInvoices = new HashSet<OrgWarehousePair>();
			this.storageType = storageType;
		}

		readonly string storageType;

		#region	LoadOrCreateInvoices

		/// <summary>
		/// Load or create invoices for jobs have not invoiced yet.
		/// </summary>
		public void LoadOrCreateInvoices(bool selectBranchFromWarehouse = false)
		{
			InvoiceDate = ZDateTime.Today;
			var candidateInvoices = InvoiceHelper.GetCandidateInvoices(Factory, selectBranchFromWarehouse);

			WarehouseAndClientWithNoInvoices.Clear();
			WarehouseAndClientWithNoInvoices = new HashSet<OrgWarehousePair>(candidateInvoices.Where(c => !c.HasInvoice));

			GetOrCreateInvoiceForClientAndWarehouse(candidateInvoices);
		}

		#endregion

		#region Invoice Date

		public ZDateTime InvoiceDate
		{
			get => invoiceDate;
			set
			{
				SetNonPersistentPropertyValue(InvoiceDateInfo, ref invoiceDate, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateInvoiceDate();
				}

				foreach (PeriodicInvoicing invoice in Invoices)
				{
					invoice.ET_BillingDate = invoiceDate;
				}
			}
		}
		ZDateTime invoiceDate;

		public ZPropertyInfo InvoiceDateInfo => GetZPropertyInfo(nameof(InvoiceDate));

		#endregion

		#region RunPreSaveValidationCore

		protected override void RunPreSaveValidationCore()
		{
			RefreshInvoiceSelectionBeforeValidation();
			base.RunPreSaveValidationCore();
			Validation.ValidateAll();
		}

		void RefreshInvoiceSelectionBeforeValidation()
		{
			foreach (PeriodicInvoicing invoice in Invoices.ToArray())
			{
				if (!invoice.IncludeInInvoicing)
				{
					RemoveInvoiceItem(invoice);
				}
			}
		}

		#endregion

		#region Invoices

		public PeriodicInvoicingCollection Invoices
		{
			get
			{
				if (invoices == null)
				{
					invoices = new PeriodicInvoicingCollection(Factory, storageType);
					RegisterEditableChildObject(invoices);
				}
				return invoices;
			}
		}

		PeriodicInvoicingCollection invoices;

		void GetOrCreateInvoiceForClientAndWarehouse(IEnumerable<OrgWarehousePair> orgWarehousePairs)
		{
			foreach (var pair in orgWarehousePairs)
			{
				var result = InvoiceHelper.GetOldestUnpostedInvoice(FactoryForQuery, pair);

				if (result == null)
				{
					result = Factory.New<PeriodicInvoicing>();
					result.ET_OH_Client = pair.OrgPK;
					result.ET_WW = pair.WarehousePK;
				}
				else
				{
					result = Factory.Load<PeriodicInvoicing>(result.PK);
				}

				result.CustomReadOnly = true;
				result.ET_BillingDate = InvoiceDate;

				Invoices.Add(result);
			}
		}

		#endregion

		#region AutoRateInvoicesWithoutPosting

		public void AutoRateInvoices(CancellationToken token, bool selectBranchFromWarehouse = false)
		{
			AutoRateInvoicesCore(token, selectBranchFromWarehouse);
			OnInvoiceCreationComplete(Invoices.CountInvoicesToInclude, false);
		}

		#endregion

		#region AutoRateAndPostInvoices

		/// <summary>
		/// Create, Autorate and Post Invoices.
		/// </summary>
		public void AutoRateAndPostInvoices(CancellationToken token)
		{
			AutoRateInvoicesCore(token);

			var allPostedInvoices = new List<InvoicingBase>();
			var createIndex = 0;
			var includeCount = Invoices.CountInvoicesToInclude;

			foreach (PeriodicInvoicing invoice in Invoices)
			{
				var postedInvoices = invoice.PostInvoice();
				if (postedInvoices != null)
				{
					allPostedInvoices.AddRange(postedInvoices.ToArray<InvoicingBase>());
				}
				OnInvoiceCreated(++createIndex, includeCount);
			}

			Factory.Save();

			using (var printTask = new PeriodicInvoicingPrintTask(allPostedInvoices.ToArray()))
			{
				printTask.Run();
#if DEBUG
				LastPrintTaskCountForTest = printTask.TaskCount;
#endif
			}

			OnInvoiceCreationComplete(includeCount, true);
		}

		#region AutoRateInvoices

		void AutoRateInvoicesCore(CancellationToken token, bool selectBranchFromWarehouse = false)
		{
			var mutexesUsed = new List<IZGlobalMutex>();
			try
			{
				foreach (PeriodicInvoicing invoice in Invoices.ToArray())
				{
					token.ThrowIfCancellationRequested();
					if (!invoice.IncludeInInvoicing)
					{
						RemoveInvoiceItem(invoice);
					}
					else
					{
						if (selectBranchFromWarehouse && WarehouseAndClientWithNoInvoices.Contains(OrgWarehousePair.Create(invoice.ET_OH_Client, invoice.ET_WW)))
						{
							OnAutoRating(hasExistingInvoice: false, invoice);
						}
						else
						{
							if (selectBranchFromWarehouse)
							{
								using (PeriodicInvoicingHelper.SetUserContextForInvoice(invoice))
								{
									mutexesUsed.Add(AutoRateAndRemoveInvoiceWithNoCharge(invoice));
								}
							}
							else
							{
								// run under current branch
								mutexesUsed.Add(AutoRateAndRemoveInvoiceWithNoCharge(invoice));
							}
						}
					}
				}
				Factory.Save();
			}
			finally
			{
				mutexesUsed.ForEach(m => m.Dispose());
			}
		}

		IZGlobalMutex AutoRateAndRemoveInvoiceWithNoCharge(PeriodicInvoicing invoice)
		{
			var mutex = InvoiceHelper.GetInvoiceBillingAutomationMutex(invoice.PK);
			try
			{
				if (mutex.HasLock)
				{
					invoice.AddRowError(Res.GetString("cf41cba3-7135-4858-bc9b-443a35867042", "A service task or another user is autorating this invoice."));
				}
				else if (mutex.Lock())
				{
					using (invoice.InvoiceBillingCheckLockSuspender())
					{
						invoice.AutoRateJobHeader(null);
						var jobHeader = invoice.JobHeader;
						if (invoice.HasErrors || jobHeader.Charges.Count == 0)
						{
							jobHeader.Dispose();
							RemoveInvoiceItem(invoice);
						}
						else
						{
							OnAutoRating(hasExistingInvoice: true, invoice);
						}
					}
				}
				else
				{
					invoice.AddRowError(Res.GetString("b57b98e5-3be7-4dc9-a4db-0d482a8532b6", "Unable to acquire Lock to prevent other users Autorating this invoice at the same time."));
				}
			}
			catch
			{
				mutex.Dispose();
				throw;
			}
			return mutex;
		}

		#endregion

		void RemoveInvoiceItem(PeriodicInvoicing invoice)
		{
			if (invoice.IsInDatabase)
			{
				Invoices.Remove(invoice);
			}
			else
			{
				Invoices.RemoveAndDelete(invoice);
			}
		}

		#endregion

#if DEBUG
		internal int LastPrintTaskCountForTest;
#endif

		BusinessObjectFactory FactoryForQuery => factoryForQuery ?? (factoryForQuery = new BusinessObjectFactory());
		BusinessObjectFactory factoryForQuery;

		#region Events

		#region Invoice Created

		public event EventHandler<InvoiceCreatedEventArgs> InvoiceCreated;

		public class InvoiceCreatedEventArgs : EventArgs
		{
			public InvoiceCreatedEventArgs(int countCompleted, int totalCount)
			{
				CountCompleted = countCompleted;
				TotalCount = totalCount;
			}

			public readonly int CountCompleted;
			public readonly int TotalCount;
		}

		void OnInvoiceCreated(int countCompleted, int totalCount)
		{
			InvoiceCreated?.Invoke(this, new InvoiceCreatedEventArgs(countCompleted, totalCount));
		}

		#endregion

		#region Invoice Creation Complete

		public event EventHandler<TextEventArgs> InvoiceCreationComplete;

		void OnInvoiceCreationComplete(int countCompleted, bool invoicesPosted)
		{
			if (InvoiceCreationComplete != null)
			{
				var message = "";
				if (countCompleted > 0)
				{
					if (invoicesPosted)
					{
						message = Res.GetString("530a55cd-537d-43a8-b3a9-9ee609a2158c", "Periodic Invoices rated and posted for relevant clients.");
					}
					else
					{
						message = Res.GetString("9af26e60-04dd-4829-92b1-a4e0cd97f22b", "Periodic Invoices rated for relevant clients.");
					}
				}
				else
				{
					message = Res.GetString("1fe31199-3e07-49a7-8c9a-104905727e46", "There are no clients that need to be invoiced at this time.");
				}

				InvoiceCreationComplete(this, new TextEventArgs(message));
			}
		}

		#endregion

		#region AutoRating

		public event EventHandler<AutoRatingEventArgs> AutoRating;

		void OnAutoRating(bool hasExistingInvoice, PeriodicInvoicing invoice)
		{
			AutoRating?.Invoke(this, new AutoRatingEventArgs(hasExistingInvoice, invoice));
		}

		#endregion

		#endregion

		#region Validation

		public PeriodicInvoicingMultiClientInvoiceValidation Validation => new(this);

		#endregion

		#region FirstTimeInvoiced

		HashSet<OrgWarehousePair> WarehouseAndClientWithNoInvoices;

		#endregion

		#region PeriodicInvoicingHelper

		IPeriodicInvoicingHelper InvoiceHelper => invoiceHelper ??= ObjectFactory.Get<IPeriodicInvoicingHelper>();
		IPeriodicInvoicingHelper invoiceHelper;

		#endregion
	}
}
