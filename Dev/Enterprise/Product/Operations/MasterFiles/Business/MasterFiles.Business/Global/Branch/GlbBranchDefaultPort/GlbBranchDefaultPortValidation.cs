//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoGlbBranchDefaultPortValidation
//
//    This class should be used for overriding validation in AutoGlbBranchDefaultPortValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class GlbBranchDefaultPortValidation : AutoGlbBranchDefaultPortValidation
	{
		public GlbBranchDefaultPortValidation(AutoGlbBranchDefaultPort parent) : base(parent)
		{
		}

		GlbBranchDefaultPort DefaultPort => Parent as GlbBranchDefaultPort;

		protected override void CheckGBP_DefaultTo()
		{
			base.CheckGBP_DefaultTo();
			MandatoryValidation.CheckEntered(DefaultPort.GBP_DefaultToInfo);
			ListValidation.ErrorIfInvalidCode(DefaultPort.GBP_DefaultToInfo, DefaultPort.Lookups.DefaultToList);
			CheckDuplicateDefaultPort(DefaultPort.GBP_DefaultToInfo);
		}

		protected override void CheckGBP_TransportMode()
		{
			base.CheckGBP_TransportMode();
			MandatoryValidation.CheckEntered(DefaultPort.GBP_TransportModeInfo);
			ListValidation.ErrorIfInvalidCode(DefaultPort.GBP_TransportModeInfo, DefaultPort.Lookups.TransportModeList);
			CheckDuplicateDefaultPort(DefaultPort.GBP_TransportModeInfo);
		}

		protected override void CheckGBP_ContainerMode()
		{
			base.CheckGBP_ContainerMode();
			MandatoryValidation.CheckEntered(DefaultPort.GBP_ContainerModeInfo);
			ListValidation.ErrorIfInvalidCode(DefaultPort.GBP_ContainerModeInfo, DefaultPort.Lookups.ContainerModeList);
			CheckDuplicateDefaultPort(DefaultPort.GBP_ContainerModeInfo);
		}

		protected override void CheckGBP_RL_NKPort()
		{
			base.CheckGBP_RL_NKPort();
			MandatoryValidation.CheckEntered(DefaultPort.GBP_RL_NKPortInfo);
			ListValidation.ErrorIfInvalidCode(DefaultPort.GBP_RL_NKPortInfo);
			if (!DefaultPort.GBP_RL_NKPortInfo.HasErrors())
			{
				if (DefaultPort.GBP_RL_NKPort != DefaultPort.Branch.GB_RL_NKHomePort && DefaultPort.Branch.ExtraPorts.Cast<GlbBranchExtraPorts>().All(port => port.GY_RL_NKAdditionalBranchRelatedPort != DefaultPort.GBP_RL_NKPort))
				{
					DefaultPort.GBP_RL_NKPortInfo.AddError(Res.GetString("452625C1-F208-4ADF-985B-6BC5B301C99E", "The Default Port Code must be either Home Port or one of the Additional Related Ports."));
				}
				else
				{
					var portUNLOCO = new RefUNLOCO.Loader(DefaultPort.Factory).Load(DefaultPort.GBP_RL_NKPort);
					if (DefaultPort.GBP_TransportMode == Core.Constants.TransportModes.Air && !portUNLOCO.RL_HasAirport)
					{
						DefaultPort.GBP_RL_NKPortInfo.AddWarning(Res.GetString("C15C109A-4D83-498E-98C2-657E8CE74B84", "The selected UNLOCO for transport mode Air does not have airport. Please check UNLOCO's 'Has Airport' attribute."));
					}
					else if (DefaultPort.GBP_TransportMode == Core.Constants.TransportModes.Sea && !portUNLOCO.RL_HasSeaport)
					{
						DefaultPort.GBP_RL_NKPortInfo.AddWarning(Res.GetString("06D0A29F-E9C6-4DEC-95E3-BB5D2D105EDC", "The selected UNLOCO for transport mode Sea does not have seaport. Please check UNLOCO's 'Has Seaport' attribute."));
					}
				}
			}
		}

		void CheckDuplicateDefaultPort(ZPropertyInfo propertyInfo)
		{
			var errorMessage = Res.GetString("C2F337C9-BE3B-4ABC-8066-3B3A17B2D3EE", "Duplicate rows are not allowed. This row is duplicate of another row.");
			DefaultPort.RemoveRowError(errorMessage);

			if (propertyInfo.HasErrors())
			{
				return;
			}

			var defaultPortCollection = DefaultPort.Branch.DefaultPorts;
			if (defaultPortCollection.ContainsDuplicatedCombination(DefaultPort))
			{
				DefaultPort.AddRowError(errorMessage);
			}
		}
	}
}
