using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.BIRD.Common;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.DataTransfer
{
	interface IBIRDProcessor
	{
		bool Process(TextReader dataReader, string attachmentFileName, INotifications notifications, List<ITransactionParticipant> documentFactories, string lineOneBlock, string lineTwoBlock);
	}

	abstract class BIRDProcessor<ControlMessageBlockB, ControlMessageBlockY> : IBIRDProcessor
		where ControlMessageBlockB : MessageBlock, IControlMessageBlockB, new()
		where ControlMessageBlockY : MessageBlock, IControlMessageBlockY, new()
	{
		protected BIRDProcessor(BusinessObjectFactoryProvider factoryProvider)
		{
			this.factoryProvider = Argument.NotNull(factoryProvider, "factoryProvider");
		}
		protected readonly BusinessObjectFactoryProvider factoryProvider;

		public bool Process(TextReader dataReader, string attachmentFileName, INotifications notifications, List<ITransactionParticipant> documentFactories, string lineOneBlock, string lineTwoBlock)
		{
			var result = false;
			if (lineOneBlock != null)
			{
				ZStringBuilder oneApplication = null;
				string birdApplicationID = null;
				var oneBlock = lineOneBlock;
				while (oneBlock != null)
				{
					oneBlock = oneBlock.PadRight(80, ' ');

					if (IsValidBlock(oneBlock))
					{
						if (IsHeaderBlock(oneBlock))
						{
							birdApplicationID = GetBIRDApplicationID(oneBlock);

							oneApplication = new ZStringBuilder();
						}
						else if (oneApplication != null && IsFooterBlock(oneBlock))
						{
							oneApplication.Append(oneBlock);

							result |= ProcessOneApplication(oneApplication, birdApplicationID, notifications, attachmentFileName, documentFactories);

							oneApplication = null;
						}

						if (oneApplication != null)
						{
							oneApplication.Append(oneBlock);
						}
					}

					if (lineTwoBlock != null)
					{
						oneBlock = lineTwoBlock;
						lineTwoBlock = null;
						continue;
					}
					oneBlock = dataReader.ReadLine();
				}

				if (birdApplicationID == null)
				{
					notifications.AddError(NoHeaderBlock);
				}
				else if (oneApplication != null)
				{
					notifications.AddError(NoFooterBlock);
				}
			}
			return result;
		}

		protected abstract string GetBIRDApplicationID(string oneBlock);

		protected abstract bool IsHeaderBlock(string oneBlock);
		protected abstract bool IsFooterBlock(string oneBlock);

		protected abstract string NoHeaderBlock { get; }
		protected abstract string NoFooterBlock { get; }

		bool IsValidBlock(string eightyByteBlock)
		{
			foreach (string applicationID in GetBIRDApplicationIdentifierCodes())
			{
				if (InputMessageBlockDeserialiser.IsValid(new string[] { EDIInterchange.ApplicationCodes.USCustomsImport }, applicationID, eightyByteBlock) ||
					OutputMessageBlockDeserialiser.IsValid(new string[] { EDIInterchange.ApplicationCodes.USCustomsImport }, applicationID, eightyByteBlock))
				{
					return true;
				}
			}
			return false;
		}

		protected abstract IEnumerable<string> GetBIRDApplicationIdentifierCodes();

		InputMessageBlockDeserialiser InputMessageBlockDeserialiser
		{
			get { return inputMessageBlockDeserialiser ?? (inputMessageBlockDeserialiser = new InputMessageBlockDeserialiser()); }
		}
		InputMessageBlockDeserialiser inputMessageBlockDeserialiser;

		OutputMessageBlockDeserialiser OutputMessageBlockDeserialiser
		{
			get { return outputMessageBlockDeserialiser ?? (outputMessageBlockDeserialiser = new OutputMessageBlockDeserialiser()); }
		}
		OutputMessageBlockDeserialiser outputMessageBlockDeserialiser;

		bool ProcessOneApplication(ZStringBuilder oneApplication, string birdApplicationID, INotifications notifications, string attachmentFileName, List<ITransactionParticipant> documentFactories)
		{
			bool result = false;

			if (oneApplication.Length > 0)
			{
				string birdText = oneApplication.ToString();

				var declaration = ProcessFromHeaderToFooter(birdText, birdApplicationID, notifications);

				if (declaration != null)
				{
					declaration.DocManagerInfo.AddFileOrDocument(Encoding.ASCII.GetBytes(birdText), attachmentFileName, "MSC");

					if (!documentFactories.Contains(declaration.DocManagerInfo.MasterFactory))
					{
						documentFactories.Add(declaration.DocManagerInfo.MasterFactory);
					}
				}

				result = declaration != null;
			}

			return result;
		}

		protected abstract BlockControlGenerator<ControlMessageBlockB, ControlMessageBlockY> GetMessageBlockGenerator(string birdText, string birdApplicationID);

		JobDeclaration ProcessFromHeaderToFooter(string birdText, string birdApplicationID, INotifications notifications)
		{
			var generator = GetMessageBlockGenerator(birdText, birdApplicationID);
			if (generator == null)
			{
				notifications.AddError("Unknown application id" + (birdApplicationID != null ? ": " + birdApplicationID : "."));
				return null;
			}

			try
			{
				generator.Deserialise(birdText);
			}
			catch (InvalidMessageFormatException ex)
			{
				notifications.AddError("The BIRD message has invalid data and system could not proceed. Please correct the invalid data and try again.\r\n" + ex.Message);
				return null;
			}

			var declaration = GetOrCreateDeclaration(birdApplicationID, generator, notifications);

			if (declaration != null)
			{
				ProcessFromHeaderToFooter(birdApplicationID, declaration, generator, notifications);
			}

			return declaration;
		}

		protected abstract void ProcessFromHeaderToFooter(string birdApplicationID, JobDeclaration declaration, BlockControlGenerator<ControlMessageBlockB, ControlMessageBlockY> generator, INotifications notifications);

		#region Import Declaration Data

		protected void ImportDeclarationData(JobDeclaration declaration, BlockControlGenerator<ControlMessageBlockB, ControlMessageBlockY> generator, INotifications notifications)
		{
			var declarationNotifications = new NotificationCollection();

			ImportData(declaration, generator, declarationNotifications);

			if (declarationNotifications.Count > 0)
			{
				notifications.AddRange(declarationNotifications);

				var note = declaration.Notes.AddNew(true, BIRDImportWarningsNoteDescription, declarationNotifications.ToUniqueMessageListString());
				note.ST_ForceRead = true;

				notifications.AddWarning(BIRDImportWarningAddedToNote + "'" + BIRDImportWarningsNoteDescription + "'.");
			}

			SendAcknowledgementRecordIfRequired(declaration, GetApplicationIdentifierCode(generator));

			if (!declaration.JE_DeclarationReference.IsEmpty)
			{
				notifications.AddWarning(declaration.JE_DeclarationReference + " has been updated.");
			}
		}

		protected abstract string GetApplicationIdentifierCode(BlockControlGenerator<ControlMessageBlockB, ControlMessageBlockY> generator);

		protected abstract void ImportData(JobDeclaration declaration, BlockControlGenerator<ControlMessageBlockB, ControlMessageBlockY> generator, NotificationCollection declarationNotifications);

		void SendAcknowledgementRecordIfRequired(JobDeclaration declaration, string applicationCode)
		{
			if (ShoudSendAcknowledgementRecord(applicationCode))
			{
				declaration.Logs.AddNew(Events.DataImport, ApplicationIdentifierCodeList.Codes.BIRDTransaction + ":" + applicationCode);

				var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry ?? declaration.ActiveEntryHeaders.CargoReleaseEntry;

				if (entry != null)
				{
					new BIRDStatusMessageBuilder(entry).BuildENRecord();
				}
			}
		}

		protected abstract bool ShoudSendAcknowledgementRecord(string applicationCode);

		#endregion

		#region Create/Load Declaration

		JobDeclaration GetOrCreateDeclaration(string birdApplication, BlockControlGenerator<ControlMessageBlockB, ControlMessageBlockY> generator, INotifications notifications)
		{
			JobDeclaration result = null;
			var error = GetAnyReasonForNotAbleToProcessData(generator);
			if (!error.IsEmpty)
			{
				notifications.AddError(error);
			}
			else
			{
				var headerIDRecord = generator.MessageBlocks.OfType<IBIRDHeaderIDRecord>().FirstOrDefault();

				var entryFilerCode = headerIDRecord != null ? headerIDRecord.EntryFilerCode : ZString.Empty;
				var entryNumber = headerIDRecord != null ? headerIDRecord.EntryNumber : ZString.Empty;

				var existingDeclarations = GetExistingDeclaration(entryFilerCode, entryNumber, GetBrokerReference(generator));

				if (existingDeclarations.Length > 1)
				{
					notifications.AddError(string.Format(CultureInfo.InvariantCulture, MoreThanOneDeclarationWithEntryFilerCodeAndEntryNumber, headerIDRecord.EntryFilerCode + headerIDRecord.EntryNumber));
				}
				else if (existingDeclarations.Length == 1)
				{
					result = existingDeclarations[0];
				}
				else
				{
					if (ShouldCreateNewDeclaration(birdApplication))
					{
						result = factoryProvider.Current.New<JobDeclaration>();
					}
					else
					{
						notifications.AddWarning(NoDeclarationFound);
					}
				}
			}
			return result;
		}

		protected virtual ZString GetAnyReasonForNotAbleToProcessData(BlockControlGenerator<ControlMessageBlockB, ControlMessageBlockY> generator)
		{
			return ZString.Empty;
		}

		protected abstract bool ShouldCreateNewDeclaration(string birdApplication);

		protected abstract ZString GetBrokerReference(BlockControlGenerator<ControlMessageBlockB, ControlMessageBlockY> generator);

		JobDeclaration[] GetExistingDeclaration(ZString entryFilerCode, ZString entryNumber, ZString originatingBrokerRef)
		{
			var result = System.Array.Empty<JobDeclaration>();
			ZQuery query = null;

			if (!entryFilerCode.IsEmpty && !entryNumber.IsEmpty)
			{
				query = Customs.Business.EntryNumberQueryGenerator.GetEntryNumberQuery(SQLComparisonOperator.Equal, entryNumber, Core.Constants.CountryCodes.UnitedStates);
				query.AddToFilter(new GenAddOnColumnQueryHelper(typeof(JobDeclaration)).GetQueryOnGenAddOnColumn(JobDeclaration.Schema.US_EntryFilerCode, entryFilerCode));
			}
			else if (!originatingBrokerRef.IsEmpty)
			{
				query = new GenAddOnColumnQueryHelper(typeof(JobDeclaration)).GetQueryOnGenAddOnColumn(JobDeclaration.Schema.US_BRDRefNo, originatingBrokerRef);
			}

			if (query != null)
			{
				query.AddToFilter(JobDeclarationSchema.JE_IsCancelled, ZBool.False);
				result = factoryProvider.Current.Load<JobDeclaration>(query);
			}

			return result;
		}

		#endregion

		public const string NoDeclarationFound = "The requested action cannot be performed because there is no declaration found.";
		public const string BIRDImportWarningsNoteDescription = "BIRD Import Warnings";
		public const string BIRDImportWarningAddedToNote = "The warnings that happened while importing have been added to 'Notes' under ";
		public const string MoreThanOneDeclarationWithEntryFilerCodeAndEntryNumber = "System could not perform the requested action because there are more than one declaration with this Entry Filer Code and Entry Number, {0}. Please locate the declarations and cancel one of them.";
	}
}
