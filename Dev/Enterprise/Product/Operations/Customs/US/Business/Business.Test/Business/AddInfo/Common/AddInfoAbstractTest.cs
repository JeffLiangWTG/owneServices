using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	abstract class AddInfoAbstractTest : NonPersistentBusinessObjectTestCase
	{
		public abstract void TestIsExport();

		public abstract void TestIsDrawback();

		public void TestJobDeclaration()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			AddInfo addinfo = new AddInfoJobDeclaration(declaration.JE_AddInfoInfo);
			AssertEquals(declaration, addinfo.Declaration);
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			addinfo = new AddInfoJobComInvoiceHeader(invoice.JZ_AddInfoInfo);
			AssertEquals(declaration, addinfo.Declaration);
			CusClassification classification = Factory.New<CusClassification>();
			AssertNull(new AddInfoCusClassification(classification.CC_AddInfoInfo).Declaration);
		}

		public void TestUS_SchDINBArrival()
		{
			AddInfo bizO = (AddInfo)GetNewBusinessObject();
			bizO.US_SchDINBArrival = "2704";
			AssertEquals("2704", bizO.US_SchDINBArrival);
		}

		public void TestUS_SchDINBExport()
		{
			AddInfo bizO = (AddInfo)GetNewBusinessObject();
			bizO.US_SchDINBExport = "2704";
			AssertEquals("2704", bizO.US_SchDINBExport);
		}

		public void TestUS_SchDUSDestination()
		{
			AddInfo bizO = (AddInfo)GetNewBusinessObject();
			bizO.US_SchDUSDestination = "2704";
			AssertEquals("2704", bizO.US_SchDUSDestination);
		}

		public void TestUS_ITPresentationPort()
		{
			AddInfo bizO = (AddInfo)GetNewBusinessObject();
			bizO.US_ITPresentationPort = "2704";
			AssertEquals("2704", bizO.US_ITPresentationPort);
		}

		[ExpectNoExceptions]
		public void TestAllLocoPortsHaveEquivalentMappingToSchD()
		{
			foreach (SchemaColumn column in USAddInfoSchema.All)
			{
				if (column.Name.Contains("_RL"))
				{
					bool foundMatch = false;
					var bizO = (AddInfo)GetNewBusinessObject();
					var propertyinfo = bizO.Parent.FindPropertyInfo(column.Name);
					if (propertyinfo != null)
					{
						propertyinfo.Value = new ZString("USLAX");
						foreach (SchemaColumn innerColumn in USAddInfoSchema.All)
						{
							if (innerColumn.Name != USAddInfoSchema.Constants.PK && bizO[innerColumn].ToString() == "2772")
							{
								foundMatch = true;
							}
						}

						if (!foundMatch)
						{
							Fail("Updating " + column.Name + " did not update an equivalent US_SchD% property value");
						}
					}
				}
			}
		}

		public void TestLookups()
		{
			AddInfo addInfo = (AddInfo)GetNewBusinessObject();
			AssertEquals("Lookups", GetExpectedLookupsType(), addInfo.Lookups.GetType());
		}

		public void TestValidation()
		{
			AddInfo addInfo = (AddInfo)GetNewBusinessObject();
			AssertEquals("Validation", GetExpectedValidationType(), addInfo.Validation.GetType());
		}

		protected abstract Type GetExpectedValidationType();

		protected abstract Type GetExpectedLookupsType();

		protected override List<string> ColumnsToClearValueAfterTested
		{
			get
			{
				List<string> result = new List<string>();
				foreach (SchemaColumn column in USAddInfoSchema.All)
				{
					result.Add(column.Name);
				}

				return result;
			}
		}
	}
}
