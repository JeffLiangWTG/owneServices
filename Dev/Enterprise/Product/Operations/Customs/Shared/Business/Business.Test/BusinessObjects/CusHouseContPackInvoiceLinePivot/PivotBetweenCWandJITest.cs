using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(InvoiceLinePackagePivot))]
	public class PivotBetweenCWandJITest : EnterpriseBusinessObjectTestCase
	{
		public void TestUniqueIndexFailureHandler()
		{
			var pivot = GetNewBusinessObject();
			var handler = GetUniqueIndexFailureHandler(pivot);
			CombineAssertions(() =>
			{
				AssertType<InvoiceLinePackagePivotUniqueIndexFailureHandler>("Correct Handler Type", handler);
				AssertSame("Cached", handler, GetUniqueIndexFailureHandler(pivot));
			});
		}

		public void TestSupportsNotes()
		{
			AssertEquals("SupportsNotes should be false", false, Factory.GetNull<InvoiceLinePackagePivot>().SupportsNotes);
		}

		public void TestDeleteParents()
		{
			var pivot = GetNewBusinessObject();
			invoiceLine.Delete();
			Assert(pivot.IsDeleted);

			pivot = GetNewBusinessObject();
			package.Delete();
			Assert(pivot.IsDeleted);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetNewBusinessObject();
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return GetNewBusinessObject();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<BaseJobDeclarationWhichSupportsPackagesPivot>();
			declaration.JE_MasterBill = "M";
			declaration.JE_TotalNoOfPieces = 1;

			var invoice = declaration.Invoices.AddNew();
			invoiceLine = Factory.New<BaseJobComInvoiceLineWhichSupportsPackagesPivot>();
			invoiceLine.JI_JZ = invoice.PK;
			invoice.InvoiceLines.Add(invoiceLine);
			package = declaration.Bills[0].PackingGroups[0].Packages.AddNew();
			package.CW_PackQty = 1;

			var collection = new BaseCusLinkPackageCollection(invoiceLine);
			var npbo = collection.AddNew();
			npbo.Package = package;
			npbo.IsLinked = true;

			var pivot = npbo.Pivot;
			pivot.NumberOfPacks = 1;

			var quantityPivot = npbo.QuantityPivot;
			quantityPivot.Quantity = 2;

			return pivot as BusinessObject;
		}

		protected BaseJobComInvoiceLine invoiceLine;
		protected BasePackage package;

		public class BaseJobDeclarationWhichSupportsPackagesPivot : BaseJobDeclaration
		{
			public BaseJobDeclarationWhichSupportsPackagesPivot(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
			{
			}

			protected override bool SupportsChcPivotBetweenInvoiceLineAndPackingCore
			{
				get { return true; }
			}

			protected override bool SupportsChzPivotBetweenInvoiceHeaderAndPackingCore
			{
				get { return true; }
			}

			public new BaseJobComInvoiceLineWhichSupportsPackagesPivotCompleteCollection InvoiceLines
			{
				get { return new BaseJobComInvoiceLineWhichSupportsPackagesPivotCompleteCollection(this); }
			}

			protected override JobDeclarationLookups GetNewLookups()
			{
				return new JobDeclarationLookupsForTestAgain(this);
			}
		}

		public class JobDeclarationLookupsForTestAgain : JobDeclarationLookups
		{
			public JobDeclarationLookupsForTestAgain(BaseJobDeclarationWhichSupportsPackagesPivot baseJobDeclarationWhichSupportsPackagesPivot)
				: base(baseJobDeclarationWhichSupportsPackagesPivot)
			{
			}

			protected override CodeDescriptionPairList PackingUnitTypesListCore
			{
				get
				{
					return Enterprise.Customs.Universal.RefCusCodeListTypes.GetCachedListValidBeforeDate(Factory, Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations,
								Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, new ZDateTime(2012, 1, 1));
				}
			}
		}

		public class BaseJobComInvoiceLineWhichSupportsPackagesPivot : BaseJobComInvoiceLine
		{
			public BaseJobComInvoiceLineWhichSupportsPackagesPivot(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
			{
			}
		}

		public class BaseJobComInvoiceLineWhichSupportsPackagesPivotCompleteCollection : InvoiceLineCompleteCollection
		{
			public BaseJobComInvoiceLineWhichSupportsPackagesPivotCompleteCollection(BaseJobDeclarationWhichSupportsPackagesPivot jobDeclaration)
			: base(jobDeclaration)
			{
			}

			public new BaseJobComInvoiceLineWhichSupportsPackagesPivot this[int index]
			{
				get { return (BaseJobComInvoiceLineWhichSupportsPackagesPivot)Elements[index]; }
			}
			public new BaseJobComInvoiceLineWhichSupportsPackagesPivot AddNew()
			{
				return (BaseJobComInvoiceLineWhichSupportsPackagesPivot)base.AddNew();
			}
		}

		IUniqueIndexFailureHandler GetUniqueIndexFailureHandler(BusinessObject pivot)
		{
			return ((IEnumerable<IUniqueIndexFailureHandler>)typeof(InvoiceLinePackagePivot).GetProperty("UniqueIndexFailureHandlers", BindingFlags.FlattenHierarchy | BindingFlags.NonPublic | BindingFlags.Instance).GetValue(pivot, null)).Single();
		}
	}
}
