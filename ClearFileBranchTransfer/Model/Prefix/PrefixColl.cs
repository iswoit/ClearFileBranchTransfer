using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;

namespace ClearFileBranchTransfer
{
    public class PrefixColl : Collection<Prefix>
    {
        public new void Add(Prefix prefix)
        {
            // 判断重复
            foreach (Prefix tmp in this)
            {
                if (prefix.Market == tmp.Market)
                    throw new Exception(string.Format(@"市场[0]的合同前缀号配置重复, 请检查配置文件!(目前程序只支持单对单转换).", prefix.Market));
            }

            base.Add(prefix);
        }
    }
}
