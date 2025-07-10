using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class AccTransactionLineDissectionAttributeLookups : AutoAccTransactionLineDissectionAttributeLookups
	{
		public AccTransactionLineDissectionAttributeLookups(AutoAccTransactionLineDissectionAttribute parent) : base(parent)
		{
		}

		public CodeDescriptionPairList AttributeValueList
		{
			get
			{
				var result = new CodeDescriptionPairList();

				if (AccTransactionLineDissectionAttribute != null)
				{
					switch (AccTransactionLineDissectionAttribute.ALD_Attribute)
					{
						case AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.OCG:
							result.AddRange(AccountingMasterFilesConstants.OCGList);
							break;
						case AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.LFO:
							result.AddRange(AccountingMasterFilesConstants.LFOList);
							break;
						case AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.LFE:
							result.AddRange(AccountingMasterFilesConstants.LFEList);
							break;
						case AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.TIC:
							result.AddRange(AccountingMasterFilesConstants.TICList);
							break;
						case AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.SPR:
							result.AddRange(AccountingMasterFilesConstants.SPRList);
							break;
					}
				}

				return result;
			}
		}

		public BusinessObjectCollection AttributeValueIDCollection
		{
			get
			{
				var result = new OrgHeaderCollection(Factory);

				if (AccTransactionLineDissectionAttribute?.TransactionLine?.GLHeader != null)
				{
					var aRAPControlAccounts = new Guid[]
					{
						ObjectFactory.Get<IAccounting>().ARControlAccount,
						ObjectFactory.Get<IAccounting>().APControlAccount,
					};

					if (aRAPControlAccounts.Contains(AccTransactionLineDissectionAttribute.TransactionLine.GLHeader.PK.ToGuid()))
					{
						var query = new ZDBOnlyQuery(typeof(OrgHeader));
						var subQuery = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.OB_OH);
						var fromAccountFilter = new ZQuery(OrgCompanyDataSchema.OB_GC, GlbCompany.CurrentCompany.PK);
						if (ObjectFactory.Get<IAccounting>().ARControlAccount == AccTransactionLineDissectionAttribute.TransactionLine.GLHeader.PK)
						{
							fromAccountFilter.AddToFilter(OrgCompanyDataSchema.OB_IsDebtor, ZBool.True);
						}
						else
						{
							fromAccountFilter.AddToFilter(OrgCompanyDataSchema.OB_IsCreditor, ZBool.True);
						}
						fromAccountFilter.AddToFilter(OrgCompanyDataSchema.OB_GC, GlbCompany.CurrentCompany.PK);
						subQuery.AddToFilter(fromAccountFilter);
						query.AddSubQuery(subQuery, JoinCondition.And);

						result = new OrgHeaderCollection(Factory, query);
						result.Load();
					}
		}

				return result;
			}
		}

		public string[] AlternateGLAccountAttributeCodeWithID => new string[]
		{
			AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.ORG,
		};

		AccTransactionLineDissectionAttribute AccTransactionLineDissectionAttribute => Parent as AccTransactionLineDissectionAttribute;
	}
}
