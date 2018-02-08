using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WDS.Utilities
{
    public class ParseHelper
    {
        /// <summary>
        /// .net 4.0以下, Version 的 TryParse
        /// </summary>
        /// <param name="value"></param>
        /// <param name="returnValue"></param>
        /// <returns></returns>
        public static bool TryParse(string value, out Version returnValue)
        {
            try
            {
                returnValue = new Version(value);
                return true;
            }
            catch
            {
                returnValue = null;
                return false;
            }
        }

        /// <summary>
        /// .net 4.0以下, Guid 的 TryParse
        /// </summary>
        /// <param name="value"></param>
        /// <param name="returnValue"></param>
        /// <returns></returns>
        public static bool TryParse(string value, out Guid returnValue)
        {
            try
            {
                returnValue = new Guid(value);
                return true;
            }
            catch
            {
                returnValue = Guid.Empty;
                return false;
            }
        }
    }
}