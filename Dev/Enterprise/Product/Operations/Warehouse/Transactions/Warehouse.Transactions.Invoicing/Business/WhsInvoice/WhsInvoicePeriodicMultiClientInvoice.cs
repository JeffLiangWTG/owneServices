using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Data.Mutex;

namespace Enterprise.Warehouse.Transactions.Invoicing
{
	public class WhsInvoicePeriodicMultiClientInvoice : NonPersistentBusinessObject
	{
		public WhsInvoicePeriodicMultiClientInvoice()
			: base(new BusinessObjectFactory())
		{
			WarehouseAndClientWithNoInvoices = new HashSet<OrgWarehousePair>();
		}

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

				foreach (WhsInvoice invoice in Invoices)
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
			foreach (WhsInvoice invoice in Invoices.ToArray())
			{
				if (!invoice.IncludeInInvoicing)
				{
					RemoveInvoiceItem(invoice);
				}
			}
		}

		#endregion

		#region Invoices

		public WhsInvoiceCollection Invoices
		{
			get
			{
				if (invoices == null)
				{
					invoices = new WhsInvoiceCollection(Factory);
					RegisterEditableChildObject(invoices);
				}
				return invoices;
			}
		}

		WhsInvoiceCollection invoices;

		void GetOrCreateInvoiceForClientAndWarehouse(IEnumerable<OrgWarehousePair> orgWarehousePairs)
		{
			foreach (var pair in orgWarehousePairs)
			{
				var result = InvoiceHelper.GetOldestUnpostedInvoice(FactoryForQuery, pair);

				if (result == null)
				{
					result = Factory.New<WhsInvoice>();
					result.ET_OH_Client = pair.OrgPK;
					result.ET_WW = pair.WarehousePK;
				}
				else
				{
					result = Factory.Load<WhsInvoice>(result.PK);
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

			foreach (WhsInvoice invoice in Invoices)
			{
				var postedInvoices = invoice.PostInvoice();
				if (postedInvoices != null)
				{
					allPostedInvoices.AddRange(postedInvoices.ToArray<InvoicingBase>());
				}
				OnInvoiceCreated(++createIndex, includeCount);
			}

			Factory.Save();

			using (var printTask = new WhsInvoicePrintTask(allPostedInvoices.ToArray()))
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
				foreach (WhsInvoice invoice in Invoices.ToArray())
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
								using (WhsInvoiceHelper.SetUserContextForInvoice(invoice))
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

		IZGlobalMutex AutoRateAndRemoveInvoiceWithNoCharge(WhsInvoice invoice)
		{
			var mutex = InvoiceHelper.GetInvoiceBillingAutomationMutex(invoice.PK);
			try
			{
				if (mutex.HasLock)
				{
					invoice.AddRowError(Res.GetString("410E78E7-275A-45DE-ACE1-BCFD3D19B16C", "A service task or another user is autorating this invoice."));
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
					invoice.AddRowError(Res.GetString("FC3BF544-0F09-4ED5-AFCF-B6177E4B1748", "Unable to acquire Lock to prevent other users Autorating this invoice at the same time."));
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

		void RemoveInvoiceItem(WhsInvoice invoice)
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
						message = Res.GetString("70bf89d9-63f7-4085-8aa5-f5d9d3239822", "Periodic Invoices rated and posted for relevant clients.");
					}
					else
					{
						message = Res.GetString("740d53f1-c020-4e4f-a41a-cdabdccd61a5", "Periodic Invoices rated for relevant clients.");
					}
				}
				else
				{
					message = Res.GetString("9c36f5b2-76c1-4615-882e-0c0eacdf2178", "There are no clients that need to be invoiced at this time.");
				}

				InvoiceCreationComplete(this, new TextEventArgs(message));
			}
		}

		#endregion

		#region AutoRating

		public event EventHandler<AutoRatingEventArgs> AutoRating;

		void OnAutoRating(bool hasExistingInvoice, WhsInvoice invoice)
		{
			AutoRating?.Invoke(this, new AutoRatingEventArgs(hasExistingInvoice, invoice));
		}

		#endregion

		#endregion

		#region Validation

		public WhsInvoicePeriodicMultiClientInvoiceValidation Validation => new WhsInvoicePeriodicMultiClientInvoiceValidation(this);

		#endregion

		#region FirstTimeInvoiced

		HashSet<OrgWarehousePair> WarehouseAndClientWithNoInvoices;

		#endregion

		#region WhsInvoiceHelper

		IWhsInvoiceHelper InvoiceHelper => invoiceHelper ?? (invoiceHelper = ObjectFactory.Get<IWhsInvoiceHelper>());
		IWhsInvoiceHelper invoiceHelper;

		#endregion
	}
}
