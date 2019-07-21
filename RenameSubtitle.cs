using System;
using System.IO;
using System.Collections.Generic;

public class RenameSubtitle{
    private static List<string> videoFileTypeList = new List<string>();
    private static List<string> subtitleFileTypeList = new List<string>();
    private string targetDirectory;
    private List<AssetFile> videoFileList = new List<AssetFile>();
    private List<AssetFile> subtitleFileList = new List<AssetFile>();
    private List<AssetMatch> assetMatchList = new List<AssetMatch>();

    //范围的权重
    private List<int> videoFileRangeWeightList = new List<int>();
    private List<int> subtitleFileRangeWeightList = new List<int>();
    
    static RenameSubtitle(){
        videoFileTypeList.Add("mp4");
        videoFileTypeList.Add("mkv");

        subtitleFileTypeList.Add("ass");
    }

    public void doWork(string [] args){
        try{
            //收集资源文件
            CollectAssetFile(args);
            
            //收集文件名中，出现数字的范围
            CollectAssetFileNumberRanage(this.videoFileList);//收集视频文件数字范围
            CollectAssetFileNumberRanage(this.subtitleFileList);//收集字幕文件数字范围

            //初始化范围权重
            this.InitAssetFileRangeWeight(this.videoFileList, this.videoFileRangeWeightList);
            this.InitAssetFileRangeWeight(this.subtitleFileList, this.subtitleFileRangeWeightList);
            
            //初始化资源文件的模板范围文本的数字
            this.initAssetsFileTargetRangeTextNumber(this.videoFileList, this.videoFileRangeWeightList);
            this.initAssetsFileTargetRangeTextNumber(this.subtitleFileList, this.subtitleFileRangeWeightList);

            //对目标视频文件进行排序
            this.SortVideoFile();

            //匹配资源
            this.MatchAsset();

            //准备执行重命名操作
            this.DoRaname();
        }catch(Exception e){
            Console.WriteLine(e);
        }

        //等待按键退出
        this.WaitKeyExit();
    }

    /// <summary>
    /// 收集资源文件
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    private void CollectAssetFile(string [] args){
        this.targetDirectory = null;
        if(args.Length < 1){
            throw new Exception("请指定目录，或者同时指定目标范围内的视频与字幕文件");
        }else{
            string targetDirectory = args[0];
            if(args.Length == 1 && Directory.Exists(targetDirectory)){//如果只指定了一个参数，而且它是一个文件夹
                this.targetDirectory = targetDirectory;
                
                string [] files = Directory.GetFiles(targetDirectory);

                //文件分类
                FileClassification(files, videoFileList, videoFileTypeList);
                FileClassification(files, subtitleFileList, subtitleFileTypeList);
            }else{
                bool allIsFile = true;
                //那么这里就是多选了
                for(int i = 0 ; i < args.Length; i++){
                    if(!File.Exists(args[i])){
                        allIsFile = false;
                        break;
                    }
                }
                if(!allIsFile){
                    throw new Exception("指定了多个参数，但不是所有选择都是文件!");
                }
                
                //如果全是文件，那么需要判断是不是在同一个文件夹下面
                bool sameFolder = true;
                targetDirectory = Path.GetDirectoryName(targetDirectory);
                for(int i = 1 ; i < args.Length; i++){
                    if(targetDirectory != Path.GetDirectoryName(args[i])){
                        sameFolder = false;
                        break;
                    }
                }

                if(!sameFolder){//如果不是相同文件夹
                    throw new Exception("选择的多个文件，不是全在同一个文件夹里面");
                }

                //成功校验
                this.targetDirectory = targetDirectory;
                
                //文件分类
                FileClassification(args, videoFileList, videoFileTypeList);
                FileClassification(args, subtitleFileList, subtitleFileTypeList);
            }
        }
    }

    /// <summary>
    /// 文件分类
    /// </summary>
    private void FileClassification(string [] files, List<AssetFile> assetFileList, List<string> fileTypeList){
        for(int i = 0 ; i < files.Length; i++){
            string fileName = Path.GetFileName(files[i]);
            int dotIndex = fileName.LastIndexOf(".");
            if(dotIndex > -1){
                string fileType = fileName.Substring(dotIndex + 1).ToLower();
                if(fileTypeList.Contains(fileType)){//如果目标资源文件
                    AssetFile assetFile = new AssetFile();
                    assetFile.fileName = fileName;
                    assetFileList.Add(assetFile);
                }
            }
        }
    }

