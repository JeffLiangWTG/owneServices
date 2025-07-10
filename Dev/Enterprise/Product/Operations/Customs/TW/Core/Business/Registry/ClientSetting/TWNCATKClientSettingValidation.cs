using System.IO;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.TW.Business
{
	public class TWNCATKClientSettingValidation
	{
		public TWNCATKClientSettingValidation(TWNCATKClientSetting parent)
		{
			this.parent = parent;
		}

		readonly TWNCATKClientSetting parent;

		internal void ValidateAll()
		{
			ValidateMachineName();
			ValidateSendToFolder();
			ValidateRunningIntervalInSeconds();
		}

		public void ValidateMachineName()
		{
			var targetInfo = parent.MachineNameInfo;
			targetInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(targetInfo);
		}

		public void ValidateSendToFolder()
		{
			var targetInfo = parent.SendToFolderInfo;
			targetInfo.ClearAllNotifications();
			var sendToFolder = parent.SendToFolder;
			MandatoryValidation.CheckEntered(targetInfo);

			if (!sendToFolder.IsEmpty)
			{
				var fullPath = TryGetFullPath(sendToFolder);
				if (fullPath.IsEmpty || fullPath != sendToFolder)
				{
					targetInfo.AddError(GetIsNotALocalPathMsg(sendToFolder));
				}
			}
		}

		public void ValidateRunningIntervalInSeconds()
		{
			var targetInfo = parent.RunningIntervalInSecondsInfo;
			targetInfo.ClearAllNotifications();

			MandatoryValidation.CheckEntered(targetInfo);
			if (parent.RunningIntervalInSeconds < 15)
			{
				targetInfo.AddError(Res.GetString("E49FC496-38FB-488E-9CDD-974519049E64", "Running Interval cannot be smaller than 15 seconds."));
			}
		}

		ZString TryGetFullPath(string path)
		{
			string fullPath = string.Empty;
			try
			{
				fullPath = Path.GetFullPath(path);
			}
			catch (System.ArgumentException) { }
			catch (System.Security.SecurityException) { }
			catch (System.NotSupportedException) { }
			catch (PathTooLongException) { }
			return fullPath;
		}

		string GetIsNotALocalPathMsg(ZString path) => Res.GetString("F81F7A50-9E7D-470E-9FD4-4E549EEA1068", "[{0}] is not a local folder path.", path);
	}
}
