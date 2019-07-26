@echo off

:: 输出帮助信息
:: 暂时先酱紫提示就好。后续有空了添加一个环境检测功能。不存在 dotnet 环境时，提示用户是否安装。
:: 用户拒绝安装则退出程序，接受则帮用户下载安装。之后才开始执行程序
echo 请确保您的电脑中已经安装了 .net，否则会输出：dotnet 不是内部或外部命名……
echo.
echo 下面开始执行
echo.


:: 变更工作目录为当前bat文件的所在的文件夹
cd /d %~dp0

:: 执行真正的命令
dotnet RenameSubtitles.dll %*
pause

