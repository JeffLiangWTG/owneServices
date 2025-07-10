using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class InvoiceLinePackagePivotUniqueIndexFailureHandler : IUniqueIndexFailureHandler
	{
		readonly InvoiceLinePackagePivot pivot;
		readonly BaseJobComInvoiceLine invoiceLine;

		public InvoiceLinePackagePivotUniqueIndexFailureHandler(InvoiceLinePackagePivot pivot)
		{
			this.pivot = Argument.NotNull(pivot, nameof(pivot));
			invoiceLine = pivot.InvoiceLine;
		}

		public IEnumerable<string> HandledUniqueIndexNames
		{
			get
			{
				yield return CusHouseContPackInvoiceLinePivotSchema.Constants.Indexes.FK_UX__CHC_JE_CHC_JI_CHC_CW;
			}
		}

		public void NotifyUserAndAttemptToResolve(INotificationHandler notifier, string indexName)
		{
			var message = GenerateMessage();
			TryToResolve();
			notifier.ReportError(message, Res.GetString("63ceab22-dd0b-4322-87c2-2133fe2adbdc", "Save Error"));
		}

		void TryToResolve()
		{
			var package = pivot.Package;

			pivot.Delete();
			invoiceLine.PackagesPivot.Reload(true);

			var containerPk = package.PackingGroup?.CR_CO_Container ?? ZGuid.Empty;
			if (!containerPk.IsEmpty)
			{
				invoiceLine.ContainersPivot.Reload(true);
				invoiceLine.ContainersPivot.Cast<CusContainerInvoiceLinePivot>().Where(x => x.C2_CO == containerPk && !x.IsInDatabase).DeleteAll();
			}
		}

		string GenerateMessage()
		{
			var packageNumber = GetPackageNumber();
			var existingItem = GetExistingPivot();
			var changedProperties = GetChangedProperties(existingItem);

			return Res.GetString("ad68870c-6a0c-472b-9b22-217b1eb04f39",
				"Package ({0}) has already been linked to invoice line ({1}) by another user ({2}). Your changes have been merged, please review your changes and save again.\r\n{3}",
				packageNumber,
				invoiceLine.InvoiceAndLineReference,
				GetLastEditUserAndTime(existingItem),
				string.Join("\r\n", changedProperties));
		}

		ZString GetPackageNumber()
		{
			var invoiceLinePk = pivot.CHC_JI;
			var declarationPk = pivot.CHC_JE;
			var packagePk = pivot.CHC_CW;

			return invoiceLine.PackagesForInvoiceLinesForBindingOnly.Cast<BaseCusLinkPackage>()
				.FirstOrDefault(x =>
				{
					var cusPackagePivot = x.Pivot;
					return cusPackagePivot != null && cusPackagePivot.ParentPK == invoiceLinePk && cusPackagePivot.DeclarationPK == declarationPk && cusPackagePivot.PackagePK == packagePk;
				})
				?.PackageNumber ?? ZString.Empty;
		}

		string GetLastEditUserAndTime(InvoiceLinePackagePivot existingItem)
		{
			var declaration = existingItem?.Declaration;
			return declaration?.JE_SystemLastEditUser + " @ " + declaration?.JE_SystemLastEditTimeUtc;
		}

		IEnumerable<string> GetChangedProperties(InvoiceLinePackagePivot existingItem)
		{
			var changedProperties = new HashSet<string>();
			if (existingItem != null)
			{
				foreach (ZPropertyInfo existedPropertyInfo in existingItem.ZPropertyInfoHash)
				{
					if (existedPropertyInfo.IsPersistent)
					{
						var currentPropertyInfo = pivot.FindPropertyInfo(existedPropertyInfo.Name);
						if (currentPropertyInfo != null && !existedPropertyInfo.Value.Equals(currentPropertyInfo.Value))
						{
							changedProperties.Add(existedPropertyInfo.Name);
						}
					}
				}
			}
			return changedProperties;
		}

		InvoiceLinePackagePivot GetExistingPivot()
		{
			var query = new ZDBOnlyQuery(typeof(InvoiceLinePackagePivot));
			query.AddToFilter(CusHouseContPackInvoiceLinePivotSchema.PK, SQLComparisonOperator.NotEqual, pivot.PK);
			query.AddToFilter(CusHouseContPackInvoiceLinePivotSchema.CHC_JE, pivot.CHC_JE);
			query.AddToFilter(CusHouseContPackInvoiceLinePivotSchema.CHC_JI, pivot.CHC_JI);
			query.AddToFilter(CusHouseContPackInvoiceLinePivotSchema.CHC_CW, pivot.CHC_CW);

			return pivot.Factory.LoadTop1<InvoiceLinePackagePivot>(query);
		}
	}
}
