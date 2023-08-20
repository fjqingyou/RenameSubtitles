using System.Collections.Generic;
using System.IO;

/// <summary>
/// 资源文件
/// </summary>
public class AssetFile {
    /// <summary>
    /// 文件名
    /// </summary>
    public string fileName;

    /// <summary>
    /// 显示名称
    /// </summary>
    public string displayName;

    /// <summary>
    /// 显示名称的缩写显示方式
    /// </summary>
    public string displayNameSimplified;

    /// <summary>
    /// 目标范围文本的数字
    /// </summary>
    public int? targetRangeTextNumber;

    /// <summary>
    /// 数字范围列表
    /// </summary>
    /// <typeparam name="Ranage"></typeparam>
    /// <returns></returns>
    public List<Ranage> numberRanageList = new List<Ranage>();


    /// <summary>
    /// 计算显示名称
    /// </summary>
    public void CalcDisplay() {
        this.displayName = this.displayNameSimplified;

        //确保存在文件扩展名，方便用户识别文件
        string extName = Path.GetExtension(this.displayName);
        if (string.IsNullOrEmpty(extName)) {//如果当前没文件扩展名
            this.displayName += Path.GetExtension(this.fileName);
        }
    }
}