    /// <summary>
    /// 收集资源文件数字范围
    /// </summary>
    private void CollectAssetFileNumberRanage(List<AssetFile> assetFileList){
        //收集文件中数字开始和结束的索引列表
        for (int i = 0; i < assetFileList.Count; i++){
            AssetFile assetFile = assetFileList[i];
            Ranage ranage = null;
            string file = assetFile.fileName;

            List<Ranage> numberRanageList = assetFile.numberRanageList;
            numberRanageList.Clear();

            for(int j = 0; j < file.Length; j++){
                char c = file[j];
                if(c >= '0' && c <= '9'){//如果这个字符是数字
                    if(ranage == null){
                        ranage = new Ranage();
                        ranage.assetFile = assetFile;
                        ranage.start = j;

                    }
                }else{//如果这个字符不是数字
                    if(ranage != null){
                        ranage.end = j;
                        ranage.text = file.Substring(ranage.start, ranage.end - ranage.start);
                        numberRanageList.Add(ranage);
                        ranage = null;
                    }
                }
            }
        }
    }

    /// <summary>
    /// 做重命名动作
    /// </summary>
    private void DoRaname(){
        if(this.assetMatchList.Count < 1){
            Console.WriteLine("没有找到匹配项");
        }else{

            //输出匹配信息
            this.PrintMatchList();

            Console.Write("请确认上方的文件映射关系，确认无误输入Y确定重命名，其他字符取消本次操作，您的选择是？：");
            ConsoleKeyInfo keyInfo = Console.ReadKey();
            if(keyInfo.KeyChar != 'y' && keyInfo.KeyChar != 'Y'){
                Console.WriteLine("\n用户放弃本次操作！");
            }else{
                for(int i = 0 ; i < this.assetMatchList.Count; i++){
                    AssetMatch matchAsset = this.assetMatchList[i];
                    AssetFile videoAssetFile = matchAsset.video;
                    AssetFile subtitleAssetFile = matchAsset.subtitle;

                    //字幕文件扩展名
                    string subtitleExtFileName = Path.GetExtension(subtitleAssetFile.fileName);

                    //获取没有扩展名的视频文件名
                    string targetFileName = Path.GetFileNameWithoutExtension(videoAssetFile.fileName);
                    targetFileName += subtitleExtFileName;
                    
                    //原始文件路径
                    string originFilePath = Path.Combine(this.targetDirectory, subtitleAssetFile.fileName);

                    //目标文件路径
                    string targetFilePath = Path.Combine(this.targetDirectory, targetFileName);

                    //重命名字幕文件
                    File.Move(originFilePath, targetFilePath);
                }
                Console.WriteLine("\n任务完成！");
            }
        }
    }

    /// <summary>
    /// 等待按键退出
    /// </summary>
    private void WaitKeyExit(){
        Console.WriteLine("按任意键退出");
        Console.ReadKey();
    }

    /// <summary>
    /// 匹配资源
    /// </summary>
    private void MatchAsset(){
        assetMatchList.Clear();

        for(int i = 0 ; i < this.videoFileList.Count; i++){
            AssetFile videoAssetFile = this.videoFileList[i];
            
            //先按照数字匹配
            List<AssetFile> subtitleAssetFileList = null;

            int ? numPtr = videoAssetFile.targetRangeTextNumber;

            //先按照数字匹配
            if(numPtr.HasValue){
                int num = numPtr.Value;
                subtitleAssetFileList = this.subtitleFileList.FindAll( x => x.targetRangeTextNumber == num);
            }
            
            if(subtitleAssetFileList == null || subtitleAssetFileList.Count == 0){//如果数字没有匹配项
                subtitleAssetFileList = new List<AssetFile>();
                AssetFile templateAssetFile = this.videoFileList.Find(x => x.targetRangeTextNumber == 1);//至少有一个 1 吧，用它作为参照
                if(templateAssetFile != null){//如果找到模板了
                    int videoWeightRangeIndex = this.getMaxWeightRangeIndex(this.videoFileRangeWeightList);
                    int subtitleWeightRangeIndex = this.getMaxWeightRangeIndex(this.subtitleFileRangeWeightList);
                    Ranage videoRange = templateAssetFile.numberRanageList[videoWeightRangeIndex];
                    string str1 = videoAssetFile.fileName.Substring(videoRange.start, videoRange.length);
                    for(int j = 0 ; j < this.subtitleFileList.Count; j++){
                        AssetFile subtitle = this.subtitleFileList[j];
                        Ranage subtitleRange = templateAssetFile.numberRanageList[videoWeightRangeIndex];
                        string str2 = subtitle.fileName.Substring(subtitleRange.start, subtitleRange.length);
                        if(str2.Contains(str1)){
                            subtitleAssetFileList.Add(subtitle);
                        }
                    }
                }
            }

            //如果匹配成功才记录匹配数据
            if(subtitleAssetFileList.Count > 0){//如果找到匹配项了
                AssetMatch assetMatch = new AssetMatch();
                assetMatch.video = videoAssetFile;
                assetMatch.subtitleList = subtitleAssetFileList;
                if(subtitleAssetFileList.Count == 1){
                    assetMatch.subtitle = subtitleAssetFileList[0];//默认选择
                }else{
                    int index;
                    for(;;){
                        Console.WriteLine("视频：" + videoAssetFile.fileName);
                        for(int j = 0 ; j < subtitleAssetFileList.Count; j++){
                            Console.WriteLine("编号：{0} -> 字幕：{1}", j, subtitleAssetFileList[j].fileName);
                        }
                        Console.Write("视频出现同时多个字幕匹配，请选择目标字幕文件的编号：");
                        string line = Console.ReadLine();
                        if(int.TryParse(line, out index)){
                            if(index < 0 || index > subtitleAssetFileList.Count){
                                Console.WriteLine("\n输出错误，请输入正确的编号！");
                            }else{
                                assetMatch.subtitle = subtitleAssetFileList[index];
                                break;
                            }
                        }
                    }
                }
                assetMatchList.Add(assetMatch);
            }
        }
    }

