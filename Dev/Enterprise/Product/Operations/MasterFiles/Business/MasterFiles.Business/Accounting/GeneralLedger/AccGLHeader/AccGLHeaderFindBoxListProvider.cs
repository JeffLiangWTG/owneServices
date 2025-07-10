
using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class AccGLHeaderFindBoxListProvider : FindBoxListProvider
	{
		public AccGLHeaderFindBoxListProvider(BusinessObjectCollection list, AccTransactionLines transactionLines = null, Action<AccGLHeaderCollection, List<AccGLHeader>> showGLAccountsForImportAction = null) : base(list)
		{
			ShowGLAccountsForImportAction = showGLAccountsForImportAction;
			TransactionLines = transactionLines;
		}

		public Action<AccGLHeaderCollection, List<AccGLHeader>> ShowGLAccountsForImportAction;

		public readonly AccTransactionLines TransactionLines;

		public override ZGuid PrimaryKeyFromCode(string code)
		{
			var result = ZGuid.Empty;

			if (!string.IsNullOrEmpty(code))
			{
				var chartPK = AccountingMasterFilesRegistry.Instance.GLAccountSelectionAndEntry.Value;
				if (chartPK != Guid.Empty)
				{
					result = GetGLHeaderFromAlternateGLAccount(chartPK, code);

					if (!result.IsEmpty)
					{
						return result;
					}
				}

				result = GLHeaderToMatch(code);

				if (result.IsEmpty)
				{
					result = base.PrimaryKeyFromCode(code);

					var bizObj = BizObjFromCodeWithRelationshipFilter(code);
					if (bizObj != null)
					{
						var bizO = bizObj as ICancellable;
						if (bizO?.IsCancelled ?? false)
						{
							result = ZGuid.Invalid;
						}
					}
				}
			}

			return result;
		}

		public ZGuid GetGLHeaderFromAlternateGLAccount(ZGuid chartPK, string code)
		{
			var result = ZGuid.Empty;
			var query = new ZQuery();
			query.AddToFilter(AccAlternateGLAccountSchema.AGA_AccountNum, code);
			query.AddToFilter(AccAlternateGLAccountSchema.AGA_AAC_AlternateChart, chartPK);
			var alternateGLAccount = List.Factory.LoadTop1<AccAlternateGLAccount>(query);
			var alternateGLAccountAttributes = alternateGLAccount?.AlternateGLAccountAttributes;

			if (alternateGLAccountAttributes != null && alternateGLAccountAttributes.Any())
			{
				var glHeader = alternateGLAccountAttributes[0].GLHeader;
				if (glHeader != null && TransactionLines != null && TransactionLines.TransactionHeader.AH_TransactionType.ToString() == TransactionTypes.GLStandardJournal)
				{
					var removingLineDissectionAttributes = new List<AccTransactionLineDissectionAttribute>();
					removingLineDissectionAttributes.AddRange(TransactionLines.AccTransactionLineDissectionAttributes.Cast<AccTransactionLineDissectionAttribute>());

					foreach (var removingLineDissection in removingLineDissectionAttributes)
					{
						TransactionLines.AccTransactionLineDissectionAttributes.RemoveAndDelete(removingLineDissection);
					}

					foreach (var alternateGLAccountAttribute in alternateGLAccountAttributes.Where(x => !x.AAA_Attribute.IsEmpty))
					{
						var transactionLine = TransactionLines.AccTransactionLineDissectionAttributes.AddNew();
						transactionLine.ALD_Attribute = alternateGLAccountAttribute.AAA_Attribute;
						transactionLine.ALD_AttributeValue = alternateGLAccountAttribute.AAA_Value;
					}
				}

				var glAccountPKs = alternateGLAccountAttributes.Select(attr => attr.AAA_AG_GLHeader).Distinct();

				if (glAccountPKs.Count() == 1)
				{
					result = glAccountPKs.First();
				}
				else
				{
					var glHeaderCollection = new AccGLHeaderCollection(List.Factory, new ZQuery(AccGLHeaderSchema.PK, glAccountPKs));
					glHeaderCollection.Load();

					if (ShowGLAccountsForImportAction != null)
					{
						var list = new List<AccGLHeader>();
						ShowGLAccountsForImportAction(glHeaderCollection, list);
						if (list.Any())
						{
							result = list[0].PK;
						}
					}
				}
			}

			return result;
		}

		ZGuid GLHeaderToMatch(string code)
		{
			ZGuid result = ZGuid.Empty;

			var query = new ZQuery();
			query.AddToFilter(AccGLAccountDescriptorSchema.AJ_LocalAccountNumber, code);
			query.AddToFilter(AccGLAccountDescriptorSchema.AJ_Language, GlbStaff.CurrentUser.GS_WorkingLanguage);
			query.AddToFilter(AccGLAccountDescriptorSchema.AJ_RN_NKCountryOfCompliance, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			query.AddToFilter(AccGLAccountDescriptorSchema.AJ_ReportType, AccGLAccountDescriptor.ReportTypeCOA);
			var descriptor = List.Factory.LoadTop1<AccGLAccountDescriptor>(query);

			if (descriptor != null)
			{
				result = descriptor.ParentGLHeaderPK;
			}

			return result;
		}
	}
}
