using System;
using System.Collections.Generic;
using System.Text;

namespace ClearFileBranchTransfer
{
    /// <summary>
    /// 市场的新旧营业部合同前缀
    /// </summary>
    public class Prefix
    {
        string _market;         // 市场
        string _oldPrefix;      // 旧合同前缀
        string _newPrefix;      // 新合同前缀


        public Prefix(string market, string oldPrefix, string newPrefix)
        {
            if (oldPrefix.Length != newPrefix.Length)
                throw new Exception(string.Format(@"市场[{0}]: 旧前缀号[{1}](长度{3})与新前缀号[{2}](长度{4})长度不一致. 请检查配置文件!", market, oldPrefix, newPrefix, oldPrefix.Length, newPrefix.Length));

            if (oldPrefix == newPrefix)
                throw new Exception(string.Format(@"市场[{0}]: 新旧前缀号一致, 都是[{1}]. 请检查配置文件!", market, oldPrefix));

            _market = market;
            _oldPrefix = oldPrefix;
            _newPrefix = newPrefix;
        }


        /// <summary>
        /// 市场
        /// </summary>
        public string Market
        {
            get { return _market; }
        }

        /// <summary>
        /// 旧合同前缀
        /// </summary>
        public string OldPrefix
        {
            get { return _oldPrefix; }
        }

        /// <summary>
        /// 新合同前缀
        /// </summary>
        public string NewPrefix
        {
            get { return _newPrefix; }
        }

        /// <summary>
        /// 营业部合同前缀长度
        /// </summary>
        public int PrefixLength
        {
            get { return _oldPrefix.Length; }
        }
    }
}
