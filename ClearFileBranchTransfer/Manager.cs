using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace ClearFileBranchTransfer
{
    public class Manager
    {
        // 初始化变量
        DateTime _dtNow;                            // 处理日期
        private static Manager _instance = null;    // 单例模式
        private PrefixColl _prefixColl;             // 新旧前缀号对象
        private AccListColl _accListColl;           // 股东号列表
        private ClearFileColl _clearFileColl;       // 清算文件列表


        // 运行时状态变量
        private bool _isRunning = false;            // 是否运行中


        /// <summary>
        /// 单例模式返回对象
        /// </summary>
        /// <returns></returns>
        public static Manager GetInstance()
        {
            if (_instance == null)
                _instance = new Manager();

            return _instance;
        }

        /// <summary>
        /// 构造函数(私有)
        /// </summary>
        private Manager()
        {
            // 初始化变量
            _dtNow = DateTime.Now;              // 日期


            // 读取配置文件
            // 判断配置文件是否存在，不存在抛出异常
            if (!File.Exists(Path.Combine(Environment.CurrentDirectory, "cfg.xml")))
                throw new Exception("未能找到配置文件cfg.xml，请重新配置该文件后重启程序!");

            // 读取配置文件
            XmlDocument doc = new XmlDocument();
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreComments = true;                                     //忽略文档里面的注释
            using (XmlReader reader = XmlReader.Create(@"cfg.xml", settings))
            {
                doc.Load(reader);


                // 0.根节点
                XmlNode rootNode = doc.SelectSingleNode("Config");   // 根节点
                if (rootNode == null)
                    throw new Exception("无法找到根配置节点<Config>，请检查配置文件!");


                // 1.合同前缀号配置
                _prefixColl = new PrefixColl();     // 合同前缀列表对象
                XmlNode prefixListXN = rootNode.SelectSingleNode("PrefixList");
                if (prefixListXN == null)
                    throw new Exception("无法找到合同前缀新旧转换节点<PrefixList>，请检查配置文件!");
                XmlNodeList prefixXNL = prefixListXN.SelectNodes("Prefix");
                foreach (XmlNode prefixXN in prefixXNL)
                {
                    // 临时变量
                    string _market = string.Empty;
                    string _old = string.Empty;
                    string _new = string.Empty;

                    XmlNode valueXN;    // 临时变量
                    valueXN = prefixXN.SelectSingleNode("Market");
                    if (valueXN != null)
                        _market = valueXN.InnerText.Trim();

                    valueXN = prefixXN.SelectSingleNode("Old");
                    if (valueXN != null)
                        _old = valueXN.InnerText.Trim();

                    valueXN = prefixXN.SelectSingleNode("New");
                    if (valueXN != null)
                        _new = valueXN.InnerText.Trim();

                    // 插入到_prefixColl
                    Prefix tmpPrefix = new Prefix(_market, _old, _new);
                    _prefixColl.Add(tmpPrefix);
                }


                // 2.股东号列表配置
                _accListColl = new AccListColl();       // 股东号列表
                XmlNode accountListXN = rootNode.SelectSingleNode("AccountList");
                if (accountListXN == null)
                    throw new Exception("无法找到股东号列表节点<AccountList>，请检查配置文件!");
                XmlNodeList accountXNL = accountListXN.SelectNodes("Account");
                foreach (XmlNode accountXN in accountXNL)
                {
                    // 临时变量
                    string _market = string.Empty;
                    List<string> _accList = new List<string>();

                    // 获取市场
                    XmlAttribute marketXA = accountXN.Attributes["Market"];
                    if (marketXA == null)
                        throw new Exception("<AccountList>下某个<Account>节点未设置属性[Market](SH、SZ), 请检查!");
                    _market = marketXA.Value.Trim();

                    // 获取股东号
                    foreach (XmlNode accXN in accountXN.ChildNodes)
                    {
                        _accList.Add(accXN.InnerText.Trim());
                    }


                    // 生成对象
                    AccList tmpAccList = new AccList(_market, _accList);

                    // 判断试产是否存在
                    if (GetPrefix(_market) == null)
                        throw new Exception(string.Format(@"<AccountList>下的Market属性[{0}]没有在<PrefixList>节点定义市场, 请检查!", _market));

                    // 插入到_accListColl
                    _accListColl.Add(tmpAccList);
                }


                // 3.清算文件
                _clearFileColl = new ClearFileColl();       // 清算文件列表
                XmlNode clearFileListXN = rootNode.SelectSingleNode("ClearFileList");
                if (clearFileListXN == null)
                    throw new Exception("无法找到清算文件列表节点<ClearFileList>，请检查配置文件!");
                XmlNodeList clearFileXNL = clearFileListXN.SelectNodes("ClearFile");
                foreach (XmlNode clearFileXN in clearFileXNL)
                {
                    string _market = string.Empty;
                    string _contractCol = string.Empty;
                    string _contractStart = string.Empty;
                    string _contractLength = string.Empty;
                    string _accountCol = string.Empty;

                    XmlNode tmpXN;    // 临时变量
                    tmpXN = clearFileXN.SelectSingleNode("Market");
                    if (tmpXN != null)
                        _market = tmpXN.InnerText.Trim();
                    tmpXN = clearFileXN.SelectSingleNode("ContractCol");
                    if (tmpXN != null)
                        _contractCol = tmpXN.InnerText.Trim();
                    tmpXN = clearFileXN.SelectSingleNode("ContractStart");
                    if (tmpXN != null)
                        _contractStart = tmpXN.InnerText.Trim();
                    tmpXN = clearFileXN.SelectSingleNode("ContractLength");
                    if (tmpXN != null)
                        _contractLength = tmpXN.InnerText.Trim();
                    tmpXN = clearFileXN.SelectSingleNode("AccountCol");
                    if (tmpXN != null)
                        _accountCol = tmpXN.InnerText.Trim();

                    // 循环<FileList>，对象在这个逻辑里加
                    tmpXN = clearFileXN.SelectSingleNode("FileList");
                    if (tmpXN != null)
                    {
                        XmlNodeList tmpXNL = tmpXN.ChildNodes;
                        foreach (XmlNode fileXN in tmpXNL)
                        {
                            string _file = fileXN.InnerText.Trim();
                            _file = Util.ReplaceStringWithDateFormat(_file, _dtNow);


                            // 判断市场是否存在
                            Prefix tmpPrefix = GetPrefix(_market);
                            if (tmpPrefix == null)
                                throw new Exception(string.Format(@"<ClearFileList>下的<Market>[{0}]没有在<PrefixList>节点定义市场, 请检查!", _market));

                            AccList tmpAccList = GetAccList(_market);

                            // 生成对象
                            ClearFile tmpClearFile = new ClearFile(_file, _market, _contractCol, _contractStart, _contractLength, _accountCol, tmpPrefix, tmpAccList);

                            // 插入列表
                            _clearFileColl.Add(tmpClearFile);
                        }
                    }
                }

            }//eof using
        }



        private Prefix GetPrefix(string market)
        {
            if (_prefixColl == null)
                throw new Exception("配置<PrefixList>(新旧合同前缀号)未加载, 请检查配置文件!");

            foreach (Prefix prefix in _prefixColl)
            {
                if (market == prefix.Market)
                    return prefix;
            }

            return null;
        }


        private AccList GetAccList(string market)
        {
            if (_accListColl == null)
                throw new Exception("配置<AccountList>(新旧合同前缀号)未加载, 请检查配置文件!");

            foreach (AccList _accList in _accListColl)
            {
                if (market == _accList.Market)
                    return _accList;
            }

            return null;
        }



        //////////////////////////////////////属性
        public PrefixColl PrefixColl
        {
            get { return _prefixColl; }
        }

        public AccListColl AccListColl
        {
            get { return _accListColl; }
        }

        public ClearFileColl ClearFileColl
        {
            get { return _clearFileColl; }
        }


        public bool IsRunning
        {
            get { return _isRunning; }
            set { _isRunning = value; }
        }


        public bool IsAllOK
        {
            get
            {
                foreach (ClearFile clearFile in _clearFileColl)
                {
                    if (clearFile.IsOK == false)
                        return false;
                }

                return true;
            }
        }

    }
}
