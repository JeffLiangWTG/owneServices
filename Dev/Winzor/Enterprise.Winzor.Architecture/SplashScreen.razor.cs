using System.Threading.Tasks;
using Enterprise.ZArchitecture.Core;
using Microsoft.AspNetCore.Components;

namespace Enterprise.Winzor.Architecture;

public partial class SplashScreen
{
	string currentMessage = (NoResString)"Launching";
	int currentProgress;

	public void UpdateMessage(string message)
	{
		currentMessage = message;
		this.NotifyRenderRequired();
	}

	string ProgressStyleString => $"{currentProgress}%";

	public void UpdateProgress(int progress)
	{
		currentProgress = progress;
		this.NotifyRenderRequired();
	}
}
