using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Remoting;
using System.Xml.Xsl;

namespace CargoWise.BizTalk.UnitTestFX
{
    public static class XsltArgListBuilder
    {
        /// <summary>
        /// Substitute existing arguments with object references provided using the namespace as a matching key
        /// </summary>
        /// <param name="realXtensions">Complete extension object collection to be fully or partially replaced</param>
        /// <param name="replacementArgList">Substitution objects</param>
        /// <returns>A recompiled extenstion object collection to be used when mapping</returns>
        public static XsltArgumentList Replace(ExtensionObjects realXtensions, Dictionary<string, object> replacementArgList)
        {
	        XsltArgumentList newArgList = new XsltArgumentList();
	        foreach (ExtensionObjectsExtensionObject xtension in realXtensions.Items)
	        {
		        if (replacementArgList.ContainsKey(xtension.Namespace))
		        {
			        newArgList.AddExtensionObject(xtension.Namespace, replacementArgList[xtension.Namespace]);
		        }
		        else
		        {
			        ObjectHandle handle = Activator.CreateInstance(xtension.AssemblyName, xtension.ClassName);
			        object xtensionObject = handle.Unwrap();
			        newArgList.AddExtensionObject(xtension.Namespace, xtensionObject);
		        }
	        }
	        return newArgList;
        }
    }
}
