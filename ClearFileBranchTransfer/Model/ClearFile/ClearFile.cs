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
        private string _status = string.Empty;


        #region 方法

        /// <summary>
        /// 构造函数
        /// </summary>
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
            _status = "未开始";


            // <ClearFile>清算文件的长度(<ContractLength>)一定要和<Prefix>一样长
            if (_contractLength != prefix.OldPrefix.Length)
                throw new Exception(string.Format(@"清算文件[{0}]合同号长度[{1}]与<Prefix>对应市场的长度[{2}]不一致, 请检查配置!", _filePath, _contractLength, prefix.OldPrefix.Length));
        }

        public void ResetStatus()
        {
            _isRunning = false;
            _status = "未开始";

            // 重置文件状态
            List<string> tmpList = new List<string>(_accList.Keys);
            for (int i = 0; i < tmpList.Count; i++)
            {
                _accList[tmpList[i]] = false;
            }
        }




        #endregion



        #region 属性
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

        public string ContractCol
        {
            get { return _contractCol; }
        }

        public int ContractStart
        {
            get { return _contractStart; }
        }

        public int ContractLength
        {
            get { return _contractLength; }
        }

        public string AccountCol
        {
            get { return _accountCol; }
        }

        public Dictionary<string, bool> AccList
        {
            get { return _accList; }
            set { _accList = value; }
        }


        public bool IsRunning
        {
            get { return _isRunning; }
            set { _isRunning = value; }
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
            get
            {
                foreach (KeyValuePair<string, bool> kv in _accList)
                {
                    if (kv.Value == false)
                        return false;
                }

                return true;
            }
        }

        public string Status
        {
            get { return _status; }
            set { _status = value; }
        }
        #endregion
    }
}
