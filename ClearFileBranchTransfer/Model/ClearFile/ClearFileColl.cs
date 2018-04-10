using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;

namespace ClearFileBranchTransfer
{
    public class ClearFileColl : Collection<ClearFile>
    {
        public new void Add(ClearFile clearFile)
        {
            // 判断重复
            foreach (ClearFile tmp in this)
            {
                if (clearFile.FilePath == tmp.FilePath)
                    throw new Exception(string.Format(@"清算文件[0]重复定义, 请检查配置文件!.", clearFile.FilePath));
            }

            base.Add(clearFile);
        }
    }
}
