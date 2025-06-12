using System;

namespace Helper
{
    public class Helper
    {
        public double circumference(double radius){
            double pi = 3.14;
            double circ = pi*radius*2;
            return circ;
        }

    }
}

//   <msxsl:script language=""C#"" implements-prefix=""user"">
//   <![CDATA[
//   public double circumference(double radius){
//     double pi = 3.14;
//     double circ = pi*radius*2;
//     return circ;
//   }
//   ]]>
//   </msxsl:script>
