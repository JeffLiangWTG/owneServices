using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	public abstract class BaseCustomsFormBasherTest : ZFormBasherTest
	{
		[DeveloperOnlyTest]
		public void TestGatherInvoiceLineGridDataImportWizardKeys()
		{
			GetData(declaration =>
			{
				var invoice = declaration.Invoices.Count > 0 ? declaration.Invoices[0] : declaration.Invoices.AddNew();
				var invoiceLine = invoice.JobComInvoiceLines.Count > 0 ? invoice.JobComInvoiceLines[0] : invoice.JobComInvoiceLines.AddNew();
				invoiceLine.Factory.Save();
			},
			declarationForm =>
			{
				Application.DoEvents();
				declarationForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = declarationForm.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var grid = declarationForm.CustomsBrokerageUserControl.InvoiceLinesUserControl.CustomsInvoiceLinesBoundGrid;
				return new[] { grid.DataImportWizardKey };
			});
		}

		void GetData(Action<BaseJobDeclaration> setupData, Func<BaseJobDeclarationForm, IEnumerable<string>> getData)
		{
			var additionalDetails = new ZStringBuilder();
			var details = new HashSet<string>();
			var declarationForm = GetFormToBashCore() as BaseJobDeclarationForm;
			if (declarationForm != null)
			{
				var declaration = declarationForm.Declaration;
				setupData(declaration);
				declarationForm.Show();
				Application.DoEvents();
				declarationForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = declarationForm.CustomsBrokerageUserControl.DeclarationTabPage;
				Application.DoEvents();
				var applicationCodes = declaration.Lookups.ApplicationCodeList.GetAllCodes();
				if (applicationCodes.Length > 0)
				{
					foreach (var applicationCode in applicationCodes)
					{
						GetDataForApplicationCode(declarationForm, applicationCode, getData).ForEach(item =>
						{
							if (details.Add(item))
							{
								additionalDetails.Append(item + $" (ApplicationCode:{declaration.JE_ApplicationCode}, MessageType:{declaration.JE_MessageType}, MessageSubType:{declaration.JE_MessageSubType}, TransportMode:{declaration.JE_TransportMode})");
							}
						});
					}
				}
				else
				{
					GetDataForApplicationCode(declarationForm, declarationForm.Declaration.JE_ApplicationCode, getData).ForEach(id => details.Add(id));
				}
				declarationForm.Dispose();
			}
			additionalDetails.Prepend(string.Join(", ", details.Select(x => "'" + x + "'")));
			additionalDetails.Prepend("This is a Developer Only test; for the sole purpose of gathering S9_ModuleID for DIW Transformation.");
			Fail(additionalDetails.ToStringWithNewLineBetweenAppends());
		}

		IEnumerable<string> GetDataForApplicationCode(BaseJobDeclarationForm declarationForm, ZString applicationCode, Func<BaseJobDeclarationForm, IEnumerable<string>> getData)
		{
			var declaration = declarationForm.Declaration;
			declaration.JE_ApplicationCode = applicationCode;
			var messageTypes = declaration.Lookups.MessageTypeList.GetAllCodes();
			if (messageTypes.Length > 0)
			{
				return messageTypes.SelectMany(messageType => GetDataForMessageType(declarationForm, messageType, getData));
			}
			else
			{
				return GetDataForMessageType(declarationForm, declaration.JE_MessageType, getData);
			}
		}

		IEnumerable<string> GetDataForMessageType(BaseJobDeclarationForm declarationForm, ZString messageType, Func<BaseJobDeclarationForm, IEnumerable<string>> getData)
		{
			var declaration = declarationForm.Declaration;
			declaration.JE_MessageType = messageType;
			var messageSubTypes = declaration.Lookups.MessageSubTypeList.GetAllCodes();
			if (messageSubTypes.Length > 0)
			{
				return messageSubTypes.SelectMany(messageSubType => GetDataForMessageSubType(declarationForm, messageSubType, getData));
			}
			else
			{
				return GetDataForMessageSubType(declarationForm, declaration.JE_MessageSubType, getData);
			}
		}

		IEnumerable<string> GetDataForMessageSubType(BaseJobDeclarationForm declarationForm, ZString messageSubType, Func<BaseJobDeclarationForm, IEnumerable<string>> getData)
		{
			var declaration = declarationForm.Declaration;
			declaration.JE_MessageSubType = messageSubType;
			var transportTypes = declaration.Lookups.TransportTypeList.GetAllCodes();
			if (transportTypes.Length > 0)
			{
				return transportTypes.SelectMany(transportType => GetDataForTransportType(declarationForm, transportType, getData));
			}
			else
			{
				return GetDataForTransportType(declarationForm, declaration.JE_TransportMode, getData);
			}
		}

		IEnumerable<string> GetDataForTransportType(BaseJobDeclarationForm declarationForm, ZString transportType, Func<BaseJobDeclarationForm, IEnumerable<string>> getData)
		{
			declarationForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = declarationForm.CustomsBrokerageUserControl.DeclarationTabPage;
			Application.DoEvents();
			declarationForm.Declaration.JE_TransportMode = transportType;
			return getData(declarationForm);
		}

		#region FormBashing
		protected sealed override Form GetFormToBashCore()
		{
			var declaration = GetPopulatedDeclarationForFormBashing();
			declaration.JE_MessageType = MessageTypeForFormBashing;
			Factory.Save();
			var result = (ZForm)Activator.CreateInstance(FormToBashType, new object[] { declaration });
			result.ControllerID = ControllerIDs.Customs.JobDeclaration;
			return result;
		}

		protected abstract BaseJobDeclaration GetPopulatedDeclarationForFormBashing();

		protected sealed override void BashScenario(Form testForm)
		{
			var declarationForm = testForm as BaseJobDeclarationForm;
			if (declarationForm != null)
			{
				var newFactory = new BusinessObjectFactory();
				newFactory.RefreshEnabled = false;
				var declaration = newFactory.Load<BaseJobDeclaration>(declarationForm.Declaration.PK);
				declaration.JE_MessageType = MessageTypeForFormBashing;
				var messageSubTypeList = GetMessageSubTypesForFormBashingTest(declaration.Lookups.MessageSubTypeList);
				if (messageSubTypeList.Any())
				{
					foreach (var messageSubType in messageSubTypeList)
					{
						BashScenarioSettingMessageSubType(declarationForm, declaration, messageSubType);
					}
				}
				else
				{
					BashScenarios(declarationForm, MessageTypeForFormBashing, "", declaration.Lookups.TransportTypeList);
				}
			}
		}

		protected virtual IEnumerable<string> GetMessageSubTypesForFormBashingTest(CodeDescriptionPairList messageSubTypeList)
		{
			return messageSubTypeList.GetAllCodes();
		}

		protected override void SetupTabPagesThatNeedToBeBashedOnlyOnce(Form testForm)
		{
			if (testForm is BaseJobDeclarationForm declarationForm)
			{
				OnceOnlyBashTabPages.AddSafe(declarationForm.CustomsBrokerageUserControl.ContainerTabPage);
				OnceOnlyBashTabPages.AddSafe(declarationForm.CustomsBrokerageUserControl.EventTabPage);
				OnceOnlyBashTabPages.AddSafe(declarationForm.CustomsBrokerageUserControl.WorkflowTabPage);
				foreach (var plugIn in declarationForm.PlugIns.Instances)
				{
					if (plugIn.TabPage != null && !OnceOnlyBashTabPages.Contains(plugIn.TabPage))
					{
						OnceOnlyBashTabPages.Add(plugIn.TabPage);
					}
				}
			}
		}

		/// <summary>
		/// This represents whether the job is IMPort or EXPort.  Return IMP or EXP to specifiy the scenario that you are testing.  
		/// </summary>
		public abstract ZString MessageTypeForFormBashing { get; }

		void BashScenarioSettingMessageSubType(BaseJobDeclarationForm declarationForm, BaseJobDeclaration declaration, ZString messageSubType)
		{
			declaration.JE_MessageSubType = messageSubType;
			BashScenarios(declarationForm, declaration.JE_MessageType, declaration.JE_MessageSubType, declaration.Lookups.TransportTypeList);
		}

		void BashScenarios(BaseJobDeclarationForm declarationForm, ZString messageType, ZString messageSubType, CodeDescriptionPairList transportTypeList)
		{
			foreach (string transportType in TransportCodesForFormBashingTest)
			{
				if (transportTypeList.ContainsCode(transportType))
				{
					BashScenario(declarationForm, messageType, messageSubType, transportType);
				}
			}
		}

		protected virtual string[] TransportCodesForFormBashingTest
		{
			get
			{
				if (transportCodesForFormBashingTest == null)
				{
					List<string> list = new List<string>();
					BusinessObjectFactory newFactory = new BusinessObjectFactory();
					BaseJobDeclaration declaration = newFactory.New<BaseJobDeclaration>();
					list.Add(declaration.TransportModeAirCodeForTesting);
					list.Add(declaration.TransportModeSeaCodeForTesting);
					list.Add(declaration.TransportModeRailCodeForTesting);
					transportCodesForFormBashingTest = list.ToArray();
				}
				return transportCodesForFormBashingTest;
			}
		}
		string[] transportCodesForFormBashingTest;

		void BashScenario(BaseJobDeclarationForm testForm, ZString messageType, ZString messageSubType, ZString transportMode)
		{
			BaseJobDeclaration declaration = testForm.Declaration;
			DeleteCollectionItems(declaration);
			SetupDeclarationForFormBashing(testForm, declaration, messageType, messageSubType, transportMode);
			((IBusinessObjectState)declaration).ClearHasChangesIncludingChildren();
			AssertEquals("Declaration should not have changes", false, declaration.HasChanges);
			base.BashScenario(testForm);
		}

		protected virtual void DeleteCollectionItems(BaseJobDeclaration declaration)
		{
			RemoveAllExceptFirstElement(declaration.JobComInvoiceGroupHeaders);
			RemoveAllExceptFirstElement(declaration.CusContainers);
			RemoveAllExceptFirstElement(declaration.Invoices);
			RemoveAllExceptFirstElement(declaration.InvoiceLines);
			RemoveAllExceptFirstElement(declaration.Packages);
			RemoveAllExceptFirstElement(declaration.CustomsEntryHeaders);
			RemoveAllExceptFirstElement(declaration.PackingGroups);
			RemoveAllExceptFirstElement(declaration.Bills);
		}

		protected void RemoveAllExceptFirstElement(IBusinessObjectCollection collection)
		{
			if (collection.Count > 1)
			{
				BusinessObject[] collectionArray = collection.ToArray();
				for (int i = 1; i < collectionArray.Length; i++)
				{
					collection.Delete(collectionArray[i]);
				}
			}
		}

		void SetupDeclarationForFormBashing(BaseJobDeclarationForm form, BaseJobDeclaration declaration, ZString messageType, ZString messageSubType, ZString transportMode)
		{
			TabControl tabControl = (TabControl)typeof(ZForm).InvokeMember("TopLevelTabControl", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetProperty, null, form, null);
			tabControl.SelectedIndex = 0; // can't change transport mode while the Containers tab is visible otherwise it disappears, which confuses .net :)

			currentFormBashingDeclaration = declaration;
			currentFormBashingDeclaration.JE_MessageType = messageType;
			currentFormBashingDeclaration.JE_MessageType_ReadOnly = true;
			currentFormBashingDeclaration.JE_MessageSubType = messageSubType;
			currentFormBashingDeclaration.SetMessageSubTypeReadOnlyForTest(true);
			currentFormBashingDeclaration.JE_TransportMode = transportMode;
			currentFormBashingDeclaration.SetTransportModeReadOnlyForTest(true);
			SetupDeclarationForSpecificFormBashing(form);
		}
		protected BaseJobDeclaration currentFormBashingDeclaration;

		protected virtual void SetupDeclarationForSpecificFormBashing(BaseJobDeclarationForm form)
		{
		}

		protected override string AddExtraDebuggingMessage(string message)
		{
			string extraMessage = "";
			if (currentFormBashingDeclaration != null)
			{
				extraMessage = string.Format("JE_MessageType = '{0}', JE_MessageSubType = '{1}', JE_TransportMode = '{2}'{3}", currentFormBashingDeclaration.JE_MessageType, currentFormBashingDeclaration.JE_MessageSubType, currentFormBashingDeclaration.JE_TransportMode, System.Environment.NewLine);
			}
			return extraMessage + base.AddExtraDebuggingMessage(message);
		}

		protected override void TearDown()
		{
			currentFormBashingDeclaration = null;
			base.TearDown();
		}
		#endregion
	}
}
