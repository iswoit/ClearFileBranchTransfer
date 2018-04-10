using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;

namespace ClearFileBranchTransfer
{
    public class AccListColl : Collection<AccList>
    {
        public new void Add(AccList accList)
        {
            // 判断重复
            foreach (AccList tmp in this)
            {
                if (accList.Market == tmp.Market)
                    throw new Exception(string.Format(@"账号列表-市场[0]重复定义, 请检查配置文件!.", accList.Market));
            }

            base.Add(accList);
        }
    }
}