    /// <summary>
    /// 输出匹配信息
    /// </summary>
    private void PrintMatchList(){
        //输出顺序
        for(int i = 0 ; i < this.assetMatchList.Count; i++){
            string videoFileName = Path.GetFileName(this.assetMatchList[i].video.fileName);
            string subtitleFileName = Path.GetFileName(this.assetMatchList[i].subtitle.fileName);
            Console.WriteLine("{0} -> {1}", videoFileName, subtitleFileName);
        }
    }

    /// <summary>
    /// 初始化资源文件的模板范围文本的数字
    /// </summary>
    private void initAssetsFileTargetRangeTextNumber(List<AssetFile> assetFileList, List<int> rangeWeightList){
        //取得目标权重索引
        int videoFileRangeIndex = this.getMaxWeightRangeIndex(rangeWeightList);

        //提取目标数字
        for(int i = 0 ; i < assetFileList.Count; i++){
            AssetFile assetFile = assetFileList[i];
            int ? targetRangeTextNumber = null;//不能识别的都放置在最后

            if(assetFile.numberRanageList.Count > videoFileRangeIndex){
                Ranage ranage = assetFile.numberRanageList[videoFileRangeIndex];
                targetRangeTextNumber = Convert.ToInt32(ranage.text);
            }

            assetFile.targetRangeTextNumber = targetRangeTextNumber;   
        }
    }

    /// <summary>
    /// 对目标视频文件进行排序
    /// </summary>
    private void SortVideoFile(){
        //根据数字对视频文件进行排序
        this.videoFileList.Sort((x, y) =>{
            if(x.targetRangeTextNumber == null){
                return 1;
            } else if(y.targetRangeTextNumber == null){
                return -1;
            }else{
                return x.targetRangeTextNumber.Value.CompareTo(y.targetRangeTextNumber.Value);
            }
        });
    }

    /// <summary>
    /// 获取权重最大的范围索引
    /// </summary>
    /// <returns></returns>
    private int getMaxWeightRangeIndex(List<int> rangeWeightList){
        //选取权重最大的范围的索引
        int count = 0;
        int index = 0;
        for(int i = 0 ; i < rangeWeightList.Count; i++){
            int c = rangeWeightList[i];
            if(c > count){
                count = c;
                index = i;
            }
        }
        return index;
    }

    /// <summary>
    /// 初始化范围权重
    /// </summary>
    private void InitAssetFileRangeWeight(List<AssetFile> assetFileList, List<int> rangeWeightList){
        //取得权重个数最大的值
        //ranage list 索引相同，但是 start end 之间数字不同的，权重值+1，数字相同的，权重-1
        int maxWeightCount = 0;//计算出最大权重
        for(int i = 0; i < assetFileList.Count; i++){
            AssetFile videoFile = assetFileList[i];
            int weight = videoFile.numberRanageList.Count;
            if(weight > maxWeightCount){
                maxWeightCount = weight;
            }
        }

        //初始化权重
        rangeWeightList.Clear();
        for(int i = 0; i < maxWeightCount; i++){
            rangeWeightList.Add(0);
        }

        //便利视频文件中包含数字的权重列表
        for(int i = 0; i < assetFileList.Count - 1; i++){
            AssetFile assetFile = assetFileList[i];
            for(int j = 0; j < assetFileList.Count; j++){
                AssetFile assetFile2 = assetFileList[j];

                int count = assetFile.numberRanageList.Count;
                int count2 = assetFile2.numberRanageList.Count;

                List<Ranage> listMin, listMax;
                if(count > count2){
                    listMax = assetFile.numberRanageList;
                    listMin = assetFile2.numberRanageList;
                }else{
                    listMin = assetFile.numberRanageList;
                    listMax = assetFile2.numberRanageList;
                }

                //遍历相同索引的范围
                for(int k = 0; k < listMin.Count; k++){
                    Ranage ranage1 = listMin[k];
                    Ranage ranage2 = listMax[k];
                    if(ranage1.text != ranage2.text){//如果这个索引项中的内容不一样
                        rangeWeightList[k]++;//那么权重加1
                    }
                }
            }
        }
    }
}