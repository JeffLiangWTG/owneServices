using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	sealed class LightValidationForDifferentMessageTypesAndTransportModesTester
	{
		public delegate void AfterInitialiseObjectsDelegate(BaseJobDeclaration declaration);
		public delegate BaseJobDeclaration DeclarationProviderDelegate();
		public delegate void LightValidationRunnerDelegate(LightValidationForDifferentMessageTypesAndTransportModesTester tester);

		readonly DeclarationProviderDelegate declarationProvider;
		readonly LightValidationRunnerDelegate lightValidationRunner;
		readonly AfterInitialiseObjectsDelegate afterInitialiseObjectsRunner;

		public LightValidationForDifferentMessageTypesAndTransportModesTester(DeclarationProviderDelegate declarationProvider, LightValidationRunnerDelegate lightValidationRunner)
			: this(declarationProvider, lightValidationRunner, null)
		{
		}

		public LightValidationForDifferentMessageTypesAndTransportModesTester(DeclarationProviderDelegate declarationProvider, LightValidationRunnerDelegate lightValidationRunner, AfterInitialiseObjectsDelegate afterInitialiseObjectsRunner)
		{
			this.declarationProvider = declarationProvider;
			this.lightValidationRunner = lightValidationRunner;
			this.afterInitialiseObjectsRunner = afterInitialiseObjectsRunner;
		}

		void InitialiseNewObjects()
		{
			Declaration = declarationProvider.Invoke();
			Declaration.JE_OH_Importer = OrgHeader.New(Declaration.Factory).PK;
			Declaration.JE_OH_Supplier = OrgHeader.New(Declaration.Factory).PK;
			Declaration.Importer.FillWithValidTestData();
			Declaration.Supplier.FillWithValidTestData();
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			InvoiceHeader = Declaration.Invoices.AddNew();
			InvoiceHeader.JZ_RX_NKInvoice_Currency = Declaration.LocalCurrencyCode;
			InvoiceHeader.MarkAsNeedingValidation();
			InvoiceLine = InvoiceHeader.JobComInvoiceLines.AddNew();

			InvoiceLine.JI_InvoiceUQ = "KG";
			InvoiceLine.JI_InvoiceQuantity = 100m;
			InvoiceLine.JI_CustomsUnitQty = "KG";
			InvoiceLine.JI_CustomsQuantity = 100m;
			InvoiceLine.JI_LinePrice = 100;
		}

		public void Test()
		{
			StringCollectionX errors = new StringCollectionX();
			BaseJobDeclaration declaration = declarationProvider.Invoke();

			if (!declaration.LightValidationEnabled)
			{
				Assertion.Assert("Not enabled", true);
				return;
			}

			foreach (string messageType in GetCodesIncludingEmpty(declaration.Lookups.MessageTypeList))
			{
				var subTypeList = GetCodesIncludingEmpty(declaration.Lookups.GetEffectiveMessageSubTypeList(messageType));
				var transportList = GetCodesIncludingEmpty(declaration.Lookups.GetEffectiveTransportTypeList(messageType));
				foreach (string messageSubType in subTypeList)
				{
					foreach (string transportMode in transportList)
					{
						InitialiseNewObjects();
						if (afterInitialiseObjectsRunner != null)
						{
							afterInitialiseObjectsRunner(Declaration);
						}
						Declaration.JE_MessageType = messageType;
						Declaration.JE_MessageSubType = messageSubType;
						Declaration.JE_TransportMode = transportMode;
						Declaration.RefreshIncotermAndChargeFactory();
						Declaration.ResumeApportionment();

						try
						{
							lightValidationRunner.Invoke(this);
						}
						catch (Exception e) when (!e.IsCriticalException())
						{
							errors.Add("\r\n\r\nMessageType = '" + messageType + "'\tMessageSubType = '" + messageSubType + "'\tTransportMode = '" + transportMode + "'\r\n");
							errors.Add(e.Message);
						}
					}
				}
			}

			if (errors.Count > 0)
			{
				Assertion.HtmlFail(errors.ToString());
			}
		}

		IEnumerable<string> GetCodesIncludingEmpty(CodeDescriptionPairList list)
		{
			yield return "";
			foreach (CargoWise.Integration.ICodeDescription pair in list)
			{
				yield return pair.Code;
			}
		}

		public BaseJobDeclaration Declaration;
		public BaseJobComInvoiceHeader InvoiceHeader;
		public BaseJobComInvoiceLine InvoiceLine;
	}
}
