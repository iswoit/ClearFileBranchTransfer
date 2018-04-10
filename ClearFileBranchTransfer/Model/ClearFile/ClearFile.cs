using System;
using System.Collections.Generic;
using System.Text;

namespace ClearFileBranchTransfer
{
    public class ClearFile
    {
        // 初始化变量
        string _filePath;
        string _market;
        string _contractCol;
        int _contractStart;
        int _contractLength;
        string _accountCol;
        Prefix _prefix;                     // 前缀引用对象
        Dictionary<string, bool> _accList;  // 股东号处理状态

        // 运行时变量
        private bool _isRunning = false;
        private bool _isOK = false;
        private string _status = string.Empty;


        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="market"></param>
        /// <param name="contractCol"></param>
        /// <param name="contractStart"></param>
        /// <param name="contractLength"></param>
        /// <param name="accountCol"></param>
        public ClearFile(string filePath, string market, string contractCol, string contractStart, string contractLength, string accountCol, Prefix prefix, AccList accList)
        {
            _filePath = filePath;
            _market = market;
            _contractCol = contractCol;
            if (!int.TryParse(contractStart, out _contractStart))
                throw new Exception("清算文件[0] 配置项<ContractStart>(合同前缀号开始位)非整数, 请检查!");
            if (!int.TryParse(contractLength, out _contractLength))
                throw new Exception("清算文件[0] 配置项<ContractLength>(合同前缀号长度)非整数, 请检查!");
            _accountCol = accountCol;
            _prefix = prefix;

            _accList = new Dictionary<string, bool>();
            if (accList != null)
            {
                foreach (string acc in accList.List)
                {
                    if (!_accList.ContainsKey(acc))
                        _accList.Add(acc, false);
                }
            }


            _isRunning = false;
            _isOK = false;
            _status = "未开始";
        }



        public string FilePath
        {
            get { return _filePath; }
        }

        public string Market
        {
            get { return _market; }
        }

        public string OldPrefix
        {
            get { return _prefix.OldPrefix; }
        }

        public string NewPrefix
        {
            get { return _prefix.NewPrefix; }
        }

        public bool IsRunning
        {
            get { return _isRunning; }
        }

        public string Procedure
        {
            get
            {
                int iFinished = 0;
                foreach (KeyValuePair<string, bool> kv in _accList)
                {
                    if (kv.Value == true)
                        iFinished++;
                }
                return string.Format(@"{0}/{1}", iFinished, _accList.Count);
            }
        }

        public bool IsOK
        {
            get { return _isOK; }
        }

        public string Status
        {
            get { return _status; }
        }
    }
}
