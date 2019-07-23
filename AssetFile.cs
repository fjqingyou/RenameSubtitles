using System.Collections.Generic;
using System.IO;

/// <summary>
/// 资源文件
/// </summary>
public class AssetFile{
    public string fileName;

    public string displayName;

    public string displayNameSimplified;

    public int ? targetRangeTextNumber;
    public List<Ranage> numberRanageList = new List<Ranage>();

    
    /// <summary>
    /// 计算显示名称
    /// </summary>
    public void CalcDisplay(){
        this.displayName = this.displayNameSimplified;
        
        //确保存在文件扩展名，方便用户识别文件
        string extName = Path.GetExtension(this.displayName);
        if(string.IsNullOrEmpty(extName)){//如果当前没文件扩展名
            this.displayName += Path.GetExtension(this.fileName);
        }
    }
}