using System;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using CargoWise.Common;
using Enterprise.BatchProcessor;
using Enterprise.Customs.SG.MHUB;
using Enterprise.Customs.SG.MHUB.MHX;
using Enterprise.Customs.SG.Registry;
using Enterprise.Customs.SG.V4.MHUB;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.SG.V4.Business.BatchProcessor
{
	public class BatchSGCInterchangeRetriever40Script : BatchProcess
	{
		public BatchSGCInterchangeRetriever40Script()
		{
			helper = new BatchSG4InterchangeHelper(Logger);
		}
		readonly BatchSG4InterchangeHelper helper;

		protected override bool IsEnvironmentDataValid()
		{
			return base.IsEnvironmentDataValid() && helper.IsEnvironmentDataValid();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1078:Do not use Process.Start to open a file or url, use WebUrlLauncher or FileOpener for proper integration with Remote Desktop Services. False alarm if you are running a process for a reason other than to open a file or url.", Justification = "Uses ProcessStartInfo")]
		protected override void Execute(CancellationToken token)
		{
			try
			{
				BatchSGBrokerChecker staffChecker = new BatchSGBrokerChecker(Logger, helper);
				staffChecker.CheckStaff();
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				ErrorReporter.ReportOnce(e.Message);
				helper.VerboseLog(Logger, "Check Staff failed (not critical) = " + e.Message);
			}

			foreach (GlbStaff broker in helper.GetValidBrokerMailboxes())
			{
				token.ThrowIfCancellationRequested();
				currentBroker = broker;

				helper.MailboxChecker.Execute(broker); // Change password etc by logging in DIRECTLY.

				var creator = new Mhx4ProfileCreator(SGGlbStaffWrapper.Get(currentBroker).Tradenetv4Password);
				Logger.Log("Retrieving for " + currentBroker.GS_FullName + " " + creator.UserId);

				creator.CreateInputScript_Retrieve();  // Make a script with all the params, and return the name of the file in whcih to look for response data
				creator.CreateProfileFile(new MHUBSettingsProvider());

				var exePath = helper.GetPathToMhaccessTwoWays();
				var process = new System.Diagnostics.Process();
				process.EnableRaisingEvents = false;
				process.StartInfo.FileName = exePath; // - e.g. C :\Program Files (x86)\MHAccess\MHAccess.exe 
				process.StartInfo.WorkingDirectory = creator.ProfileFile.DirectoryName;
				process.StartInfo.Arguments = creator.ProfileFile.FullName;

				try
				{
					process.Start();
					process.WaitForExit(SGCustomsDataRegistry.Instance.FTPProcessTimeout.Value);
					var checker = new MhxScriptSuccessChecker(creator, MHUBConstants.CommandType.Retrieve);
					var successFlag = checker.ResponseSuccessFlag;
					if (successFlag == MHUBConstants.ResponseStatusCodes.MailboxEmpty)
					{
						Logger.Log("Mailbox empty");
					}
					else if (successFlag == MHUBConstants.ResponseStatusCodes.Failed)
					{
						var lastLineOfOutput = checker.ResponseToCommandPairs.LastOrDefault();
						var lastLineString = lastLineOfOutput.ToString();
						if (lastLineString.Contains("mhx.ini")
								&& lastLineString.Contains("File Not Found")
								&& new FileInfo(Path.Combine(new FileInfo(exePath).Directory.FullName, "mhx.ini")).Exists)  // mhx.ini DOES exist but Java says nay. Permissions. Effing permissions. 
						{
							lastLineString += " WTG: This most likely means that the NTFS permissions to the MHAccess folder have not been set correctly during installation. You need to ensure that 'Users' has the 'Modify' permission. Attempting to set that now...";
							lastLineString += SetNtfsPermissionBecauseInformationServicesForgotToFollowTheInstructions();
						}
						Logger.LogWarning(lastLineOfOutput != null ? lastLineString : "Command failed");
					}

					// Regardless of success or failure or even empty mailbox, let's just try saving whatever we can
					if (successFlag == MHUBConstants.ResponseStatusCodes.PartialSuccess)
					{
						Logger.LogWarning("Only partial success. Will attempt processing anyway... " + checker.AllOutput);
					}
					var downloadsSubDir = new DirectoryInfo(Path.Combine(creator.OutputFile.DirectoryName, "Cw1Downloads"));
					var downloadedFiles = downloadsSubDir.GetFiles();
					foreach (var file in downloadedFiles)
					{
						if (file.Extension != ".bad")
						{
							Logger.Log("Saving file: " + file.Name);
							var savedOk = BatchSGCInterchangeRetriever30.TryAddInterchange(file.FullName, Logger);
							if (!savedOk)
							{
								File.Move(file.FullName, file.FullName + ".bad");
							}
							else
							{
								file.Delete();
							}
						}
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					Logger.LogError("Exception in Execute MHX4: " + ex.Message);
				}
			}
			Logger.Log("Retrieving Complete");
			Logger.AddBlankLine();
		}

		string SetNtfsPermissionBecauseInformationServicesForgotToFollowTheInstructions()
		{
			try
			{
				var exe = new FileInfo(helper.GetPathToMhaccessTwoWays());
				var dir = exe.Directory;
				var security = dir.GetAccessControl();
				var identity = new SecurityIdentifier(WellKnownSidType.AuthenticatedUserSid, null);
				var rule = new FileSystemAccessRule(identity,
														FileSystemRights.Modify,
														InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None,
														AccessControlType.Allow
										);
				security.AddAccessRule(rule);
				dir.SetAccessControl(security);
			}
			catch (UnauthorizedAccessException e)
			{
				// It is extremely likely that CW1 process controller will see this. But as per Ed Miliband: "At least I tried"
				return "Could not set NTFS permissions: " + e.Message + " Please ask your system administrator to review this message and set the correct permissions on server " + System.Environment.MachineName;
			}
			return "";
		}

		GlbStaff currentBroker;
	}
}
