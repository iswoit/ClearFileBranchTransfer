using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace ClearFileBranchTransfer
{
    public static class Util
    {
        private static char[] arrMddConvert;  // mdd格式的字典数组
        static Util()
        {
            arrMddConvert = new char[] { '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c' };
        }

        public static string ReplaceStringWithDateFormat(string fileName, DateTime dtNow)
        {
            string strTmp = fileName;   // 返回值

            string yyyymmdd_replacement = dtNow.ToString("yyyyMMdd");
            string yymmdd_replacement = dtNow.ToString("yyMMdd");
            string mmdd_replacement = string.Format("{0}{1}", dtNow.Month.ToString().PadLeft(2, '0'), dtNow.Day.ToString().PadLeft(2, '0'));
            string mdd_replacement = string.Format("{0}{1}", arrMddConvert[dtNow.Month - 1], dtNow.Day.ToString().PadLeft(2, '0'));


            strTmp = Regex.Replace(strTmp, "yyyymmdd", yyyymmdd_replacement, RegexOptions.IgnoreCase);  // 1.替换yyyymmdd
            strTmp = Regex.Replace(strTmp, "yymmdd", yymmdd_replacement, RegexOptions.IgnoreCase);      // 2.替换yymmdd
            strTmp = Regex.Replace(strTmp, "mmdd", mmdd_replacement, RegexOptions.IgnoreCase);          // 3.替换mmdd
            strTmp = Regex.Replace(strTmp, "mdd", mdd_replacement, RegexOptions.IgnoreCase);            // 4.替换mdd
            return strTmp;
        }


        /// <summary>
        /// 路径是否存在, 带超时时间
        /// </summary>
        /// <param name="strPath">路径</param>
        /// <param name="iSec">超时秒数</param>
        /// <returns></returns>
        public static bool IsPathExistWithTimeout(string strPath, int iSec = 15)
        {
            CheckPathTimeout timeout = new CheckPathTimeout(new DoHandler(IsPathExist)); // 委托
            bool isTimeout = timeout.DoWithTimeout(new TimeSpan(0, 0, 0, iSec), strPath);       // 超过15秒失败
            bool isAvailable = timeout.bReturn;     // 是否可访问

            if (isTimeout == true || isAvailable == false)
                return false;
            else
                return true;
        }


        /// <summary>
        /// 路径是否存在
        /// </summary>
        /// <param name="strPath"></param>
        /// <returns></returns>
        public static bool IsPathExist(string strPath)
        {
            if (Directory.Exists(strPath))
                return true;
            else
                return false;
        }
    }
}
