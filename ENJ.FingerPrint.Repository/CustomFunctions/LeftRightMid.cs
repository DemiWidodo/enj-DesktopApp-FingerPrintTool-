using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENJ.FingerPrint.Repository.CustomFunctions
{
    // how to use this class utility function
    // string MyString = "Demi Prasetyo Widodo"
    // var TestLeft  = Left(MyString ,4)  * Result = "Demi"
    // var TestRight = Right(MyString ,6) * Result = "Widodo"
    // var TestMid   = Mid(MyString,5,4)  * Result = "Pras"
    // end how to use this class utility function
    // Created By   : Demi Prasetyo Widodo
    // Date Created : 2016-05-03

    public class LeftRightMid
    {
        public static string Left(string param, int length)
        {
            string result = param.Substring(0, length);
            return result;
        }
        public static string Right(string param, int length)
        {
            string result = param.Substring(param.Length - length, length);
            return result;
        }

        public static string Mid(string param, int startIndex, int length)
        {
            string result = param.Substring(startIndex, length);
            return result;
        }

        public static string Mid(string param, int startIndex)
        {
            string result = param.Substring(startIndex);
            return result;
        }
    }
}
